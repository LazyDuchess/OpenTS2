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
	/// Miscellaneous functions
	/// </summary>
    internal class Misc
	{
		/// <summary>
		/// Builds an Asymmetric "pseudo-Hamming" window.
		/// </summary>
		/// <returns>an Asymmetric "pseudo-Hamming" window.</returns>
		public static float[] Window(int windowSize, int subFrameSize)
		{
			int i;
			int part1 = subFrameSize * 7 / 2;
			int part2 = subFrameSize * 5 / 2;
			float[] window = new float[windowSize];
			for (i = 0; i < part1; i++)
				window[i] = (float)(0.54d - 0.46d * System.Math
						.Cos(System.Math.PI * i / part1));
			for (i = 0; i < part2; i++)
				window[part1 + i] = (float)(0.54d + 0.46d * System.Math
						.Cos(System.Math.PI * i / part2));
			return window;
		}

		/// <summary>
		/// Create the window for autocorrelation (lag-windowing).
		/// </summary>
		/// <returns>the window for autocorrelation.</returns>
		public static float[] LagWindow(int lpcSize, float lagFactor)
		{
			float[] lagWindow = new float[lpcSize + 1];
			for (int i = 0; i < lpcSize + 1; i++)
				lagWindow[i] = (float)Math.Exp(-0.5d
									* (2 * System.Math.PI * lagFactor * i)
									* (2 * System.Math.PI * lagFactor * i));
			return lagWindow;
		}
	}
}
