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
using OpenTS2.Content.Interfaces;
using OpenTS2.Files;

namespace OpenTS2.Content
{
    /// <summary>
    /// Manages the game's asset saving/loading/caching and its filesystem.
    /// </summary>
    public static class ContentManager
    {
        public static ContentProvider Provider;

        public static ContentCache Cache
        {
            get
            {
                return Provider.Cache;
            }
        }

        public static ContentChanges Changes
        {
            get
            {
                return Provider.Changes;
            }
        }
    }
}
