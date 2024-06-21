// Copyright (c) 2023 Nicholas Hayes
// SPDX-License-Identifier: MIT
//
// Portions of this file have been adapted from zlib version 1.2.3

// Some code has been replaced to work properly in Unity.

/*
zlib.h -- interface of the 'zlib' general purpose compression library
version 1.2.3, July 18th, 2005

Copyright (C) 1995-2005 Jean-loup Gailly and Mark Adler

This software is provided 'as-is', without any express or implied
warranty.  In no event will the authors be held liable for any damages
arising from the use of this software.

Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:

1. The origin of this software must not be misrepresented; you must not
   claim that you wrote the original software. If you use this software
   in a product, an acknowledgment in the product documentation would be
   appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be
   misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.

Jean-loup Gailly        Mark Adler
jloup@gzip.org          madler@alumni.caltech.edu
*/

using System;
using System.Numerics;

namespace DBPFSharp
{
    /// <summary>
    /// An implementation of the QFS/RefPack compression format used in SC4 DBPF files.
    /// </summary>
    /// <remarks>
    /// QFS/RefPack is a byte oriented compression format similar to LZ77.<br/>
    /// <br/>
    /// References:<br/>
    /// https://wiki.sc4devotion.com/index.php?title=DBPF_Compression<br/>
    /// http://wiki.niotso.org/RefPack
    /// </remarks>
    internal static class QfsCompression
    {
        /// <summary>
        /// The minimum size in bytes of an uncompressed buffer that can be compressed with QFS compression.
        /// </summary>
        /// <remarks>
        /// This is an optimization to skip compression for very small files.
        /// The QFS format used by SC4 has a 9 byte header.
        /// </remarks>
        private const int UncompressedDataMinSize = 10;

        /// <summary>
        /// The maximum size in bytes of an uncompressed buffer that can be compressed with QFS compression.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The standard version of the QFS format represents the uncompressed length using a 3 byte unsigned integer.
        /// </para>
        /// <para>
        /// There is a variant that represents the uncompressed length using a 4 byte unsigned integer, but I do not know
        /// if SC4 can read this format.
        /// </para>
        /// </remarks>
        private const int UncompressedDataMaxSize = 16777215;

        /// <summary>
        /// Compresses the input byte array with QFS compression
        /// </summary>
        /// <param name="input">The input byte array to compress</param>
        /// <param name="prefixLength">If set to <c>true</c> prefix the size of the compressed data, as is used by SC4; otherwise <c>false</c>.</param>
        /// <returns>A byte array containing the compressed data or null if the data cannot be compressed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is null.</exception>
        public static byte[] Compress(byte[] input, bool prefixLength)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (input.Length < UncompressedDataMinSize || input.Length > UncompressedDataMaxSize)
            {
                return null;
            }

            return new ZlibQFS(input, prefixLength).Compress();
        }

        /// <summary>
        /// Decompresses a QFS compressed byte array.
        /// </summary>
        /// <param name="compressedData">The byte array to decompress</param>
        /// <returns>A byte array containing the decompressed data.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="compressedData"/> is null.</exception>
        /// <exception cref="NotSupportedException"><paramref name="compressedData"/> uses an unsupported compression format.</exception>
        public static byte[] Decompress(byte[] compressedData)
        {
            if (compressedData == null)
            {
                throw new ArgumentNullException(nameof(compressedData));
            }

            int headerStartOffset = 0;

            if ((compressedData[0] & HeaderFlags.Mask) != 0x10 || compressedData[1] != 0xFB)
            {
                if ((compressedData[4] & HeaderFlags.Mask) != 0x10 || compressedData[5] != 0xFB)
                {
                    throw new NotSupportedException("Compression format is unsupported");
                }
                headerStartOffset = 4;
            }

            int index = headerStartOffset + 2;

            // The first byte contains flags that describes the information in the header.
            byte headerFlags = compressedData[headerStartOffset];

            bool largeSizeFields = (headerFlags & HeaderFlags.LargeSizeFields) != 0;
            bool compressedSizePresent = (headerFlags & HeaderFlags.CompressedSizePresent) != 0;

            if (compressedSizePresent)
            {
                // Some files may write the compressed size after the signature.
                index += largeSizeFields ? 4 : 3;
            }

            uint outLength;

            // The uncompressed size is a 3 or 4 byte unsigned big endian integer.
            if (largeSizeFields)
            {
                outLength = (uint)((compressedData[index] << 24) |
                                   (compressedData[index + 1] << 16) |
                                   (compressedData[index + 2] << 8) |
                                   compressedData[index + 3]);
                index += 4;
            }
            else
            {
                outLength = (uint)((compressedData[index] << 16) |
                                   (compressedData[index + 1] << 8) |
                                   compressedData[index + 2]);
                index += 3;
            }

            byte[] uncompressedData = new byte[checked((int)outLength)];

            byte controlByte1;
            byte controlByte2;
            byte controlByte3;
            byte controlByte4;

            int outIndex = 0;
            int plainCount;
            int copyCount;
            int copyOffset;

            int length = compressedData.Length;

            while (index < length && compressedData[index] < 0xFC)
            {
                controlByte1 = compressedData[index];
                index++;

                if (controlByte1 >= 0xE0) // 1 byte literal op code 0xE0 - 0xFB
                {
                    plainCount = ((controlByte1 & 0x1F) << 2) + 4;
                    copyCount = 0;
                    copyOffset = 0;
                }
                else if (controlByte1 >= 0xC0) // 4 byte op code 0xC0 - 0xDF
                {
                    controlByte2 = compressedData[index];
                    index++;
                    controlByte3 = compressedData[index];
                    index++;
                    controlByte4 = compressedData[index];
                    index++;

                    plainCount = (controlByte1 & 3);
                    copyCount = ((controlByte1 & 0x0C) << 6) + controlByte4 + 5;
                    copyOffset = (((controlByte1 & 0x10) << 12) + (controlByte2 << 8)) + controlByte3 + 1;
                }
                else if (controlByte1 >= 0x80) // 3 byte op code 0x80 - 0xBF
                {
                    controlByte2 = compressedData[index];
                    index++;
                    controlByte3 = compressedData[index];
                    index++;

                    plainCount = (controlByte2 & 0xC0) >> 6;
                    copyCount = (controlByte1 & 0x3F) + 4;
                    copyOffset = ((controlByte2 & 0x3F) << 8) + controlByte3 + 1;
                }
                else // 2 byte op code 0x00 - 0x7F
                {
                    controlByte2 = compressedData[index];
                    index++;

                    plainCount = (controlByte1 & 3);
                    copyCount = ((controlByte1 & 0x1C) >> 2) + 3;
                    copyOffset = ((controlByte1 & 0x60) << 3) + controlByte2 + 1;
                }

                for (int i = 0; i < plainCount; i++)
                {
                    uncompressedData[outIndex] = compressedData[index];
                    index++;
                    outIndex++;
                }

                if (copyCount > 0)
                {
                    int srcIndex = outIndex - copyOffset;

                    for (int i = 0; i < copyCount; i++)
                    {
                        uncompressedData[outIndex] = uncompressedData[srcIndex];
                        srcIndex++;
                        outIndex++;
                    }
                }
            }

            // Write the trailing bytes.
            if (index < length && outIndex < outLength)
            {
                // 1 byte EOF op code 0xFC - 0xFF.
                plainCount = (compressedData[index] & 3);
                index++;

                for (int i = 0; i < plainCount; i++)
                {
                    uncompressedData[outIndex] = compressedData[index];
                    index++;
                    outIndex++;
                }
            }

            return uncompressedData;
        }

        /// <summary>
        /// The flags may be present in the first byte of the compression signature.
        /// </summary>
        /// <remarks>
        /// See http://wiki.niotso.org/RefPack#Header
        /// These values may not be used by SC4.
        /// </remarks>
        private static class HeaderFlags
        {
            /// <summary>
            /// The uncompressed size field and compressed size field (if present) are 4-byte values.
            /// </summary>
            /// <remarks>
            /// If this flag is unset then the fields are 3-byte values.
            /// This may not be used by SC4.
            /// </remarks>
            internal const int LargeSizeFields = 128;
            internal const int Unknown1 = 64;
            // Other values are unused, with 16 reserved for the compression signature.

            /// <summary>
            /// The compressed size follows the header.
            /// </summary>
            /// <remarks>
            /// This may be unused by SC4 as many of the game files
            /// place the compressed size before the QFS header.
            /// </remarks>
            internal const int CompressedSizePresent = 1;

            internal const int Mask = ~(LargeSizeFields | Unknown1 | CompressedSizePresent);
        }

        /// <summary>
        /// A QFS implementation derived from the zlib compression library.
        /// </summary>
        private sealed class ZlibQFS
        {
            private const int QfsHeaderSize = 5;
            /// <summary>
            /// The maximum length of a literal run.
            /// </summary>
            private const int LiteralRunMaxLength = 112;

            private const int MaxWindowSize = 131072;
            private const int MaxHashSize = 65536;
            private const int GoodLength = 32;
            private const int MaxLazy = 258;
            private const int NiceLength = 258;
            private const int MaxChain = 4096;
            private const int MinMatch = 3;
            private const int MaxMatch = 1028;

            private readonly byte[] input;
            private byte[] output;
            private readonly int inputLength;
            private readonly int outputLength;
            private int outIndex;
            private int readPosition;
            private int lastWritePosition;
            private int remaining;
            private readonly bool prefixLength;

            private int hash;
            private readonly int[] head;
            private readonly int[] prev;

            private readonly int windowSize;
            private readonly int windowMask;
            private readonly int maxWindowOffset;
            private readonly int hashSize;
            private readonly int hashMask;
            private readonly int hashShift;

            private int matchStart;
            private int matchLength;
            private int prevLength;

            public ZlibQFS(byte[] input, bool prefixLength)
            {
                if (input == null)
                    throw new ArgumentNullException("input");

                this.input = input;
                this.inputLength = input.Length;
                this.output = new byte[this.inputLength - 1];
                this.outputLength = output.Length;

                if (this.inputLength < MaxWindowSize)
                {
                    windowSize = 1 << Log2((uint)this.inputLength);
                    hashSize = Math.Max(windowSize / 2, 32);
                    hashShift = (TrailingZeroCount(hashSize) + MinMatch - 1) / MinMatch;
                }
                else
                {
                    windowSize = MaxWindowSize;
                    hashSize = MaxHashSize;
                    hashShift = 6;
                }
                maxWindowOffset = windowSize - 1;
                windowMask = maxWindowOffset;
                hashMask = hashSize - 1;

                this.hash = 0;
                this.head = new int[hashSize];
                this.prev = new int[windowSize];
                this.readPosition = 0;
                this.remaining = inputLength;
                this.outIndex = QfsHeaderSize;
                this.lastWritePosition = 0;
                this.prefixLength = prefixLength;

                for (var i = 0; i < head.Length; i++)
                    head[i] = -1;
            }

            int TrailingZeroCount(int n)
            {
                var bits = 0;
                for(var i=0;i<32;i++)
                {
                    var currentBit = 1;
                    if (i > 0)
                        currentBit = i * 2;
                    if ((n & currentBit) != currentBit)
                        return bits;
                    bits++;
                }
                return 8;
            }

            int Log2(uint n)
            {
                int bits = 0;
                if (n > 32767)
                {
                    n >>= 16;
                    bits += 16;
                }
                if (n > 127)
                {
                    n >>= 8;
                    bits += 8;
                }
                if (n > 7)
                {
                    n >>= 4;
                    bits += 4;
                }
                if (n > 1)
                {
                    n >>= 2;
                    bits += 2;
                }
                if (n > 0)
                {
                    bits++;
                }
                return bits;
            }

            /// <summary>
            /// Compresses this instance.
            /// </summary>
            /// <returns>
            /// <see langword="true"/> if the data was compressed; otherwise, <see langword="false"/>.
            /// </returns>
            /// <remarks>
            /// This method has been adapted from deflate.c in zlib version 1.2.3.
            /// </remarks>
            public byte[] Compress()
            {
                this.hash = input[0];
                this.hash = ((this.hash << hashShift) ^ input[1]) & hashMask;

                int lastMatch = this.inputLength - MinMatch;

                while (remaining > 0)
                {
                    this.prevLength = this.matchLength;
                    int prev_match = this.matchStart;
                    this.matchLength = MinMatch - 1;

                    int hash_head = -1;

                    // Insert the string window[readPosition .. readPosition+2] in the
                    // dictionary, and set hash_head to the head of the hash chain:
                    if (this.remaining >= MinMatch)
                    {
                        this.hash = ((this.hash << hashShift) ^ input[this.readPosition + MinMatch - 1]) & hashMask;

                        hash_head = head[this.hash];
                        prev[this.readPosition & windowMask] = hash_head;
                        head[this.hash] = this.readPosition;
                    }

                    if (hash_head >= 0 && this.prevLength < MaxLazy && this.readPosition - hash_head <= windowSize)
                    {
                        int bestLength = LongestMatch(hash_head);

                        if (bestLength >= MinMatch)
                        {
                            int bestOffset = this.readPosition - this.matchStart;

                            if (bestOffset <= 1024 ||
                                bestOffset <= 16384 && bestLength >= 4 ||
                                bestOffset <= windowSize && bestLength >= 5)
                            {
                                this.matchLength = bestLength;
                            }
                        }
                    }

                    // If there was a match at the previous step and the current
                    // match is not better, output the previous match:
                    if (this.prevLength >= MinMatch && this.matchLength <= this.prevLength)
                    {
                        if (!WriteCompressedData(prev_match))
                        {
                            return null;
                        }

                        // Insert in hash table all strings up to the end of the match.
                        // readPosition-1 and readPosition are already inserted. If there is not
                        // enough lookahead, the last two strings are not inserted in
                        // the hash table.

                        this.remaining -= (this.prevLength - 1);
                        this.prevLength -= 2;

                        do
                        {
                            this.readPosition++;

                            if (this.readPosition < lastMatch)
                            {
                                this.hash = ((this.hash << hashShift) ^ input[this.readPosition + MinMatch - 1]) & hashMask;

                                hash_head = head[this.hash];
                                prev[this.readPosition & windowMask] = hash_head;
                                head[this.hash] = this.readPosition;
                            }
                            this.prevLength--;
                        }
                        while (prevLength > 0);

                        this.matchLength = MinMatch - 1;
                        this.readPosition++;
                    }
                    else
                    {
                        this.readPosition++;
                        this.remaining--;
                    }
                }

                if (!WriteTrailingBytes())
                {
                    return null;
                }

                // Write the compressed data header.
                output[0] = 0x10;
                output[1] = 0xFB;
                output[2] = (byte)((inputLength >> 16) & 0xff);
                output[3] = (byte)((inputLength >> 8) & 0xff);
                output[4] = (byte)(inputLength & 0xff);

                // Trim the output array to its actual size.
                if (prefixLength)
                {
                    int finalLength = outIndex + 4;
                    if (finalLength >= inputLength)
                    {
                        return null;
                    }

                    byte[] temp = new byte[finalLength];

                    // Write the compressed data length in little endian byte order.
                    temp[0] = (byte)(outIndex & 0xff);
                    temp[1] = (byte)((outIndex >> 8) & 0xff);
                    temp[2] = (byte)((outIndex >> 16) & 0xff);
                    temp[3] = (byte)((outIndex >> 24) & 0xff);

                    Buffer.BlockCopy(this.output, 0, temp, 4, outIndex);
                    this.output = temp;
                }
                else
                {
                    byte[] temp = new byte[outIndex];
                    Buffer.BlockCopy(this.output, 0, temp, 0, outIndex);

                    this.output = temp;
                }

                return output;
            }

            /// <summary>
            /// Writes the compressed data.
            /// </summary>
            /// <param name="startOffset">The start offset.</param>
            /// <returns>
            /// <see langword="true"/> if the data was compressed; otherwise, <see langword="false"/>.
            /// </returns>
            private bool WriteCompressedData(int startOffset)
            {
                int endOffset = this.readPosition - 1;
                int run = endOffset - this.lastWritePosition;

                while (run > 3) // 1 byte literal op code 0xE0 - 0xFB
                {
                    int blockLength = Math.Min(run & ~3, LiteralRunMaxLength);

                    if ((this.outIndex + blockLength + 1) >= this.outputLength)
                    {
                        return false; // data did not compress
                    }

                    this.output[this.outIndex] = (byte)(0xE0 + ((blockLength / 4) - 1));
                    this.outIndex++;

                    // A for loop is faster than Buffer.BlockCopy for data less than or equal to 32 bytes.
                    if (blockLength <= 32)
                    {
                        for (int i = 0; i < blockLength; i++)
                        {
                            this.output[this.outIndex] = this.input[this.lastWritePosition];
                            this.lastWritePosition++;
                            this.outIndex++;
                        }
                    }
                    else
                    {
                        Buffer.BlockCopy(this.input, this.lastWritePosition, this.output, this.outIndex, blockLength);
                        this.lastWritePosition += blockLength;
                        this.outIndex += blockLength;
                    }

                    run -= blockLength;
                }

                int copyLength = this.prevLength;
                // Subtract one before encoding the copy offset, the QFS decompression algorithm adds it back when decoding.
                int copyOffset = endOffset - startOffset - 1;

                if (copyLength <= 10 && copyOffset < 1024) // 2 byte op code  0x00 - 0x7f
                {
                    if ((this.outIndex + run + 2) >= this.outputLength)
                    {
                        return false; // data did not compress
                    }

                    this.output[this.outIndex] = (byte)((((copyOffset >> 8) << 5) + ((copyLength - 3) << 2)) + run);
                    this.output[this.outIndex + 1] = (byte)(copyOffset & 0xff);
                    this.outIndex += 2;
                }
                else if (copyLength <= 67 && copyOffset < 16384)  // 3 byte op code 0x80 - 0xBF
                {
                    if ((this.outIndex + run + 3) >= this.outputLength)
                    {
                        return false; // data did not compress
                    }

                    this.output[this.outIndex] = (byte)(0x80 + (copyLength - 4));
                    this.output[this.outIndex + 1] = (byte)((run << 6) + (copyOffset >> 8));
                    this.output[this.outIndex + 2] = (byte)(copyOffset & 0xff);
                    this.outIndex += 3;
                }
                else // 4 byte op code 0xC0 - 0xDF
                {
                    if ((this.outIndex + run + 4) >= this.outputLength)
                    {
                        return false; // data did not compress
                    }

                    this.output[this.outIndex] = (byte)(((0xC0 + ((copyOffset >> 16) << 4)) + (((copyLength - 5) >> 8) << 2)) + run);
                    this.output[this.outIndex + 1] = (byte)((copyOffset >> 8) & 0xff);
                    this.output[this.outIndex + 2] = (byte)(copyOffset & 0xff);
                    this.output[this.outIndex + 3] = (byte)((copyLength - 5) & 0xff);
                    this.outIndex += 4;
                }

                for (int i = 0; i < run; i++)
                {
                    this.output[this.outIndex] = this.input[this.lastWritePosition];
                    this.lastWritePosition++;
                    this.outIndex++;
                }
                this.lastWritePosition += copyLength;

                return true;
            }

            /// <summary>
            /// Writes the trailing bytes after the last compressed block.
            /// </summary>
            /// <returns>
            /// <see langword="true"/> if the data was compressed; otherwise, <see langword="false"/>.
            /// </returns>
            private bool WriteTrailingBytes()
            {
                int run = this.readPosition - this.lastWritePosition;

                while (run > 3) // 1 byte literal op code 0xE0 - 0xFB
                {
                    int blockLength = Math.Min(run & ~3, LiteralRunMaxLength);

                    if ((this.outIndex + blockLength + 1) >= this.outputLength)
                    {
                        return false; // data did not compress
                    }

                    this.output[this.outIndex] = (byte)(0xE0 + ((blockLength / 4) - 1));
                    this.outIndex++;

                    // A for loop is faster than Buffer.BlockCopy for data less than or equal to 32 bytes.
                    if (blockLength <= 32)
                    {
                        for (int i = 0; i < blockLength; i++)
                        {
                            this.output[this.outIndex] = this.input[this.lastWritePosition];
                            this.lastWritePosition++;
                            this.outIndex++;
                        }
                    }
                    else
                    {
                        Buffer.BlockCopy(this.input, this.lastWritePosition, this.output, this.outIndex, blockLength);
                        this.lastWritePosition += blockLength;
                        this.outIndex += blockLength;
                    }
                    run -= blockLength;
                }

                if ((this.outIndex + run + 1) >= this.outputLength)
                {
                    return false;
                }

                this.output[this.outIndex] = (byte)(0xFC + run);
                this.outIndex++;

                for (int i = 0; i < run; i++)
                {
                    this.output[this.outIndex] = this.input[this.lastWritePosition];
                    this.lastWritePosition++;
                    this.outIndex++;
                }

                return true;
            }

            /// <summary>
            /// Finds the longest the run of data to compress.
            /// </summary>
            /// <param name="currentMatch">The current match length.</param>
            /// <returns>The longest the run of data to compress.</returns>
            /// <remarks>
            /// This method has been adapted from deflate.c in zlib version 1.2.3.
            /// </remarks>
            private int LongestMatch(int currentMatch)
            {
                int chainLength = MaxChain;
                int scan = this.readPosition;
                int bestLength = this.prevLength;

                if (bestLength >= this.remaining)
                {
                    return remaining;
                }

                byte scanEnd1 = input[scan + bestLength - 1];
                byte scanEnd = input[scan + bestLength];

                // Do not waste too much time if we already have a good match:
                if (this.prevLength >= GoodLength)
                {
                    chainLength >>= 2;
                }
                int niceLength = NiceLength;

                // Do not look for matches beyond the end of the input. This is necessary
                // to make deflate deterministic.
                if (niceLength > this.remaining)
                {
                    niceLength = this.remaining;
                }

                int maxLength = Math.Min(this.remaining, MaxMatch);
                int limit = this.readPosition > maxWindowOffset ? this.readPosition - maxWindowOffset : 0;

                do
                {
                    int match = currentMatch;

                    // Skip to next match if the match length cannot increase
                    // or if the match length is less than 2:
                    if (input[match + bestLength] != scanEnd ||
                        input[match + bestLength - 1] != scanEnd1 ||
                        input[match] != input[scan] ||
                        input[match + 1] != input[scan + 1])
                    {
                        continue;
                    }

                    int len = 2;
                    do
                    {
                        len++;
                    }
                    while (len < maxLength && input[scan + len] == input[match + len]);

                    if (len > bestLength)
                    {
                        this.matchStart = currentMatch;
                        bestLength = len;
                        if (len >= niceLength)
                        {
                            break;
                        }
                        scanEnd1 = input[scan + bestLength - 1];
                        scanEnd = input[scan + bestLength];
                    }
                }
                while ((currentMatch = prev[currentMatch & windowMask]) >= limit && --chainLength > 0);

                return bestLength;
            }
        }
    }
}