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
	/// Noise codebook search
	/// </summary>
    internal class NoiseSearch : CodebookSearch
	{
		/// <summary>
		/// Codebook Search Quantification (Noise).
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
			int i;
			SpeexWord16[] tmp = new SpeexWord16[nsf];
			NSpeex.Filters.Residue_percep_zero(target, 0, ak, awk1, awk2, tmp, nsf, p);

			for (i = 0; i < nsf; i++)
				exc[es + i] += tmp[i];
			for (i = 0; i < nsf; i++)
				target[i] = 0;
		}

		/// <summary>
		/// Codebook Search Unquantification (Noise).
		/// </summary>
		///
		/// <param name="exc"> -</param>
		/// <param name="es"> -</param>
		/// <param name="nsf"> -</param>
		/// <param name="bits"> -</param>
		public sealed override void Unquantify(SpeexWord32[] exc, int es, int nsf, Bits bits)
		{
			for (int i = 0; i < nsf; i++)
#if FIXED_POINT
				exc[es + i] += (int)((new Random().Next(Int16.MaxValue)) << 14);
#else
				exc[es + i] += (float)(3.0d * (new Random().NextDouble() - .5d));
#endif
		}
	}
}
