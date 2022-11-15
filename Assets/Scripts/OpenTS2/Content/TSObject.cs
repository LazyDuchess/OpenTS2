using OpenTS2.Content.DBPF;

namespace OpenTS2.Content
{
    public class TSObject
    {
        public uint GUID => Definition.GUID;
        public ObjectDefinitionAsset Definition => _definition;

        readonly ObjectDefinitionAsset _definition;
        public TSObject(ObjectDefinitionAsset ObjectDefinition)
        {
            _definition = ObjectDefinition;
        }
    }
}
