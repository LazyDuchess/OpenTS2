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
	/// LSP Quantisation and Unquantisation (Lbr)
	/// </summary>
    internal class LbrLspQuant : LspQuant
	{
		/// <summary>
		/// Line Spectral Pair Quantification (Lbr).
		/// </summary>
		public sealed override void Quant(float[] lsp, float[] qlsp, int order, Bits bits)
		{
			int i;
			float tmp1, tmp2;
			int id;
			float[] quant_weight = new float[NSpeex.LspQuant.MAX_LSP_SIZE];

			for (i = 0; i < order; i++)
				qlsp[i] = lsp[i];
			quant_weight[0] = 1 / (qlsp[1] - qlsp[0]);
			quant_weight[order - 1] = 1 / (qlsp[order - 1] - qlsp[order - 2]);
			for (i = 1; i < order - 1; i++)
			{
				tmp1 = 1 / ((.15f + qlsp[i] - qlsp[i - 1]) * (.15f + qlsp[i] - qlsp[i - 1]));
				tmp2 = 1 / ((.15f + qlsp[i + 1] - qlsp[i]) * (.15f + qlsp[i + 1] - qlsp[i]));
				quant_weight[i] = (tmp1 > tmp2) ? tmp1 : tmp2;
			}

			for (i = 0; i < order; i++)
				qlsp[i] -= ((Single?)(.25d * i + .25d)).Value;
			for (i = 0; i < order; i++)
				qlsp[i] *= 256;

			id = NSpeex.LspQuant.Lsp_quant(qlsp, 0,
					NSpeex.Codebook_Constants.cdbk_nb,
					NSpeex.Codebook_Constants.NB_CDBK_SIZE, order);
			bits.Pack(id, 6);

			for (i = 0; i < order; i++)
				qlsp[i] *= 2;
			id = NSpeex.LspQuant.Lsp_weight_quant(qlsp, 0, quant_weight, 0,
					NSpeex.Codebook_Constants.cdbk_nb_low1,
					NSpeex.Codebook_Constants.NB_CDBK_SIZE_LOW1, 5);
			bits.Pack(id, 6);
			id = NSpeex.LspQuant.Lsp_weight_quant(qlsp, 5, quant_weight, 5,
					NSpeex.Codebook_Constants.cdbk_nb_high1,
					NSpeex.Codebook_Constants.NB_CDBK_SIZE_HIGH1, 5);
			bits.Pack(id, 6);

			for (i = 0; i < order; i++)
				qlsp[i] *= ((Single?)0.0019531d).Value;
			for (i = 0; i < order; i++)
				qlsp[i] = lsp[i] - qlsp[i];
		}

		/// <summary>
		/// Line Spectral Pair Unquantification (Lbr).
		/// </summary>
		///
		/// <param name="lsp"> -</param>
		/// <param name="order"></param>
		/// <param name="bits"> -</param>
		public sealed override void Unquant(float[] lsp, int order,
				Bits bits)
		{
			for (int i = 0; i < order; i++)
				lsp[i] = .25f * i + .25f;
			UnpackPlus(lsp, NSpeex.Codebook_Constants.cdbk_nb, bits, 0.0039062f, 10, 0);
			UnpackPlus(lsp, NSpeex.Codebook_Constants.cdbk_nb_low1, bits, 0.0019531f, 5, 0);
			UnpackPlus(lsp, NSpeex.Codebook_Constants.cdbk_nb_high1, bits, 0.0019531f, 5, 5);
		}
	}
}
