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
	/// Narrowband Speex Encoder
	/// </summary>
    internal class NbEncoder : NbCodec, IEncoder
	{
		/// <summary>
		/// The Narrowband Quality map indicates which narrowband submode to use for
		/// the given narrowband quality setting
		/// </summary>
		public static readonly int[] NB_QUALITY_MAP = { 1, 8, 2, 3, 3, 4, 4, 5, 5, 6, 7 };

		/// <summary>
		/// Next frame should not rely on previous frames for pitch
		/// </summary>
		private int bounded_pitch;
		private int[] pitch;

		/// <summary>
		/// 1-element memory for pre-emphasis
		/// </summary>
		private float pre_mem2;

		/// <summary>
		/// "Pitch enhanced" excitation
		/// </summary>
		private float[] exc2Buf;

		/// <summary>
		/// "Pitch enhanced" excitation
		/// </summary>
		private int exc2Idx;

		/// <summary>
		/// Weighted signal buffer
		/// </summary>
		private float[] swBuf;

		/// <summary>
		/// Start of weighted signal frame
		/// </summary>
		private int swIdx;

		/// <summary>
		/// Temporary (Hanning) window
		/// </summary>
		private float[] window;

		/// <summary>
		/// 2nd temporary buffer
		/// </summary>
		private float[] buf2;

		/// <summary>
		/// auto-correlation
		/// </summary>
		private float[] autocorr;

		/// <summary>
		/// Window applied to auto-correlation
		/// </summary>
		private float[] lagWindow;

		/// <summary>
		/// LSPs for current frame
		/// </summary>
		private float[] lsp;

		/// <summary>
		/// LSPs for previous frame
		/// </summary>
		private float[] old_lsp;

		/// <summary>
		/// Interpolated LSPs
		/// </summary>
		private float[] interp_lsp;

		/// <summary>
		/// Interpolated LPCs
		/// </summary>
		private float[] interp_lpc;

		/// <summary>
		/// LPCs after bandwidth expansion by gamma1 for perceptual weighting
		/// </summary>
		private float[] bw_lpc1;

		/// <summary>
		/// LPCs after bandwidth expansion by gamma2 for perceptual weighting
		/// </summary>
		private float[] bw_lpc2;

		/// <summary>
		/// Reflection coefficients
		/// </summary>
		private float[] rc;

		/// <summary>
		/// Filter memory for perceptually-weighted signal
		/// </summary>
		private float[] mem_sw;

		/// <summary>
		/// Filter memory for perceptually-weighted signal (whole frame)
		/// </summary>
		private float[] mem_sw_whole;

		/// <summary>
		/// Filter memory for excitation (whole frame)
		/// </summary>
		private float[] mem_exc;

		/// <summary>
		/// State of the VBR data
		/// </summary>
		private Vbr vbr;

		/// <summary>
		/// Number of consecutive DTX frames
		/// </summary>
		private int dtx_count;

		private float[] innov2;

		/// <summary>
		/// Complexity setting (0-10 from least complex to most complex)
		/// </summary>
		protected internal int complexity;

		/// <summary>
		/// 1 for enabling VBR, 0 otherwise
		/// </summary>
		protected internal int vbr_enabled;

		/// <summary>
		/// 1 for enabling VAD, 0 otherwise
		/// </summary>
		protected internal int vad_enabled;

		/// <summary>
		/// ABR setting (in bps), 0 if off
		/// </summary>
		protected internal int abr_enabled;

		/// <summary>
		/// Quality setting for VBR encoding
		/// </summary>
		protected internal float vbr_quality;

		/// <summary>
		/// Relative quality that will be needed by VBR
		/// </summary>
		protected internal float relative_quality;

		protected internal float abr_drift;
		protected internal float abr_drift2;
		protected internal float abr_count;
		protected internal int sampling_rate;

		/// <summary>
		/// Mode chosen by the user (may differ from submodeID if VAD is on)
		/// </summary>
		protected internal int submodeSelect;

		/// <summary>
		/// Initialisation
		/// </summary>
		protected override void Init(int frameSize, int subframeSize, int lpcSize, int bufSize)
		{
			base.Init(frameSize, subframeSize, lpcSize, bufSize);

			complexity = 3; // in C it's 2 here, but set to 3 automatically by the
			// encoder
			vbr_enabled = 0; // disabled by default
			vad_enabled = 0; // disabled by default
			abr_enabled = 0; // disabled by default
			vbr_quality = 8;

			submodeSelect = 5;
			pre_mem2 = 0;
			bounded_pitch = 1;

			exc2Buf = new float[bufSize];
			exc2Idx = bufSize - windowSize;
			swBuf = new float[bufSize];
			swIdx = bufSize - windowSize;

			window = NSpeex.Misc.Window(windowSize, subframeSize);
			lagWindow = NSpeex.Misc.LagWindow(lpcSize, lag_factor);

			autocorr = new float[lpcSize + 1];
			buf2 = new float[windowSize];

			interp_lpc = new float[lpcSize + 1];
			interp_qlpc = new float[lpcSize + 1];
			bw_lpc1 = new float[lpcSize + 1];
			bw_lpc2 = new float[lpcSize + 1];
			lsp = new float[lpcSize];
			qlsp = new float[lpcSize];
			old_lsp = new float[lpcSize];
			old_qlsp = new float[lpcSize];
			interp_lsp = new float[lpcSize];
			interp_qlsp = new float[lpcSize];

			rc = new float[lpcSize];
			mem_sp = new float[lpcSize]; // why was there a *5 before ?!?
			mem_sw = new float[lpcSize];
			mem_sw_whole = new float[lpcSize];
			mem_exc = new float[lpcSize];

			vbr = new Vbr();
			dtx_count = 0;
			abr_count = 0;
			sampling_rate = 8000;

			awk1 = new float[lpcSize + 1];
			awk2 = new float[lpcSize + 1];
			awk3 = new float[lpcSize + 1];
			innov2 = new float[40];

			pitch = new int[nbSubframes];
		}

		/// <summary>
		/// Encode the given input signal.
		/// </summary>
		/// <returns>return 1 if successful.</returns>
		public virtual int Encode(Bits bits, float[] ins0)
		{
			int i;
			float[] res, target, mem;
			float[] syn_resp;
			float[] orig;

			/* Copy new data in input buffer */
			System.Array.Copy(frmBuf, frameSize, frmBuf, 0, bufSize
							- frameSize);
			frmBuf[bufSize - frameSize] = ins0[0] - preemph * pre_mem;
			for (i = 1; i < frameSize; i++)
				frmBuf[bufSize - frameSize + i] = ins0[i] - preemph * ins0[i - 1];
			pre_mem = ins0[frameSize - 1];

			/* Move signals 1 frame towards the past */
			System.Array.Copy(exc2Buf, frameSize, exc2Buf, 0, bufSize
							- frameSize);
			System.Array.Copy(excBuf, frameSize, excBuf, 0, bufSize
							- frameSize);
			System.Array.Copy(swBuf, frameSize, swBuf, 0, bufSize
							- frameSize);

			/* Window for analysis */
			for (i = 0; i < windowSize; i++)
				buf2[i] = frmBuf[i + frmIdx] * window[i];

			/* Compute auto-correlation */
			NSpeex.Lpc.Autocorr(buf2, autocorr, lpcSize + 1, windowSize);

			autocorr[0] += 10; /* prevents NANs */
			autocorr[0] *= lpc_floor; /* Noise floor in auto-correlation domain */

			/* Lag windowing: equivalent to filtering in the power-spectrum domain */
			for (i = 0; i < lpcSize + 1; i++)
				autocorr[i] *= lagWindow[i];

			/* Levinson-Durbin */
			NSpeex.Lpc.Wld(lpc, autocorr, rc, lpcSize); // tmperr
			System.Array.Copy(lpc, 0, lpc, 1, lpcSize);
			lpc[0] = 1;

			/* LPC to LSPs (x-domain) transform */
			int roots = NSpeex.Lsp.Lpc2lsp(lpc, lpcSize, lsp, 15, 0.2f);
			/* Check if we found all the roots */
			if (roots == lpcSize)
			{
				/* LSP x-domain to angle domain */
				for (i = 0; i < lpcSize; i++)
					lsp[i] = (float)System.Math.Acos(lsp[i]);
			}
			else
			{
				/* Search again if we can afford it */
				if (complexity > 1)
					roots = NSpeex.Lsp.Lpc2lsp(lpc, lpcSize, lsp, 11, 0.05f);
				if (roots == lpcSize)
				{
					/* LSP x-domain to angle domain */
					for (i = 0; i < lpcSize; i++)
						lsp[i] = (float)System.Math.Acos(lsp[i]);
				}
				else
				{
					/*
					 * If we can't find all LSP's, do some damage control and use
					 * previous filter
					 */
					for (i = 0; i < lpcSize; i++)
					{
						lsp[i] = old_lsp[i];
					}
				}
			}

			float lsp_dist = 0;
			for (i = 0; i < lpcSize; i++)
				lsp_dist += (old_lsp[i] - lsp[i]) * (old_lsp[i] - lsp[i]);

			/*
			 * Whole frame analysis (open-loop estimation of pitch and excitation
			 * gain)
			 */
			float ol_gain;
			int ol_pitch;
			float ol_pitch_coef;
			{
				if (first != 0)
					for (i = 0; i < lpcSize; i++)
						interp_lsp[i] = lsp[i];
				else
					for (i = 0; i < lpcSize; i++)
						interp_lsp[i] = .375f * old_lsp[i] + .625f * lsp[i];

				NSpeex.Lsp.Enforce_margin(interp_lsp, lpcSize, .002f);

				/* Compute interpolated LPCs (unquantized) for whole frame */
				for (i = 0; i < lpcSize; i++)
					interp_lsp[i] = (float)System.Math.Cos(interp_lsp[i]);
				m_lsp.Lsp2lpc(interp_lsp, interp_lpc, lpcSize);

				/* Open-loop pitch */
				if (submodes[submodeID] == null || vbr_enabled != 0
						|| vad_enabled != 0
						|| submodes[submodeID].ForcedPitchGain != 0
						|| submodes[submodeID].LbrPitch != -1)
				{
					int[] nol_pitch = new int[6];
					float[] nol_pitch_coef = new float[6];

					NSpeex.Filters.Bw_lpc(gamma1, interp_lpc, bw_lpc1, lpcSize);
					NSpeex.Filters.Bw_lpc(gamma2, interp_lpc, bw_lpc2, lpcSize);

					NSpeex.Filters.Filter_mem2(frmBuf, frmIdx, bw_lpc1, bw_lpc2,
							swBuf, swIdx, frameSize, lpcSize, mem_sw_whole, 0);

					NSpeex.Ltp.Open_loop_nbest_pitch(swBuf, swIdx, min_pitch,
							max_pitch, frameSize, nol_pitch, nol_pitch_coef, 6);
					ol_pitch = nol_pitch[0];
					ol_pitch_coef = nol_pitch_coef[0];
					/* Try to remove pitch multiples */
					for (i = 1; i < 6; i++)
					{
						if ((nol_pitch_coef[i] > .85d * ol_pitch_coef)
								&& (Math.Abs(nol_pitch[i] - ol_pitch
																	/ 2.0d) <= 1
										|| Math.Abs(nol_pitch[i]
																					- ol_pitch / 3.0d) <= 1
										|| Math.Abs(nol_pitch[i]
																					- ol_pitch / 4.0d) <= 1 || Math.Abs(nol_pitch[i] - ol_pitch / 5.0d) <= 1))
						{
							/* ol_pitch_coef=nol_pitch_coef[i]; */
							ol_pitch = nol_pitch[i];
						}
					}
					/*
					 * if (ol_pitch>50) ol_pitch/=2;
					 */
					/* ol_pitch_coef = sqrt(ol_pitch_coef); */
				}
				else
				{
					ol_pitch = 0;
					ol_pitch_coef = 0;
				}
				/* Compute "real" excitation */
				NSpeex.Filters.Fir_mem2(frmBuf, frmIdx, interp_lpc, excBuf, excIdx,
						frameSize, lpcSize, mem_exc);

				/* Compute open-loop excitation gain */
				ol_gain = 0;
				for (i = 0; i < frameSize; i++)
					ol_gain += excBuf[excIdx + i] * excBuf[excIdx + i];

				ol_gain = (float)Math.Sqrt(1 + ol_gain / frameSize);
			}

			/* VBR stuff */
			if (vbr != null && (vbr_enabled != 0 || vad_enabled != 0))
			{
				if (abr_enabled != 0)
				{
					float qual_change = 0;
					if (abr_drift2 * abr_drift > 0)
					{
						/*
						 * Only adapt if long-term and short-term drift are the same
						 * sign
						 */
						qual_change = -.00001f * abr_drift / (1 + abr_count);
						if (qual_change > .05f)
							qual_change = .05f;
						if (qual_change < -.05f)
							qual_change = -.05f;
					}
					vbr_quality += qual_change;
					if (vbr_quality > 10)
						vbr_quality = 10;
					if (vbr_quality < 0)
						vbr_quality = 0;
				}
				relative_quality = vbr.Analysis(ins0, frameSize, ol_pitch,
						ol_pitch_coef);
				/* if (delta_qual<0) */
				/* delta_qual*=.1*(3+st->vbr_quality); */
				if (vbr_enabled != 0)
				{
					int mode;
					int choice = 0;
					float min_diff = 100;
					mode = 8;
					while (mode > 0)
					{
						int v1;
						float thresh;
						v1 = (int)Math.Floor(vbr_quality);
						if (v1 == 10)
							thresh = NSpeex.Vbr.nb_thresh[mode][v1];
						else
							thresh = (vbr_quality - v1)
									* NSpeex.Vbr.nb_thresh[mode][v1 + 1]
									+ (1 + v1 - vbr_quality)
									* NSpeex.Vbr.nb_thresh[mode][v1];
						if (relative_quality > thresh
								&& relative_quality - thresh < min_diff)
						{
							choice = mode;
							min_diff = relative_quality - thresh;
						}
						mode--;
					}
					mode = choice;
					if (mode == 0)
					{
						if (dtx_count == 0 || lsp_dist > .05d || dtx_enabled == 0
								|| dtx_count > 20)
						{
							mode = 1;
							dtx_count = 1;
						}
						else
						{
							mode = 0;
							dtx_count++;
						}
					}
					else
					{
						dtx_count = 0;
					}
					Mode = mode;

					if (abr_enabled != 0)
					{
						int bitrate;
						bitrate = BitRate;
						abr_drift += (bitrate - abr_enabled);
						abr_drift2 = .95f * abr_drift2 + .05f
								* (bitrate - abr_enabled);
						abr_count += ((Single?)1.0d).Value;
					}

				}
				else
				{
					/* VAD only case */
					int mode_0;
					if (relative_quality < 2)
					{
						if (dtx_count == 0 || lsp_dist > .05d || dtx_enabled == 0
								|| dtx_count > 20)
						{
							dtx_count = 1;
							mode_0 = 1;
						}
						else
						{
							mode_0 = 0;
							dtx_count++;
						}
					}
					else
					{
						dtx_count = 0;
						mode_0 = submodeSelect;
					}
					/* speex_encoder_ctl(state, SPEEX_SET_MODE, &mode); */
					submodeID = mode_0;
				}
			}
			else
			{
				relative_quality = -1;
			}

			/* First, transmit a zero for narrowband */
			bits.Pack(0, 1);

			/* Transmit the sub-mode we use for this frame */
			bits.Pack(submodeID, NSpeex.NbCodec.NB_SUBMODE_BITS);

			/* If null mode (no transmission), just set a couple things to zero */
			if (submodes[submodeID] == null)
			{
				for (i = 0; i < frameSize; i++)
					excBuf[excIdx + i] = exc2Buf[exc2Idx + i] = swBuf[swIdx + i] = NSpeex.NbCodec.VERY_SMALL;

				for (i = 0; i < lpcSize; i++)
					mem_sw[i] = 0;
				first = 1;
				bounded_pitch = 1;

				/* Final signal synthesis from excitation */
				NSpeex.Filters.Iir_mem2(excBuf, excIdx, interp_qlpc, frmBuf, frmIdx,
						frameSize, lpcSize, mem_sp);

				ins0[0] = frmBuf[frmIdx] + preemph * pre_mem2;
				for (i = 1; i < frameSize; i++)
					ins0[i] = frmBuf[frmIdx = i] + preemph * ins0[i - 1];
				pre_mem2 = ins0[frameSize - 1];

				return 0;
			}

			/* LSP Quantization */
			if (first != 0)
			{
				for (i = 0; i < lpcSize; i++)
					old_lsp[i] = lsp[i];
			}

			/* Quantize LSPs */
			// #if 1 /*0 for unquantized*/
			submodes[submodeID].LsqQuant.Quant(lsp, qlsp, lpcSize, bits);
			// #else
			// for (i=0;i<lpcSize;i++)
			// qlsp[i]=lsp[i];
			// #endif

			/* If we use low bit-rate pitch mode, transmit open-loop pitch */
			if (submodes[submodeID].LbrPitch != -1)
			{
				bits.Pack(ol_pitch - min_pitch, 7);
			}

			if (submodes[submodeID].ForcedPitchGain != 0)
			{
				int quant;
				quant = (int)Math.Floor(.5d + 15 * ol_pitch_coef);
				if (quant > 15)
					quant = 15;
				if (quant < 0)
					quant = 0;
				bits.Pack(quant, 4);
				ol_pitch_coef = (float)0.066667d * quant;
			}

			/* Quantize and transmit open-loop excitation gain */
			{
				int qe = (int)(Math.Floor(0.5d + 3.5d * Math.Log(ol_gain)));
				if (qe < 0)
					qe = 0;
				if (qe > 31)
					qe = 31;
				ol_gain = (float)Math.Exp(qe / 3.5d);
				bits.Pack(qe, 5);
			}

			/* Special case for first frame */
			if (first != 0)
			{
				for (i = 0; i < lpcSize; i++)
					old_qlsp[i] = qlsp[i];
			}

			/* Filter response */
			res = new float[subframeSize];
			/* Target signal */
			target = new float[subframeSize];
			syn_resp = new float[subframeSize];
			mem = new float[lpcSize];
			orig = new float[frameSize];
			for (i = 0; i < frameSize; i++)
				orig[i] = frmBuf[frmIdx + i];

			/* Loop on sub-frames */
			for (int sub = 0; sub < nbSubframes; sub++)
			{
				float tmp;
				int offset;
				int sp, sw, exc, exc2;
				int pitchval;

				/* Offset relative to start of frame */
				offset = subframeSize * sub;
				/* Original signal */
				sp = frmIdx + offset;
				/* Excitation */
				exc = excIdx + offset;
				/* Weighted signal */
				sw = swIdx + offset;

				exc2 = exc2Idx + offset;

				/* LSP interpolation (quantized and unquantized) */
				tmp = (float)(1.0d + sub) / nbSubframes;
				for (i = 0; i < lpcSize; i++)
					interp_lsp[i] = (1 - tmp) * old_lsp[i] + tmp * lsp[i];
				for (i = 0; i < lpcSize; i++)
					interp_qlsp[i] = (1 - tmp) * old_qlsp[i] + tmp * qlsp[i];

				/* Make sure the filters are stable */
				NSpeex.Lsp.Enforce_margin(interp_lsp, lpcSize, .002f);
				NSpeex.Lsp.Enforce_margin(interp_qlsp, lpcSize, .002f);

				/* Compute interpolated LPCs (quantized and unquantized) */
				for (i = 0; i < lpcSize; i++)
					interp_lsp[i] = (float)System.Math.Cos(interp_lsp[i]);
				m_lsp.Lsp2lpc(interp_lsp, interp_lpc, lpcSize);

				for (i = 0; i < lpcSize; i++)
					interp_qlsp[i] = (float)System.Math.Cos(interp_qlsp[i]);
				m_lsp.Lsp2lpc(interp_qlsp, interp_qlpc, lpcSize);

				/* Compute analysis filter gain at w=pi (for use in SB-CELP) */
				tmp = 1;
				pi_gain[sub] = 0;
				for (i = 0; i <= lpcSize; i++)
				{
					pi_gain[sub] += tmp * interp_qlpc[i];
					tmp = -tmp;
				}

				/*
				 * Compute bandwidth-expanded (unquantized) LPCs for perceptual
				 * weighting
				 */
				NSpeex.Filters.Bw_lpc(gamma1, interp_lpc, bw_lpc1, lpcSize);
				if (gamma2 >= 0)
					NSpeex.Filters.Bw_lpc(gamma2, interp_lpc, bw_lpc2, lpcSize);
				else
				{
					bw_lpc2[0] = 1;
					bw_lpc2[1] = -preemph;
					for (i = 2; i <= lpcSize; i++)
						bw_lpc2[i] = 0;
				}

				/* Compute impulse response of A(z/g1) / ( A(z)*A(z/g2) ) */
				for (i = 0; i < subframeSize; i++)
					excBuf[exc + i] = 0;
				excBuf[exc] = 1;
				NSpeex.Filters.Syn_percep_zero(excBuf, exc, interp_qlpc, bw_lpc1,
						bw_lpc2, syn_resp, subframeSize, lpcSize);

				/* Reset excitation */
				for (i = 0; i < subframeSize; i++)
					excBuf[exc + i] = 0;
				for (i = 0; i < subframeSize; i++)
					exc2Buf[exc2 + i] = 0;

				/* Compute zero response of A(z/g1) / ( A(z/g2) * A(z) ) */
				for (i = 0; i < lpcSize; i++)
					mem[i] = mem_sp[i];
				NSpeex.Filters.Iir_mem2(excBuf, exc, interp_qlpc, excBuf, exc,
						subframeSize, lpcSize, mem);

				for (i = 0; i < lpcSize; i++)
					mem[i] = mem_sw[i];
				NSpeex.Filters.Filter_mem2(excBuf, exc, bw_lpc1, bw_lpc2, res, 0,
						subframeSize, lpcSize, mem, 0);

				/* Compute weighted signal */
				for (i = 0; i < lpcSize; i++)
					mem[i] = mem_sw[i];
				NSpeex.Filters.Filter_mem2(frmBuf, sp, bw_lpc1, bw_lpc2, swBuf, sw,
						subframeSize, lpcSize, mem, 0);

				/* Compute target signal */
				for (i = 0; i < subframeSize; i++)
					target[i] = swBuf[sw + i] - res[i];

				for (i = 0; i < subframeSize; i++)
					excBuf[exc + i] = exc2Buf[exc2 + i] = 0;

				/* If we have a long-term predictor (otherwise, something's wrong) */
				// if (submodes[submodeID].ltp.quant)
				// {
				int pit_min, pit_max;
				/* Long-term prediction */
				if (submodes[submodeID].LbrPitch != -1)
				{
					/* Low bit-rate pitch handling */
					int margin;
					margin = submodes[submodeID].LbrPitch;
					if (margin != 0)
					{
						if (ol_pitch < min_pitch + margin - 1)
							ol_pitch = min_pitch + margin - 1;
						if (ol_pitch > max_pitch - margin)
							ol_pitch = max_pitch - margin;
						pit_min = ol_pitch - margin + 1;
						pit_max = ol_pitch + margin;
					}
					else
					{
						pit_min = pit_max = ol_pitch;
					}
				}
				else
				{
					pit_min = min_pitch;
					pit_max = max_pitch;
				}

				/* Force pitch to use only the current frame if needed */
				if (bounded_pitch != 0 && pit_max > offset)
					pit_max = offset;

				/* Perform pitch search */
				pitchval = submodes[submodeID].Ltp.Quant(target, swBuf, sw,
						interp_qlpc, bw_lpc1, bw_lpc2, excBuf, exc, pit_min,
						pit_max, ol_pitch_coef, lpcSize, subframeSize, bits,
						exc2Buf, exc2, syn_resp, complexity);

				pitch[sub] = pitchval;

				// } else {
				// speex_error ("No pitch prediction, what's wrong");
				// }

				/* Update target for adaptive codebook contribution */
				NSpeex.Filters.Syn_percep_zero(excBuf, exc, interp_qlpc, bw_lpc1,
						bw_lpc2, res, subframeSize, lpcSize);
				for (i = 0; i < subframeSize; i++)
					target[i] -= res[i];

				/* Quantization of innovation */
				{
					int innovptr;
					float ener = 0, ener_1;

					innovptr = sub * subframeSize;
					for (i = 0; i < subframeSize; i++)
						innov[innovptr + i] = 0;

					NSpeex.Filters.Residue_percep_zero(target, 0, interp_qlpc,
							bw_lpc1, bw_lpc2, buf2, subframeSize, lpcSize);
					for (i = 0; i < subframeSize; i++)
						ener += buf2[i] * buf2[i];
					ener = (float)Math.Sqrt(.1f + ener / subframeSize);
					/*
					 * for (i=0;i<subframeSize;i++) System.out.print(buf2[i]/ener +
					 * "\t");
					 */

					ener /= ol_gain;

					/* Calculate gain correction for the sub-frame (if any) */
					if (submodes[submodeID].HaveSubframeGain != 0)
					{
						int qe_1;
						ener = (float)Math.Log(ener);
						if (submodes[submodeID].HaveSubframeGain == 3)
						{
							qe_1 = NSpeex.VQ.Index(ener,
									NSpeex.NbCodec.exc_gain_quant_scal3, 8);
							bits.Pack(qe_1, 3);
							ener = NSpeex.NbCodec.exc_gain_quant_scal3[qe_1];
						}
						else
						{
							qe_1 = NSpeex.VQ.Index(ener,
									NSpeex.NbCodec.exc_gain_quant_scal1, 2);
							bits.Pack(qe_1, 1);
							ener = NSpeex.NbCodec.exc_gain_quant_scal1[qe_1];
						}
						ener = (float)Math.Exp(ener);
					}
					else
					{
						ener = 1;
					}

					ener *= ol_gain;

					/* System.out.println(ener + " " + ol_gain); */

					ener_1 = 1 / ener;

					/* Normalize innovation */
					for (i = 0; i < subframeSize; i++)
						target[i] *= ener_1;

					/* Quantize innovation */
					// if (submodes[submodeID].innovation != null)
					// {
					/* Codebook search */
					submodes[submodeID].Innovation.Quantify(target, interp_qlpc,
							bw_lpc1, bw_lpc2, lpcSize, subframeSize, innov,
							innovptr, syn_resp, bits, complexity);

					/* De-normalize innovation and update excitation */
					for (i = 0; i < subframeSize; i++)
						innov[innovptr + i] *= ener;
					for (i = 0; i < subframeSize; i++)
						excBuf[exc + i] += innov[innovptr + i];
					// } else {
					// speex_error("No fixed codebook");
					// }

					/*
					 * In some (rare) modes, we do a second search (more bits) to
					 * reduce noise even more
					 */
					if (submodes[submodeID].DoubleCodebook != 0)
					{
						float[] innov2_2 = new float[subframeSize];
						// for (i=0;i<subframeSize;i++)
						// innov2[i]=0;
						for (i = 0; i < subframeSize; i++)
							target[i] *= 2.2f;
						submodes[submodeID].Innovation.Quantify(target, interp_qlpc,
								bw_lpc1, bw_lpc2, lpcSize, subframeSize, innov2_2,
								0, syn_resp, bits, complexity);
						for (i = 0; i < subframeSize; i++)
							innov2_2[i] *= (float)(ener * (1 / 2.2d));
						for (i = 0; i < subframeSize; i++)
							excBuf[exc + i] += innov2_2[i];
					}

					for (i = 0; i < subframeSize; i++)
						target[i] *= ener;
				}

				/* Keep the previous memory */
				for (i = 0; i < lpcSize; i++)
					mem[i] = mem_sp[i];
				/* Final signal synthesis from excitation */
				NSpeex.Filters.Iir_mem2(excBuf, exc, interp_qlpc, frmBuf, sp,
						subframeSize, lpcSize, mem_sp);

				/*
				 * Compute weighted signal again, from synthesized speech (not sure
				 * it's the right thing)
				 */
				NSpeex.Filters.Filter_mem2(frmBuf, sp, bw_lpc1, bw_lpc2, swBuf, sw,
						subframeSize, lpcSize, mem_sw, 0);
				for (i = 0; i < subframeSize; i++)
					exc2Buf[exc2 + i] = excBuf[exc + i];
			}

			/* Store the LSPs for interpolation in the next frame */
			if (submodeID >= 1)
			{
				for (i = 0; i < lpcSize; i++)
					old_lsp[i] = lsp[i];
				for (i = 0; i < lpcSize; i++)
					old_qlsp[i] = qlsp[i];
			}

			if (submodeID == 1)
			{
				if (dtx_count != 0)
				{
					bits.Pack(15, 4);
				}
				else
				{
					bits.Pack(0, 4);
				}
			}

			/* The next frame will not be the first (Duh!) */
			first = 0;

			{
				float ener_3 = 0, err = 0;
				float snr;
				for (i = 0; i < frameSize; i++)
				{
					ener_3 += frmBuf[frmIdx + i] * frmBuf[frmIdx + i];
					err += (frmBuf[frmIdx + i] - orig[i])
							* (frmBuf[frmIdx + i] - orig[i]);
				}
				snr = (float)(10 * Math.Log((ener_3 + 1) / (err + 1)));
				/*
				 * System.out.println("Frame result: SNR="+snr+" E="+ener+"
				 * Err="+err+"\r\n");
				 */
			}

			/* Replace input by synthesized speech */
			ins0[0] = frmBuf[frmIdx] + preemph * pre_mem2;
			for (i = 1; i < frameSize; i++)
				ins0[i] = frmBuf[frmIdx + i] + preemph * ins0[i - 1];
			pre_mem2 = ins0[frameSize - 1];

			if (submodes[submodeID].Innovation is NoiseSearch || submodeID == 0)
				bounded_pitch = 1;
			else
				bounded_pitch = 0;

			return 1;
		}

		public virtual int EncodedFrameSize
		{
			get
			{
				return NSpeex.NbCodec.NB_FRAME_SIZE[submodeID];
			}
		}

		public virtual int Quality
		{
			set
			{
				if (value < 0)
					value = 0;
				if (value > 10)
					value = 10;
				submodeID = submodeSelect = NB_QUALITY_MAP[value];
			}
		}

		public virtual int BitRate
		{
			get
			{
				if (submodes[submodeID] != null)
					return sampling_rate * submodes[submodeID].BitsPerFrame
							/ frameSize;
				else
					return sampling_rate * (NSpeex.NbCodec.NB_SUBMODE_BITS + 1)
							/ frameSize;
			}
			set
			{
				for (int i = 10; i >= 0; i--)
				{
					Quality = i;
					if (BitRate <= value)
						return;
				}
			}
		}


		public virtual int Mode
		{
			get
			{
				return submodeID;
			}
			set
			{
				if (value < 0)
					value = 0;
				submodeID = submodeSelect = value;
			}
		}


		public virtual bool Vbr
		{
			get
			{
				return vbr_enabled != 0;
			}
			set
			{
				vbr_enabled = (value) ? 1 : 0;
			}
		}

		public virtual bool Vad
		{
			get
			{
				return vad_enabled != 0;
			}
			set
			{
				vad_enabled = (value) ? 1 : 0;
			}
		}

		public virtual bool Dtx
		{
			get
			{
				return dtx_enabled == 1;
			}
			set
			{
				dtx_enabled = (value) ? 1 : 0;
			}
		}

		public virtual int Abr
		{
			get
			{
				return abr_enabled;
			}
			set
			{
				abr_enabled = (value != 0) ? 1 : 0;
				vbr_enabled = 1;
				{
					int i = 10, rate, target;
					float vbr_qual;
					target = value;
					while (i >= 0)
					{
						Quality = i;
						rate = BitRate;
						if (rate <= target)
							break;
						i--;
					}
					vbr_qual = i;
					if (vbr_qual < 0)
						vbr_qual = 0;
					VbrQuality = vbr_qual;
					abr_count = 0;
					abr_drift = 0;
					abr_drift2 = 0;
				}
			}
		}

		public virtual float VbrQuality
		{
			get
			{
				return vbr_quality;
			}
			set
			{
				if (value < 0f)
					value = 0f;
				if (value > 10f)
					value = 10f;
				vbr_quality = value;
			}
		}

		public virtual int Complexity
		{
			get
			{
				return complexity;
			}
			set
			{
				if (value < 0)
					value = 0;
				if (value > 10)
					value = 10;
				this.complexity = value;
			}
		}

		public virtual int SamplingRate
		{
			get
			{
				return sampling_rate;
			}
			set
			{
				sampling_rate = value;
			}
		}

		public virtual int LookAhead
		{
			get
			{
				return windowSize - frameSize;
			}
		}

		public virtual float RelativeQuality
		{
			get
			{
				return relative_quality;
			}
		}
	}
}
