using OpenTS2.Files.Utils;
using OpenTS2.Lua.Disassembly.OpCodes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenTS2.Lua.Disassembly
{
    public class LuaC50
    {
        private Function _mainFunction;
        private static readonly byte[] ExpectedMagic = new byte[] { 0x1b, (byte)'L', (byte)'u', (byte)'a' };
        private static readonly byte[] ExpectedSample = new byte[] { 0xb6, 0x09, 0x93, 0x68, 0xe7, 0xf5, 0x7d, 0x41 };

        private byte _endianness = 1;

        private byte _intSize = 4;
        private byte _sizeTSize = 4;
        private byte _instructionSize = 4;

        private byte _operandBits = 6;
        private byte _bits1 = 8;
        private byte _bits2 = 9;
        private byte _bits3 = 9;

        public byte ABits
        {
            get { return _bits1; }
        }

        public byte BBits
        {
            get { return _bits2; }
        }

        public byte CBits
        {
            get { return _bits3; }
        }

        public byte NumberSize
        {
            get { return _numberSize; }
        }

        public byte OpcodeBits
        {
            get { return _operandBits; }
        }

        internal uint OpcodeMaks
        {
            get { return (uint)Math.Pow(2, this.OpcodeBits) - 1; }
        }
        internal byte OpcodeShift
        {
            get { return 0; }
        }

        internal uint AMaks
        {
            get { return (uint)Math.Pow(2, this.ABits) - 1; }
        }
        internal byte AShift
        {
            get { return (byte)(this.BShift + this.BBits); }
        }

        internal uint BMaks
        {
            get { return (uint)Math.Pow(2, this.BBits) - 1; }
        }
        internal byte BShift
        {
            get { return (byte)(this.CShift + this.CBits); }
        }

        internal uint CMaks
        {
            get { return (uint)Math.Pow(2, this.CBits) - 1; }
        }
        internal byte CShift
        {
            get { return (byte)(this.OpcodeShift + this.OpcodeBits); }
        }

        internal int Bias
        {
            get
            {
                return ((int)Math.Pow(2, BBits + CBits) - 1) / 2;
            }
        }
        private byte _numberSize = 8;

        public LuaC50(IoBuffer reader)
        {
            Deserialize(reader);
        }

        static string ReadLengthPrefixedString(IoBuffer reader)
        {
            var len = reader.ReadUInt32();
            var str = Encoding.UTF8.GetString(reader.ReadBytes(len));
            if (str.Length == 0)
                return str;
            if (str[str.Length - 1] == 0)
                str = str.Substring(0, str.Length - 1);
            return str;
        }

        public string Disassemble()
        {
            return _mainFunction.Disassemble().ToString();
        }

        void Deserialize(IoBuffer reader)
        {
            var magic = reader.ReadBytes(ExpectedMagic.Length);

            if (!magic.SequenceEqual(ExpectedMagic))
                throw new IOException("Invalid Lua header!");

            var version = reader.ReadByte();

            if (version != 0x50)
                throw new IOException("Lua version is not 5.0!");

            _endianness = reader.ReadByte();

            _intSize = reader.ReadByte();
            _sizeTSize = reader.ReadByte();
            _instructionSize = reader.ReadByte();

            _operandBits = reader.ReadByte();
            _bits1 = reader.ReadByte();
            _bits2 = reader.ReadByte();
            _bits3 = reader.ReadByte();

            _numberSize = reader.ReadByte();

            var sample = reader.ReadBytes(ExpectedSample.Length);

            if (!sample.SequenceEqual(ExpectedSample))
                throw new IOException("Invalid Lua header!");

            _mainFunction = new Function(reader, this);
        }

        public class Function
        {
            public LuaC50 Owner;
            public string Name;

            private uint _lineDef;
            private byte _nUps;
            private byte _argumentCount;
            private byte _isInOut;
            private byte _stackSize;

            public List<SourceLine> SourceLines = new List<SourceLine>();
            public List<LocalVariable> LocalVariables = new List<LocalVariable>();
            public List<UpValue> UpValues = new List<UpValue>();
            public List<Constant> Constants = new List<Constant>();
            public List<Function> Functions = new List<Function>();
            public List<OpCode> OpCodes = new List<OpCode>();
            public byte ArgumentCount => _argumentCount;

            public Function(IoBuffer reader, LuaC50 lua)
            {
                Owner = lua;
                Deserialize(reader);
            }

            public void Deserialize(IoBuffer reader)
            {
                Name = ReadLengthPrefixedString(reader);

                _lineDef = reader.ReadUInt32();
                _nUps = reader.ReadByte();
                _argumentCount = reader.ReadByte();
                _isInOut = reader.ReadByte();
                _stackSize = reader.ReadByte();

                uint sourceLinesAmount = reader.ReadUInt32();

                for (var i = 0; i < sourceLinesAmount; i++)
                {
                    var sourceLine = new SourceLine(reader);

                    SourceLines.Add(sourceLine);
                }

                uint localVarAmount = reader.ReadUInt32();

                for (var i = 0; i < localVarAmount; i++)
                {
                    var localVar = new LocalVariable(reader);

                    LocalVariables.Add(localVar);
                }

                uint upValueAmount = reader.ReadUInt32();

                for (var i = 0; i < upValueAmount; i++)
                {
                    var upValue = new UpValue(reader);

                    UpValues.Add(upValue);
                }

                uint constAmount = reader.ReadUInt32();

                for (var i = 0; i < constAmount; i++)
                {
                    var cnst = new Constant(reader, this);

                    Constants.Add(cnst);
                }

                uint functionAmount = reader.ReadUInt32();

                for (var i = 0; i < functionAmount; i++)
                {
                    var func = new Function(reader, Owner);

                    Functions.Add(func);
                }

                uint codeAmount = reader.ReadUInt32();

                for (var i = 0; i < codeAmount; i++)
                {
                    var operands = reader.ReadUInt32();

                    var op = OpCodeFactory.GetOpCode(operands, this);
                    op.PC = OpCodes.Count;
                    OpCodes.Add(op);
                }
            }

            public void DisassembleIntoContext(Context context)
            {
                var inheritedContext = context;

                context = new Context();
                context.Parent = inheritedContext;
                context.Level = inheritedContext.Level + 1;
                context.Code = new CodeBuilder();
                context.Code.Indentation = inheritedContext.Code.Indentation;
                context.PC = 0;
                context.Function = this;

                var paramList = "";
                for(var i=0;i<_argumentCount;i++)
                {
                    if (i > 0) paramList += ", ";
                    paramList += context.R((ushort)i);
                }
                context.Code.WriteLine("function " + inheritedContext.R(inheritedContext.OpCode.A) + "("+paramList+")");
                context.Code.Indentation++;
                DisassembleInternal(context);
                context.Code.Indentation--;
                context.Code.WriteLine("end");

                var oldIndent = inheritedContext.Code.Indentation;
                inheritedContext.Code.Indentation = 0;
                inheritedContext.Code.WriteLine(context.Code.ToString());
                inheritedContext.Code.Indentation = oldIndent;
            }

            void DisassembleInternal(Context context)
            {
                for (var i = OpCodes.Count - 1; i >= 0; i--)
                {
                    var returnOpCode = OpCodes[i] as RETURN;
                    if (returnOpCode != null)
                    {
                        context.ReturnOpCode = returnOpCode;
                        break;
                    }
                }
                for (var i = 0; i < OpCodes.Count; i++)
                {
                    context.PC = i;
                    var opCode = OpCodes[i];
                    context.OpCode = opCode;
                    opCode.PreProcess(context);
                }
                for (var i = 0; i < OpCodes.Count; i++)
                {
                    context.PC = i;
                    foreach(var forloop in context.ForLoops)
                    {
                        if (forloop.PC == context.PC)
                            forloop.ForLoop.DisassembleBegin(context, forloop);
                    }
                    var jumpLabels = context.GetJumpLabelsHere(context.PC);
                    foreach(var jumpLabel in jumpLabels)
                    {
                        context.Code.WriteLabel(jumpLabel);
                    }
                    var opCode = OpCodes[i];
                    context.OpCode = opCode;
                    opCode.Disassemble(context);
                }
            }

            public CodeBuilder Disassemble()
            {
                var context = new Context();
                context.Code = new CodeBuilder();
                context.PC = 0;
                context.Function = this;
                DisassembleInternal(context);
                return context.Code;
            }
        }

        public class SourceLine
        {
            public SourceLine(IoBuffer reader)
            {
                // Maybe a length prefixed string?
                reader.ReadUInt32();
            }
        }

        public class LocalVariable
        {
            public LocalVariable(IoBuffer reader)
            {
                var name = ReadLengthPrefixedString(reader);
                var start = reader.ReadUInt32();
                var end = reader.ReadUInt32();
            }
        }

        public class UpValue
        {
            public UpValue(IoBuffer reader)
            {
                reader.ReadUInt32();
            }
        }

        public class Constant
        {
            public enum Type : byte
            {
                Empty = 0x00,
                Number = 0x03,
                String = 0x04
            }

            public Type ConstantType;
            public string String;
            public double Number;

            public Constant(IoBuffer reader, Function func)
            {
                var type = (Type)reader.ReadByte();
                ConstantType = type;
                if (type == Type.String)
                    String = ReadLengthPrefixedString(reader);
                else if (type == Type.Number)
                {
                    if (func.Owner._numberSize == 8)
                        Number = reader.ReadDouble();
                    else if (func.Owner._numberSize == 4)
                        Number = reader.ReadSingle();
                    else
                        throw new IOException("Number size unsupported.");
                }
                else if (type != Type.Empty)
                    throw new IOException("Invalid constant type!");
            }
        }

        public class OpCode
        {
            public static readonly int FPF = 50;
            public int PC;
            public uint Operands;
            public Function Function;
            public ushort A
            {
                get
                {
                    return (ushort)((Operands & (Function.Owner.AMaks << Function.Owner.AShift)) >> Function.Owner.AShift);
                }
                set
                {
                    Operands = ((uint)(Operands & (0xFFFFFFFF - (Function.Owner.AMaks << Function.Owner.AShift))) | (uint)((value & Function.Owner.AMaks) << Function.Owner.AShift));
                }
            }

            public ushort B
            {
                get
                {
                    return (ushort)((Operands & (Function.Owner.BMaks << Function.Owner.BShift)) >> Function.Owner.BShift);
                }
                set
                {
                    Operands = ((uint)(Operands & (0xFFFFFFFF - (Function.Owner.BMaks << Function.Owner.BShift))) | (uint)((value & Function.Owner.BMaks) << Function.Owner.BShift));
                }
            }

            public ushort C
            {
                get
                {
                    return (ushort)((Operands & (Function.Owner.CMaks << Function.Owner.CShift)) >> Function.Owner.CShift);
                }
                set
                {
                    Operands = ((uint)(Operands & (0xFFFFFFFF - (Function.Owner.CMaks << Function.Owner.CShift))) | (uint)((value & Function.Owner.CMaks) << Function.Owner.CShift));
                }
            }

            public uint Bx
            {
                get { return ((B & Function.Owner.BMaks) << Function.Owner.CBits) | (C & Function.Owner.CMaks); }
            }

            public int sBx
            {
                get { return (int)((long)Bx - Function.Owner.Bias); }
            }

            public virtual int GetPCForJumpTarget()
            {
                return PC;
            }

            public virtual void PreProcess(Context context)
            {

            }

            public virtual void Disassemble(Context context)
            {
                UnityEngine.Debug.Log("Tried to use unimplemented opcode.");
                context.Code.WriteLine("-- Not implemented.");
            }

            protected bool GetBool(ushort value)
            {
                if (value > 0)
                    return true;
                return false;
            }

            protected string GetLuaBool(bool value)
            {
                if (value)
                    return "true";
                return "false";
            }
        }

        public class JumpLabel
        {
            public int TargetPC;
            public string Label;

            public JumpLabel(int index, int targetPC)
            {
                Label = $"jumpTarget_{index}";
                TargetPC = targetPC;
            }
        }

        public class BeginFORLOOP
        {
            public int PC;
            public FORLOOP ForLoop;

            public BeginFORLOOP(int pc, FORLOOP forLoop)
            {
                PC = pc;
                ForLoop = forLoop;
            }
        }

        public class Context
        {
            public OpCode OpCode;
            public CodeBuilder Code;
            public Function Function;
            public List<JumpLabel> JumpLabels = new List<JumpLabel>();
            public List<BeginFORLOOP> ForLoops = new List<BeginFORLOOP>();
            public RETURN ReturnOpCode;
            public int Level = 0;
            public Context Parent;
            /// <summary>
            /// Program Counter
            /// </summary>
            public int PC;
            public string ReturnTable => $"ReturnTable_{Level}";
            private static readonly int RK_OFFSET = 250;

            public List<JumpLabel> GetJumpLabelsHere(int pc)
            {
                var outputs = new List<JumpLabel>();
                foreach(var label in JumpLabels)
                {
                    if (label.TargetPC == pc)
                        outputs.Add(label);
                }
                return outputs;
            }

            public JumpLabel MakeRelativeJump(int offset)
            {
                return MakeAbsoluteJump(PC + offset);
            }

            public JumpLabel MakeAbsoluteJump(int pc)
            {
                var opCodeAtPC = Function.OpCodes[pc];
                pc = opCodeAtPC.GetPCForJumpTarget();
                var jLabels = GetJumpLabelsHere(pc);
                if (jLabels.Count > 0)
                    return jLabels[0];
                var jumpLabelID = JumpLabels.Count;
                var jumpLabel = new JumpLabel(jumpLabelID, pc);
                JumpLabels.Add(jumpLabel);
                return jumpLabel;
            }

            public Function KProto(uint index)
            {
                return Function.Functions[(int)index];
            }

            public string RKAsString(ushort value)
            {
                if (value < RK_OFFSET) return R(value);
                return KAsString((ushort)(value - RK_OFFSET));
            }

            public string RK(ushort value)
            {
                if (value < RK_OFFSET) return R(value);
                return K((ushort)(value - RK_OFFSET));
            }

            public string R(ushort value)
            {
                var prefix = "Reg";
                if (Function.ArgumentCount > value)
                    prefix = "Arg";
                return $"{prefix}_{value}_{Level}";
            }

            public string K(uint value)
            {
                var cnst = Function.Constants[(int)value];
                switch(cnst.ConstantType)
                {
                    case Constant.Type.Empty:
                        return "nil";
                    case Constant.Type.String:
                        return PutQuotesIfAppropriate(EscapeString(cnst.String));
                    case Constant.Type.Number:
                        return cnst.Number.ToString(CultureInfo.InvariantCulture);
                }
                return "nil";
            }

            public string EscapeString(string str)
            {
                return Regex.Replace(str, @"\r\n?|\n", @"\n");
            }

            public string KAsString(uint value)
            {
                var cnst = Function.Constants[(int)value];
                switch (cnst.ConstantType)
                {
                    case Constant.Type.Empty:
                        return "nil";
                    case Constant.Type.String:
                        return $"\"{EscapeString(cnst.String)}\"";
                    case Constant.Type.Number:
                        return cnst.Number.ToString(CultureInfo.InvariantCulture);
                }
                return "nil";
            }

            string PutQuotesIfAppropriate(string str)
            {
                if (str.Contains(" "))
                    return $"\"{str}\"";
                return str;
            }

            public string U(ushort value)
            {
                if (Parent != null)
                {
                    return Parent.R(value);
                }
                if (value >= Function.UpValues.Count)
                    return "0";
                return Function.UpValues[value].ToString();
            }

            public string G(ushort value)
            {
                return K(value);
            }
        }
    }
}
