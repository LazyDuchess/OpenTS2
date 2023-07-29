/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using OpenTS2.Common;
using OpenTS2.Common.Utils;
using OpenTS2.Engine;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Content
{
    /// <summary>
    /// Represents a saveable DBPF asset.
    /// </summary>
    public abstract class AbstractAsset : ICloneable, IDisposable
    {
        /// <summary>
        /// Global TGI.
        /// </summary>
        public ResourceKey GlobalTGI
        {
            get
            {
                if (Package == null)
                    return TGI;
                return TGI.LocalGroupID(Package.GroupID);
            }
        }
        /// <summary>
        /// Internal TGI, relative to parent package.
        /// </summary>
        public ResourceKey TGI = default(ResourceKey);
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
                // TODO - Compression code is borked atm, throws on terrain asset, so for now we disable this.
                return false;
                //return true;
            }
        }
        bool _compressed = false;
        public bool Disposed = false;

        /// <summary>
        /// Save changes to this asset in memory.
        /// </summary>
        public void Save()
        {
            Package?.Changes?.Set(this);
        }
        /// <summary>
        /// Mark this asset as deleted in memory.
        /// </summary>
        public void Delete()
        {
            Package?.Changes?.Delete(this.TGI);
        }
        /// <summary>
        /// Unmark this asset as deleted in memory.
        /// </summary>
        public void Restore()
        {
            Package?.Changes?.Restore(this.TGI);
        }

        /// <summary>
        /// Makes a copy of the asset.
        /// </summary>
        /// <returns>Copy of the asset.</returns>
        public object Clone()
        {
            var codec = Codecs.Get(TGI.TypeID);
            var serialized = codec.Serialize(this);
            var clone = codec.Deserialize(serialized, TGI, Package);
            clone.TGI = TGI;
            clone.Package = Package;
            clone.Compressed = Compressed;
            return clone;
        }

        public virtual void FreeUnmanagedResources()
        {
            
        }

        public Action OnDispose;

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                Disposed = true;
                OnDispose?.Invoke();
                MemoryController.MarkForRemoval(FreeUnmanagedResources);
            }
        }

        ~AbstractAsset()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
