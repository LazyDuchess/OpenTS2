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
	/// Narrowband Codec. This class contains all the basic structures needed by the
	/// Narrowband encoder and decoder.
	/// </summary>
    internal class NbCodec
	{
		#region Constants

		/// <summary>
		/// Very small initial value for some of the buffers.
		/// </summary>
		protected const float VERY_SMALL = (float)0e-30d;
		
		/// <summary>
		/// The Narrowband Frame Size gives the size in bits of a Narrowband frame
		/// for a given narrowband submode.
		/// </summary>
		protected static readonly int[] NB_FRAME_SIZE = { 5, 43, 119, 160, 220, 300, 364,
				492, 79, 1, 1, 1, 1, 1, 1, 1 };
		
		/// <summary>
		/// The Narrowband Submodes gives the number of submodes possible for the
		/// Narrowband codec.
		/// </summary>
		protected const int NB_SUBMODES = 16;
		
		/// <summary>
		/// The Narrowband Submodes Bits gives the number bits used to encode the
		/// Narrowband Submode
		/// </summary>
		protected const int NB_SUBMODE_BITS = 4;
		protected static readonly float[] exc_gain_quant_scal1 = { -0.35f, 0.05f };
		protected static readonly float[] exc_gain_quant_scal3 = { -2.794750f,
				-1.810660f, -1.169850f, -0.848119f, -0.587190f, -0.329818f,
				-0.063266f, 0.282826f };

		#endregion

		#region Tools

		protected internal Lsp m_lsp;
		protected internal Filters filters;

		#endregion

		#region Parameters

		/// <summary>
		/// Sub-mode data
		/// </summary>
		protected internal SubMode[] submodes;

		/// <summary>
		/// Activated sub-mode
		/// </summary>
		protected internal int submodeID;

		/// <summary>
		/// Is this the first frame?
		/// </summary>
		protected internal int first;

		/// <summary>
		/// Size of frames
		/// </summary>
		protected internal int frameSize;

		/// <summary>
		/// Size of sub-frames
		/// </summary>
		protected internal int subframeSize;

		/// <summary>
		/// Number of sub-frames
		/// </summary>
		protected internal int nbSubframes;

		/// <summary>
		/// Analysis (LPC) window length
		/// </summary>
		protected internal int windowSize;

		/// <summary>
		/// LPC order
		/// </summary>
		protected internal int lpcSize;

		/// <summary>
		/// Buffer size
		/// </summary>
		protected internal int bufSize;

		/// <summary>
		/// Minimum pitch value allowed
		/// </summary>
		protected internal int min_pitch;

		/// <summary>
		/// Maximum pitch value allowed
		/// </summary>
		protected internal int max_pitch;

		/// <summary>
		/// Perceptual filter: A(z/gamma1)
		/// </summary>
		protected internal float gamma1;

		/// <summary>
		/// Perceptual filter: A(z/gamma2)
		/// </summary>
		protected internal float gamma2;

		/// <summary>
		/// Lag windowing Gaussian width
		/// </summary>
		protected internal float lag_factor;

		/// <summary>
		/// Noise floor multiplier for A[0] in LPC analysis
		/// </summary>
		protected internal float lpc_floor;

		/// <summary>
		/// Pre-emphasis: P(z) = 1 - a*z^-1
		/// </summary>
		protected internal float preemph;

		/// <summary>
		/// 1-element memory for pre-emphasis
		/// </summary>
		protected internal float pre_mem;

		#endregion

		#region Variables

		/// <summary>
		/// Input buffer (original signal)
		/// </summary>
		protected internal float[] frmBuf;

		protected internal int frmIdx;

		/// <summary>
		/// Excitation buffer
		/// </summary>
		protected internal float[] excBuf;

		/// <summary>
		/// Start of excitation frame
		/// </summary>
		protected internal int excIdx;

		/// <summary>
		/// Innovation for the frame
		/// </summary>
		protected internal float[] innov;

		/// <summary>
		/// LPCs for current frame
		/// </summary>
		protected internal float[] lpc;

		/// <summary>
		/// Quantized LSPs for current frame
		/// </summary>
		protected internal float[] qlsp;

		/// <summary>
		/// Quantized LSPs for previous frame
		/// </summary>
		protected internal float[] old_qlsp;

		/// <summary>
		/// Interpolated quantized LSPs
		/// </summary>
		protected internal float[] interp_qlsp;

		/// <summary>
		/// Interpolated quantized LPCs
		/// </summary>
		protected internal float[] interp_qlpc;

		/// <summary>
		/// Filter memory for synthesis signal
		/// </summary>
		protected internal float[] mem_sp;

		/// <summary>
		/// Gain of LPC filter at theta=pi (fe/2)
		/// </summary>
		protected internal float[] pi_gain;
		protected internal float[] awk1, awk2, awk3;

		// Vocoder data
		protected internal float voc_m1;
		protected internal float voc_m2;
		protected internal float voc_mean;
		protected internal int voc_offset;

		/** 1 for enabling DTX, 0 otherwise */
		protected internal int dtx_enabled;

		#endregion

		public NbCodec()
		{
			m_lsp = new Lsp();
			filters = new Filters();
			Nbinit();
		}

		/// <summary>
		/// Narrowband initialisation.
		/// </summary>
		private void Nbinit()
		{
			// Initialize SubModes
			submodes = BuildNbSubModes();
			submodeID = 5;
			// Initialize narrwoband parameters and variables
			Init(160, 40, 10, 640);
		}

		/// <summary>
		/// Initialisation.
		/// </summary>
		protected virtual void Init(
			int frameSize, int subframeSize,
			int lpcSize, int bufSize)
		{
			first = 1;
			// Codec parameters, should eventually have several "modes"
			this.frameSize = frameSize;
			this.windowSize = frameSize * 3 / 2;
			this.subframeSize = subframeSize;
			this.nbSubframes = frameSize / subframeSize;
			this.lpcSize = lpcSize;
			this.bufSize = bufSize;
			min_pitch = 17;
			max_pitch = 144;
			preemph = 0.0f;
			pre_mem = 0.0f;
			gamma1 = 0.9f;
			gamma2 = 0.6f;
			lag_factor = .01f;
			lpc_floor = 1.0001f;

			frmBuf = new float[bufSize];
			frmIdx = bufSize - windowSize;
			excBuf = new float[bufSize];
			excIdx = bufSize - windowSize;
			innov = new float[frameSize];

			lpc = new float[lpcSize + 1];
			qlsp = new float[lpcSize];
			old_qlsp = new float[lpcSize];
			interp_qlsp = new float[lpcSize];
			interp_qlpc = new float[lpcSize + 1];
			mem_sp = new float[5 * lpcSize]; // TODO - check why 5 (why not 2 or 1)
			pi_gain = new float[nbSubframes];

			awk1 = new float[lpcSize + 1];
			awk2 = new float[lpcSize + 1];
			awk3 = new float[lpcSize + 1];

			voc_m1 = voc_m2 = voc_mean = 0;
			voc_offset = 0;
			dtx_enabled = 0; // disabled by default
		}

		/// <summary>
		/// Build narrowband submodes
		/// </summary>
		private static SubMode[] BuildNbSubModes()
		{
			// Initialize Long Term Predictions
			Ltp3Tap ltpNb = new Ltp3Tap(NSpeex.Codebook_Constants.gain_cdbk_nb, 7, 7);
			Ltp3Tap ltpVlbr = new Ltp3Tap(NSpeex.Codebook_Constants.gain_cdbk_lbr, 5, 0);
			Ltp3Tap ltpLbr = new Ltp3Tap(NSpeex.Codebook_Constants.gain_cdbk_lbr, 5, 7);
			Ltp3Tap ltpMed = new Ltp3Tap(NSpeex.Codebook_Constants.gain_cdbk_lbr, 5, 7);
			LtpForcedPitch ltpFP = new LtpForcedPitch();
			// Initialize Codebook Searches
			NoiseSearch noiseSearch = new NoiseSearch();
			SplitShapeSearch ssNbVlbrSearch = new SplitShapeSearch(40, 10, 4, NSpeex.Codebook_Constants.exc_10_16_table, 4, 0);
			SplitShapeSearch ssNbLbrSearch = new SplitShapeSearch(40, 10, 4, NSpeex.Codebook_Constants.exc_10_32_table, 5, 0);
			SplitShapeSearch ssNbSearch = new SplitShapeSearch(40, 5, 8, NSpeex.Codebook_Constants.exc_5_64_table, 6, 0);
			SplitShapeSearch ssNbMedSearch = new SplitShapeSearch(40, 8, 5, NSpeex.Codebook_Constants.exc_8_128_table, 7, 0);
			SplitShapeSearch ssSbSearch = new SplitShapeSearch(40, 5, 8, NSpeex.Codebook_Constants.exc_5_256_table, 8, 0);
			SplitShapeSearch ssNbUlbrSearch = new SplitShapeSearch(40, 20, 2, NSpeex.Codebook_Constants.exc_20_32_table, 5, 0);
			// Initialize Line Spectral Pair Quantizers
			NbLspQuant nbLspQuant = new NbLspQuant();
			LbrLspQuant lbrLspQuant = new LbrLspQuant();
			// Initialize narrow-band modes
			SubMode[] nbSubModes = new SubMode[NB_SUBMODES];
			// 2150 bps "vocoder-like" mode for comfort noise
			nbSubModes[1] = new SubMode(0, 1, 0, 0, lbrLspQuant, ltpFP, noiseSearch, .7f, .7f, -1, 43);
			// 5.95 kbps very low bit-rate mode
			nbSubModes[2] = new SubMode(0, 0, 0, 0, lbrLspQuant, ltpVlbr, ssNbVlbrSearch, 0.7f, 0.5f, .55f, 119);
			// 8 kbps low bit-rate mode
			nbSubModes[3] = new SubMode(-1, 0, 1, 0, lbrLspQuant, ltpLbr, ssNbLbrSearch, 0.7f, 0.55f, .45f, 160);
			// 11 kbps medium bit-rate mode
			nbSubModes[4] = new SubMode(-1, 0, 1, 0, lbrLspQuant, ltpMed, ssNbMedSearch, 0.7f, 0.63f, .35f, 220);
			// 15 kbps high bit-rate mode
			nbSubModes[5] = new SubMode(-1, 0, 3, 0, nbLspQuant, ltpNb, ssNbSearch, 0.7f, 0.65f, .25f, 300);
			// 18.2 high bit-rate mode
			nbSubModes[6] = new SubMode(-1, 0, 3, 0, nbLspQuant, ltpNb, ssSbSearch, 0.68f, 0.65f, .1f, 364);
			// 24.6 kbps high bit-rate mode
			nbSubModes[7] = new SubMode(-1, 0, 3, 1, nbLspQuant, ltpNb, ssNbSearch, 0.65f, 0.65f, -1, 492);
			// 3.95 kbps very low bit-rate mode
			nbSubModes[8] = new SubMode(0, 1, 0, 0, lbrLspQuant, ltpFP, ssNbUlbrSearch, .7f, .5f, .65f, 79);
			// Return the Narrowband SubModes
			return nbSubModes;
		}

		public virtual int FrameSize
		{
			get
			{
				return frameSize;
			}
		}

		public float[] PiGain
		{
			get
			{
				return pi_gain;
			}
		}

		public virtual float[] Exc
		{
			get
			{
				float[] excTmp = new float[frameSize];
				System.Array.Copy(excBuf, excIdx, excTmp, 0, frameSize);
				return excTmp;
			}
		}

		public virtual float[] Innov
		{
			get
			{
				return innov;
			}
		}
	}
}
