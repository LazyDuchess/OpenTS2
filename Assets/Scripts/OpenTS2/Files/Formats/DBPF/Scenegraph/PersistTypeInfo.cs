using System.IO;
using OpenTS2.Files.Utils;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph
{
    /// <summary>
    /// A common prefix used in many areas of Scenegraph serialization/deserialization. 
    /// </summary>
    public readonly struct PersistTypeInfo
    {
        public string Name { get; }
        public uint TypeId { get; }
        public uint Version { get; }

        public PersistTypeInfo(string name, uint typeId, uint version) =>
            (Name, TypeId, Version) = (name, typeId, version);

        public static PersistTypeInfo Deserialize(IoBuffer reader)
        {
            var name = reader.ReadVariableLengthPascalString();
            var typeId = reader.ReadUInt32();
            var version = reader.ReadUInt32();
            return new PersistTypeInfo(name, typeId, version);
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(TypeId);
            writer.Write(Version);
        }

        public override string ToString()
        {
            return $"{nameof(TypeId)}: {TypeId:X}, {nameof(Version)}: {Version}, {nameof(Name)}: {Name}";
        }
    }
}