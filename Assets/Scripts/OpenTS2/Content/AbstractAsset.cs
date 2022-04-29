/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using OpenTS2.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content
{
    /// <summary>
    /// Represents a saveable DBPF asset.
    /// </summary>
    public abstract class AbstractAsset
    {
        public TGI tgi = TGI.Default;
        public string sourceFile = "";
    }
}
