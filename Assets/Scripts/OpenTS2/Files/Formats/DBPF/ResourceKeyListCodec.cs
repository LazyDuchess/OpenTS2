using System.IO;
using OpenTS2.Common;
using OpenTS2.Common.Utils;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Utils;

namespace OpenTS2.Files.Formats.DBPF
{
    /// <summary>
    /// cTSPersistResKeyList reading/writing codec.
    /// </summary>
    [Codec(TypeIDs.RES_KEY_LIST)]
    public class ResourceKeyListCodec : AbstractCodec
    {
        private const uint VersionSentinel = 0xDEADBEEF;
        private const int WriteVersion = 2;

        public override byte[] Serialize(AbstractAsset asset)
        {
            var keyListAsset = asset as ResourceKeyListAsset;
            var stream = new MemoryStream(0);
            var writer = new BinaryWriter(stream);

            writer.Write(VersionSentinel);
            writer.Write(WriteVersion);
            writer.Write((uint)keyListAsset.Keys.Count);
            foreach (var key in keyListAsset.Keys)
            {
                writer.Write(key.TypeID);
                writer.Write(key.GroupID);
                writer.Write(key.InstanceID);
                writer.Write(key.InstanceHigh);
            }

            var buffer = StreamUtils.GetBuffer(stream);
            writer.Dispose();
            stream.Dispose();
            return buffer;
        }

        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);

            var first = reader.ReadUInt32();
            int version;
            uint count;
            if (first == VersionSentinel)
            {
                version = reader.ReadInt32();
                count = reader.ReadUInt32();
            }
            else
            {
                version = 1;
                count = first;
            }

            var keyListAsset = new ResourceKeyListAsset();
            for (var i = 0; i < count; i++)
            {
                var typeId = reader.ReadUInt32();
                var groupId = reader.ReadUInt32();
                var instanceId = reader.ReadUInt32();
                uint instanceHigh = 0;
                if (version > 1)
                    instanceHigh = reader.ReadUInt32();
                keyListAsset.Keys.Add(new ResourceKey(instanceId, instanceHigh, groupId, typeId));
            }

            return keyListAsset;
        }
    }
}
