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
using System.IO;

namespace NSpeex
{
	/// <summary>
	/// Narrowband Speex Decoder
	/// </summary>
    internal class NbDecoder : NbCodec, IDecoder
	{
		private float[] innov2;

		/// <summary>
		/// Packet loss
		/// </summary> 
		private int count_lost;

		/// <summary>
		/// Pitch of last correctly decoded frame
		/// </summary>
		private int last_pitch;

		/// <summary>
		/// Pitch gain of last correctly decoded frame
		/// </summary>
		private float last_pitch_gain;

		/// <summary>
		/// Pitch gain of last decoded frames
		/// </summary>
		private float[] pitch_gain_buf;

		/// <summary>
		/// Tail of the buffer
		/// </summary>
		private int pitch_gain_buf_idx;

		/// <summary>
		/// Open-loop gain for previous frame
		/// </summary>
		private float last_ol_gain;

		protected internal Random random;
		protected internal Stereo stereo;
		protected internal Inband inband;
		protected internal bool enhanced;

		public NbDecoder()
		{
			this.random = new Random();
			stereo = new Stereo();
			inband = new Inband(stereo);
			enhanced = true;
		}

		protected override void Init(int frameSize, int subframeSize, int lpcSize, int bufSize)
		{
			base.Init(frameSize, subframeSize, lpcSize, bufSize);
			innov2 = new float[40];

			count_lost = 0;
			last_pitch = 40;
			last_pitch_gain = 0;
			pitch_gain_buf = new float[3];
			pitch_gain_buf_idx = 0;
			last_ol_gain = 0;
		}

		/// <summary>
		/// Decode the given input bits.
		/// </summary>
		/// <returns>1 if a terminator was found, 0 if not.</returns>
        /// <exception cref="InvalidFormatException">If there is an error detected in the data stream.</exception>
		public virtual int Decode(Bits bits, float[] xout)
		{
			int i, sub, pitch, ol_pitch = 0, m;
			float[] pitch_gain = new float[3];
			float ol_gain = 0.0f, ol_pitch_coef = 0.0f;
			int best_pitch = 40;
			float best_pitch_gain = 0;
			float pitch_average = 0;

			if (bits == null && dtx_enabled != 0)
			{
				submodeID = 0;
			}
			else
			{
				/*
				 * If bits is NULL, consider the packet to be lost (what could we do
				 * anyway)
				 */
				if (bits == null)
				{
					DecodeLost(xout);
					return 0;
				}
				/*
				 * Search for next narrowband block (handle requests, skip wideband
				 * blocks)
				 */
				do
				{
                    if (bits.BitsRemaining() < 5)
                        return -1;

					if (bits.Unpack(1) != 0)
					{ /*
												 * Skip wideband block (for
												 * compatibility)
												 */
						// Wideband
						/* Get the sub-mode that was used */
						m = bits.Unpack(NSpeex.SbCodec.SB_SUBMODE_BITS);
						int advance = NSpeex.SbCodec.SB_FRAME_SIZE[m];
						if (advance < 0)
						{
							throw new InvalidFormatException(
									"Invalid sideband mode encountered (1st sideband): "
											+ m);
							// return -2;
						}
						advance -= (NSpeex.SbCodec.SB_SUBMODE_BITS + 1);
						bits.Advance(advance);
						if (bits.Unpack(1) != 0)
						{ /*
													 * Skip ultra-wideband block
													 * (for compatibility)
													 */
							/* Get the sub-mode that was used */
							m = bits.Unpack(NSpeex.SbCodec.SB_SUBMODE_BITS);
							advance = NSpeex.SbCodec.SB_FRAME_SIZE[m];
							if (advance < 0)
							{
                                throw new InvalidFormatException(
										"Invalid sideband mode encountered. (2nd sideband): "
												+ m);
								// return -2;
							}
							advance -= (NSpeex.SbCodec.SB_SUBMODE_BITS + 1);
							bits.Advance(advance);
							if (bits.Unpack(1) != 0)
							{ /* Sanity check */
                                throw new InvalidFormatException(
										"More than two sideband layers found");
								// return -2;
							}
						}
						// */
					}

                    if (bits.BitsRemaining() < 4)
                        return 1;

					/* Get the sub-mode that was used */
					m = bits.Unpack(NSpeex.NbCodec.NB_SUBMODE_BITS);
					if (m == 15)
					{ /* We found a terminator */
						return 1;
					}
					else if (m == 14)
					{ /* Speex in-band request */
						inband.SpeexInbandRequest(bits);
					}
					else if (m == 13)
					{ /* User in-band request */
						inband.UserInbandRequest(bits);
					}
					else if (m > 8)
					{ /* Invalid mode */
                        throw new InvalidFormatException(
								"Invalid mode encountered: " + m);
						// return -2;
					}
				} while (m > 8);
				submodeID = m;
			}

			/* Shift all buffers by one frame */
			System.Array.Copy(frmBuf, frameSize, frmBuf, 0, bufSize
							- frameSize);
			System.Array.Copy(excBuf, frameSize, excBuf, 0, bufSize
							- frameSize);

			/* If null mode (no transmission), just set a couple things to zero */
			if (submodes[submodeID] == null)
			{
				NSpeex.Filters.Bw_lpc(.93f, interp_qlpc, lpc, 10);

				float innov_gain = 0;
				for (i = 0; i < frameSize; i++)
					innov_gain += innov[i] * innov[i];
				innov_gain = (float)Math.Sqrt(innov_gain / frameSize);
				for (i = excIdx; i < excIdx + frameSize; i++)
				{
					excBuf[i] = (float)(3 * innov_gain * (random.NextDouble() - .5f));
				}
				first = 1;

				/* Final signal synthesis from excitation */
				NSpeex.Filters.Iir_mem2(excBuf, excIdx, lpc, frmBuf, frmIdx,
						frameSize, lpcSize, mem_sp);

				xout[0] = frmBuf[frmIdx] + preemph * pre_mem;
				for (i = 1; i < frameSize; i++)
					xout[i] = frmBuf[frmIdx + i] + preemph * xout[i - 1];
				pre_mem = xout[frameSize - 1];
				count_lost = 0;
				return 0;
			}

			/* Unquantize LSPs */
			submodes[submodeID].LsqQuant.Unquant(qlsp, lpcSize, bits);

			/* Damp memory if a frame was lost and the LSP changed too much */
			if (count_lost != 0)
			{
				float lsp_dist = 0, fact;
				for (i = 0; i < lpcSize; i++)
					lsp_dist += Math.Abs(old_qlsp[i] - qlsp[i]);
				fact = (float)(.6d * Math.Exp(-.2d * lsp_dist));
				for (i = 0; i < 2 * lpcSize; i++)
					mem_sp[i] *= fact;
			}

			/* Handle first frame and lost-packet case */
			if (first != 0 || count_lost != 0)
			{
				for (i = 0; i < lpcSize; i++)
					old_qlsp[i] = qlsp[i];
			}

			/* Get open-loop pitch estimation for low bit-rate pitch coding */
			if (submodes[submodeID].LbrPitch != -1)
			{
				ol_pitch = min_pitch + bits.Unpack(7);
			}

			if (submodes[submodeID].ForcedPitchGain != 0)
			{
				int quant = bits.Unpack(4);
				ol_pitch_coef = 0.066667f * quant;
			}

			/* Get global excitation gain */
			int qe = bits.Unpack(5);
			ol_gain = (float)Math.Exp(qe / 3.5d);

			/* unpacks unused dtx bits */
			if (submodeID == 1)
			{
				int extra = bits.Unpack(4);
				if (extra == 15)
					dtx_enabled = 1;
				else
					dtx_enabled = 0;
			}
			if (submodeID > 1)
				dtx_enabled = 0;

			/* Loop on subframes */
			for (sub = 0; sub < nbSubframes; sub++)
			{
				int offset, spIdx, extIdx;
				float tmp;
				/* Offset relative to start of frame */
				offset = subframeSize * sub;
				/* Original signal */
				spIdx = frmIdx + offset;
				/* Excitation */
				extIdx = excIdx + offset;

				/* LSP interpolation (quantized and unquantized) */
				tmp = (1.0f + sub) / nbSubframes;
				for (i = 0; i < lpcSize; i++)
					interp_qlsp[i] = (1 - tmp) * old_qlsp[i] + tmp * qlsp[i];

				/* Make sure the LSP's are stable */
				NSpeex.Lsp.Enforce_margin(interp_qlsp, lpcSize, .002f);

				/* Compute interpolated LPCs (unquantized) */
				for (i = 0; i < lpcSize; i++)
					interp_qlsp[i] = (float)System.Math.Cos(interp_qlsp[i]);
				m_lsp.Lsp2lpc(interp_qlsp, interp_qlpc, lpcSize);

				/* Compute enhanced synthesis filter */
				if (enhanced)
				{
					float r = .9f;
					float k1, k2, k3;

					k1 = submodes[submodeID].LpcEnhK1;
					k2 = submodes[submodeID].LpcEnhK2;
					k3 = (1 - (1 - r * k1) / (1 - r * k2)) / r;
					NSpeex.Filters.Bw_lpc(k1, interp_qlpc, awk1, lpcSize);
					NSpeex.Filters.Bw_lpc(k2, interp_qlpc, awk2, lpcSize);
					NSpeex.Filters.Bw_lpc(k3, interp_qlpc, awk3, lpcSize);
				}

				/* Compute analysis filter at w=pi */
				tmp = 1;
				pi_gain[sub] = 0;
				for (i = 0; i <= lpcSize; i++)
				{
					pi_gain[sub] += tmp * interp_qlpc[i];
					tmp = -tmp;
				}

				/* Reset excitation */
				for (i = 0; i < subframeSize; i++)
					excBuf[extIdx + i] = 0;

				/* Adaptive codebook contribution */
				int pit_min, pit_max;

				/* Handle pitch constraints if any */
				if (submodes[submodeID].LbrPitch != -1)
				{
					int margin = submodes[submodeID].LbrPitch;
					if (margin != 0)
					{
						pit_min = ol_pitch - margin + 1;
						if (pit_min < min_pitch)
							pit_min = min_pitch;
						pit_max = ol_pitch + margin;
						if (pit_max > max_pitch)
							pit_max = max_pitch;
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

				/* Pitch synthesis */
				pitch = submodes[submodeID].Ltp.Unquant(excBuf, extIdx, pit_min,
						ol_pitch_coef, subframeSize, pitch_gain, bits, count_lost,
						offset, last_pitch_gain);

				/* If we had lost frames, check energy of last received frame */
				if (count_lost != 0 && ol_gain < last_ol_gain)
				{
					float fact_0 = ol_gain / (last_ol_gain + 1);
					for (i = 0; i < subframeSize; i++)
						excBuf[excIdx + i] *= fact_0;
				}

				tmp = Math.Abs(pitch_gain[0] + pitch_gain[1]
									+ pitch_gain[2]);
				tmp = Math.Abs(pitch_gain[1]);
				if (pitch_gain[0] > 0)
					tmp += pitch_gain[0];
				else
					tmp -= .5f * pitch_gain[0];
				if (pitch_gain[2] > 0)
					tmp += pitch_gain[2];
				else
					tmp -= .5f * pitch_gain[0];

				pitch_average += tmp;
				if (tmp > best_pitch_gain)
				{
					best_pitch = pitch;
					best_pitch_gain = tmp;
				}

				/* Unquantize the innovation */
				int q_energy, ivi = sub * subframeSize;
				float ener;

				for (i = ivi; i < ivi + subframeSize; i++)
					innov[i] = 0.0f;

				/* Decode sub-frame gain correction */
				if (submodes[submodeID].HaveSubframeGain == 3)
				{
					q_energy = bits.Unpack(3);
					ener = (float)(ol_gain * Math.Exp(NSpeex.NbCodec.exc_gain_quant_scal3[q_energy]));
				}
				else if (submodes[submodeID].HaveSubframeGain == 1)
				{
					q_energy = bits.Unpack(1);
					ener = (float)(ol_gain * Math.Exp(NSpeex.NbCodec.exc_gain_quant_scal1[q_energy]));
				}
				else
				{
					ener = ol_gain;
				}

				if (submodes[submodeID].Innovation != null)
				{
					/* Fixed codebook contribution */
					submodes[submodeID].Innovation.Unquantify(innov, ivi,
							subframeSize, bits);
				}

				/* De-normalize innovation and update excitation */
				for (i = ivi; i < ivi + subframeSize; i++)
					innov[i] *= ener;

				/* Vocoder mode */
				if (submodeID == 1)
				{
					float g = ol_pitch_coef;

					for (i = 0; i < subframeSize; i++)
						excBuf[extIdx + i] = 0;
					while (voc_offset < subframeSize)
					{
						if (voc_offset >= 0)
							excBuf[extIdx + voc_offset] = (float)Math.Sqrt(1.0f * ol_pitch);
						voc_offset += ol_pitch;
					}
					voc_offset -= subframeSize;

					g = .5f + 2 * (g - .6f);
					if (g < 0)
						g = 0;
					if (g > 1)
						g = 1;
					for (i = 0; i < subframeSize; i++)
					{
						float itmp = excBuf[extIdx + i];
						excBuf[extIdx + i] = .8f * g * excBuf[extIdx + i] * ol_gain
								+ .6f * g * voc_m1 * ol_gain + .5f * g
								* innov[ivi + i] - .5f * g * voc_m2 + (1 - g)
								* innov[ivi + i];
						voc_m1 = itmp;
						voc_m2 = innov[ivi + i];
						voc_mean = .95f * voc_mean + .05f * excBuf[extIdx + i];
						excBuf[extIdx + i] -= voc_mean;
					}
				}
				else
				{
					for (i = 0; i < subframeSize; i++)
						excBuf[extIdx + i] += innov[ivi + i];
				}

				/* Decode second codebook (only for some modes) */
				if (submodes[submodeID].DoubleCodebook != 0)
				{
					for (i = 0; i < subframeSize; i++)
						innov2[i] = 0;
					submodes[submodeID].Innovation.Unquantify(innov2, 0, subframeSize,
							bits);
					for (i = 0; i < subframeSize; i++)
						innov2[i] *= ener * (1 / 2.2f);
					for (i = 0; i < subframeSize; i++)
						excBuf[extIdx + i] += innov2[i];
				}

				for (i = 0; i < subframeSize; i++)
					frmBuf[spIdx + i] = excBuf[extIdx + i];

				/* Signal synthesis */
				if (enhanced && submodes[submodeID].CombGain > 0)
				{
					filters.Comb_filter(excBuf, extIdx, frmBuf, spIdx,
							subframeSize, pitch, pitch_gain,
							submodes[submodeID].CombGain);
				}

				if (enhanced)
				{
					/* Use enhanced LPC filter */
					NSpeex.Filters.Filter_mem2(frmBuf, spIdx, awk2, awk1,
							subframeSize, lpcSize, mem_sp, lpcSize);
					NSpeex.Filters.Filter_mem2(frmBuf, spIdx, awk3, interp_qlpc,
							subframeSize, lpcSize, mem_sp, 0);
				}
				else
				{
					/* Use regular filter */
					for (i = 0; i < lpcSize; i++)
						mem_sp[lpcSize + i] = 0;
					NSpeex.Filters.Iir_mem2(frmBuf, spIdx, interp_qlpc, frmBuf,
							spIdx, subframeSize, lpcSize, mem_sp);
				}
			}

			/* Copy output signal */
			xout[0] = frmBuf[frmIdx] + preemph * pre_mem;
			for (i = 1; i < frameSize; i++)
				xout[i] = frmBuf[frmIdx + i] + preemph * xout[i - 1];
			pre_mem = xout[frameSize - 1];

			/* Store the LSPs for interpolation in the next frame */
			for (i = 0; i < lpcSize; i++)
				old_qlsp[i] = qlsp[i];

			/* The next frame will not be the first (Duh!) */
			first = 0;
			count_lost = 0;
			last_pitch = best_pitch;
			last_pitch_gain = .25f * pitch_average;
			pitch_gain_buf[pitch_gain_buf_idx++] = last_pitch_gain;
			if (pitch_gain_buf_idx > 2) /* rollover */
				pitch_gain_buf_idx = 0;
			last_ol_gain = ol_gain;

			return 0;
		}

		/// <summary>
		/// Decode when packets are lost.
		/// </summary>
		/// <returns>0 if successful.</returns>
		public int DecodeLost(float[] xout)
		{
			int i;
			float pitch_gain, fact, gain_med;

			fact = (float)Math.Exp(-.04d * count_lost * count_lost);
			// median3(a, b, c) = (a<b ? (b<c ? b : (a<c ? c : a))
			// : (c<b ? b : (c<a ? c : a)))
			gain_med = ((pitch_gain_buf[0] < pitch_gain_buf[1]) ? ((pitch_gain_buf[1] < pitch_gain_buf[2]) ? pitch_gain_buf[1]
					: ((pitch_gain_buf[0] < pitch_gain_buf[2]) ? pitch_gain_buf[2]
							: pitch_gain_buf[0]))
					: ((pitch_gain_buf[2] < pitch_gain_buf[1]) ? pitch_gain_buf[1]
							: ((pitch_gain_buf[2] < pitch_gain_buf[0]) ? pitch_gain_buf[2]
									: pitch_gain_buf[0])));
			if (gain_med < last_pitch_gain)
				last_pitch_gain = gain_med;

			pitch_gain = last_pitch_gain;
			if (pitch_gain > .95f)
				pitch_gain = .95f;

			pitch_gain *= fact;

			/* Shift all buffers by one frame */
			System.Array.Copy(frmBuf, frameSize, frmBuf, 0, bufSize
							- frameSize);
			System.Array.Copy(excBuf, frameSize, excBuf, 0, bufSize
							- frameSize);

			for (int sub = 0; sub < nbSubframes; sub++)
			{
				int offset;
				int spIdx, extIdx;
				/* Offset relative to start of frame */
				offset = subframeSize * sub;
				/* Original signal */
				spIdx = frmIdx + offset;
				/* Excitation */
				extIdx = excIdx + offset;
				/* Excitation after post-filter */

				/* Calculate perceptually enhanced LPC filter */
				if (enhanced)
				{
					float r = .9f;
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
					k3 = (1 - (1 - r * k1) / (1 - r * k2)) / r;
					NSpeex.Filters.Bw_lpc(k1, interp_qlpc, awk1, lpcSize);
					NSpeex.Filters.Bw_lpc(k2, interp_qlpc, awk2, lpcSize);
					NSpeex.Filters.Bw_lpc(k3, interp_qlpc, awk3, lpcSize);
				}
				/* Make up a plausible excitation */
				/* THIS CAN BE IMPROVED */
				/*
				 * if (pitch_gain>.95) pitch_gain=.95;
				 */
				{
					float innov_gain = 0;
					for (i = 0; i < frameSize; i++)
						innov_gain += innov[i] * innov[i];
					innov_gain = (float)Math.Sqrt(innov_gain / frameSize);
					for (i = 0; i < subframeSize; i++)
					{
						// #if 0
						// excBuf[extIdx+i] = pitch_gain*excBuf[extIdx+i-last_pitch]
						// + fact*((float)Math.sqrt(1-pitch_gain))*innov[i+offset];
						// /*Just so it give the same lost packets as with if 0*/
						// /*rand();*/
						// #else
						/*
						 * excBuf[extIdx+i] = pitch_gain*excBuf[extIdx+i-last_pitch] +
						 * fact*innov[i+offset];
						 */
						excBuf[extIdx + i] = pitch_gain
								* excBuf[extIdx + i - last_pitch] + fact
								* ((float)Math.Sqrt(1 - pitch_gain)) * 3
								* innov_gain * (((float)random.NextDouble()) - 0.5f);
						// #endif
					}
				}
				for (i = 0; i < subframeSize; i++)
					frmBuf[spIdx + i] = excBuf[extIdx + i];

				/* Signal synthesis */
				if (enhanced)
				{
					/* Use enhanced LPC filter */
					NSpeex.Filters.Filter_mem2(frmBuf, spIdx, awk2, awk1,
							subframeSize, lpcSize, mem_sp, lpcSize);
					NSpeex.Filters.Filter_mem2(frmBuf, spIdx, awk3, interp_qlpc,
							subframeSize, lpcSize, mem_sp, 0);
				}
				else
				{
					/* Use regular filter */
					for (i = 0; i < lpcSize; i++)
						mem_sp[lpcSize + i] = 0;
					NSpeex.Filters.Iir_mem2(frmBuf, spIdx, interp_qlpc, frmBuf,
							spIdx, subframeSize, lpcSize, mem_sp);
				}
			}

			xout[0] = frmBuf[0] + preemph * pre_mem;
			for (i = 1; i < frameSize; i++)
				xout[i] = frmBuf[i] + preemph * xout[i - 1];
			pre_mem = xout[frameSize - 1];
			first = 0;
			count_lost++;
			pitch_gain_buf[pitch_gain_buf_idx++] = pitch_gain;
			if (pitch_gain_buf_idx > 2) /* rollover */
				pitch_gain_buf_idx = 0;

			return 0;
		}

		/// <summary>
		/// Decode the given bits to stereo.
		/// </summary>
		public virtual void DecodeStereo(float[] data, int frameSize)
		{
			stereo.Decode(data, frameSize);
		}

		public bool Dtx
		{
			get
			{
				// TODO - should return DTX for the NbCodec
				return dtx_enabled != 0;
			}
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
