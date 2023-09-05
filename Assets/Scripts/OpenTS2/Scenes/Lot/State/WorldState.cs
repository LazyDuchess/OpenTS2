namespace OpenTS2.Scenes.Lot.State
{
    public enum WallsMode
    {
        Down,
        Cutaway,
        Up,
        Roof
    }

    public readonly struct WorldState
    {
        public readonly int Level;
        public readonly WallsMode Walls;

        public WorldState(int level, WallsMode walls)
        {
            Level = level;
            Walls = walls;
        }
    }
}