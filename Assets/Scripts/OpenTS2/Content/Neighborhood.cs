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
        public NeighborhoodDecorationsAsset Decorations => ContentProvider.Get()
            .GetAsset<NeighborhoodDecorationsAsset>(new ResourceKey(0, GroupID, TypeIDs.NHOOD_DECORATIONS));
        public NeighborhoodTerrainAsset Terrain => ContentProvider.Get().GetAsset<NeighborhoodTerrainAsset>(new ResourceKey(0, GroupID, TypeIDs.NHOOD_TERRAIN));
        public Type NeighborhoodType => _info.NeighborhoodType;
        public string Prefix => _info.MainPrefix;
        public string PackageFilePath => _info.Package.FilePath;
        public string FolderPath => Directory.GetParent(PackageFilePath).FullName;
        public uint GroupID => _info.Package.GroupID;
        public Texture2D Thumbnail => _thumbnail;
        public string ReiaPath => Path.ChangeExtension(PackageFilePath, ".reia");
        public List<Lot> Lots { get; } = new List<Lot>();
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
            var thumbnailPath = Path.ChangeExtension(PackageFilePath, ".png");
            if (File.Exists(thumbnailPath))
                _thumbnail.LoadImage(File.ReadAllBytes(thumbnailPath));

            foreach (var entry in _info.Package.Entries)
            {
                if (entry.TGI.TypeID != TypeIDs.LOT_INFO)
                {
                    continue;
                }

                var lotAsset = entry.GetAsset<LotInfoAsset>();
                var lotPackage = Path.Combine(FolderPath, "Lots", $"{Prefix}_Lot{lotAsset.LotId}.package");
                Lots.Add(new Lot(lotAsset, lotPackage));
            }
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
