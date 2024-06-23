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
	/// LPC - and Reflection Coefficients.
	/// 
	/// The next two functions calculate linear prediction coefficients and/or the
	/// related reflection coefficients from the first P_MAX+1 values of the
	/// autocorrelation function.
	/// 
	/// Invented by N. Levinson in 1947, modified by J. Durbin in 1959.
	/// </summary>
    internal class Lpc
	{
		/// <summary>
		/// Returns minimum mean square error.
		/// </summary>
		/// <returns>minimum mean square error.</returns>
		public static float Wld(float[] lpc, float[] ac, float[] xref, int p)
		{
			int i, j;
			float r, error = ac[0];
			if (ac[0] == 0)
			{
				for (i = 0; i < p; i++)
					xref[i] = 0;
				return 0;
			}
			for (i = 0; i < p; i++)
			{
				// Sum up this iteration's reflection coefficient.
				r = -ac[i + 1];
				for (j = 0; j < i; j++)
					r -= lpc[j] * ac[i - j];
				xref[i] = r /= error;
				// Update LPC coefficients and total error.
				lpc[i] = r;
				for (j = 0; j < i / 2; j++)
				{
					float tmp = lpc[j];
					lpc[j] += r * lpc[i - 1 - j];
					lpc[i - 1 - j] += r * tmp;
				}
				if ((i % 2) != 0)
					lpc[j] += lpc[j] * r;
				error *= ((Single?)1.0d).Value - r * r;
			}
			return error;
		}

		/// <summary>
		/// Compute the autocorrelation ,--, ac(i) = > x(n)/// x(n-i) for all n `--'
		/// for lags between 0 and lag-1, and x == 0 outside 0...n-1
		/// </summary>
		public static void Autocorr(float[] x, float[] ac, int lag, int n)
		{
			float d;
			int i;
			while (lag-- > 0)
			{
				for (i = lag, d = 0; i < n; i++)
					d += x[i] * x[i - lag];
				ac[lag] = d;
			}
		}
	}
}
