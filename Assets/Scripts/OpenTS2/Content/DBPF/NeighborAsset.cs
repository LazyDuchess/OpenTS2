using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content.DBPF
{
    public class NeighborAsset : AbstractAsset
    {
        public short Id = 0;
        public uint GUID;
        public ObjectDefinitionAsset ObjectDefinition;

        public NeighborAsset(short neighborId, uint objectGuid)
        {
            Id  = neighborId;
            GUID = objectGuid;
            ObjectDefinition = ObjectManager.Instance.GetObjectByGUID(GUID);
        }
    }
}
