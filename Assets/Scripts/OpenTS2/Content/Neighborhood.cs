using OpenTS2.Client;
using OpenTS2.Common;
using OpenTS2.Content.DBPF;
using OpenTS2.Files;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Content
{
    public class Neighborhood
    {
        public NeighborhoodTerrainAsset Terrain => ContentProvider.Get().GetAsset<NeighborhoodTerrainAsset>(new ResourceKey(0, GroupID, TypeIDs.NHOOD_TERRAIN));
        public Type NeighborhoodType => _info.NeighborhoodType;
        public string Prefix => _info.MainPrefix;
        public string FilePath => _info.Package.FilePath;
        public uint GroupID => _info.Package.GroupID;
        public Texture2D Thumbnail => _thumbnail;
        public string ReiaPath => Path.ChangeExtension(FilePath, ".reia");
        public enum Type
        {
            Main = 1,
            Campus,
            Downtown,
            Suburb,
            Village,
            Lakes,
            Island
        }
        private Texture2D _thumbnail;
        private StringSetAsset _stringSet;
        private NeighborhoodInfoAsset _info;
        public Neighborhood(NeighborhoodInfoAsset infoAsset)
        {
            _info = infoAsset;
            var contentProvider = ContentProvider.Get();
            var stringSetKey = new ResourceKey(1, GroupID, TypeIDs.CTSS);
            _stringSet = contentProvider.GetAsset<StringSetAsset>(stringSetKey);
            _thumbnail = new Texture2D(1, 1);
            var thumbnailPath = Path.ChangeExtension(FilePath, ".png");
            if (File.Exists(thumbnailPath))
                _thumbnail.LoadImage(File.ReadAllBytes(thumbnailPath));
        }

        public string GetLocalizedName()
        {
            if (_stringSet == null)
                return Prefix;
            return _stringSet.GetString(0);
        }

        public string GetLocalizedDescription()
        {
            if (_stringSet == null)
                return "";
            return _stringSet.GetString(1);
        }

        public void SetName(string name)
        {
            if (_stringSet == null)
                return;
            _stringSet.SetString(name, 0);
            _stringSet.Save();
        }

        public void SetDescription(string desc)
        {
            if (_stringSet == null)
                return;
            _stringSet.SetString(desc, 1);
            _stringSet.Save();
        }
    }
}
