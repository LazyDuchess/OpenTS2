using OpenTS2.Audio;
using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Diagnostic
{
    public class PlayMusicCategoryCheat : Cheat
    {
        public override string Name => "playMusicCategory";
        public override string Description => "Makes the MusicController play a specific MusicCategory.";

        public override void Execute(CheatArguments arguments, IConsoleOutput output = null)
        {
            var musicController = MusicController.Instance;
            if (musicController == null) return;
            if (arguments.Count < 2) return;
            musicController.StartMusicCategory(arguments.GetString(1));
        }
    }
}
