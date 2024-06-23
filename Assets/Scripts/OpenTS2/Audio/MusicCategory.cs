using OpenTS2.Common;
using OpenTS2.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Audio
{
    public class MusicCategory
    {
        public List<Song> CurrentPlaylist;
        public string LocalizedName;
        public string Name { get; private set; }
        public uint Hash { get; private set; }
        private string[] _rawPlaylist;

        public MusicCategory(string name, string[] playlist)
        {
            Name = name;
            Hash = FileUtils.LowHash(name);
            _rawPlaylist = playlist;
        }

        public void InitializePlaylist()
        {
            CurrentPlaylist = new List<Song>();
            var musicManager = MusicManager.Instance;
            foreach(var playlistName in _rawPlaylist)
            {
                var playList = new List<Song>(musicManager.GetPlaylist(playlistName));
                playList.Shuffle();
                CurrentPlaylist.AddRange(playList);
            }
        }
    }
}
