using OpenTS2.Audio;
using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Diagnostic
{
    public class PauseMusicCheat : Cheat
    {
        public override string Name => "pauseMusic";
        public override string Description => "Pauses(true) or unpauses(false) the music.";

        public override void Execute(CheatArguments arguments, IConsoleOutput output = null)
        {
            var musicController = MusicController.Instance;
            if (musicController == null) return;
            if (arguments.Count < 2) return;
            if (arguments.GetBool(1))
                musicController.Pause();
            else
                musicController.Resume();
        }
    }
}
