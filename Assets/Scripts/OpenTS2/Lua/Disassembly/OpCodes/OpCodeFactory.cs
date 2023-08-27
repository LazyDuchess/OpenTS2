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
			typeof(LuaC50.OpCode), //LOADK
			typeof(LOADBOOL), //LOADBOOL
			typeof(LuaC50.OpCode), //LOADNIL
			typeof(LuaC50.OpCode), //GETUPVAL
			typeof(GETGLOBAL), //GETGLOBAL
			typeof(LuaC50.OpCode), //GETTABLE
			typeof(SETGLOBAL), //SETGLOBAL
			typeof(LuaC50.OpCode), //SETUPVAL
			typeof(SETTABLE), //SETTABLE
			typeof(NEWTABLE), //NEWTABLE
			typeof(LuaC50.OpCode), //SELF
			typeof(LuaC50.OpCode), //ADD
			typeof(LuaC50.OpCode), //SUB
			typeof(LuaC50.OpCode), //MUL
			typeof(LuaC50.OpCode), //DIV
			typeof(LuaC50.OpCode), //POW
			typeof(LuaC50.OpCode), //UNM
			typeof(LuaC50.OpCode), //NOT
			typeof(LuaC50.OpCode), //CONCAT
			typeof(JMP), //JMP
			typeof(EQ), //EQ
			typeof(LT), //LT
			typeof(LuaC50.OpCode), //LE
			typeof(TEST), //TEST
			typeof(LuaC50.OpCode), //CALL
			typeof(LuaC50.OpCode), //TAILCALL
			typeof(RETURN), //RETURN
			typeof(LuaC50.OpCode), //FORLOOP
			typeof(LuaC50.OpCode), //TFORLOOP
			typeof(LuaC50.OpCode), //TFORREP
			typeof(LuaC50.OpCode), //SETLIST
			typeof(LuaC50.OpCode), //SETLISTO
			typeof(LuaC50.OpCode), //CLOSE
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