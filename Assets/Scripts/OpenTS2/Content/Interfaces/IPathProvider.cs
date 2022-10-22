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
using OpenTS2.Content;

namespace OpenTS2.Content.Interfaces
{
    /// <summary>
    /// Interface for providing paths to the game's user data and game folders.
    /// </summary>
    public interface IPathProvider
    {
        public string GetPathForProduct(ProductFlags productFlag);
        public string GetDataPathForProduct(ProductFlags productFlag);
        public string GetBinPathForProduct(ProductFlags productFlag);
        public string GetUserPath();
    }
}
