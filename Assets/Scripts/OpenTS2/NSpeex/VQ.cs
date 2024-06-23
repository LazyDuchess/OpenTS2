//
// Copyright (C) 2003 Jean-Marc Valin
// Copyright (C) 1999-2003 Wimba S.A., All Rights Reserved.
// Copyright (C) 2008 Filip Navara
// Copyright (C) 2009-2010 Christoph Fröschl
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 
// - Redistributions of source code must retain the above copyright
// notice, this list of conditions and the following disclaimer.
// 
// - Redistributions in binary form must reproduce the above copyright
// notice, this list of conditions and the following disclaimer in the
// documentation and/or other materials provided with the distribution.
// 
// - Neither the name of the Xiph.org Foundation nor the names of its
// contributors may be used to endorse or promote products derived from
// this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// ``AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE FOUNDATION OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
// EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

#if FIXED_POINT
using SpeexWord16 = System.Int16;
using SpeexWord32 = System.Int32;
#else
using SpeexWord16 = System.Single;
using SpeexWord32 = System.Single;
#endif

namespace NSpeex
{
	/// <summary>
	/// Vector Quantization.
	/// </summary>
    internal class VQ
	{
		/// <summary>
		/// Finds the index of the entry in a codebook that best matches the input.
		/// </summary>
		/// <returns>the index of the entry in a codebook that best matches the input.</returns>
		public static int Index(SpeexWord16 ins0, SpeexWord16[] codebook, int entries)
		{
			int i;
			SpeexWord32 min_dist = 0;
			int best_index = 0;
			for (i = 0; i < entries; i++)
			{
				SpeexWord32 dist = ins0 - codebook[i];
				dist = dist * dist;
				if (i == 0 || dist < min_dist)
				{
					min_dist = dist;
					best_index = i;
				}
			}
			return best_index;
		}

		/// <summary>
		/// Finds the index of the entry in a codebook that best matches the input.
		/// </summary>
		/// <returns>the index of the entry in a codebook that best matches the input.</returns>
		public static int Index(SpeexWord16[] ins0, SpeexWord16[] codebook, int len, int entries)
		{
			int i, j, k = 0;
			SpeexWord32 min_dist = 0;
			int best_index = 0;
			for (i = 0; i < entries; i++)
			{
				SpeexWord32 dist = 0;
				for (j = 0; j < len; j++)
				{
					SpeexWord32 tmp = ins0[j] - codebook[k++];
					dist += tmp * tmp;
				}
				if (i == 0 || dist < min_dist)
				{
					min_dist = dist;
					best_index = i;
				}
			}
			return best_index;
		}

		/// <summary>
		/// Finds the indices of the n-best entries in a codebook
		/// </summary>
		public static void Nbest(
			SpeexWord16[] ins0, int offset, SpeexWord16[] codebook, int len, int entries,
			SpeexWord32[] E, int N, int[] nbest, SpeexWord32[] best_dist)
		{
			int i, j, k, l = 0, used = 0;
			for (i = 0; i < entries; i++)
			{
#if FIXED_POINT
				SpeexWord32 dist = E[i] >> 1;
#else
				SpeexWord32 dist = .5f * E[i];
#endif
				for (j = 0; j < len; j++)
					dist -= ins0[offset + j] * codebook[l++];
				if (i < N || dist < best_dist[N - 1])
				{
					for (k = N - 1; (k >= 1) && (k > used || dist < best_dist[k - 1]); k--)
					{
						best_dist[k] = best_dist[k - 1];
						nbest[k] = nbest[k - 1];
					}
					best_dist[k] = dist;
					nbest[k] = i;
					used++;
				}
			}
		}

		/// <summary>
		/// Finds the indices of the n-best entries in a codebook with sign
		/// </summary>
		public static void Nbest_sign(
			SpeexWord16[] ins0, int offset, SpeexWord16[] codebook, int len, int entries,
			SpeexWord32[] E, int N, int[] nbest, SpeexWord32[] best_dist)
		{
			int i, j, k, l = 0, sign, used = 0;
			for (i = 0; i < entries; i++)
			{
				SpeexWord32 dist = 0;
				for (j = 0; j < len; j++)
					dist -= ins0[offset + j] * codebook[l++];
				if (dist > 0)
				{
					sign = 1;
					dist = -dist;
				}
				else
				{
					sign = 0;
				}
#if FIXED_POINT
				dist += E[i] >> 1;
#else
				dist += .5f * E[i];
#endif
				if (i < N || dist < best_dist[N - 1])
				{
					for (k = N - 1; (k >= 1)
							&& (k > used || dist < best_dist[k - 1]); k--)
					{
						best_dist[k] = best_dist[k - 1];
						nbest[k] = nbest[k - 1];
					}
					best_dist[k] = dist;
					nbest[k] = i;
					used++;
					if (sign != 0)
						nbest[k] += entries;
				}
			}
		}
	}
}
