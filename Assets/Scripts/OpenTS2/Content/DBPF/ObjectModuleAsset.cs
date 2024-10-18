namespace OpenTS2.Content.DBPF
{
    /// <summary>
    /// Called cEdithObjectModule in game.
    /// </summary>
    public class ObjectModuleAsset : AbstractAsset
    {
        public ObjectModuleAsset(int version)
        {
            Version = version;
        }

        public int Version { get; }
    }
}