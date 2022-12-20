using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace OpenTS2.Files.Formats.JPEGWithAlfaSegment
{
    class JpegFileWithAlfaSegment
    {

        /// <summary>
        /// Returns transparency values from the ALFA segment in the JPEG if present,
        /// otherwise returns null.
        /// </summary>
        /// <param name="source">The raw bytes of the JPEG.</param>
        /// <returns></returns>
        public static byte[] GetTransparencyFromAlfaSegment(byte[] source)
        {
            using (BinaryReader binReader = new BinaryReader(new MemoryStream(source)))
            {
                // Consume the two magic Start-of-Image bytes
                byte[] startOfImageMagic = binReader.ReadBytes(2);
                Debug.Assert(startOfImageMagic[0] == 0xff);
                Debug.Assert(startOfImageMagic[1] == 0xd8);

                // Based off http://fileformats.archiveteam.org/wiki/JPEG#Format
                // and https://en.wikipedia.org/wiki/JPEG_File_Interchange_Format#File_format_structure
                while (binReader.BaseStream.Position != binReader.BaseStream.Length)
                {
                    // Read the segment marker.
                    byte sectionMarker1 = binReader.ReadByte();
                    Debug.Assert(sectionMarker1 == 0xff);
                    byte sectionMarker2 = binReader.ReadByte();
                    // ALFA segments are encoded as part of JFIF APP0 markers and they need to
                    // be at the start of the file, so once we get past APP0 segments just break
                    // out of this loop.
                    if (sectionMarker2 != 0xe0)
                    {
                        break;
                    }

                    // These segments have a size, we check if they are the ALFA segment
                    // or skip them. Swap the order as BinaryReader is little-endian and
                    // JFIF files use big-endian.
                    ushort segmentSize = Endian.SwapUInt16(binReader.ReadUInt16());
                    string segmentName = Encoding.UTF8.GetString(binReader.ReadBytes(4));
                    if (segmentName == "ALFA")
                    {
                        return RunLengthDecodeAlfaSegment(binReader, segmentSize - 6);
                    }
                    // Not an ALFA segment, seek forward to the next.
                    binReader.BaseStream.Seek(segmentSize - 4 - 2, SeekOrigin.Current);
                }
            }

            return null;
        }

        private static byte[] RunLengthDecodeAlfaSegment(BinaryReader alfaContents, int size)
        {
            var alphaChannel = new List<byte>();

            var currentPosition = alfaContents.BaseStream.Position;
            while (alfaContents.BaseStream.Position != currentPosition + size)
            {
                sbyte rleByte = alfaContents.ReadSByte();
                if (rleByte < 0)
                {
                    // The next transparency byte repeats ((-n) + 1) times
                    var numRepeats = (-rleByte) + 1;
                    byte transparency = alfaContents.ReadByte();
                    for (var i = 0; i < numRepeats; i++)
                    {
                        alphaChannel.Add(transparency);
                    }
                }
                else
                {
                    // There are n unique transparency bytes coming.
                    var numRepeats = rleByte + 1;
                    for (var i = 0; i < numRepeats; i++)
                    {
                        alphaChannel.Add(alfaContents.ReadByte());
                    }
                }
            }

            return alphaChannel.ToArray();
        }
    }
}
