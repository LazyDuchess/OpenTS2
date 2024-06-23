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
	/// Line Spectral Pair
	/// </summary>
    internal class Lsp
	{
		private float[] pw;

		/// <summary>
		/// Constructor
		/// </summary>
		///
		public Lsp()
		{
			pw = new float[42];
		}

		/// <summary>
		/// This function evaluates a series of Chebyshev polynomials.
		/// </summary>
		/// <returns>the value of the polynomial at point x.</returns>
		public static float Cheb_poly_eva(float[] coef, float x, int m)
		{
			int i;
			float sum;
			float[] T;
			int m2 = m >> 1;
			/* Allocate memory for Chebyshev series formulation */
			T = new float[m2 + 1];
			/* Initialise values */
			T[0] = 1;
			T[1] = x;
			/* Evaluate Chebyshev series formulation using iterative approach */
			/* Evaluate polynomial and return value also free memory space */
			sum = coef[m2] + coef[m2 - 1] * x;
			x *= 2;
			for (i = 2; i <= m2; i++)
			{
				T[i] = x * T[i - 1] - T[i - 2];
				sum += coef[m2 - i] * T[i];
			}
			return sum;
		}

		/// <summary>
		/// This function converts LPC coefficients to LSP coefficients.
		/// </summary>
		/// <returns>the number of roots (the LSP coefs are returned in the array).</returns>
		public static int Lpc2lsp(float[] a, int lpcrdr, float[] freq, int nb, float delta)
		{
			float psuml, psumr, psumm, temp_xr, xl, xr, xm = 0;
			float temp_psumr;
			int i, j, m, flag, k;
			float[] Q; // ptrs for memory allocation
			float[] P;
			int px; // ptrs of respective P'(z) & Q'(z)
			int qx;
			int p;
			int q;
			float[] pt; // ptr used for cheb_poly_eval() whether P' or Q'
			int roots = 0; // DR 8/2/94: number of roots found
			flag = 1; // program is searching for a root when, 1 else has found
			// one
			m = lpcrdr / 2; // order of P'(z) & Q'(z) polynomials

			/* Allocate memory space for polynomials */
			Q = new float[m + 1];
			P = new float[m + 1];

			/*
			 * determine P'(z)'s and Q'(z)'s coefficients where P'(z) = P(z)/(1 +
			 * z^(-1)) and Q'(z) = Q(z)/(1-z^(-1))
			 */

			px = 0; /* initialise ptrs */
			qx = 0;
			p = px;
			q = qx;
			P[px++] = 1.0f;
			Q[qx++] = 1.0f;
			for (i = 1; i <= m; i++)
			{
				P[px++] = a[i] + a[lpcrdr + 1 - i] - P[p++];
				Q[qx++] = a[i] - a[lpcrdr + 1 - i] + Q[q++];
			}
			px = 0;
			qx = 0;
			for (i = 0; i < m; i++)
			{
				P[px] = 2 * P[px];
				Q[qx] = 2 * Q[qx];
				px++;
				qx++;
			}
			px = 0; /* re-initialise ptrs */
			qx = 0;

			/*
			 * Search for a zero in P'(z) polynomial first and then alternate to
			 * Q'(z). Keep alternating between the two polynomials as each zero is
			 * found
			 */

			xr = 0; /* initialise xr to zero */
			xl = 1.0f; /* start at point xl = 1 */

			for (j = 0; j < lpcrdr; j++)
			{
				if (j % 2 != 0) /* determines whether P' or Q' is eval. */
					pt = Q;
				else
					pt = P;

				psuml = Cheb_poly_eva(pt, xl, lpcrdr); /* evals poly. at xl */
				flag = 1;
				while ((flag == 1) && (xr >= -1.0d))
				{
					float dd;
					/* Modified by JMV to provide smaller steps around x=+-1 */
					dd = (float)(delta * (1 - .9d * xl * xl));
					if (Math.Abs(psuml) < .2d)
						dd *= ((Single?).5d).Value;

					xr = xl - dd; /* interval spacing */
					psumr = Cheb_poly_eva(pt, xr, lpcrdr); /* poly(xl-delta_x) */
					temp_psumr = psumr;
					temp_xr = xr;

					/*
					 * if no sign change increment xr and re-evaluate poly(xr).
					 * Repeat til sign change. if a sign change has occurred the
					 * interval is bisected and then checked again for a sign change
					 * which determines in which interval the zero lies in. If there
					 * is no sign change between poly(xm) and poly(xl) set interval
					 * between xm and xr else set interval between xl and xr and
					 * repeat till root is located within the specified limits
					 */

					if ((psumr * psuml) < 0.0d)
					{
						roots++;

						psumm = psuml;
						for (k = 0; k <= nb; k++)
						{
							xm = (xl + xr) / 2; /* bisect the interval */
							psumm = Cheb_poly_eva(pt, xm, lpcrdr);
							if (psumm * psuml > 0.0)
							{
								psuml = psumm;
								xl = xm;
							}
							else
							{
								psumr = psumm;
								xr = xm;
							}
						}

						/* once zero is found, reset initial interval to xr */
						freq[j] = xm;
						xl = xm;
						flag = 0; /* reset flag for next search */
					}
					else
					{
						psuml = temp_psumr;
						xl = temp_xr;
					}
				}
			}
			return roots;
		}

		/// <summary>
		/// Line Spectral Pair to Linear Prediction Coefficients
		/// </summary>
		public void Lsp2lpc(float[] freq, float[] ak, int lpcrdr)
		{
			int i, j;
			float xout1, xout2, xin1, xin2;
			int n1, n2, n3, n4 = 0;
			int m = lpcrdr / 2;

			for (i = 0; i < 4 * m + 2; i++)
			{
				pw[i] = 0.0f;
			}

			xin1 = 1.0f;
			xin2 = 1.0f;

			/*
			 * reconstruct P(z) and Q(z) by cascading second order polynomials in
			 * form 1 - 2xz(-1) +z(-2), where x is the LSP coefficient
			 */
			for (j = 0; j <= lpcrdr; j++)
			{
				int i2 = 0;

				for (i = 0; i < m; i++, i2 += 2)
				{
					n1 = i * 4;
					n2 = n1 + 1;
					n3 = n2 + 1;
					n4 = n3 + 1;
					xout1 = xin1 - 2 * (freq[i2]) * pw[n1] + pw[n2];
					xout2 = xin2 - 2 * (freq[i2 + 1]) * pw[n3] + pw[n4];
					pw[n2] = pw[n1];
					pw[n4] = pw[n3];
					pw[n1] = xin1;
					pw[n3] = xin2;
					xin1 = xout1;
					xin2 = xout2;
				}
				xout1 = xin1 + pw[n4 + 1];
				xout2 = xin2 - pw[n4 + 2];
				ak[j] = (xout1 + xout2) * 0.5f;
				pw[n4 + 1] = xin1;
				pw[n4 + 2] = xin2;
				xin1 = 0.0f;
				xin2 = 0.0f;
			}
		}

		/// <summary>
		/// Makes sure the LSPs are stable.
		/// </summary>
		public static void Enforce_margin(float[] lsp, int len, float margin)
		{
			int i;

			if (lsp[0] < margin)
				lsp[0] = margin;

			if (lsp[len - 1] > (float)System.Math.PI - margin)
				lsp[len - 1] = (float)System.Math.PI - margin;

			for (i = 1; i < len - 1; i++)
			{
				if (lsp[i] < lsp[i - 1] + margin)
					lsp[i] = lsp[i - 1] + margin;

				if (lsp[i] > lsp[i + 1] - margin)
					lsp[i] = .5f * (lsp[i] + lsp[i + 1] - margin);
			}
		}
	}
}
