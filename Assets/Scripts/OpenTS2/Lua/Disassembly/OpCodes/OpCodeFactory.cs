using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Lua.Disassembly.OpCodes
{
    public static class OpCodeFactory
    {
		private static readonly Type[] OpCodeMap = new Type[]
		{
			typeof(MOVE), //MOVE
			typeof(LOADK), //LOADK
			typeof(LOADBOOL), //LOADBOOL
			typeof(LOADNIL), //LOADNIL
			typeof(GETUPVAL), //GETUPVAL
			typeof(GETGLOBAL), //GETGLOBAL
			typeof(GETTABLE), //GETTABLE
			typeof(SETGLOBAL), //SETGLOBAL
			typeof(SETUPVAL), //SETUPVAL
			typeof(SETTABLE), //SETTABLE
			typeof(NEWTABLE), //NEWTABLE
			typeof(SELF), //SELF
			typeof(ADD), //ADD
			typeof(SUB), //SUB
			typeof(MUL), //MUL
			typeof(DIV), //DIV
			typeof(POW), //POW
			typeof(UNM), //UNM
			typeof(NOT), //NOT
			typeof(CONCAT), //CONCAT
			typeof(JMP), //JMP
			typeof(EQ), //EQ
			typeof(LT), //LT
			typeof(LE), //LE
			typeof(TEST), //TEST
			typeof(CALL), //CALL
			typeof(TAILCALL), //TAILCALL
			typeof(RETURN), //RETURN
			typeof(FORLOOP), //FORLOOP
			typeof(TFORLOOP), //TFORLOOP
			typeof(TFORPREP), //TFORPREP
			typeof(SETLIST), //SETLIST
			// TODO: I think this is the same??? Idk double check.
			typeof(SETLIST), //SETLISTO
			typeof(CLOSE), //CLOSE - TODO?
			typeof(CLOSURE) //CLOSURE
		};

		public static LuaC50.OpCode GetOpCode(uint value, LuaC50.Function function)
        {
			var opCodeID = GetOpCodeID(value, function);
			var opCode = Activator.CreateInstance(OpCodeMap[opCodeID]) as LuaC50.OpCode;
			opCode.Function = function;
			opCode.Operands = value;
			return opCode;
        }

		static byte GetOpCodeID(uint val, LuaC50.Function function)
		{
			return (byte)((val & (function.Owner.OpcodeMaks << function.Owner.OpcodeShift)) >> function.Owner.OpcodeShift);
		}
	}
}

/*
 * static string[] opcodes = new string[]{
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
*/