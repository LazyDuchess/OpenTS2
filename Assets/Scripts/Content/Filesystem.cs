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
using System.IO;
using OpenTS2.Content.Interfaces;

namespace OpenTS2.Content
{
    public class Filesystem
    {
        string _DataDirectory;
        string _UserDirectory;
        string _BinDirectory;
        public string DataDirectory
        {
            get { return _DataDirectory; }
        }

        public string UserDirectory
        {
            get { return _UserDirectory; }
        }

        public string BinDirectory
        {
            get { return _BinDirectory; }
        }

        public Filesystem(IPathProvider pathProvider)
        {
            _DataDirectory = pathProvider.GetGameRootPath() + "/TSData";
            _UserDirectory = pathProvider.GetUserPath();
            _BinDirectory = pathProvider.GetGameRootPath() + "/TSBin";
        }
    }
}
