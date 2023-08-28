using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Lua.Disassembly
{
    public class CodeBuilder
    {
        public int Position => _stringBuilder.Length;
        public int Indentation = 0;
        private StringBuilder _stringBuilder = new StringBuilder();

        void Indent()
        {
            for(var i=0;i<Indentation;i++)
            {
                _stringBuilder.Append('\t');
            }
        }

        public void Insert(int index, string value)
        {
            _stringBuilder.Insert(index, value);
        }

        public void NewLine()
        {
            _stringBuilder.AppendLine();
            Indent();
        }

        public void WriteLine(string line)
        {
            if (_stringBuilder.Length > 0)
            {
                if (_stringBuilder[_stringBuilder.Length - 1] != Environment.NewLine[0])
                    NewLine();
            }
            _stringBuilder.Append(line);
        }

        public void WriteLabel(LuaC50.JumpLabel jumpLabel)
        {
            WriteLabel(jumpLabel.Label);
        }

        public void WriteLabel(string label)
        {
            WriteLine($"::{label}::");
        }

        public void WriteEnd()
        {
            WriteLine("end");
        }

        public void WriteElse()
        {
            WriteLine("else");
        }

        public void WriteGoto(LuaC50.JumpLabel targetLabel)
        {
            WriteGoto(targetLabel.Label);
        }

        public void WriteGoto(string target)
        {
            WriteLine($"goto {target}");
        }

        public override string ToString()
        {
            return _stringBuilder.ToString();
        }
    }
}
