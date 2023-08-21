namespace OpenTS2.Content.DBPF
{
    /// <summary>
    /// Contains a collection of resource names for terrain textures.
    /// </summary>
    public class LotTexturesAsset : AbstractAsset
    {
        public int Width { get; }
        public int Height { get; }
        public string BaseTexture { get; }
        public string[] BlendTextures { get; }

        public LotTexturesAsset(int width, int height, string baseTexture, string[] blendTextures)
        {
            Width = width;
            Height = height;
            BaseTexture = baseTexture;
            BlendTextures = blendTextures;
        }

        public override string ToString()
        {
            return $"Lot Textures {TGI.InstanceID}: \n {BaseTexture}: {string.Join(", ", BlendTextures)}";
        }
    }
}