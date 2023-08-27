using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Files.Utils;
using OpenTS2.Lua.Decompiler;
using OpenTS2.Lua.Disassembly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Lua
{
    [Codec(TypeIDs.LUA_GLOBAL, TypeIDs.LUA_LOCAL)]
    public class LuaCodec : AbstractCodec
    {
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var stream = new MemoryStream(bytes);
            var reader = new IoBuffer(stream);
            reader.ByteOrder = ByteOrder.LITTLE_ENDIAN;

            var unknown = reader.ReadInt32();
            var filenameLength = reader.ReadInt32();
            var filename = Encoding.UTF8.GetString(reader.ReadBytes(filenameLength));

            var lua50 = new LuaC50(reader);
            var source = lua50.Disassemble();

            var asset = new LuaAsset(filename, source);

            stream.Dispose();
            reader.Dispose();

            return asset;
        }
    }
}
