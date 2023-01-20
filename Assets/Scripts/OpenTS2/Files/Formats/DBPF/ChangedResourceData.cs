using OpenTS2.Common;
using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Files.Formats.DBPF
{
    /// <summary>
    /// Holds the actual data of a changed DBPF Resource.
    /// </summary>
    public abstract class ChangedResourceData
    {
        public abstract byte[] GetBytes();
        public abstract AbstractAsset GetAsset();
        public abstract uint FileSize { get; }
        public T GetAsset<T>() where T : AbstractAsset
        {
            return GetAsset() as T;
        }
    }
}
