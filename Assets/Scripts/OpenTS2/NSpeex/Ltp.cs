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

using System;

namespace NSpeex
{
	/// <summary>
	/// Abstract class that is the base for the various LTP (Long Term Prediction)
	/// Quantisation and Unquantisation methods.
	/// </summary>
    internal abstract class Ltp
	{
		/// <summary>
		/// Long Term Prediction Quantification.
		/// </summary>
		/// <returns>pitch</returns>
		public abstract int Quant(
			float[] target, float[] sw, int sws, float[] ak,
			float[] awk1, float[] awk2, float[] exc, int es, int start,
			int end, float pitch_coef, int p, int nsf, Bits bits, float[] exc2,
			int e2s, float[] r, int complexity);

		/// <summary>
		/// Long Term Prediction Unquantification.
		/// </summary>
		/// <returns>pitch</returns>
		public abstract int Unquant(
			float[] exc, int es, int start,
			float pitch_coef, int nsf, float[] gain_val, Bits bits,
			int count_lost, int subframe_offset, float last_pitch_gain);

		/// <summary>
		/// Calculates the inner product of the given vectors.
		/// </summary>>
		/// <returns>the inner product of the given vectors.</returns>
		protected static internal float Inner_prod(
			float[] x, int xs, float[] y, int ys, int len)
		{
			int i;
			float sum1 = 0, sum2 = 0, sum3 = 0, sum4 = 0;
			for (i = 0; i < len; )
			{
				sum1 += x[xs + i] * y[ys + i];
				sum2 += x[xs + i + 1] * y[ys + i + 1];
				sum3 += x[xs + i + 2] * y[ys + i + 2];
				sum4 += x[xs + i + 3] * y[ys + i + 3];
				i += 4;
			}
			return sum1 + sum2 + sum3 + sum4;
		}

		/// <summary>
		/// Find the n-best pitch in Open Loop.
		/// </summary>
		protected static internal void Open_loop_nbest_pitch(
			float[] sw, int swIdx, int start, int end, int len,
			int[] pitch, float[] gain, int N)
		{
			int i, j, k;
			/* float corr=0; */
			/* float energy; */
			float[] best_score;
			float e0;
			float[] corr, energy, score;

			best_score = new float[N];
			corr = new float[end - start + 1];
			energy = new float[end - start + 2];
			score = new float[end - start + 1];
			for (i = 0; i < N; i++)
			{
				best_score[i] = -1;
				gain[i] = 0;
				pitch[i] = start;
			}
			energy[0] = Inner_prod(sw, swIdx - start, sw, swIdx - start, len);
			e0 = Inner_prod(sw, swIdx, sw, swIdx, len);
			for (i = start; i <= end; i++)
			{
				/* Update energy for next pitch */
				energy[i - start + 1] = energy[i - start] + sw[swIdx - i - 1]
						* sw[swIdx - i - 1] - sw[swIdx - i + len - 1]
						* sw[swIdx - i + len - 1];
				if (energy[i - start + 1] < 1)
					energy[i - start + 1] = 1;
			}
			for (i = start; i <= end; i++)
			{
				corr[i - start] = 0;
				score[i - start] = 0;
			}

			for (i = start; i <= end; i++)
			{
				// Compute correlation
				corr[i - start] = Inner_prod(sw, swIdx, sw, swIdx - i, len);
				score[i - start] = corr[i - start] * corr[i - start]
						/ (energy[i - start] + 1);
			}
			for (i = start; i <= end; i++)
			{
				if (score[i - start] > best_score[N - 1])
				{
					float g1, g;
					g1 = corr[i - start] / (energy[i - start] + 10);
					g = (float)Math.Sqrt(g1 * corr[i - start]
											/ (e0 + 10));
					if (g > g1)
						g = g1;
					if (g < 0)
						g = 0;
					for (j = 0; j < N; j++)
					{
						if (score[i - start] > best_score[j])
						{
							for (k = N - 1; k > j; k--)
							{
								best_score[k] = best_score[k - 1];
								pitch[k] = pitch[k - 1];
								gain[k] = gain[k - 1];
							}
							best_score[j] = score[i - start];
							pitch[j] = i;
							gain[j] = g;
							break;
						}
					}
				}
			}
		}
	}
}
