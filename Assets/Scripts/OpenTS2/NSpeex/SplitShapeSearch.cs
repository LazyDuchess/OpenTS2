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

using System.Collections.Generic;
using System.Collections;
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
	/// Split shape codebook search
	/// </summary>
	internal class SplitShapeSearch : CodebookSearch
	{
		private const int MAX_COMPLEXITY = 10;

		private int subframesize;
		private int subvect_size;
		private int nb_subvect;
		private int[] shape_cb;
		private int shape_cb_size;
		private int shape_bits;
		private int have_sign;
		private int[] ind;
		private int[] signs;
		// Varibles used by the encoder
		private SpeexWord32[] E;
		private SpeexWord16[] t;
		private SpeexWord16[] r2;
		private SpeexWord32[] e;
		private SpeexWord16[][] ot, nt;
		private int[,] nind, oind;

		/// <summary>
		/// Constructor
		/// </summary>
		public SplitShapeSearch(
			int subframesize_0, int subvect_size_1,
			int nb_subvect_2, int[] shape_cb_3,
			int shape_bits_4, int have_sign_5)
		{
			this.subframesize = subframesize_0;
			this.subvect_size = subvect_size_1;
			this.nb_subvect = nb_subvect_2;
			this.shape_cb = shape_cb_3;
			this.shape_bits = shape_bits_4;
			this.have_sign = have_sign_5;
			this.ind = new int[nb_subvect_2];
			this.signs = new int[nb_subvect_2];
			shape_cb_size = 1 << shape_bits_4;
			ot = CreateJaggedArray<SpeexWord16>(MAX_COMPLEXITY, subframesize_0);
			nt = CreateJaggedArray<SpeexWord16>(MAX_COMPLEXITY, subframesize_0);
			oind = new int[MAX_COMPLEXITY, nb_subvect_2];
			nind = new int[MAX_COMPLEXITY, nb_subvect_2];
			t = new SpeexWord16[subframesize_0];
			e = new SpeexWord32[subframesize_0];
			r2 = new SpeexWord16[subframesize_0];
			E = new SpeexWord32[shape_cb_size];
		}

		private T[][] CreateJaggedArray<T>(int dim1, int dim2)
		{
			T[][] a1 = new T[dim1][];
			for (int i = 0; i < dim1; i++)
			{
				a1[i] = new T[dim2];
				Array.Clear(a1[i], 0, dim2);
			}
			return a1;
		}

		/// <summary>
		/// Codebook Search Quantification (Split Shape).
		/// </summary>
		/// <param name="target">target vector</param>
		/// <param name="ak">LPCs for this subframe</param>
		/// <param name="awk1">Weighted LPCs for this subframe</param>
		/// <param name="awk2">Weighted LPCs for this subframe</param>
		/// <param name="p">number of LPC coeffs</param>
		/// <param name="nsf">number of samples in subframe</param>
		/// <param name="exc">excitation array.</param>
		/// <param name="es">position in excitation array.</param>
		/// <param name="r"></param>
		/// <param name="bits">Speex bits buffer.</param>
		/// <param name="complexity"></param>
		public sealed override void Quantify(
			SpeexWord16[] target, SpeexWord16[] ak, SpeexWord16[] awk1,
			SpeexWord16[] awk2, int p, int nsf, SpeexWord32[] exc, int es, SpeexWord16[] r,
			Bits bits, int complexity)
		{
			int i, j, k, m, n, q;
			SpeexWord16[] resp;
			SpeexWord32[] ndist, odist;
			int[] best_index;
			SpeexWord32[] best_dist;
			int[] best_nind;
			int[] best_ntarget;

			int N = complexity;
			if (N > 10)
				N = 10;

			resp = new SpeexWord16[shape_cb_size * subvect_size];

			best_index = new int[N];
			best_dist = new SpeexWord32[N];
			ndist = new SpeexWord32[N];
			odist = new SpeexWord32[N];
			best_nind = new int[N];
			best_ntarget = new int[N];

			for (i = 0; i < N; i++)
			{
				for (j = 0; j < nb_subvect; j++)
					nind[i, j] = oind[i, j] = -1;
			}

			for (j = 0; j < N; j++)
				for (i = 0; i < nsf; i++)
					ot[j][i] = target[i];

			// System.arraycopy(target, 0, t, 0, nsf);

			/* Pre-compute codewords response and energy */
			for (i = 0; i < shape_cb_size; i++)
			{
				int res;
				int shape;

				res = i * subvect_size;
				shape = i * subvect_size;

				/* Compute codeword response using convolution with impulse response */
				for (j = 0; j < subvect_size; j++)
				{
					resp[res + j] = 0;
					for (k = 0; k <= j; k++)
#if FIXED_POINT
						resp[res + j] += (short)((shape_cb[shape + k] * r[j - k]) >> 13);
#else
						resp[res + j] += 0.03125f * shape_cb[shape + k] * r[j - k];
#endif
				}

				/* Compute codeword energy */
				E[i] = 0;
				for (j = 0; j < subvect_size; j++)
					E[i] += resp[res + j] * resp[res + j];
			}

			for (j = 0; j < N; j++)
				odist[j] = 0;

			/* For all subvectors */
			for (i = 0; i < nb_subvect; i++)
			{
				int offset = i * subvect_size;

				/* "erase" nbest list */
				for (j = 0; j < N; j++)
					ndist[j] = Int32.MaxValue;
				/* This is not strictly necessary, but it provides an additonal safety 
				   to prevent crashes in case something goes wrong in the previous
				   steps (e.g. NaNs) */
				for (j = 0; j < N; j++)
					best_nind[j] = best_ntarget[j] = 0;

				/* For all n-bests of previous subvector */
				for (j = 0; j < N; j++)
				{
					SpeexWord32 tener = 0;
					for (m = offset; m < offset + subvect_size; m++)
						tener += ot[j][m] * ot[j][m];
#if FIXED_POINT
					tener = tener >> 1;
#else
					tener *= 0.5f;
#endif

					/* Find new n-best based on previous n-best j */
					if (have_sign != 0)
						NSpeex.VQ.Nbest_sign(
							ot[j], offset, resp, subvect_size,
							shape_cb_size, E, N, best_index, best_dist);
					else
						NSpeex.VQ.Nbest(
							ot[j], offset, resp, subvect_size,
							shape_cb_size, E, N, best_index, best_dist);

					/* For all new n-bests */
					for (k = 0; k < N; k++)
					{
						/* Compute total distance (including previous sub-vectors */
						SpeexWord32 err = odist[j] + best_dist[k] + tener;

						/* Update n-best list */
						if (err < ndist[N - 1])
						{
							for (m = 0; m < N; m++)
							{
								if (err < ndist[m])
								{
									for (n = N - 1; n > m; n--)
									{
										ndist[n] = ndist[n - 1];
										best_nind[n] = best_nind[n - 1];
										best_ntarget[n] = best_ntarget[n - 1];
									}
									/* n is equal to m here, so they're interchangeable */
									ndist[m] = err;
									best_nind[n] = best_index[k];
									best_ntarget[n] = j;
									break;
								}
							}
						}
					}
					if (i == 0)
						break;
				}

				for (j = 0; j < N; j++)
				{
					/*previous target (we don't care what happened before*/
					for (m = (i + 1) * subvect_size; m < nsf; m++)
						nt[j][m] = ot[best_ntarget[j]][m];

					/* New code: update the rest of the target only if it's worth it */
					for (m = 0; m < subvect_size; m++)
					{
						SpeexWord16 g;
						int rind;
						SpeexWord16 sign = 1;
						rind = best_nind[j];
						if (rind >= shape_cb_size)
						{
							sign = -1;
							rind -= shape_cb_size;
						}

						q = subvect_size - m;
#if FIXED_POINT
						g = (short)(sign * shape_cb[rind * subvect_size + m]);
#else
						g = sign * 0.03125f * shape_cb[rind * subvect_size + m];
#endif
						int ni;
						for (n = 0, ni = offset + subvect_size; n < nsf - subvect_size * (i + 1); n++, ni++)
#if FIXED_POINT
							nt[j][ni] -= (short)((g * r[n + q]) >> 13);
#else
							nt[j][ni] -= (g * r[n + q]);
#endif
					}

					for (q = 0; q < nb_subvect; q++)
						nind[j, q] = oind[best_ntarget[j], q];
					nind[j, i] = best_nind[j];
				}

				/*update old-new data*/
				/* just swap pointers instead of a long copy */
				{
					SpeexWord16[][] tmp2;
					tmp2 = ot;
					ot = nt;
					nt = tmp2;
				}

				for (j = 0; j < N; j++)
					for (m = 0; m < nb_subvect; m++)
						oind[j, m] = nind[j, m];
				for (j = 0; j < N; j++)
					odist[j] = ndist[j];
			}

			/* save indices */
			for (i = 0; i < nb_subvect; i++)
			{
				ind[i] = nind[0, i];
				bits.Pack(ind[i], shape_bits + have_sign);
			}

			/* Put everything back together */
			for (i = 0; i < nb_subvect; i++)
			{
				int rind_3;
				SpeexWord16 sign_4 = 1;
				rind_3 = ind[i];
				if (rind_3 >= shape_cb_size)
				{
					sign_4 = -1;
					rind_3 -= shape_cb_size;
				}

#if FIXED_POINT
				if (sign_4 == 1)
				{
					for (j = 0; j < subvect_size; j++)
						e[subvect_size * i + j] = (int)(shape_cb[rind_3 * subvect_size + j]) >> (14 - 5);
				}
				else
				{
					for (j = 0; j < subvect_size; j++)
						e[subvect_size * i + j] = -((int)(shape_cb[rind_3 * subvect_size + j]) >> (14 - 5));
				}
#else
				for (j = 0; j < subvect_size; j++)
					e[subvect_size * i + j] = sign_4 * 0.03125f * shape_cb[rind_3 * subvect_size + j];
#endif
			}
			/* Update excitation */
			for (j = 0; j < nsf; j++)
				exc[es + j] += e[j];

			/* Update target */
			NSpeex.Filters.Syn_percep_zero(e, 0, ak, awk1, awk2, r2, nsf, p);
			for (j = 0; j < nsf; j++)
				target[j] -= r2[j];
		}

		/// <summary>
		/// Codebook Search Unquantification (Split Shape).
		/// </summary>
		public sealed override void Unquantify(SpeexWord32[] exc, int es, int nsf, Bits bits)
		{
			int i, j;

			/* Decode codewords and gains */
			for (i = 0; i < nb_subvect; i++)
			{
				if (have_sign != 0)
					signs[i] = bits.Unpack(1);
				else
					signs[i] = 0;
				ind[i] = bits.Unpack(shape_bits);
			}

			/* Compute decoded excitation */
			for (i = 0; i < nb_subvect; i++)
			{
				float s = 1.0f;
				if (signs[i] != 0)
					s = -1.0f;
#if FIXED_POINT
				if (s == 1)
				{
					for (j = 0; j < subvect_size; j++)
						exc[subvect_size * i + j] = (int)(shape_cb[ind[i] * subvect_size + j]) >> (14 - 5);
				}
				else
				{
					for (j = 0; j < subvect_size; j++)
						exc[subvect_size * i + j] = -((int)(shape_cb[ind[i] * subvect_size + j]) << (14 - 5));
				}
#else
				for (j = 0; j < subvect_size; j++)
					exc[es + subvect_size * i + j] += s * 0.03125f * (float)shape_cb[ind[i] * subvect_size + j];
#endif
			}
		}
	}
}
