using System.Collections.Generic;

namespace OpenTS2.Content.DBPF
{
    /// <summary>
    /// Called cEdithObjectModule in game.
    /// </summary>
    public class ObjectModuleAsset : AbstractAsset
    {
        public ObjectModuleAsset(int version, Dictionary<int, int> objectIdToSaveType)
        {
            Version = version;
            ObjectIdToSaveType = objectIdToSaveType;
        }

        public int Version { get; }

        public Dictionary<int, int> ObjectIdToSaveType { get; }
    }
}