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
        public Texture2D Thumbnail => _thumbnail;
        public string Folder => _folder;
        public string ReiaPath => Path.Combine(Filesystem.GetUserPath(), "Neighborhoods", Folder, $"{Folder}_Neighborhood.reia");
        private Texture2D _thumbnail;
        private string _folder;
        private StringSetAsset _stringSet;
        public Neighborhood(string folder)
        {
            _folder = folder;
            var neighborhoodGroup = $"{_folder}_Neighborhood";
            var stringSetKey = new ResourceKey(1, neighborhoodGroup, TypeIDs.CTSS);
            var contentProvider = ContentProvider.Get();
            _stringSet = contentProvider.GetAsset<StringSetAsset>(stringSetKey);
            _thumbnail = new Texture2D(1, 1);
            var thumbnailPath = Path.Combine(Filesystem.GetUserPath(), "Neighborhoods", Folder, $"{Folder}_Neighborhood.png");
            if (File.Exists(thumbnailPath))
                _thumbnail.LoadImage(File.ReadAllBytes(thumbnailPath));
        }

        public static Neighborhood Load(string folder)
        {
            var fullPathToPackage = Path.Combine(Filesystem.GetUserPath(), "Neighborhoods", folder, $"{folder}_Neighborhood.package");
            if (!File.Exists(fullPathToPackage))
                return null;
            return new Neighborhood(folder);
        }

        public string GetLocalizedName()
        {
            if (_stringSet == null)
                return Folder;
            return _stringSet.GetString(0);
        }

        public string GetLocalizedDescription()
        {
            if (_stringSet == null)
                return "";
            return _stringSet.GetString(1);
        }
    }
}
