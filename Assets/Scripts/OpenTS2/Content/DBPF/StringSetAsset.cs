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

namespace OpenTS2.Content.DBPF
{
    /// <summary>
    /// A string set with a value and a description.
    /// </summary>
    public class StringValue
    {
        public string value;
        public string description;

        public StringValue(string value, string description)
        {
            this.value = value;
            this.description = description;
        }
    }
    public class StringSetData
    {
        public string fileName;
        public Dictionary<Languages, List<StringValue>> strings = new Dictionary<Languages, List<StringValue>>();

        /// <summary>
        /// Gets a string by its ID and in the specified language.
        /// </summary>
        /// <param name="id">ID of the string to retrieve.</param>
        /// <param name="language">Language bytecode.</param>
        /// <returns>Localized string.</returns>
        public string GetString(int id, Languages language)
        {
            return strings[language][id].value;
        }
    }
    /// <summary>
    /// Contains game strings. Asset wrapper around the STR file format.
    /// </summary>
    public class StringSetAsset : AbstractAsset
    {
        public StringSetData StringData
        {
            get { return _StringData; }
        }
        StringSetData _StringData;

        /// <summary>
        /// Constructs a StringTable from StringTableData.
        /// </summary>
        /// <param name="str">STR to use</param>
        public StringSetAsset(StringSetData stringData)
        {
            _StringData = stringData;
        }

        /// <summary>
        /// Gets a localized string by its ID.
        /// </summary>
        /// <param name="id">ID of the string in the string table</param>
        /// <returns>Localized string</returns>
        public string GetString(int id)
        {
            return _StringData.GetString(id, Settings.Get().language);
        }
    }
}
