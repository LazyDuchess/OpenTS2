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
	/// Sideband Speex Decoder
	/// </summary>
    internal class SbDecoder : SbCodec, IDecoder
	{
		protected internal IDecoder lowdec;
		protected internal Stereo stereo;
		protected internal bool enhanced;

		private float[] innov2;

		/// <summary>
		/// Constructor
		/// </summary>
		public SbDecoder(bool ultraWide)
			: base(ultraWide)
		{
			stereo = new Stereo();
			enhanced = true;
			if (ultraWide)
				Uwbinit();
			else
				Wbinit();
		}

		/// <summary>
		/// Wideband initialisation
		/// </summary>
		private void Wbinit()
		{
			lowdec = new NbDecoder();
			lowdec.PerceptualEnhancement = enhanced;
			// Initialize variables
			Init(160, 40, 8, 640, .7f);
		}

		/// <summary>
		/// Ultra-wideband initialisation
		/// </summary>
		private void Uwbinit()
		{
			lowdec = new SbDecoder(false);
			lowdec.PerceptualEnhancement = enhanced;
			// Initialize variables
			Init(320, 80, 8, 1280, .5f);
		}

		protected override void Init(
			int frameSize, int subframeSize,
			int lpcSize, int bufSize, float foldingGain)
		{
			base.Init(frameSize, subframeSize, lpcSize, bufSize, foldingGain);
			excIdx = 0;
			innov2 = new float[subframeSize];
		}

		/// <summary>
		/// Decode the given input bits.
		/// </summary>
		/// <returns>1 if a terminator was found, 0 if not.</returns>
		public virtual int Decode(Bits bits, float[] xout)
		{
			int i, sub, wideband, ret;
			float[] low_pi_gain, low_exc, low_innov;

			/* Decode the low-band */
			ret = lowdec.Decode(bits, x0d);
			if (ret != 0)
			{
				return ret;
			}
			bool dtx = lowdec.Dtx;
			if (bits == null)
			{
				DecodeLost(xout, dtx);
				return 0;
			}
			/* Check "wideband bit" */
			wideband = bits.Peek();
			if (wideband != 0)
			{
				/* Regular wideband frame, read the submode */
				wideband = bits.Unpack(1);
				submodeID = bits.Unpack(3);
			}
			else
			{
				/* was a narrowband frame, set "null submode" */
				submodeID = 0;
			}

			for (i = 0; i < frameSize; i++)
				excBuf[i] = 0;

			/* If null mode (no transmission), just set a couple things to zero */
			if (submodes[submodeID] == null)
			{
				if (dtx)
				{
					DecodeLost(xout, true);
					return 0;
				}
				for (i = 0; i < frameSize; i++)
					excBuf[i] = NSpeex.NbCodec.VERY_SMALL;

				first = 1;
				/* Final signal synthesis from excitation */
				NSpeex.Filters.Iir_mem2(excBuf, excIdx, interp_qlpc, high, 0,
						frameSize, lpcSize, mem_sp);
				filters.Fir_mem_up(x0d, NSpeex.Codebook_Constants.h0, y0,
						fullFrameSize, NSpeex.SbCodec.QMF_ORDER, g0_mem);
				filters.Fir_mem_up(high, NSpeex.Codebook_Constants.h1, y1,
						fullFrameSize, NSpeex.SbCodec.QMF_ORDER, g1_mem);

				for (i = 0; i < fullFrameSize; i++)
					xout[i] = 2 * (y0[i] - y1[i]);
				return 0;
			}
			low_pi_gain = lowdec.PiGain;
			low_exc = lowdec.Exc;
			low_innov = lowdec.Innov;
			submodes[submodeID].LsqQuant.Unquant(qlsp, lpcSize, bits);

			if (first != 0)
			{
				for (i = 0; i < lpcSize; i++)
					old_qlsp[i] = qlsp[i];
			}

			for (sub = 0; sub < nbSubframes; sub++)
			{
				float tmp, filter_ratio, el = 0.0f, rl = 0.0f, rh = 0.0f;
				int subIdx = subframeSize * sub;

				/* LSP interpolation */
				tmp = (1.0f + sub) / nbSubframes;
				for (i = 0; i < lpcSize; i++)
					interp_qlsp[i] = (1 - tmp) * old_qlsp[i] + tmp * qlsp[i];

				NSpeex.Lsp.Enforce_margin(interp_qlsp, lpcSize, .05f);

				/* LSPs to x-domain */
				for (i = 0; i < lpcSize; i++)
					interp_qlsp[i] = (float)System.Math.Cos(interp_qlsp[i]);

				/* LSP to LPC */
				m_lsp.Lsp2lpc(interp_qlsp, interp_qlpc, lpcSize);

				if (enhanced)
				{
					float k1, k2, k3;
					k1 = submodes[submodeID].LpcEnhK1;
					k2 = submodes[submodeID].LpcEnhK2;
					k3 = k1 - k2;
					NSpeex.Filters.Bw_lpc(k1, interp_qlpc, awk1, lpcSize);
					NSpeex.Filters.Bw_lpc(k2, interp_qlpc, awk2, lpcSize);
					NSpeex.Filters.Bw_lpc(k3, interp_qlpc, awk3, lpcSize);
				}

				/*
				 * Calculate reponse ratio between low & high filter in band middle
				 * (4000 Hz)
				 */
				tmp = 1;
				pi_gain[sub] = 0;
				for (i = 0; i <= lpcSize; i++)
				{
					rh += tmp * interp_qlpc[i];
					tmp = -tmp;
					pi_gain[sub] += interp_qlpc[i];
				}
				rl = low_pi_gain[sub];
				rl = 1 / (Math.Abs(rl) + .01f);
				rh = 1 / (Math.Abs(rh) + .01f);
				filter_ratio = Math.Abs(.01f + rh)
						/ (.01f + Math.Abs(rl));

				/* reset excitation buffer */
				for (i = subIdx; i < subIdx + subframeSize; i++)
					excBuf[i] = 0;

				if (submodes[submodeID].Innovation == null)
				{
					float g;
					int quant;

					quant = bits.Unpack(5);
					g = (float)Math.Exp(((double)quant - 10) / 8.0d);
					g /= filter_ratio;

					/* High-band excitation using the low-band excitation and a gain */
					for (i = subIdx; i < subIdx + subframeSize; i++)
						excBuf[i] = foldingGain * g * low_innov[i];
				}
				else
				{
					float gc, scale;
					int qgc = bits.Unpack(4);

					for (i = subIdx; i < subIdx + subframeSize; i++)
						el += low_exc[i] * low_exc[i];

					gc = (float)Math.Exp((1 / 3.7f) * qgc - 2);
					scale = gc * (float)Math.Sqrt(1 + el) / filter_ratio;
					submodes[submodeID].Innovation.Unquantify(excBuf, subIdx,
							subframeSize, bits);

					for (i = subIdx; i < subIdx + subframeSize; i++)
						excBuf[i] *= scale;

					if (submodes[submodeID].DoubleCodebook != 0)
					{
						for (i = 0; i < subframeSize; i++)
							innov2[i] = 0;
						submodes[submodeID].Innovation.Unquantify(innov2, 0,
								subframeSize, bits);
						for (i = 0; i < subframeSize; i++)
							innov2[i] *= scale * (1 / 2.5f);
						for (i = 0; i < subframeSize; i++)
							excBuf[subIdx + i] += innov2[i];
					}
				}

				for (i = subIdx; i < subIdx + subframeSize; i++)
					high[i] = excBuf[i];

				if (enhanced)
				{
					/* Use enhanced LPC filter */
					NSpeex.Filters.Filter_mem2(high, subIdx, awk2, awk1,
							subframeSize, lpcSize, mem_sp, lpcSize);
					NSpeex.Filters.Filter_mem2(high, subIdx, awk3, interp_qlpc,
							subframeSize, lpcSize, mem_sp, 0);
				}
				else
				{
					/* Use regular filter */
					for (i = 0; i < lpcSize; i++)
						mem_sp[lpcSize + i] = 0;
					NSpeex.Filters.Iir_mem2(high, subIdx, interp_qlpc, high, subIdx,
							subframeSize, lpcSize, mem_sp);
				}
			}

			filters.Fir_mem_up(x0d, NSpeex.Codebook_Constants.h0, y0, fullFrameSize,
					NSpeex.SbCodec.QMF_ORDER, g0_mem);
			filters.Fir_mem_up(high, NSpeex.Codebook_Constants.h1, y1,
					fullFrameSize, NSpeex.SbCodec.QMF_ORDER, g1_mem);

			for (i = 0; i < fullFrameSize; i++)
				xout[i] = 2 * (y0[i] - y1[i]);

			for (i = 0; i < lpcSize; i++)
				old_qlsp[i] = qlsp[i];

			first = 0;
			return 0;
		}

		/// <summary>
		/// Decode when packets are lost.
		/// </summary>
		/// <returns>0 if successful.</returns>
		public int DecodeLost(float[] xout, bool dtx)
		{
			int i;
			int saved_modeid = 0;

			if (dtx)
			{
				saved_modeid = submodeID;
				submodeID = 1;
			}
			else
			{
				NSpeex.Filters.Bw_lpc(0.99f, interp_qlpc, interp_qlpc, lpcSize);
			}

			first = 1;
			awk1 = new float[lpcSize + 1];
			awk2 = new float[lpcSize + 1];
			awk3 = new float[lpcSize + 1];

			if (enhanced)
			{
				float k1, k2, k3;
				if (submodes[submodeID] != null)
				{
					k1 = submodes[submodeID].LpcEnhK1;
					k2 = submodes[submodeID].LpcEnhK2;
				}
				else
				{
					k1 = k2 = 0.7f;
				}
				k3 = k1 - k2;
				NSpeex.Filters.Bw_lpc(k1, interp_qlpc, awk1, lpcSize);
				NSpeex.Filters.Bw_lpc(k2, interp_qlpc, awk2, lpcSize);
				NSpeex.Filters.Bw_lpc(k3, interp_qlpc, awk3, lpcSize);
			}

			/* Final signal synthesis from excitation */
			if (!dtx)
			{
				for (i = 0; i < frameSize; i++)
					excBuf[excIdx + i] *= ((Single?).9d).Value;
			}
			for (i = 0; i < frameSize; i++)
				high[i] = excBuf[excIdx + i];

			if (enhanced)
			{
				/* Use enhanced LPC filter */
				NSpeex.Filters.Filter_mem2(high, 0, awk2, awk1, high, 0, frameSize,
						lpcSize, mem_sp, lpcSize);
				NSpeex.Filters.Filter_mem2(high, 0, awk3, interp_qlpc, high, 0,
						frameSize, lpcSize, mem_sp, 0);
			}
			else
			{ /* Use regular filter */
				for (i = 0; i < lpcSize; i++)
					mem_sp[lpcSize + i] = 0;
				NSpeex.Filters.Iir_mem2(high, 0, interp_qlpc, high, 0, frameSize,
						lpcSize, mem_sp);
			}
			/*
			 * iir_mem2(st->exc, st->interp_qlpc, st->high, st->frame_size,
			 * st->lpcSize, st->mem_sp);
			 */

			/* Reconstruct the original */
			filters.Fir_mem_up(x0d, NSpeex.Codebook_Constants.h0, y0, fullFrameSize,
					NSpeex.SbCodec.QMF_ORDER, g0_mem);
			filters.Fir_mem_up(high, NSpeex.Codebook_Constants.h1, y1,
					fullFrameSize, NSpeex.SbCodec.QMF_ORDER, g1_mem);
			for (i = 0; i < fullFrameSize; i++)
				xout[i] = 2 * (y0[i] - y1[i]);

			if (dtx)
			{
				submodeID = saved_modeid;
			}
			return 0;
		}

		/// <summary>
		/// Decode the given bits to stereo.
		/// </summary>
		public virtual void DecodeStereo(float[] data, int frameSize)
		{
			stereo.Decode(data, frameSize);
		}

		public virtual bool PerceptualEnhancement
		{
			get
			{
				return enhanced;
			}
			set
			{
				this.enhanced = value;
			}
		}
	}
}
