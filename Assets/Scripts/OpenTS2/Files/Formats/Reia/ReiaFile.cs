using OpenTS2.Files.Utils;
using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

namespace OpenTS2.Files.Formats.Reia
{
    public class ReiaFile
    {
        public int Width => _width;
        public int Height => _height;
        public int FramesPerSecond => _fps;
        public int NumberOfFrames => _numFrames;
        public List<ReiaFrame> Frames => _frames;

        private readonly int _width;
        private readonly int _height;
        private readonly int _fps;
        private readonly int _numFrames;
        private readonly List<ReiaFrame> _frames;

        public ReiaFile(int width, int height, int fps, int numFrames, List<ReiaFrame> frames)
        {
            _width = width;
            _height = height;
            _fps = fps;
            _numFrames = numFrames;
            _frames = frames;
        }

        public static ReiaFile Read(Stream stream)
        {
            var io = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);

            var riffMagic = io.ReadCString(4);
            if (riffMagic != "RIFF")
            {
                throw new Exception("Not a RIFF container");
            }

            var fileSize = io.ReadUInt32();

            var reiaHeadMagic = io.ReadCString(8);
            if (reiaHeadMagic != "Reiahead")
            {
                throw new Exception("Incorrect Reia header magic");
            }

            var metadataSize = io.ReadUInt32();

            if (metadataSize != 24)
            {
                throw new Exception("Reiahead metadata size doesn't equal 24");
            }

            Debug.Assert(io.ReadUInt32() == 1);

            var width = io.ReadUInt32();
            var height = io.ReadUInt32();

            var fpsNumerator = io.ReadUInt32();
            var fpsDenominator = io.ReadUInt32();
            var fps = (float)fpsNumerator / fpsDenominator;

            var numFrames = io.ReadUInt32();
            var frames = ReiaFrame.ReadFrames(io, (int)width, (int)height);
            return new ReiaFile((int)width, (int)height, (int)fps, (int)numFrames, frames);
        }
    }
}
