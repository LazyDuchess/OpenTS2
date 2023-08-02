namespace OpenTS2.Content.DBPF.Effects
{
    public readonly struct CameraEffect : IBaseEffect
    {
        public readonly float Life;
        public readonly float ShakeAspect;
        public readonly string CameraSelectName;

        public CameraEffect(float life, float shakeAspect, string cameraSelectName)
        {
            Life = life;
            ShakeAspect = shakeAspect;
            CameraSelectName = cameraSelectName;
        }
    }
}