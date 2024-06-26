﻿/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Client
{
    /// <summary>
    /// Stores global user specific settings such as language.
    /// </summary>
    public class Settings
    {
        public static Settings Instance { get; private set; }
        public Languages Language = Languages.USEnglish;
        public bool CustomContentEnabled = true;

        public Settings()
        {
            Instance = this;
        }
    }
}
