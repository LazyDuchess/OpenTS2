namespace OpenTS2.Content.DBPF.Effects
{
    public readonly struct WaterEffect : IBaseEffect
    {
        public readonly uint Flags;

        public WaterEffect(uint flags)
        {
            Flags = flags;
        }
    }
}