/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using OpenTS2.Common;
using OpenTS2.Common.Utils;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content
{
    /// <summary>
    /// Represents a saveable DBPF asset.
    /// </summary>
    public abstract class AbstractAsset : ICloneable
    {
        /// <summary>
        /// Save changes to this asset in memory.
        /// </summary>
        public void Save()
        {
            package.Changes.Set(this);
        }
        /// <summary>
        /// Mark this asset as deleted in memory.
        /// </summary>
        public void Delete()
        {
            package.Changes.Delete(this.internalTGI);
        }
        /// <summary>
        /// Unmark this asset as deleted in memory.
        /// </summary>
        public void Restore()
        {
            package.Changes.Restore(this.internalTGI);
        }

        public object Clone()
        {
            var codec = Codecs.Get(TGI.TypeID);
            var serialized = codec.Serialize(this);
            var clone = codec.Deserialize(serialized, this.globalTGI, package);
            clone.globalTGI = globalTGI;
            clone.internalTGI = internalTGI;
            clone.package = package;
            clone.Compressed = Compressed;
            return clone;
        }

        public ResourceKey TGI
        {
            get
            {
                return globalTGI;
            }
            set
            {
                internalTGI = value;
                globalTGI = value.LocalGroupID(package.GroupID);
            }
        }
        public ResourceKey globalTGI = ResourceKey.Default;
        /// <summary>
        /// Original TGI, as written to file.
        /// </summary>
        public ResourceKey internalTGI = ResourceKey.Default;
        public DBPFFile package;
        public bool Compressed
        {
            get
            {
                if (!CanBeCompressed)
                    return false;
                return compressed;
            }
            set
            {
                if (CanBeCompressed)
                    compressed = value;
            }
        }
        public virtual bool CanBeCompressed
        {
            get
            {
                return true;
            }
        }
        bool compressed = false;
    }
}
