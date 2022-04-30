/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Common
{
    /// <summary>
    /// Represents a color in RGBA composed of floats that range from 0 to 1.
    /// </summary>
    public partial class Color
    {
        public float R = 1.0f;
        public float G = 1.0f;
        public float B = 1.0f;
        public float A = 1.0f;

        /// <summary>
        /// Constructs an RGBA color.
        /// </summary>
        /// <param name="R">Red amount. Min 0, max 1.</param>
        /// <param name="G">Green amount. Min 0, max 1.</param>
        /// <param name="B">Blue amount. Min 0, max 1.</param>
        /// <param name="A">Alpha amount. Min 0, max 1.</param>
        public Color(float R, float G, float B, float A)
        {
            this.R = R;
            this.G = G;
            this.B = B;
            this.A = A;
        }
        /// <summary>
        /// Constructs an RGB color.
        /// </summary>
        /// <param name="R">Red amount. Min 0, max 1.</param>
        /// <param name="G">Green amount. Min 0, max 1.</param>
        /// <param name="B">Blue amount. Min 0, max 1.</param>
        public Color(float R, float G, float B)
        {
            this.R = R;
            this.G = G;
            this.B = B;
        }
    }
}
