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
        public ResourceKey TGI
        {
            get
            {
                return GlobalTGI;
            }
            set
            {
                InternalTGI = value;
                GlobalTGI = value.LocalGroupID(Package.GroupID);
            }
        }
        public ResourceKey GlobalTGI = ResourceKey.Default;
        /// <summary>
        /// Original TGI, as written to file.
        /// </summary>
        public ResourceKey InternalTGI = ResourceKey.Default;
        public DBPFFile Package;
        public bool Compressed
        {
            get
            {
                if (!CanBeCompressed)
                    return false;
                return _compressed;
            }
            set
            {
                if (CanBeCompressed)
                    _compressed = value;
            }
        }
        public virtual bool CanBeCompressed
        {
            get
            {
                return true;
            }
        }
        bool _compressed = false;

        /// <summary>
        /// Save changes to this asset in memory.
        /// </summary>
        public void Save()
        {
            Package.Changes.Set(this);
        }
        /// <summary>
        /// Mark this asset as deleted in memory.
        /// </summary>
        public void Delete()
        {
            Package.Changes.Delete(this.InternalTGI);
        }
        /// <summary>
        /// Unmark this asset as deleted in memory.
        /// </summary>
        public void Restore()
        {
            Package.Changes.Restore(this.InternalTGI);
        }

        public object Clone()
        {
            var codec = Codecs.Get(TGI.TypeID);
            var serialized = codec.Serialize(this);
            var clone = codec.Deserialize(serialized, this.GlobalTGI, Package);
            clone.GlobalTGI = GlobalTGI;
            clone.InternalTGI = InternalTGI;
            clone.Package = Package;
            clone.Compressed = Compressed;
            return clone;
        }
    }
}
