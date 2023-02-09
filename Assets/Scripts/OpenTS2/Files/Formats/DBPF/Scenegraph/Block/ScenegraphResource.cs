using System;
using OpenTS2.Files.Utils;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph.Block
{
    /// <summary>
    /// A common header type (cSGResource) used to give names to Scenegraph resources.
    /// </summary>
    public readonly struct ScenegraphResource
    {
        private readonly PersistTypeInfo _resourceTypeInfo;
        public string ResourceName { get; }

        private ScenegraphResource(PersistTypeInfo typeInfo, string resourceName)
        {
            if (typeInfo.Name != "cSGResource")
            {
                throw new Exception("Attempted to load a ScenegraphResource whose type was not cSGResource");
            }

            _resourceTypeInfo = typeInfo;
            ResourceName = resourceName;
        }

        public static ScenegraphResource Deserialize(IoBuffer reader)
        {
            return new ScenegraphResource(PersistTypeInfo.Deserialize(reader), reader.ReadVariableLengthPascalString());
        }
        
        public override string ToString()
        {
            return $"{nameof(_resourceTypeInfo)}: {_resourceTypeInfo}, {nameof(ResourceName)}: {ResourceName}";
        }
    }
}