/***************************************************************************
 *   Copyright (C) 2005 by Ambertation                                     *
 *   quaxi@ambertation.de                                                  *
 *                                                                         *
 *   This program is free software; you can redistribute it and/or modify  *
 *   it under the terms of the GNU General Public License as published by  *
 *   the Free Software Foundation; either version 2 of the License, or     *
 *   (at your option) any later version.                                   *
 *                                                                         *
 *   This program is distributed in the hope that it will be useful,       *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of        *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
 *   GNU General Public License for more details.                          *
 *                                                                         *
 *   You should have received a copy of the GNU General Public License     *
 *   along with this program; if not, write to the                         *
 *   Free Software Foundation, Inc.,                                       *
 *   59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.             *
 ***************************************************************************/

using System;
using System.Collections;
using System.Text;

namespace OpenTS2.Lua.Decompiler
{

	/// <summary>
	/// Represents a PackedFile in SDsc Format
	/// </summary>
	public class ObjLua
	{
		enum Endian : byte
		{
			big = 0x0,
			little = 0x1
		}

		#region Attributes
		string flname;
		public string FileName
		{
			get { return flname; }
			set { flname = value; }
		}
		uint resversion;

		uint id;
		byte version;
		Endian byteorder;
		byte intsz;
		byte sztsz;
		byte instsz;
		byte operandbits;
		public byte OpcodeBits
		{
			get { return operandbits; }
		}

		byte bits1;
		public byte ABits
		{
			get { return bits1; }
		}
		byte bits2;
		public byte BBits
		{
			get { return bits2; }
		}
		byte bits3;
		public byte CBits
		{
			get { return bits3; }
		}

		byte nrsz;
		public byte NumberSize
		{
			get { return nrsz; }
		}
		byte[] sample;


		ObjLuaFunction root;
		public ObjLuaFunction Root
		{
			get { return root; }
		}
		#endregion

		#region Code Properties 
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

		public ObjLua()
		{
			resversion = 0;
			version = 0x50;
			byteorder = Endian.little;
			intsz = 4;
			sztsz = 4;
			instsz = 4;
			operandbits = 6;
			bits1 = 8;
			bits2 = 9;
			bits3 = 9;
			nrsz = 8;
			id = 0x61754C1B;
			sample = new byte[] { 0xb6, 0x09, 0x93, 0x68, 0xe7, 0xf5, 0x7d, 0x41 };

			flname = "";

			root = new ObjLuaFunction(this);
		}

		internal static string ReadString(System.IO.BinaryReader reader)
		{
			int ct = reader.ReadInt32();
			var str = Encoding.UTF8.GetString(reader.ReadBytes(ct));
			if (str.Length > 0)
			{
				if (str[str.Length - 1] == 0)
				{
					str = str.Substring(0, str.Length - 1);
				}
			}
			return str;
		}


		public string ToSource()
		{
				System.IO.StreamWriter sw = new System.IO.StreamWriter(new System.IO.MemoryStream());
				try
				{
					string[] regs = new string[0xff];
					for (int i = 0; i < regs.Length; i++) regs[i] = "";

					Context cx = new Context();
					root.ToSource(sw, cx);
					sw.Flush();

					System.IO.StreamReader sr = new System.IO.StreamReader(sw.BaseStream);
					sw.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
					return sr.ReadToEnd();
				}
				finally
				{
					sw.Close();
				}
		}


		public void UnserializeLua(System.IO.BinaryReader reader)
		{
			id = reader.ReadUInt32();

			version = reader.ReadByte();
			byteorder = (Endian)reader.ReadByte();

			intsz = reader.ReadByte();
			sztsz = reader.ReadByte();
			instsz = reader.ReadByte();

			operandbits = reader.ReadByte();
			bits1 = reader.ReadByte();
			bits2 = reader.ReadByte();
			bits3 = reader.ReadByte();

			nrsz = reader.ReadByte();
			sample = reader.ReadBytes(sample.Length);


			root.Unserialize(reader);
		}

		public Byte[] FileSignature
		{
			get
			{
				Byte[] sig = {
							 };
				return sig;
			}
		}

		#endregion


		public class JumpLabel
        {
			public string Label;
			public string FormattedLabel => $"{"::"}{Label}{"::"}";
			public int TargetPC;
			public JumpLabel(int index, int targetPC)
            {
				Label = "jtarget_" + index;
				TargetPC = targetPC;
            }
        }

	public class ObjLuaFunction : System.IDisposable, System.Collections.IEnumerable
	{
			internal static bool DEBUG = false;

		#region Attributes
		ObjLua parent;
		public ObjLua Parent
		{
			get { return parent; }
		}

		string name;
		uint linedef;
		byte nups;
		byte argc;
		public byte ArgumentCount
		{
			get { return argc; }
		}
		byte isinout;
		byte stacksz;
		public byte StackSize
		{
			get { return stacksz; }
		}

			ArrayList contants, functions, codes, srcln, upval, local;

		public ArrayList Constants
		{
			get { return contants; }
		}

		public ArrayList UpValues
		{
			get { return upval; }
		}

		public ArrayList Locals
		{
			get { return local; }
		}

		public ArrayList SourceLine
		{
			get { return srcln; }
		}

		public ArrayList Functions
		{
			get { return functions; }
		}

		public ArrayList Codes
		{
			get { return codes; }
		}
		#endregion

		public ObjLuaFunction(ObjLua parent)
		{
			this.parent = parent;
			name = "";

			contants = new ArrayList();
			functions = new ArrayList();
			codes = new ArrayList();
			srcln = new ArrayList();
			local = new ArrayList();
			upval = new ArrayList();
		}

		#region Source Code
		void PrintLine(ArrayList sw, Context cx, ObjLuaCode line, string plusindent)
		{
			if (line != null)
			{
				string content = "";
				if (line is IOperator)
				{
					IOperator lop = line as IOperator;
					if (ObjLuaFunction.DEBUG)
						content = lop.ToString(cx) + " #" + line.GetType().Name;
					else
						content = lop.ToString(cx);
					lop.Run(cx);
				}
				else
				{
					content = line.ToString();
				}

				if (content.Trim() != "")
				{
					sw.Add(plusindent + cx.Indent + content);
				}
			}
		}

		void AddIndent(ref string indent)
		{
			indent += "\t";
		}
		void BackIndent(ref string indent)
		{
			if (indent.Length > 0) indent = indent.Substring(0, indent.Length - 1);
		}

		internal void ToSource(System.IO.StreamWriter writer, Context cx)
		{
			cx.Init(this);

			ObjLuaCode line = null;
			ArrayList endline = new ArrayList();
			ArrayList elseline = new ArrayList();
			ArrayList oplines = new ArrayList();
			ArrayList sw = new ArrayList();
			string pindent = "";
			for (int i = 0; i < codes.Count - 1; i++)
			{
				oplines.Add(sw.Count);
				cx.GoToLine(i);
				line = cx.CurrentLine;
				//for loop check
				if (line is SUB)
				{
					int pc = cx.PC;
					ObjLuaCode nline = cx.NextLine();
					if (nline is JMP)
					{
						cx.GoToLine(cx.PC + nline.SBX + 1);
						ObjLuaCode fline = cx.CurrentLine;
						if (fline is FORLOOP)
						{
							cx.GoToLine(pc);
							FORLOOP fl = fline as FORLOOP;
							fl.IsStart = true;
							PrintLine(sw, cx, fline, pindent);
							fl.IsStart = false;

							continue;
						}
					}
					cx.GoToLine(pc);
				}


				foreach (var jLabel in cx.JumpLabels )
                {
						if (cx.PC == jLabel.TargetPC)
							sw.Add(pindent + jLabel.FormattedLabel);
                }

			    if (line is IAddEnd)
				{
					IAddEnd end = line as IAddEnd;
					endline.Add(cx.PC + end.Offset);
				}

				if (line is IIfOperator)
				{
					PrintLine(sw, cx, line, pindent);
					AddIndent(ref pindent);
						line = cx.NextLine();
						PrintLine(sw, cx, line, pindent);
						BackIndent(ref pindent);
						sw.Add(pindent + "end");
						/*
					int pc = cx.PC;
					ObjLuaCode oline = line;
					line = cx.NextLine();
					int ifblsz = line.SBX;
					if (ifblsz < 0) //while block
					{

						int npc = (int)oplines[oplines.Count + line.SBX + 1];
						for (int id = npc; id < sw.Count; id++)
							sw[id] = "\t" + sw[id].ToString();

						oline.A = (ushort)Math.Abs(oline.A - 1);
						sw.Insert(npc, pindent + "while " + ((IOperator)oline).ToString(cx).Replace("if ", "").Replace(" then", "") + " do");
						oline.A = (ushort)Math.Abs(oline.A - 1);

						this.BackIndent(ref pindent);
						PrintLine(sw, cx, new TextLine(0, this, "end"), pindent);

						continue;
					}
					else
					{
						cx.PrepareJumpToLine(cx.PC + ifblsz);
						line = cx.NextLine();
						if (line is JMP)  //having an else Block
						{
							elseline.Add(cx.PC - 1);
							endline.Add(cx.PC + line.SBX);
						}
						else
							endline.Add(cx.PC);
					}

					cx.GoToLine(pc);
					line = cx.CurrentLine;*/
				}

				PrintLine(sw, cx, line, pindent);

				if (line is TFORREP)
				{
					int pc = cx.PC;
					ObjLuaCode eline = line;
					while (!(eline is TFORLOOP))
						eline = cx.NextLine();

					((TFORLOOP)eline).Setup(cx);
					cx.GoToLine(pc);

					((TFORREP)line).TFORLOOP = eline as TFORLOOP;
					PrintLine(sw, cx, line, pindent);
					((TFORREP)line).TFORLOOP = null;
				}


				while (endline.Contains(i))
				{
					BackIndent(ref pindent);
					sw.Add(pindent + cx.Indent + "end");
					endline.Remove(i);
				}
				if (elseline.Contains(i))
				{
					BackIndent(ref pindent);
					sw.Add(pindent + cx.Indent + "else");
					AddIndent(ref pindent);
				}
			}

			foreach (string ln in sw)
				writer.WriteLine(ln);
		}

		internal bool IsLocalRegister(ushort regnr, Context cx)
		{
			ObjLuaCode line = null;
			for (int i = cx.PC + 1; i < codes.Count; i++)
			{
				line = codes[i] as ObjLuaCode;
				if (line != null)

					if (line is ILoadOperator)
					{
						ILoadOperator lop = line as ILoadOperator;
						if (lop.LoadsRegister(regnr)) return false;
					}
			}

			return true;
		}
		#endregion

		internal void Unserialize(System.IO.BinaryReader reader)
		{
			contants.Clear();
			functions.Clear();
			codes.Clear();
			srcln.Clear();
			local.Clear();
			upval.Clear();

			name = ObjLua.ReadString(reader);

			linedef = reader.ReadUInt32();
			nups = reader.ReadByte();
			argc = reader.ReadByte();
			isinout = reader.ReadByte();
			stacksz = reader.ReadByte();

			uint linenfosz = reader.ReadUInt32();
			for (uint i = 0; i < linenfosz; i++)
			{
				ObjLuaSourceLine item = new ObjLuaSourceLine(this);
				item.Unserialize(reader);

				srcln.Add(item);
			}

			uint locvarsz = reader.ReadUInt32();
			for (uint i = 0; i < locvarsz; i++)
			{
				ObjLuaLocalVar item = new ObjLuaLocalVar(this);
				item.Unserialize(reader);

				local.Add(item);
			}

			uint upvalsz = reader.ReadUInt32();
			for (uint i = 0; i < upvalsz; i++)
			{
				ObjLuaUpValue item = new ObjLuaUpValue(this);
				item.Unserialize(reader);

				upval.Add(item);
			}

			uint constsz = reader.ReadUInt32();
			for (uint i = 0; i < constsz; i++)
			{
				ObjLuaConstant item = new ObjLuaConstant(this);
				item.Unserialize(reader);

				contants.Add(item);
			}

			uint fncsz = reader.ReadUInt32();
			for (uint i = 0; i < fncsz; i++)
			{
				ObjLuaFunction item = new ObjLuaFunction(this.parent);
				item.Unserialize(reader);

				functions.Add(item);
			}

			uint codesz = reader.ReadUInt32();
			for (uint i = 0; i < codesz; i++)
			{
				ObjLuaCode item = ObjLuaCode.Unserialize(reader, this);

				codes.Add(item);
			}

		}



		#region IDisposable Member

		public void Dispose()
		{
			parent = null;
			if (contants != null) contants.Clear();
			contants = null;
		}

		#endregion

		public override string ToString()
		{
			return name + ": " + this.argc.ToString() + " Arguments, Stacksize " + this.stacksz.ToString() + ", " + contants.Count.ToString() + " Constants, " + functions.Count.ToString() + " SubFunctions, " + codes.Count.ToString() + " Instructions";
		}
		#region IEnumerable Member

		public IEnumerator GetEnumerator()
		{
			return contants.GetEnumerator();
		}

		#endregion



	}


	public class ObjLuaConstant : System.IDisposable
	{
		public enum Type : byte
		{
			Empty = 0x00,
			Number = 0x03,
			String = 0x04
		}

		#region Attributes
		ObjLuaFunction parent;
		public ObjLuaFunction Parent
		{
			get { return parent; }
		}

		Type type;
		public Type InstructionType
		{
			get { return type; }
			set { type = value; }
		}

		string str;
		public string String
		{
			get { return str; }
			set { str = value; }
		}


		double dval;
		public double Value
		{
			get { return dval; }
			set { dval = value; }
		}

		uint[] bdata;
		byte[] badd;
		byte[] bheader;

		#endregion

		public ObjLuaConstant(ObjLuaFunction parent)
		{
			this.parent = parent;
			str = "";


			bdata = new uint[0];
			badd = new byte[0];
			bheader = new byte[3];
		}

			

			internal void Unserialize(System.IO.BinaryReader reader)
		{
			type = (Type)reader.ReadByte();

			str = "";
			bdata = new uint[0];
			badd = new byte[0];
			if (type == Type.String)
			{
				str = ObjLua.ReadString(reader);
			}
			else if (type == Type.Number)
			{
				if (parent.Parent.NumberSize == 8)
				{
					dval = reader.ReadDouble();
				}
				else if (parent.Parent.NumberSize == 4)
				{
					dval = reader.ReadSingle();

				}
				else throw new Exception("Number Size " + parent.Parent.NumberSize.ToString() + " is not supported!");
			}
			else if (type == Type.Empty)
			{
			}
			else
			{
				throw new Exception("unknown constant type");
			}
		}
		#region IDisposable Member

		public void Dispose()
		{
			parent = null;
			str = null;

			badd = null;
			bheader = null;
			bdata = null;
		}

		#endregion

		public override string ToString()
		{
			string s = type.ToString() + ": ";
			if (type == Type.String) s += str;
			else if (type == Type.Number) s += dval.ToString();

			return s;
		}

	}

	public class ObjLuaSourceLine : System.IDisposable
	{

		#region Attributes
		ObjLuaFunction parent;

		uint val;
		public uint Value
		{
			get { return val; }
			set { val = value; }
		}



		#endregion

		public ObjLuaSourceLine(ObjLuaFunction parent)
		{
			this.parent = parent;
		}

		internal void Unserialize(System.IO.BinaryReader reader)
		{
			val = reader.ReadUInt32();
		}


		internal void Serialize(System.IO.BinaryWriter writer)
		{
			writer.Write(val);
		}
		#region IDisposable Member

		public void Dispose()
		{

		}

		#endregion

	}

	public class ObjLuaLocalVar : System.IDisposable
	{

		#region Attributes
		ObjLuaFunction parent;

		uint start, end;
		public uint Start
		{
			get { return start; }
			set { start = value; }
		}

		public uint End
		{
			get { return end; }
			set { end = value; }
		}

		string name;
		public string Name
		{
			get { return name; }
			set { name = value; }
		}



		#endregion

		public ObjLuaLocalVar(ObjLuaFunction parent)
		{
			this.parent = parent;
			name = "";
		}
			internal void Unserialize(System.IO.BinaryReader reader)
			{
				name = ObjLua.ReadString(reader);
				start = reader.ReadUInt32();
				end = reader.ReadUInt32();
			}

			#region IDisposable Member

			public void Dispose()
		{
			name = null;
		}

		#endregion

	}

	public class ObjLuaUpValue : ObjLuaSourceLine
	{
		public ObjLuaUpValue(ObjLuaFunction parent) : base(parent)
		{
		}
	}

	public class ObjLuaCode : System.IDisposable
	{
		static Hashtable ocmap;
		protected static void PrepareOpcodeMap()
		{
			ocmap = new Hashtable();

			ocmap["MOVE"] = typeof(MOVE);
			ocmap["LOADNIL"] = typeof(LOADNIL);
			ocmap["LOADK"] = typeof(LOADK);
			ocmap["LOADBOOL"] = typeof(LOADBOOL);
			ocmap["SETGLOBAL"] = typeof(SETGLOBAL);
			ocmap["GETGLOBAL"] = typeof(GETGLOBAL);
			ocmap["CALL"] = typeof(CALL);
			ocmap["CLOSURE"] = typeof(CLOSURE);
			ocmap["CONCAT"] = typeof(CONCAT);
			ocmap["NEWTABLE"] = typeof(NEWTABLE);
			ocmap["SELF"] = typeof(SELF);
			ocmap["SETTABLE"] = typeof(SETTABLE);
			ocmap["SETLIST"] = typeof(SETLIST);
			ocmap["TEST"] = typeof(TEST);
			ocmap["TFORLOOP"] = typeof(TFORLOOP);
			ocmap["TFORREP"] = typeof(TFORREP);
			ocmap["FORLOOP"] = typeof(FORLOOP);
			ocmap["GETTABLE"] = typeof(GETTABLE);
			ocmap["RETURN"] = typeof(RETURN);
			ocmap["ADD"] = typeof(ADD);
			ocmap["SUB"] = typeof(SUB);
			ocmap["MUL"] = typeof(MUL);
			ocmap["POW"] = typeof(POW);
			ocmap["DIV"] = typeof(DIV);
			ocmap["UNM"] = typeof(UNM);
			ocmap["NOT"] = typeof(NOT);
			ocmap["GETUPVAL"] = typeof(GETUPVAL);

			ocmap["JMP"] = typeof(JMP);
			ocmap["EQ"] = typeof(EQ);
			ocmap["LE"] = typeof(LE);
			ocmap["GE"] = typeof(GE);
			ocmap["LT"] = typeof(LT);
			ocmap["GT"] = typeof(GT);
		}

		protected static Type GetOpcodeType(byte opcode)
		{
			PrepareOpcodeMap();
			string n = GetOpcodeName(opcode);
			Type t = ocmap[n] as Type;

			if (t == null) return typeof(ObjLuaCode);
			return t;
		}

			public static string GetOpcodeName(byte oc)
			{
				if (oc >= 0 && oc < opcodes.Length) return opcodes[oc];
				else return "unk";
			}

			public static ObjLuaCode CreateOperator(uint val, ObjLuaFunction parent)
		{
			byte oc = GetOpCode(val, parent);
			Type t = GetOpcodeType(oc);

			ObjLuaCode ret = (ObjLuaCode)System.Activator.CreateInstance(t, new object[] { val, parent });
			return ret;
		}

		public const int RK_OFFSET = 250;
		#region OpCodes		
		static string[] opcodes = new string[]{
												  "MOVE",
												  "LOADK",
												  "LOADBOOL",
												  "LOADNIL",
												  "GETUPVAL",
												  "GETGLOBAL",
												  "GETTABLE",
												  "SETGLOBAL",
												  "SETUPVAL",
												  "SETTABLE",
												  "NEWTABLE",
												  "SELF",
												  "ADD",
												  "SUB",
												  "MUL",
												  "DIV",
												  "POW",
												  "UNM",
												  "NOT",
												  "CONCAT",
												  "JMP",
												  "EQ",
												  "LT",
												  "LE",
												  "TEST",
												  "CALL",
												  "TAILCALL",
												  "RETURN",
												  "FORLOOP",
												  "TFORLOOP",
												  "TFORREP",
												  "SETLIST",
												  "SETLISTO",
												  "CLOSE",
												  "CLOSURE"
											  };
		static string[] opcodedesc = new string[]{
												  "Copy a value between registers",
												  "Load a constant into a register",
												  "Load a boolean into a register",
												  "Load null values into a range of registers",
												  "Read an upvalue into a register",
												  "Read a global variable into a register",
												  "Read a table element into a register",
												  "Write a register value into a global variable",
												  "Write a register value into an upvalue",
												  "Write a register value into a table element",
												  "Create a new table",
												  "Prepare an object method for calling",
												  "Addition",
												  "Subtraction",
												  "Multiplication",
												  "Division",
												  "Exponentiation",
												  "Unary minus",
												  "Logical NOT",
												  "Concatenate a range of registers",
												  "Unconditional jump",
												  "Equality test",
												  "Less than test",
												  "Less than or equal to test",
												  "Test for short-circuit logical and and or evaluation",
												  "Call a closure",
												  "Perform a tail call",
												  "Return from function call",
												  "Iterate a numeric for loop",
												  "Iterate a generic for loop",
												  "Initialization for a generic for loop",
												  "Set a range of array elements for a table",
												  "Set a variable number of table elements",
												  "Close a range of locals being used as upvalues",
												  "Create a closure of a function prototype"
											  };

		#endregion

		#region Attributes
		ObjLuaFunction parent;

		protected ObjLuaFunction Parent
		{
			get { return parent; }
		}

		uint val;
		public uint Value
		{
			get { return val; }
			set { val = value; }
		}

		protected static byte GetOpCode(uint val, ObjLuaFunction parent)
		{
			return (byte)((val & (parent.Parent.OpcodeMaks << parent.Parent.OpcodeShift)) >> parent.Parent.OpcodeShift);
		}

		protected void SetOpcode(byte oc)
		{
			val = ((uint)(val & (0xFFFFFFFF - (parent.Parent.OpcodeMaks << parent.Parent.OpcodeShift))) | (uint)((oc & parent.Parent.OpcodeMaks) << parent.Parent.OpcodeShift));
		}

		public byte Opcode
		{
			get
			{
				return GetOpCode(val, this.parent);
			}
			/*set 
			{
				SetOpcode(value);
			}*/
		}

		public ushort A
		{
			get
			{
				return (ushort)((val & (parent.Parent.AMaks << parent.Parent.AShift)) >> parent.Parent.AShift);
			}
			set
			{
				val = ((uint)(val & (0xFFFFFFFF - (parent.Parent.AMaks << parent.Parent.AShift))) | (uint)((value & parent.Parent.AMaks) << parent.Parent.AShift));
			}
		}

		public ushort B
		{
			get
			{
				return (ushort)((val & (parent.Parent.BMaks << parent.Parent.BShift)) >> parent.Parent.BShift);
			}
			set
			{
				val = ((uint)(val & (0xFFFFFFFF - (parent.Parent.BMaks << parent.Parent.BShift))) | (uint)((value & parent.Parent.BMaks) << parent.Parent.BShift));
			}
		}

		public ushort C
		{
			get
			{
				return (ushort)((val & (parent.Parent.CMaks << parent.Parent.CShift)) >> parent.Parent.CShift);
			}
			set
			{
				val = ((uint)(val & (0xFFFFFFFF - (parent.Parent.CMaks << parent.Parent.CShift))) | (uint)((value & parent.Parent.CMaks) << parent.Parent.CShift));
			}
		}

		public uint BX
		{
			get { return ((B & parent.Parent.BMaks) << parent.Parent.CBits) | (C & parent.Parent.CMaks); }
		}

		public int SBX
		{
			get { return (int)((long)BX - parent.Parent.Bias); }
		}

		#endregion

		#region Opcode Translation

		protected static string R(ushort v, string[] regs, bool use)
		{
			if (use) return R(v, regs);
			else return R(v);
		}

		protected static string R(ushort v)
		{
			return "R_" + v.ToString();
		}

		protected static string R(ushort v, string[] regs)
		{
			if (regs[v] == null) return "null";
			return regs[v];
		}

		protected string RK(ushort v)
		{
			if (v < RK_OFFSET) return R(v);
			else return (Kst((uint)(v - RK_OFFSET)));
		}
		protected string Kst(uint v)
		{
			if (v >= 0 && v < parent.Constants.Count)
			{
				ObjLuaConstant oci = (ObjLuaConstant)parent.Constants[(int)v];
				if (oci.InstructionType == ObjLuaConstant.Type.String) return oci.String;
				else if (oci.InstructionType == ObjLuaConstant.Type.Number) return oci.Value.ToString();
				else return "null";
			}
			return v.ToString();
		}

		protected string UpValue(ushort v)
		{
			if (v >= 0 && v < parent.UpValues.Count) return parent.UpValues[v].ToString();
			return v.ToString();
		}

		protected static string Bool(ushort v)
		{
			if (v == 0) return "false";
			else return "true";
		}

		protected static string Gbl(string n)
		{
			return "GLOBAL[" + n + "]";
		}

		protected static string Tbl(ushort v)
		{
			return "Tbl" + v.ToString();
		}

		protected static string TblFbp(ushort v)
		{
			return Context.TblFbp(v).ToString();
		}

		protected static string TblSz(ushort v)
		{
			return Context.TblSz(v).ToString();
		}

		protected static string ListR(int start, int end)
		{
			return ListR(start, end, "", " ... ");
		}

		protected static string ListR(int start, int end, string prefix, string infix)
		{
			if (end < start) return "";
			if (end == start) return R((ushort)start) + prefix;
			return R((ushort)start) + infix + R((ushort)end) + prefix;
		}


			#endregion

			string TranslateOpcode(byte oc, ushort a, ushort b, ushort c)
			{
				uint bx = BX;
				int sbx = SBX;

				string name = GetOpcodeName(oc);
				string ret = "";
				//if (name=="MOVE") ret =  R(a) + " = " + R(b);
				//else if (name=="LOADNIL") ret = ListR(a, b, " = ", " = ... = ")+"null";
				//else if (name=="LOADK") ret = R(a) +" = " + Kst(bx);
				//if (name=="LOADBOOL") ret = R(a) + " = " + Bool(b) +"; if ("+Bool(c)+") PC++";
				//else if (name=="GETGLOBAL") ret = R(a) + " = " + Gbl(Kst(bx));
				//else if (name=="SETGLOBAL") ret = Gbl(Kst(bx)) + " = " + R(a);
				//else if (name=="GETUPVAL") ret = R(a) + " = " + UpValue(b);
				if (name == "SETUPVAL") ret = UpValue(b) + " = " + R(a);
				//else if (name=="GETTABLE") ret = R(a) + " = " + R(b)+"["+RK(c)+"]";
				//else if (name=="SETTABLE") ret = R(a)+"["+RK(b)+"]" + " = " + RK(c);
				//else if (name=="ADD") ret = R(a) + " = " + RK(b) + " + " + RK(c);
				//else if (name=="SUB") ret = R(a) + " = " + RK(b) + " - " + RK(c);
				//else if (name=="MUL") ret = R(a) + " = " + RK(b) + " * " + RK(c);
				//else if (name=="DIV") ret = R(a) + " = " + RK(b) + " / " + RK(c);
				//else if (name=="POW") ret = R(a) + " = " + RK(b) + " ^ " + RK(c);
				//else if (name=="UNM") ret = R(a) + " = -" + R(b);
				//else if (name=="NOT") ret = R(a) + " = !" + R(b);
				//else if (name=="CONCAT") ret = R(a) + " = " + ListR(b, c);
				//else if (name=="JMP") ret = " PC += " + sbx.ToString();
				//else if (name=="CALL") ret = ListR(a, a+c-2, " = ", ", ..., ") + R(a) + "(" + ListR(a+1, a+b-1, "", ", ..., ") + ")";
				//else if (name=="RETURN") ret = "return " + ListR(a, a+b-2, "", ", ..., ");
				else if (name == "TAILCALL") ret = "return " + R(a) + "(" + ListR(a + 1, a + b - 1, "", ", ..., ") + ")";
				//else if (name=="SELF") ret = R((ushort)(a+1)) + " = " + R(b) + "; " + R(a) + " = " + R(b) + "["+RK(c)+"]";			
				//else if (name=="EQ") ret = "if ((" + RK(b) + " == " + RK(c) + ") == " + Bool(a) + " then PC++";
				//else if (name=="LT") ret = "if ((" + RK(b) + " < " + RK(c) + ") == " + Bool(a) + " then PC++";
				//else if (name=="LE") ret = "if ((" + RK(b) + " <= " + RK(c) + ") == " + Bool(a) + " then PC++";
				//else if (name=="TEST") ret = "if (" + R(b) + " <=> " + c.ToString() + ") then " + R(a) + " = " + R(b) + " else PC++";
				//else if (name=="FORLOOP") ret = R(a) + " += " + R((ushort)(a+2)) + "; if " + R(a) + " <= " +  R((ushort)(a+1)) + " then PC += " + sbx.ToString();
				//else if (name=="TFORREP") ret = "if type("+R(a)+") == table then " + R((ushort)(a+1)) + " = " + R(a) + "; " + R(a) + "= next; PC += " + sbx.ToString();
				//else if (name=="TFORLOOP") ret = R((ushort)(a+2)) + ", ..., " + R((ushort)(a+2+c)) + " = " + R(a) + "("+R(a)+", "+R((ushort)(a+2))+"); if " +  R((ushort)(a+2)) + " == null then PC++";
				//else if (name=="NEWTABLE") ret = R(a) + " = new table["+TblFbp(b)+", "+TblSz(c)+"]";
				//else if (name=="CLOSURE") ret = R(a) + " = closure(KPROTO["+bx.ToString()+"], "+R(a)+", ...)";
				else if (name == "CLOSE") ret = "close all to " + R(a);

				return ret + "; //" + name;
				//return ret+"; //"+name+" (a=0x"+Helper.HexString(a)+", b=0x"+Helper.HexString(b)+", c=0x"+Helper.HexString(c)+", bx="+bx.ToString()+", sbx="+sbx.ToString()+") "+GetOpcodeDescription(oc);
			}

			public override string ToString()
			{
				return /*"0x"+Helper.HexString(val)+": "+*/TranslateOpcode(this.Opcode, this.A, this.B, this.C);
			}
			public ObjLuaCode(ObjLuaFunction parent) : this(0, parent)
		{
		}

		public ObjLuaCode(uint val, ObjLuaFunction parent)
		{
			this.parent = parent;
			this.val = val;
		}


		internal static ObjLuaCode Unserialize(System.IO.BinaryReader reader, ObjLuaFunction parent)
		{
			uint val = reader.ReadUInt32();
			ObjLuaCode ret = ObjLuaCode.CreateOperator(val, parent);
			return ret;
		}


		internal void Serialize(System.IO.BinaryWriter writer)
		{
			writer.Write(val);
		}

		public void Dispose()
		{

		}

	}
}
}