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
	/// Filters
	/// </summary>
	///
	/// @author Jim Lawrence, helloNetwork.com
	/// @author Marc Gimpel, Wimba S.A. (mgimpel@horizonwimba.com)
	/// @version $Revision: 1.2 $
    internal class Filters
	{
		private int last_pitch;
		private float[] last_pitch_gain;
		private float smooth_gain;
		private float[] xx;

		public Filters()
		{
			last_pitch_gain = new float[3];
			xx = new float[1024];
			last_pitch = 0;
			last_pitch_gain[0] = last_pitch_gain[1] = last_pitch_gain[2] = 0;
			smooth_gain = 1;
		}

		public static void Bw_lpc(float gamma, float[] lpc_in,
				float[] lpc_out, int order)
		{
			float tmp = 1;
			for (int i = 0; i < order + 1; i++)
			{
				lpc_out[i] = tmp * lpc_in[i];
				tmp *= gamma;
			}
		}

		public static void Filter_mem2(SpeexWord16[] x, int xs,
				SpeexWord16[] num, SpeexWord16[] den, int N, int ord,
				SpeexWord16[] mem, int ms)
		{
			int i, j;
			SpeexWord16 xi, yi;
			for (i = 0; i < N; i++)
			{
				xi = x[xs + i];
				x[xs + i] = (SpeexWord16)(num[0] * xi + mem[ms + 0]);
				yi = x[xs + i];
				for (j = 0; j < ord - 1; j++)
				{
					mem[ms + j] = (SpeexWord16)(mem[ms + j + 1] + num[j + 1] * xi - den[j + 1] * yi);
				}
				mem[ms + ord - 1] = (SpeexWord16)(num[ord] * xi - den[ord] * yi);
			}
		}

		public static void Filter_mem2(SpeexWord16[] x, int xs,
				SpeexWord16[] num, SpeexWord16[] den, SpeexWord16[] y,
				int ys, int N, int ord, SpeexWord16[] mem,
				int ms)
		{
			int i, j;
			float xi, yi;
			for (i = 0; i < N; i++)
			{
				xi = x[xs + i];
				y[ys + i] = (SpeexWord16)(num[0] * xi + mem[0]);
				yi = y[ys + i];
				for (j = 0; j < ord - 1; j++)
				{
					mem[ms + j] = (SpeexWord16)(mem[ms + j + 1] + num[j + 1] * xi - den[j + 1] * yi);
				}
				mem[ms + ord - 1] = (SpeexWord16)(num[ord] * xi - den[ord] * yi);
			}
		}

		public static void Iir_mem2(SpeexWord16[] x, int xs,
				SpeexWord16[] den, SpeexWord16[] y, int ys, int N,
				int ord, SpeexWord16[] mem)
		{
			int i, j;
			for (i = 0; i < N; i++)
			{
				y[ys + i] = (SpeexWord16)(x[xs + i] + mem[0]);
				for (j = 0; j < ord - 1; j++)
				{
					mem[j] = (SpeexWord16)(mem[j + 1] - den[j + 1] * y[ys + i]);
				}
				mem[ord - 1] = (SpeexWord16)(-den[ord] * y[ys + i]);
			}
		}

		public static void Fir_mem2(float[] x, int xs,
				float[] num, float[] y, int ys, int N,
				int ord, float[] mem)
		{
			int i, j;
			float xi;
			for (i = 0; i < N; i++)
			{
				xi = x[xs + i];
				y[ys + i] = num[0] * xi + mem[0];
				for (j = 0; j < ord - 1; j++)
				{
					mem[j] = mem[j + 1] + num[j + 1] * xi;
				}
				mem[ord - 1] = num[ord] * xi;
			}
		}

		public static void Syn_percep_zero(SpeexWord16[] xx_0, int xxs,
				SpeexWord16[] ak, SpeexWord16[] awk1, SpeexWord16[] awk2,
				SpeexWord16[] y, int N, int ord)
		{
			int i;
			SpeexWord16[] mem = new SpeexWord16[ord];
			// for (i=0;i<ord;i++)
			// mem[i]=0;
			Filter_mem2(xx_0, xxs, awk1, ak, y, 0, N, ord, mem, 0);
			for (i = 0; i < ord; i++)
				mem[i] = 0;
			Iir_mem2(y, 0, awk2, y, 0, N, ord, mem);
		}

		public static void Residue_percep_zero(SpeexWord16[] xx_0,
				int xxs, SpeexWord16[] ak, SpeexWord16[] awk1,
				SpeexWord16[] awk2, SpeexWord16[] y, int N, int ord)
		{
			int i;
			float[] mem = new float[ord];
			// for (i=0;i<ord;i++)
			// mem[i]=0;
			Filter_mem2(xx_0, xxs, ak, awk1, y, 0, N, ord, mem, 0);
			for (i = 0; i < ord; i++)
				mem[i] = 0;
			Fir_mem2(y, 0, awk2, y, 0, N, ord, mem);
		}

		public void Fir_mem_up(float[] x, float[] a, float[] y,
				int N, int M, float[] mem)
		{
			int i, j;

			for (i = 0; i < N / 2; i++)
				xx[2 * i] = x[N / 2 - 1 - i];
			for (i = 0; i < M - 1; i += 2)
				xx[N + i] = mem[i + 1];

			for (i = 0; i < N; i += 4)
			{
				float y0, y1, y2, y3, x0;
				y0 = y1 = y2 = y3 = 0.0f;
				x0 = xx[N - 4 - i];
				for (j = 0; j < M; j += 4)
				{
					float x1, a0, a1;
					a0 = a[j];
					a1 = a[j + 1];
					x1 = xx[N - 2 + j - i];
					y0 += a0 * x1;
					y1 += a1 * x1;
					y2 += a0 * x0;
					y3 += a1 * x0;
					a0 = a[j + 2];
					a1 = a[j + 3];
					x0 = xx[N + j - i];
					y0 += a0 * x0;
					y1 += a1 * x0;
					y2 += a0 * x1;
					y3 += a1 * x1;
				}
				y[i] = y0;
				y[i + 1] = y1;
				y[i + 2] = y2;
				y[i + 3] = y3;
			}
			for (i = 0; i < M - 1; i += 2)
				mem[i + 1] = xx[i];
		}

		/// <summary>
		/// Comb Filter
		/// </summary>
		public void Comb_filter(float[] exc, int esi,
				float[] new_exc, int nsi, int nsf,
				int pitch, float[] pitch_gain, float comb_gain)
		{
			int i, j;
			float exc_energy = 0.0f, new_exc_energy = 0.0f;
			float gain, step, fact, g = 0.0f;

			/* Compute excitation energy prior to enhancement */
			for (i = esi; i < esi + nsf; i++)
			{
				exc_energy += exc[i] * exc[i];
			}

			/* Some gain adjustment if pitch is too high or if unvoiced */
			g = .5f * Math.Abs(pitch_gain[0] + pitch_gain[1]
							+ pitch_gain[2] + last_pitch_gain[0] + last_pitch_gain[1]
							+ last_pitch_gain[2]);
			if (g > 1.3f)
				comb_gain *= 1.3f / g;
			if (g < .5f)
				comb_gain *= 2.0f * g;

			step = 1.0f / nsf;
			fact = 0;

			/* Apply pitch comb-filter (filter out noise between pitch harmonics) */
			for (i = 0, j = esi; i < nsf; i++, j++)
			{
				fact += step;
				new_exc[nsi + i] = exc[j]
						+ comb_gain
						* fact
						* (pitch_gain[0] * exc[j - pitch + 1] + pitch_gain[1]
								* exc[j - pitch] + pitch_gain[2]
								* exc[j - pitch - 1])
						+ comb_gain
						* (1.0f - fact)
						* (last_pitch_gain[0] * exc[j - last_pitch + 1]
								+ last_pitch_gain[1] * exc[j - last_pitch] + last_pitch_gain[2]
								* exc[j - last_pitch - 1]);
			}

			last_pitch_gain[0] = pitch_gain[0];
			last_pitch_gain[1] = pitch_gain[1];
			last_pitch_gain[2] = pitch_gain[2];
			last_pitch = pitch;

			/* Gain after enhancement */
			for (i = nsi; i < nsi + nsf; i++)
				new_exc_energy += new_exc[i] * new_exc[i];

			/* Compute scaling factor and normalize energy */
			gain = (float)(Math.Sqrt(exc_energy / (.1f + new_exc_energy)));
			if (gain < .5f)
			{
				gain = .5f;
			}
			if (gain > 1.0f)
			{
				gain = 1.0f;
			}

			for (i = nsi; i < nsi + nsf; i++)
			{
				smooth_gain = .96f * smooth_gain + .04f * gain;
				new_exc[i] *= smooth_gain;
			}
		}

		/// <summary>
		/// Quadrature Mirror Filter to Split the band in two. A 16kHz signal is thus
		/// divided into two 8kHz signals representing the low and high bands. (used
		/// by wideband encoder)
		/// </summary>
		public static void Qmf_decomp(float[] xx_0, float[] aa,
				float[] y1, float[] y2, int N, int M,
				float[] mem)
		{
			int i, j, k, M2;
			float[] a;
			float[] x;
			int x2;

			a = new float[M];
			x = new float[N + M - 1];
			x2 = M - 1;
			M2 = M >> 1;
			for (i = 0; i < M; i++)
				a[M - i - 1] = aa[i];
			for (i = 0; i < M - 1; i++)
				x[i] = mem[M - i - 2];
			for (i = 0; i < N; i++)
				x[i + M - 1] = xx_0[i];
			for (i = 0, k = 0; i < N; i += 2, k++)
			{
				y1[k] = 0;
				y2[k] = 0;
				for (j = 0; j < M2; j++)
				{
					y1[k] += a[j] * (x[i + j] + x[x2 + i - j]);
					y2[k] -= a[j] * (x[i + j] - x[x2 + i - j]);
					j++;
					y1[k] += a[j] * (x[i + j] + x[x2 + i - j]);
					y2[k] += a[j] * (x[i + j] - x[x2 + i - j]);
				}
			}
			for (i = 0; i < M - 1; i++)
				mem[i] = xx_0[N - i - 1];
		}
	}
}
