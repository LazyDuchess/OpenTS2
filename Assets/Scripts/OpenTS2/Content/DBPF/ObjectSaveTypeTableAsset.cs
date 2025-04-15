using System.Collections.Generic;

namespace OpenTS2.Content.DBPF
{
    /// <summary>
    /// ObjectSaveTypeTable. Saves "object selectors" which seem to be an abstract identifier for objects, though
    /// usually just uses the GUID.
    /// </summary>
    public class ObjectSaveTypeTableAsset : AbstractAsset
    {
        public List<ObjectSelector> Selectors { get; }

        public ObjectSaveTypeTableAsset(List<ObjectSelector> entries)
        {
            Selectors = entries;
        }

        public readonly struct ObjectSelector
        {
            public readonly uint objectGuid;
            public readonly int saveType;
            public readonly string catalogResourceName;

            public ObjectSelector(uint objectGuid, int saveType, string catalogResourceName)
            {
                this.objectGuid = objectGuid;
                this.saveType = saveType;
                this.catalogResourceName = catalogResourceName;
            }
        }
    }
}