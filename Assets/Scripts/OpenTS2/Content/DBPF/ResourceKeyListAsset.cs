using OpenTS2.Common;
using System.Collections.Generic;

namespace OpenTS2.Content.DBPF
{
    /// <summary>
    /// cTSPersistResKeyList - a flat list of resource keys.
    ///
    /// Used as a sidecar resource sharing the group/instance of another resource, letting that resource
    /// refer to keys in this list by index instead of embedding a full TGI for each reference.
    /// </summary>
    public class ResourceKeyListAsset : AbstractAsset
    {
        public List<ResourceKey> Keys = new List<ResourceKey>();
    }
}
