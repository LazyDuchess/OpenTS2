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
            Debug.Log($"{_lotInfo.LotName}, creationFront: {_lotInfo.CreationFrontEdge}, frontEdge: {_lotInfo.FrontEdge}, Width: {_lotInfo.Width}, Depth: {_lotInfo.Depth}, Flags: {_lotInfo.Flags:X}, Edges: {_lotInfo.RoadsAlongEdges:X}, Type: {_lotInfo.LotType:X}");
            var rotationObject = new GameObject("imposter_rotation_" + _lotInfo.LotName)
            {
                transform =
                {
                    position = gameObject.transform.position
                }
            };
            var lotCenter = _lotInfo.GetLotCenter();
            rotationObject.transform.position +=
                new Vector3(lotCenter.Item1 * NeighborhoodTerrainAsset.TerrainGridSize,
                    0,
                    lotCenter.Item2 * NeighborhoodTerrainAsset.TerrainGridSize);
            gameObject.transform.SetParent(rotationObject.transform);

            // Rotate based on the whether the frontEdge has changed from the lot's creation time.
            var rotation = (_lotInfo.CreationFrontEdge - _lotInfo.FrontEdge) * -90;
            rotationObject.transform.Rotate(0, rotation, 0);

            // For debugging...
            var positionRef = new GameObject("imposter_debug_position_" + _lotInfo.LotName);
            positionRef.transform.position = new Vector3(
                _lotInfo.LocationX * NeighborhoodTerrainAsset.TerrainGridSize,
                _lotInfo.NeighborhoodToLotHeightOffset,
                _lotInfo.LocationY * NeighborhoodTerrainAsset.TerrainGridSize);

            return gameObject;
        }
    }
}