using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Engine;
using OpenTS2.Files.Formats.XML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Audio
{
    public class MusicManager : MonoBehaviour
    {
        public static AudioAsset SplashAudio
        {
            get
            {
                var contentProvider = ContentProvider.Get();
                return contentProvider.GetAsset<AudioAsset>(SplashKey);
            }
        }

        public static MusicManager Instance { get; private set; }

        readonly static ResourceKey MusicCategoriesXMLKey = new ResourceKey(0xFFD3706D, 0x38888A91, 0x4C1940C5, 0xEBFEE33F);
        readonly static ResourceKey SplashKey = new ResourceKey(0xFF8DFFC2, 0xE4085DD2, 0xFF8DFFC2, 0x2026960B);
        private ContentProvider _contentProvider;

        private void Awake()
        {
            Instance = this;
            _contentProvider = ContentProvider.Get();
            Core.OnFinishedLoading += LoadMusicCategories;
        }

        private void LoadMusicCategories()
        {
            var musicCategoriesBytes = _contentProvider.GetEntry(MusicCategoriesXMLKey).GetBytes();
            if (musicCategoriesBytes == null)
                throw new IOException("Can't find Music Categories XML!");
            var xml = new PropertySet(musicCategoriesBytes);
            if (xml == null)
                throw new IOException("Can't load Music Categories XML!");
            foreach(var prop in xml.Properties)
            {
                Debug.Log(prop.Key);
            }
        }
    }
}
