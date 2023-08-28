using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Lua
{
    /// <summary>
    /// Represents a disassembled Lua 5.0 script, as can be found in ObjectScripts.
    /// </summary>
    public class LuaAsset : AbstractAsset
    {
        public string FileName;
        public string Source;

        public LuaAsset(string filename, string source)
        {
            FileName = filename;
            Source = source;
        }
    }
}
