using OpenTS2.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content
{
    public class GameGlobals
    {
        [GameProperty(true)]
        public static bool allowCustomContent = true;
        public static GameGlobals Instance { get; private set; }
        public Languages Language = Languages.USEnglish;

        public GameGlobals()
        {
            Instance = this;
        }
    }
}
