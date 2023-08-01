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
        public const float MaxLotSize = 64f;

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
            return Package.GetAssetByTGI<ScenegraphResourceAsset>(
                ResourceKey.ScenegraphResourceKey("imposter_cres", GroupIDs.Local, TypeIDs.SCENEGRAPH_CRES));
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

            var gameObject = imposter.CreateRootGameObject();

            gameObject.transform.position = new Vector3(
                _lotInfo.LocationX * NeighborhoodTerrainAsset.TerrainGridSize,
                _lotInfo.NeighborhoodToLotHeightOffset,
                _lotInfo.LocationY * NeighborhoodTerrainAsset.TerrainGridSize);

            // We have to create a GameObject right at the center of the lot so we can pivot our rotation around the
            // center instead of at the corner.
            var rotationObject = new GameObject("imposter_rotation")
            {
                transform =
                {
                    position = gameObject.transform.position
                }
            };
            // TODO: this isn't quite right, for some lots this doesn't end up at dead center, look into why.
            rotationObject.transform.position +=
                new Vector3(_lotInfo.Width * NeighborhoodTerrainAsset.TerrainGridSize / 2.0f, 0,
                    _lotInfo.Depth * NeighborhoodTerrainAsset.TerrainGridSize / 2.0f);
            gameObject.transform.SetParent(rotationObject.transform);

            // Rotate based on the whether the frontEdge has changed from creation time.
            var rotation = (_lotInfo.CreationFrontEdge - _lotInfo.FrontEdge) * -90;
            rotationObject.transform.Rotate(0, rotation, 0);

            return rotationObject;
        }
    }
}