/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenTS2.Common;

namespace OpenTS2.Unity.Tests
{
    public class HashTestComponent : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            var reftg = new TGI("HeightCheater_cres", 0x1C050000, 0xE519C933);
            Debug.Log(reftg.ToString());
        }
    }
}