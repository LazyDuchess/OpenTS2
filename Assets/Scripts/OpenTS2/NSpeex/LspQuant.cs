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

namespace NSpeex
{
	/// <summary>
	/// Abstract class that is the base for the various LSP Quantisation and
	/// Unquantisation methods.
	/// </summary>
    internal abstract class LspQuant
	{
		protected const int MAX_LSP_SIZE = 20;

		protected internal LspQuant()
		{
		}

		/// <summary>
		/// Line Spectral Pair Quantification.
		/// </summary>
		public abstract void Quant(float[] lsp, float[] qlsp, int order, Bits bits);

		/// <summary>
		/// Line Spectral Pair Unquantification.
		/// </summary>
		public abstract void Unquant(float[] lsp, int order, Bits bits);

		/// <summary>
		/// Read the next 6 bits from the buffer, and using the value read and the
		/// given codebook, rebuild LSP table.
		/// </summary>
		protected internal void UnpackPlus(float[] lsp, int[] tab, Bits bits, float k, int ti, int li)
		{
			int id = bits.Unpack(6);
			for (int i = 0; i < ti; i++)
				lsp[i + li] += k * (float)tab[id * ti + i];
		}

		/// <summary>
		/// LSP quantification Note: x is modified
		/// </summary>
		/// <returns>the index of the best match in the codebook (NB x is also</returns>
		protected static internal int Lsp_quant(float[] x, int xs, int[] cdbk, int nbVec, int nbDim)
		{
			int i, j;
			float dist, tmp;
			float best_dist = 0;
			int best_id = 0;
			int ptr = 0;
			for (i = 0; i < nbVec; i++)
			{
				dist = 0;
				for (j = 0; j < nbDim; j++)
				{
					tmp = (x[xs + j] - cdbk[ptr++]);
					dist += tmp * tmp;
				}
				if (dist < best_dist || i == 0)
				{
					best_dist = dist;
					best_id = i;
				}
			}

			for (j = 0; j < nbDim; j++)
				x[xs + j] -= cdbk[best_id * nbDim + j];

			return best_id;
		}

		/// <summary>
		/// LSP weighted quantification Note: x is modified
		/// </summary>
		/// <returns>the index of the best match in the codebook (NB x is also</returns>
		protected static internal int Lsp_weight_quant(
			float[] x, int xs,
			float[] weight, int ws, int[] cdbk,
			int nbVec, int nbDim)
		{
			int i, j;
			float dist, tmp;
			float best_dist = 0;
			int best_id = 0;
			int ptr = 0;
			for (i = 0; i < nbVec; i++)
			{
				dist = 0;
				for (j = 0; j < nbDim; j++)
				{
					tmp = (x[xs + j] - cdbk[ptr++]);
					dist += weight[ws + j] * tmp * tmp;
				}
				if (dist < best_dist || i == 0)
				{
					best_dist = dist;
					best_id = i;
				}
			}
			for (j = 0; j < nbDim; j++)
				x[xs + j] -= cdbk[best_id * nbDim + j];
			return best_id;
		}
	}
}
