using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph.Block
{
    public class AnimResourceConstBlock : ScenegraphDataBlock
    {
        public const uint TYPE_ID = 0xfb00791e;
        public const string BLOCK_NAME = "cAnimResourceConst";
        public override string BlockName => BLOCK_NAME;

        public AnimResourceConstBlock(PersistTypeInfo blockTypeInfo) : base(blockTypeInfo)
        {
        }

        public struct AnimTarget
        {
            public int NumSharedChannels;
            public string TagName;
        }
    }

    public class AnimResourceConstBlockReader : IScenegraphDataBlockReader<AnimResourceConstBlock>
    {
        public AnimResourceConstBlock Deserialize(IoBuffer reader, PersistTypeInfo blockTypeInfo)
        {
            var resource = ScenegraphResource.Deserialize(reader);

            // The game treats the data as a free byte array and uses this size to refer to it...
            // we just read it normally.
            var serializedSize = reader.ReadUInt32();

            var durationTicks = reader.ReadUInt16();
            var numAnimTargets = reader.ReadUInt16();
            var numEventKeys = reader.ReadUInt16();
            var dataStringLength = reader.ReadByte();
            var version = reader.ReadByte();

            var flags = reader.ReadByte();
            var priority = reader.ReadByte();
            var locomotionType = reader.ReadByte();
            var skeletonTagLength = reader.ReadByte();

            // 4 ignored uint32s.
            reader.ReadBytes(4 * 4);

            var headingOffset = reader.ReadFloat();
            var locomotionDistance = reader.ReadFloat();
            var locomotionCyclePortionCompleted = reader.ReadFloat();
            var locomotionStrideLength = reader.ReadFloat();
            var locomotionStrideVelocityMPT = reader.ReadFloat();
            var velocityMPT = reader.ReadFloat();
            var turnRotation = reader.ReadFloat();
            var preStretchFactor = reader.ReadFloat();
            var rootStretchDisplacement = reader.ReadFloat();

            // We need to note the stream position here because the rest of the data is aligned on 4-byte boundaries
            // sometimes.
            var initialStreamPosition = reader.Stream.Position;

            var skeletonTag = reader.ReadNullTerminatedString();
            Debug.Assert(skeletonTag.Length == skeletonTagLength);
            var dataString = reader.ReadNullTerminatedString();
            Debug.Assert(dataString.Length == dataStringLength);

            Debug.Log($"skeletonTag: {skeletonTag}, dataString: {dataString}");

            ReadPadding(reader, reader.Stream.Position - initialStreamPosition);

            var animTargets = new AnimResourceConstBlock.AnimTarget[numAnimTargets];
            for (var i = 0; i < numAnimTargets; i++)
            {
                // 2 ignored uint32s
                reader.ReadBytes(4 * 2);

                var animType = reader.ReadUInt16();
                animTargets[i].NumSharedChannels = reader.ReadUInt16();
                var numIKChains = reader.ReadUInt16();
                var tagLengthMaybe = reader.ReadUInt16();

                // 3 ignored uint32s.
                reader.ReadBytes(4 * 3);
            }

            for (var i = 0; i < numAnimTargets; i++)
            {
                animTargets[i].TagName = reader.ReadNullTerminatedString();
                Debug.Log($"  animTargetTagString: {animTargets[i].TagName}");
            }

            ReadPadding(reader, reader.Stream.Position - initialStreamPosition);

            for (var i = 0; i < numAnimTargets; i++)
            {
                var target = animTargets[i];
                for (var j = 0; j < target.NumSharedChannels; j++)
                {
                    // 2 ignored uint32s
                    reader.ReadBytes(4 * 2);

                    var boneHash = reader.ReadUInt32();
                    Debug.Log($"  boneHash: {boneHash:X}");
                    reader.ReadUInt32(); // 1 ignored uint32.
                    var channelFlags = reader.ReadUInt32();
                    reader.ReadUInt32(); // 1 ignored uint32.
                }
            }

            for (var i = 0; i < numAnimTargets; i++)
            {
                var target = animTargets[i];
                for (var j = 0; j < target.NumSharedChannels; j++)
                {
                    var channelName = reader.ReadNullTerminatedString();
                    Debug.Log($"  channelName: {channelName}");
                }
            }

            return new AnimResourceConstBlock(blockTypeInfo);
        }

        // For some reason this format pads to 4-byte boundaries sometimes...we deal with that here.
        private static void ReadPadding(IoBuffer reader, long length)
        {
            for (var i = 0; i < (length % 4); i++)
            {
                var paddingByte = reader.ReadByte();
                Debug.Log($"padding: {paddingByte:x}");
                Debug.Assert(i == paddingByte);
            }
        }
    }
}