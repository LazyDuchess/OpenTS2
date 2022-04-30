/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using UnityEngine;
using OpenTS2.Content;
using OpenTS2.Unity.Content;

namespace OpenTS2.Unity.Game
{
    class GameController : MonoBehaviour
    {
        private void Awake()
        {
            var contentInitializeArgs = new ContentInitializationArgs();
            contentInitializeArgs.pathProvider = new JSONPathProvider();
            contentInitializeArgs.textureFactory = new TextureFactory();
            ContentManager.Initialize(contentInitializeArgs);
        }
    }
}
