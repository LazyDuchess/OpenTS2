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
	/// Sideband Codec. This class contains all the basic structures needed by the
	/// Sideband encoder and decoder.
	/// </summary>
    internal class SbCodec : NbCodec
	{
		#region Constants

		/// <summary>
		/// The Sideband Frame Size gives the size in bits of a Sideband frame for a
		/// given sideband submode.
		/// </summary>
		public static readonly int[] SB_FRAME_SIZE = { 4, 36, 112, 192, 352, -1, -1, -1 };

		/// <summary>
		/// The Sideband Submodes gives the number of submodes possible for the
		/// Sideband codec.
		/// </summary>
		public const int SB_SUBMODES = 8;

		/// <summary>
		/// The Sideband Submodes Bits gives the number bits used to encode the
		/// Sideband Submode
		/// </summary>
		public const int SB_SUBMODE_BITS = 3;

		/// <summary>
		/// Quadratic Mirror Filter Order
		/// </summary>
		public const int QMF_ORDER = 64;

		#endregion

		#region Parameters

		protected internal int fullFrameSize;
		protected internal float foldingGain;

		#endregion

		#region Variables

		protected internal float[] high;
		protected internal float[] y0, y1;
		protected internal float[] x0d;
		protected internal float[] g0_mem, g1_mem;

		#endregion

		public SbCodec(bool ultraWide)
		{
			if (ultraWide)
			{
				// Initialize SubModes
				submodes = BuildUwbSubModes();
				submodeID = 1;
			}
			else
			{
				// Initialize SubModes
				submodes = BuildWbSubModes();
				submodeID = 3;
			}
		}

		protected virtual void Init(
			int frameSize, int subframeSize,
			int lpcSize, int bufSize, float foldingGain_0)
		{
			base.Init(frameSize, subframeSize, lpcSize, bufSize);
			this.fullFrameSize = 2 * frameSize;
			this.foldingGain = foldingGain_0;

			lag_factor = 0.002f;

			high = new float[fullFrameSize];
			y0 = new float[fullFrameSize];
			y1 = new float[fullFrameSize];
			x0d = new float[frameSize];
			g0_mem = new float[QMF_ORDER];
			g1_mem = new float[QMF_ORDER];
		}

		/// <summary>
		/// Build wideband submodes.
		/// </summary>
		/// <returns>the wideband submodes.</returns>
		protected static internal SubMode[] BuildWbSubModes()
		{
			// Initialize Long Term Predictions
			HighLspQuant highLU = new HighLspQuant();
			// Initialize Codebook Searches
			SplitShapeSearch ssCbHighLbrSearch = new SplitShapeSearch(40, 10, 4, NSpeex.Codebook_Constants.hexc_10_32_table, 5, 0);
			SplitShapeSearch ssCbHighSearch = new SplitShapeSearch(40, 8, 5, NSpeex.Codebook_Constants.hexc_table, 7, 1);
			// Initialize wide-band modes
			SubMode[] wbSubModes = new SubMode[SB_SUBMODES];
			wbSubModes[1] = new SubMode(0, 0, 1, 0, highLU, null, null, .75f, .75f, -1, 36);
			wbSubModes[2] = new SubMode(0, 0, 1, 0, highLU, null, ssCbHighLbrSearch, .85f, .6f, -1, 112);
			wbSubModes[3] = new SubMode(0, 0, 1, 0, highLU, null, ssCbHighSearch, .75f, .7f, -1, 192);
			wbSubModes[4] = new SubMode(0, 0, 1, 1, highLU, null, ssCbHighSearch, .75f, .75f, -1, 352);
			return wbSubModes;
		}

		/// <summary>
		/// Build ultra-wideband submodes.
		/// </summary>
		/// <returns>the ultra-wideband submodes.</returns>
		protected static internal SubMode[] BuildUwbSubModes()
		{
			/* Initialize Long Term Predictions */
			HighLspQuant highLU = new HighLspQuant();
			SubMode[] uwbSubModes = new SubMode[SB_SUBMODES];
			uwbSubModes[1] = new SubMode(0, 0, 1, 0, highLU, null, null, .75f, .75f, -1, 2);
			return uwbSubModes;
		}

		public override int FrameSize
		{
			get
			{
				return fullFrameSize;
			}
		}

		public bool Dtx
		{
			get
			{
				// TODO - should return DTX for the NbCodec
				return dtx_enabled != 0;
			}
		}

		public override float[] Exc
		{
			get
			{
				int i;
				float[] excTmp = new float[fullFrameSize];
				for (i = 0; i < frameSize; i++)
					excTmp[2 * i] = 2 * excBuf[excIdx + i];
				return excTmp;
			}
		}

		public override float[] Innov
		{
			get
			{
				return Exc;
			}
		}
	}
}
