using System;
using System.Collections.Generic;
using System.IO;
using OpenTS2.Common;
using OpenTS2.Content.DBPF;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF;
using UnityEngine;

namespace OpenTS2.Content
{
    /// <summary>
    /// Represents a single lot in a neighborhood.
    /// </summary>
    public class Lot
    {
        private LotInfoAsset _lotInfo;

        /// <summary>
        /// Path to lot file like ".../Neighborhoods/G001/Lots/G001_Lot3.package"
        /// </summary>
        public string LotPackagePath { get; }

        private DBPFFile Package => ContentProvider.Get().GetPackageByPath(LotPackagePath);

        public Lot(LotInfoAsset lotInfo, string lotPackage)
        {
            _lotInfo = lotInfo;
            LotPackagePath = lotPackage;
        }

        private ScenegraphResourceAsset GetLotImposter()
        {
            return Package.GetAssetByTGI<ScenegraphResourceAsset>(ResourceKey.ScenegraphResourceKey("imposter_cres", GroupIDs.Local, TypeIDs.SCENEGRAPH_CRES));
        }

        /// <summary>
        /// Renders a gameobject that represents this lot's imposter shown on the neighborhood view.
        /// </summary>
        public GameObject CreateLotImposter()
        {
            var imposter = GetLotImposter();
            if (imposter == null)
            {
                throw new KeyNotFoundException("Lot does not have imposter");
            }
            var gameObject = imposter.CreateGameObjectForShape();
            gameObject.transform.position = new Vector3(
                _lotInfo.LocationX * NeighborhoodTerrainAsset.TerrainGridSize,
                _lotInfo.NeighborhoodToLotHeightOffset,
                _lotInfo.LocationY * NeighborhoodTerrainAsset.TerrainGridSize);

            return gameObject;
        }
    }
}