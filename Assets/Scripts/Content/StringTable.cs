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
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Client;

namespace OpenTS2.Content
{
    /// <summary>
    /// Contains game strings. Asset wrapper around the STR file format.
    /// </summary>
    public class StringTable : AbstractAsset
    {
        public STR Str
        {
            get { return _Str; }
        }
        STR _Str;

        /// <summary>
        /// Constructs a StringTable from a deserialized STR.
        /// </summary>
        /// <param name="str">STR to use</param>
        public StringTable(STR str)
        {
            this._Str = str;
        }

        /// <summary>
        /// Gets a localized string by its ID.
        /// </summary>
        /// <param name="id">ID of the string in the string table</param>
        /// <returns>Localized string</returns>
        public string GetString(int id)
        {
            return _Str.GetString(id, GlobalSettings.language);
        }
    }
}
