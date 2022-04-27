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

namespace OpenTS2.Content
{
    public class ContentInitializationArgs
    {
        public IPathProvider pathProvider;
    }
    public static class ContentManager
    {
        public static Filesystem fileSystem;

        public static void Initialize(ContentInitializationArgs args)
        {
            fileSystem = new Filesystem(args.pathProvider);
        }
    }
}
