/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Files.Formats.DBPF
{
    /// <summary>
    /// Codecs for DBPF archives.
    /// </summary>
    public static class Codecs
    {
        //AbstractCodec classes with CodecAttributes should be automatically added here.
        static readonly Dictionary<uint, AbstractCodec> s_codecsByTypeID = new Dictionary<uint, AbstractCodec>();

        /// <summary>
        /// Register a codec.
        /// </summary>
        /// <param name="type">Type ID.</param>
        /// <param name="codec">Codec instance.</param>
        public static void Register(uint type, AbstractCodec codec)
        {
            s_codecsByTypeID[type] = codec;
        }

        /// <summary>
        /// Get the codec for a filetype.
        /// </summary>
        /// <param name="type">The Type ID</param>
        /// <returns>Codec for this type.</returns>
        public static AbstractCodec Get(uint type)
        {
            if (s_codecsByTypeID.TryGetValue(type, out AbstractCodec codecOut))
                return codecOut;
            return null;
        }
    }
}
