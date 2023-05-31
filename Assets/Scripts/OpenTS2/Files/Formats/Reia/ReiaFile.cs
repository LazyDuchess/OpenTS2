using OpenTS2.Files.Utils;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTS2.Files.Formats.Reia
{
    public class ReiaFile : IDisposable
    {
        public int Width => _width;
        public int Height => _height;
        public int FramesPerSecond => _fps;
        public int NumberOfFrames => _numFrames;

        private readonly int _width;
        private readonly int _height;
        private readonly int _fps;
        private readonly int _numFrames;
        private readonly ReiaFrameStream _frameStream;

        public ReiaFile(int width, int height, int fps, int numFrames, ReiaFrameStream frameStream)
        {
            _width = width;
            _height = height;
            _fps = fps;
            _numFrames = numFrames;
            _frameStream = frameStream;
            MoveNextFrame();
        }

        public void MoveNextFrame()
        {
            if (!_frameStream.MoveNext())
            {
                _frameStream.Reset();
                _frameStream.MoveNext();
            }
        }

        public ReiaFrame GetCurrentFrame()
        {
            return _frameStream.Current;
        }

        public static ReiaFile Read(Stream stream, bool streamed = true)
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

            var unknown1 = io.ReadUInt32();

            Debug.Assert(unknown1 == 1);

            var width = io.ReadUInt32();
            var height = io.ReadUInt32();

            var fpsNumerator = io.ReadUInt32();
            var fpsDenominator = io.ReadUInt32();
            var fps = (float)fpsNumerator / fpsDenominator;
            var numFrames = io.ReadUInt32();
            var ioFrameStreamPosition = io.Position;
            ReiaFrameStream frameStream;
            if (streamed)
            {
                var frameEnumerable = ReiaFrame.ReadFrameEnumerable(io, (int)width, (int)height);
                frameStream = new ReiaFrameStream(frameEnumerable.GetEnumerator(), io, (int)ioFrameStreamPosition, (int)width, (int)height, stream);
            }
            else
            {
                var frameEnumerable = ReiaFrame.ReadFrames(io, (int)width, (int)height);
                frameStream = new ReiaFrameMemoryStream(frameEnumerable.GetEnumerator(), frameEnumerable, io, (int)ioFrameStreamPosition, (int)width, (int)height, stream);
            }
            return new ReiaFile((int)width, (int)height, (int)fps, (int)numFrames, frameStream);
        }

        public void Dispose()
        {
            _frameStream.Dispose();
        }
    }
}
