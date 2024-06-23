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
	/// Speex SubMode
	/// </summary>
    internal class SubMode
	{
		private readonly int lbrPitch;
		private readonly int forcedPitchGain;
		private readonly int haveSubframeGain;
		private readonly int doubleCodebook;
		private readonly LspQuant lsqQuant;
		private readonly Ltp ltp;
		private readonly CodebookSearch innovation;
		private readonly float lpcEnhK1;
		private readonly float lpcEnhK2;
		private readonly float combGain;
		private readonly int bitsPerFrame;

		public SubMode(
			int lbrPitch,
			int forcedPitchGain,
			int haveSubframeGain,
			int doubleCodebook,
			LspQuant lspQuant,
			Ltp ltp,
			CodebookSearch innovation,
			float lpcEnhK1,
			float lpcEnhK2,
			float combGain,
			int bitsPerFrame)
		{
			this.lbrPitch = lbrPitch;
			this.forcedPitchGain = forcedPitchGain;
			this.haveSubframeGain = haveSubframeGain;
			this.doubleCodebook = doubleCodebook;
			this.lsqQuant = lspQuant;
			this.ltp = ltp;
			this.innovation = innovation;
			this.lpcEnhK1 = lpcEnhK1;
			this.lpcEnhK2 = lpcEnhK2;
			this.combGain = combGain;
			this.bitsPerFrame = bitsPerFrame;
		}

		/// <summary>
		/// Set to -1 for "normal" modes, otherwise encode pitch using a global pitch
		/// and allowing a +- lbr_pitch variation (for low not-rates)
		/// </summary>
		public int LbrPitch
		{
			get { return lbrPitch; }
		}

		/// <summary>
		/// Use the same (forced) pitch gain for all sub-frames
		/// </summary>
		public int ForcedPitchGain
		{
			get { return forcedPitchGain; }
		}

		/// <summary>
		/// Number of bits to use as sub-frame innovation gain
		/// </summary>
		public int HaveSubframeGain
		{
			get { return haveSubframeGain; }
		}

		/// <summary>
		/// Apply innovation quantization twice for higher quality (and higher
		/// bit-rate)
		/// </summary>
		public int DoubleCodebook
		{
			get { return doubleCodebook; }
		}

		/// <summary>
		/// LSP quantization/unquantization function
		/// </summary>
		public LspQuant LsqQuant
		{
			get { return lsqQuant; }
		}

		/// <summary>
		/// Long-term predictor (pitch) un-quantizer
		/// </summary>
		public Ltp Ltp
		{
			get { return ltp; }
		}

		/// <summary>
		/// Codebook Search un-quantizer
		/// </summary>
		public CodebookSearch Innovation
		{
			get { return innovation; }
		}

		/// <summary>
		/// Enhancer constant
		/// </summary>
		public float LpcEnhK1
		{
			get { return lpcEnhK1; }
		}

		/// <summary>
		/// Enhancer constant
		/// </summary>
		public float LpcEnhK2
		{
			get { return lpcEnhK2; }
		}

		/// <summary>
		/// Gain of enhancer comb filter
		/// </summary>
		public float CombGain
		{
			get { return combGain; }
		}

		/// <summary>
		/// Number of bits per frame after encoding
		/// </summary>
		public int BitsPerFrame
		{
			get { return bitsPerFrame; }
		}
	}
}
