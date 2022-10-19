using OpenTS2.Content.DBPF;

namespace OpenTS2.Content
{
    public class TSObject
    {
        public uint GUID => Definition.guid;
        public ObjectDefinitionAsset Definition => _Definition;
        ObjectDefinitionAsset _Definition;
        public TSObject(ObjectDefinitionAsset ObjectDefinition)
        {
            _Definition = ObjectDefinition;
        }
    }
}
