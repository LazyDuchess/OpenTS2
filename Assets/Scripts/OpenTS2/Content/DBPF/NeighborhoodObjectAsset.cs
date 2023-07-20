using OpenTS2.Files.Formats.XML;

namespace OpenTS2.Content.DBPF
{
    public class NeighborhoodObjectAsset : AbstractAsset
    {
        /// <summary>
        /// The name of the model for this neighborhood object.
        /// </summary>
        public string ModelName { get; }
        /// <summary>
        /// The global unique id for the object.
        /// </summary>
        public uint Guid { get; }

        public NeighborhoodObjectAsset(string modelName, uint guid) => (ModelName, Guid) = (modelName, guid);
    }
}