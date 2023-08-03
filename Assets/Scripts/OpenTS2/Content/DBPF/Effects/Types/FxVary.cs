namespace OpenTS2.Content.DBPF.Effects.Types
{
    public static class FxVary
    {
        /// <summary>
        /// This calculation is how the game decides how much to vary a value. The vary parameter gives a percentage
        /// of how much the original value can be fudged in either direction.
        /// </summary>
        public static (float, float) CalculateVary(float value, float vary)
        {
            return (VaryValueMin(value, vary), VaryValueMax(value, vary));
        }

        public static float VaryValueMin(float value, float vary)
        {
            return (1 - vary) * value;
        }

        public static float VaryValueMax(float value, float vary)
        {
            return (1 + vary) * value;
        }
    }
}