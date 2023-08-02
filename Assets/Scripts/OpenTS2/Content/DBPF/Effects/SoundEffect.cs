namespace OpenTS2.Content.DBPF.Effects
{
    public readonly struct SoundEffect : IBaseEffect
    {
        public readonly uint AudioId;
        public readonly float Volume;

        public SoundEffect(uint audioId, float volume)
        {
            AudioId = audioId;
            Volume = volume;
        }
    }
}