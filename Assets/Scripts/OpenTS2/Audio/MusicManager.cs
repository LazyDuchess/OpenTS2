using OpenTS2.Common;
using OpenTS2.Common.Utils;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Engine;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Files.Formats.XML;
using OpenTS2.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
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

        public Dictionary<uint, MusicCategory> MusicCategoryByHash = new Dictionary<uint, MusicCategory>();
        public Dictionary<uint, List<Song>> PlaylistByHash = new Dictionary<uint, List<Song>>();

        private ContentProvider _contentProvider;

        public MusicCategory GetMusicCategory (string name)
        {
            var hash = FileUtils.LowHash(name);
            return GetMusicCategory(hash);
        }

        public MusicCategory GetMusicCategory(uint hash)
        {
            if (MusicCategoryByHash.TryGetValue(hash, out var category))
                return category;
            return null;
        }

        public List<Song> GetPlaylist(string name)
        {
            var hash = FileUtils.LowHash(name);
            return GetPlaylist(hash);
        }

        public List<Song> GetPlaylist(uint hash)
        {
            if (PlaylistByHash.TryGetValue(hash, out var playlist))
                return playlist;
            return null;
        }

        private void Awake()
        {
            Instance = this;
            _contentProvider = ContentProvider.Get();
            AudioManager.OnInitialized += Initialize;
        }

        private List<ResourceKey> GetMusicTitles()
        {
            var musicTitlesGroupID = FileUtils.GroupHash("MusicTitles");
            var musicStringSets = _contentProvider.ResourceMap.Keys.Where(x => x.GroupID == musicTitlesGroupID && x.TypeID == TypeIDs.STR).ToList();
            return musicStringSets;
        }

        private void InitializeMusicCategoryPlaylists()
        {
            foreach(var musicCategory in MusicCategoryByHash)
            {
                musicCategory.Value.InitializePlaylist();
            }
        }

        private void LoadPlaylists(List<ResourceKey> musicTitles)
        {
            var audioResourceKeys = AudioManager.AudioAssets;
            foreach(var musicCategory in MusicCategoryByHash)
            {
                var resourceKeys = audioResourceKeys.Where(x => x.GroupID == musicCategory.Key).ToList();
                var songList = new List<Song>();
                foreach(var key in resourceKeys)
                {
                    var localizedName = key.ToString();
                    foreach(var musicTitle in musicTitles)
                    {
                        var stringSet = _contentProvider.GetAsset<StringSetAsset>(musicTitle);
                        var englishStrings = stringSet.StringData.Strings[Languages.USEnglish];

                        for (var i = 0; i < englishStrings.Count; i++)
                        {
                            var englishString = englishStrings[i].Value;
                            var hiHash = FileUtils.HighHash(englishString);
                            if (hiHash == key.InstanceHigh)
                            {
                                localizedName = stringSet.GetString(i);
                            }
                        }
                    }
                    var song = new Song(key, localizedName);
                    songList.Add(song);
                }
                PlaylistByHash[musicCategory.Key] = songList;
            }
        }

        private void LoadMusicCategories(List<ResourceKey> musicTitles)
        {
            var musicCategoriesEntry = _contentProvider.GetEntry(MusicCategoriesXMLKey);
            if (musicCategoriesEntry == null)
                throw new IOException("Can't find Music Categories XML!");
            var musicCategoriesBytes = musicCategoriesEntry.GetBytes();
            var xml = new PropertySet(musicCategoriesBytes);
            if (xml == null)
                throw new IOException("Can't load Music Categories XML!");

            foreach (var prop in xml.Properties)
            {
                var musicCategory = new MusicCategory(prop.Key, ((StringProp)prop.Value).Value.Split(','));
                MusicCategoryByHash[musicCategory.Hash] = musicCategory;
            }

            foreach (var stringSetKey in musicTitles)
            {
                var stringSet = _contentProvider.GetAsset<StringSetAsset>(stringSetKey);

                var englishStrings = stringSet.StringData.Strings[Languages.USEnglish];

                for (var i = 0; i < englishStrings.Count; i++)
                {
                    var englishString = englishStrings[i].Value;
                    var loHash = FileUtils.LowHash(englishString);
                    if (MusicCategoryByHash.TryGetValue(loHash, out var category))
                    {
                        category.LocalizedName = stringSet.GetString(i);
                    }
                }
            }
        }

        private void Initialize()
        {
            var musicTitles = GetMusicTitles();
            LoadMusicCategories(musicTitles);
            LoadPlaylists(musicTitles);
            InitializeMusicCategoryPlaylists();
        }
    }
}
