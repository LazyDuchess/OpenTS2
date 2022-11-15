/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Files.Utils
{
    public interface IBCFReadProxy : IDisposable
    {
        byte ReadByte();
        ushort ReadUInt16();
        short ReadInt16();
        int ReadInt32();
        uint ReadUInt32();
        float ReadFloat();
        string ReadPascalString();
        string ReadLongPascalString();
    }

    public class BCFReadString : IBCFReadProxy
    {
        private StreamReader Reader;
        public int Version;
        private string[] NumBuf = new string[0];
        private int NumInd = 1;

        public BCFReadString(Stream input, bool version)
        {
            Reader = new StreamReader(input);

            if (!version) return;
            //skip to version
            var line = "";
            while (!line.StartsWith("version "))
                line = Reader.ReadLine();
            Version = int.Parse(line.Substring(8));
        }

        private string ReadNum()
        {
            //contrary to popular belief, this function that returns a string does indeed read a number
            if (NumInd >= NumBuf.Length)
            {
                NumBuf = Reader.ReadLine().Trim().Split(' ').ToArray();
                NumInd = 0;
            }
            return NumBuf[NumInd++];
        }

        public byte ReadByte() { return byte.Parse(ReadNum()); }
        public ushort ReadUInt16() { return ushort.Parse(ReadNum()); }
        public short ReadInt16() { return short.Parse(ReadNum()); }
        public int ReadInt32() { return int.Parse(ReadNum()); }
        public uint ReadUInt32() { return uint.Parse(ReadNum()); }
        public float ReadFloat() { return float.Parse(ReadNum()); }
        public string ReadPascalString() { return Reader.ReadLine(); }
        public string ReadLongPascalString() { return Reader.ReadLine(); }

        public void Dispose()
        {
            Reader.Dispose();
        }
    }
}