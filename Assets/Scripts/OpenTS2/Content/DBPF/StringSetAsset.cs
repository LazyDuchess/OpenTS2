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
        public string Value;
        public string Description;

        public StringValue(string value, string description)
        {
            this.Value = value;
            this.Description = description;
        }
    }
    public class StringSetData
    {
        public string FileName;
        public Dictionary<Languages, List<StringValue>> Strings = new Dictionary<Languages, List<StringValue>>();

        /// <summary>
        /// Gets a string by its ID and in the specified language.
        /// </summary>
        /// <param name="id">ID of the string to retrieve.</param>
        /// <param name="language">Language bytecode.</param>
        /// <returns>Localized string.</returns>
        public string GetString(int id, Languages language)
        {
            if (!Strings.ContainsKey(language))
                language = Languages.USEnglish;
            if (!Strings.ContainsKey(language))
                return "";
            var languageStr = Strings[language];
            if (id >= languageStr.Count)
                return "";
            return languageStr[id].Value;
        }
    }
    /// <summary>
    /// Contains game strings. Asset wrapper around the STR file format.
    /// </summary>
    public class StringSetAsset : AbstractAsset
    {
        public StringSetData StringData
        {
            get { return _stringData; }
        }

        readonly StringSetData _stringData;

        /// <summary>
        /// Constructs a StringTable from StringTableData.
        /// </summary>
        /// <param name="str">STR to use</param>
        public StringSetAsset(StringSetData stringData)
        {
            _stringData = stringData;
        }

        /// <summary>
        /// Gets a localized string by its ID.
        /// </summary>
        /// <param name="id">ID of the string in the string table</param>
        /// <returns>Localized string</returns>
        public string GetString(int id)
        {
            return _stringData.GetString(id, Settings.Get().Language);
        }
    }
}
