/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF
{
	public static class DBPFCompression
	{
		/// <summary>
		///  Copies data from source to destination array.<br>
		///  The copy is byte by byte from srcPos to destPos and given length.
		/// </summary>
		/// <param name="Src">The source array.</param>
		/// <param name="SrcPos">The source Position.</param>
		/// <param name="Dest">The destination array.</param>
		/// <param name="DestPos">The destination Position.</param>
		/// <param name="Length">The length.</param>
		public static void ArrayCopy2(byte[] Src, int SrcPos, ref byte[] Dest, int DestPos, long Length)
		{
			if (Dest.Length < DestPos + Length)
			{
				byte[] DestExt = new byte[(int)(DestPos + Length)];
				Array.Copy(Dest, 0, DestExt, 0, Dest.Length);
				Dest = DestExt;
			}

			for (int i = 0; i < Length/* - 1*/; i++)
				Dest[DestPos + i] = Src[SrcPos + i];
		}
		/// <summary>
		/// Copies data from array at destPos-srcPos to array at destPos.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="srcPos">The Position to copy from (reverse from end of array!)</param>
		/// <param name="destPos">The Position to copy to.</param>
		/// <param name="length">The length of data to copy.</param>
		public static void OffsetCopy(ref byte[] array, int srcPos, int destPos, long length)
		{
			srcPos = destPos - srcPos;

			if (array.Length < destPos + length)
			{
				byte[] NewArray = new byte[(int)(destPos + length)];
				Array.Copy(array, 0, NewArray, 0, array.Length);
				array = NewArray;
			}

			if (array.Length < srcPos + length)
			{
				byte[] NewArray = new byte[(int)(srcPos + length)];
				Array.Copy(array, 0, NewArray, 0, array.Length);
				array = NewArray;
			}

			for (int i = 0; i < length /*- 1*/; i++)
			{
				try
				{
					array[destPos + i] = array[srcPos + i];
				}
				catch (Exception)
				{
					//Fail silently :(
				}
			}
		}
		public static byte[] Decompress(byte[] Data, uint UncompressedFileSize)
		{

			MemoryStream MemData = new MemoryStream(Data);
			BinaryReader Reader = new BinaryReader(MemData);

			if (Data.Length > 9)
			{
				byte[] DecompressedData = new byte[(int)UncompressedFileSize];
				int DataPos = 0;

				int Pos = 9;
				long Control1 = 0;

				while (Control1 != 0xFC && Pos < Data.Length)
				{
					Control1 = Data[Pos];
					Pos++;

					if (Pos == Data.Length)
						break;

					if (Control1 >= 0 && Control1 <= 127)
					{
						// 0x00 - 0x7F
						long control2 = Data[Pos];
						Pos++;
						long numberOfPlainText = (Control1 & 0x03);
						ArrayCopy2(Data, Pos, ref DecompressedData, DataPos, numberOfPlainText);
						DataPos += (int)numberOfPlainText;
						Pos += (int)numberOfPlainText;

						if (DataPos == (DecompressedData.Length))
							break;

						int offset = (int)(((Control1 & 0x60) << 3) + (control2) + 1);
						long numberToCopyFromOffset = ((Control1 & 0x1C) >> 2) + 3;
						OffsetCopy(ref DecompressedData, offset, DataPos, numberToCopyFromOffset);
						DataPos += (int)numberToCopyFromOffset;

						if (DataPos == (DecompressedData.Length))
							break;
					}
					else if ((Control1 >= 128 && Control1 <= 191))
					{
						// 0x80 - 0xBF
						long control2 = Data[Pos];
						Pos++;
						long control3 = Data[Pos];
						Pos++;

						long numberOfPlainText = (control2 >> 6) & 0x03;
						ArrayCopy2(Data, Pos, ref DecompressedData, DataPos, numberOfPlainText);
						DataPos += (int)numberOfPlainText;
						Pos += (int)numberOfPlainText;

						if (DataPos == (DecompressedData.Length))
							break;

						int offset = (int)(((control2 & 0x3F) << 8) + (control3) + 1);
						long numberToCopyFromOffset = (Control1 & 0x3F) + 4;
						OffsetCopy(ref DecompressedData, offset, DataPos, numberToCopyFromOffset);
						DataPos += (int)numberToCopyFromOffset;

						if (DataPos == (DecompressedData.Length))
							break;
					}
					else if (Control1 >= 192 && Control1 <= 223)
					{
						// 0xC0 - 0xDF
						long numberOfPlainText = (Control1 & 0x03);
						long control2 = Data[Pos];
						Pos++;
						long control3 = Data[Pos];
						Pos++;
						long control4 = Data[Pos];
						Pos++;
						ArrayCopy2(Data, Pos, ref DecompressedData, DataPos, numberOfPlainText);
						DataPos += (int)numberOfPlainText;
						Pos += (int)numberOfPlainText;

						if (DataPos == (DecompressedData.Length))
							break;

						int offset = (int)(((Control1 & 0x10) << 12) + (control2 << 8) + (control3) + 1);
						long numberToCopyFromOffset = ((Control1 & 0x0C) << 6) + (control4) + 5;
						OffsetCopy(ref DecompressedData, offset, DataPos, numberToCopyFromOffset);
						DataPos += (int)numberToCopyFromOffset;

						if (DataPos == (DecompressedData.Length))
							break;
					}
					else if (Control1 >= 224 && Control1 <= 251)
					{
						// 0xE0 - 0xFB
						long numberOfPlainText = ((Control1 & 0x1F) << 2) + 4;
						ArrayCopy2(Data, Pos, ref DecompressedData, DataPos, numberOfPlainText);
						DataPos += (int)numberOfPlainText;
						Pos += (int)numberOfPlainText;

						if (DataPos == (DecompressedData.Length))
							break;
					}
					else
					{
						long numberOfPlainText = (Control1 & 0x03);
						ArrayCopy2(Data, Pos, ref DecompressedData, DataPos, numberOfPlainText);

						DataPos += (int)numberOfPlainText;
						Pos += (int)numberOfPlainText;

						if (DataPos == (DecompressedData.Length))
							break;
					}
				}

				return DecompressedData;
			}

			//No data to decompress
			return Data;
		}

		#region compression		
		//some Compression Data
		const int MAX_OFFSET = 0x20000;
		const int MAX_COPY_COUNT = 0x404;

		//used to finetune the lookup (small values increase the compression for Big Files)		
		static int compstrength = 0x80;

		/// <summary>
		/// Returns /Sets the compression Strength
		/// </summary>
		public static int CompressionStrength
		{
			get { return compstrength; }
			set { compstrength = value; }
		}

		/// <summary>
		/// Compresses the passed content
		/// </summary>
		/// <param name="data">The content</param>
		/// <returns>the compressed Data (including the header)</returns>
		public static byte[] Compress(byte[] data)
		{
			/*
			try
			{*/
			//return Comp(data, true);
			#region Init Variables
			//contains the latest offset for a combination of two characters
			ArrayList[] cmpmap = new ArrayList[0x1000000];

			//will contain the compressed Data
			byte[] cdata = new byte[data.Length];

			//init some vars
			int writeindex = 0;
			int lastreadindex = 0;
			ArrayList indexlist = null;
			int copyoffset = 0;
			int copycount = 0;
			writeindex = 0;
			int index = -1;
			lastreadindex = 0;
			byte[] retdata;
			bool end = false;
			#endregion
			try
			{
				//begin main Compression Loop			
				while (index < data.Length - 3)
				{
					#region get all Compression Candidates (list of offsets for all occurances of the current 3 bytes)
					do
					{
						index++;
						if (index >= data.Length - 2)
						{
							end = true;
							break;
						}
						int mapindex = data[index] | (data[index + 1] << 0x08) | (data[index + 2] << 0x10);

						indexlist = cmpmap[mapindex];
						if (indexlist == null)
						{
							indexlist = new ArrayList();
							cmpmap[mapindex] = indexlist;
						}
						indexlist.Add(index);
					} while (index < lastreadindex);
					if (end) break;

					#endregion

					#region find the longest repeating byte sequence in the index List (for offset copy)
					int offsetcopycount = 0;
					int loopcount = 1;
					while ((loopcount < indexlist.Count) && (loopcount < compstrength))
					{
						int foundindex = (int)indexlist[(indexlist.Count - 1) - loopcount];
						if ((index - foundindex) >= MAX_OFFSET) break;

						loopcount++;
						copycount = 3;
						while ((data.Length > index + copycount) && (data[index + copycount] == data[foundindex + copycount]) && (copycount < MAX_COPY_COUNT))
							copycount++;

						if (copycount > offsetcopycount)
						{
							int cof = index - foundindex;
							offsetcopycount = copycount;
							copyoffset = index - foundindex;
						}
					}
					#endregion

					#region Compression

					//check if we can compress this
					if (offsetcopycount < 3) offsetcopycount = 0;
					else if ((offsetcopycount < 4) && (copyoffset > 0x400)) offsetcopycount = 0;
					else if ((offsetcopycount < 5) && (copyoffset > 0x4000)) offsetcopycount = 0;


					//this is offset-compressable? so do the compression
					if (offsetcopycount > 0)
					{
						//plaincopy
						while ((index - lastreadindex) > 3)
						{
							copycount = (index - lastreadindex);
							while (copycount > 0x71) copycount -= 0x71;
							copycount = copycount & 0xfc;
							int realcopycount = (copycount >> 2);

							cdata[writeindex++] = (byte)(0xdf + realcopycount);
							for (int i = 0; i < copycount; i++) cdata[writeindex++] = data[lastreadindex++];
						}

						//offsetcopy
						copycount = index - lastreadindex;
						copyoffset--;
						if ((offsetcopycount <= 0xa) && (copyoffset < 0x400))
						{
							cdata[writeindex++] = (byte)((((copyoffset >> 3) & 0x60) | ((offsetcopycount - 3) << 2)) | copycount);
							cdata[writeindex++] = (byte)(copyoffset & 0xff);
						}
						else if ((offsetcopycount <= 0x43) && (copyoffset < 0x4000))
						{
							cdata[writeindex++] = (byte)(0x80 | (offsetcopycount - 4));
							cdata[writeindex++] = (byte)((copycount << 6) | (copyoffset >> 8));
							cdata[writeindex++] = (byte)(copyoffset & 0xff);
						}
						else if ((offsetcopycount <= MAX_COPY_COUNT) && (copyoffset < MAX_OFFSET))
						{
							cdata[writeindex++] = (byte)(((0xc0 | ((copyoffset >> 0x0c) & 0x10)) + (((offsetcopycount - 5) >> 6) & 0x0c)) | copycount);
							cdata[writeindex++] = (byte)((copyoffset >> 8) & 0xff);
							cdata[writeindex++] = (byte)(copyoffset & 0xff);
							cdata[writeindex++] = (byte)((offsetcopycount - 5) & 0xff);
						}
						else
						{
							copycount = 0;
							offsetcopycount = 0;
						}

						//do the offset copy
						for (int i = 0; i < copycount; i++) cdata[writeindex++] = data[lastreadindex++];
						lastreadindex += offsetcopycount;
					}
					#endregion
				} //while (main Loop)

				#region Add remaining Data
				//add the End Record
				index = data.Length;
				lastreadindex = Math.Min(index, lastreadindex);
				while ((index - lastreadindex) > 3)
				{
					copycount = (index - lastreadindex);
					while (copycount > 0x71) copycount -= 0x71;
					copycount = copycount & 0xfc;
					int realcopycount = (copycount >> 2);

					cdata[writeindex++] = (byte)(0xdf + realcopycount);
					for (int i = 0; i < copycount; i++) cdata[writeindex++] = data[lastreadindex++];
				}

				copycount = index - lastreadindex;
				cdata[writeindex++] = (byte)(0xfc + copycount);
				for (int i = 0; i < copycount; i++) cdata[writeindex++] = data[lastreadindex++];
				#endregion

				#region Trim Data & and add Header
				//make a resulting Array of the apropriate size
				retdata = new byte[writeindex + 9];

				byte[] sz = BitConverter.GetBytes((uint)(retdata.Length));
				for (int i = 0; i < 4; i++) retdata[i] = sz[i];
				//Compress signature
				sz = BitConverter.GetBytes(0xFB10);
				for (int i = 0; i < 2; i++) retdata[i + 4] = sz[i];

				sz = BitConverter.GetBytes((uint)data.Length);
				for (int i = 0; i < 3; i++) retdata[i + 6] = sz[2 - i];

				for (int i = 0; i < writeindex; i++) retdata[i + 9] = cdata[i];
				#endregion
				return retdata;
			}
			finally
			{
				foreach (ArrayList a in cmpmap)
					if (a != null) a.Clear();

				cmpmap = null;
				cdata = null;
				retdata = null;
				if (indexlist != null) indexlist.Clear();
				indexlist = null;
			}
			/*}
		
			catch (Exception ex)
			{
				throw ex;
			}*/

		}
		#endregion
	}
}