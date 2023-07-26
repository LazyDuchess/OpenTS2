/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using OpenTS2.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Content.DBPF
{
    /// <summary>
    /// Texture DBPF Asset
    /// </summary>
    public class TextureAsset : AbstractAsset
    {
        public TextureAsset(Texture2D texture2D)
        {
            _texture2D = texture2D;
        }

        public override void FreeUnmanagedResources()
        {
            if (_texture2D == null)
                return;
            _texture2D.Free();
        }
        Texture2D _texture2D;
        public Texture2D Texture => _texture2D;
    }
}
