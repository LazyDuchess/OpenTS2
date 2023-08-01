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

            // Rotate based on the whether the frontEdge has changed from the lot's creation time.
            var rotation = (_lotInfo.CreationFrontEdge - _lotInfo.FrontEdge) * -90;

            // Lot imposters are always stored rotated as per their CreationFrontEdge. So a lot facing a road in the
            // positive x direction, i.e the "front" of the house is towards positive y will have its imposter model
            // have the house starting at (0, 0) and ending at (width, depth).
            //
            // However, regardless of rotation, a lot in the neighborhood is stored with its grid coordinates
            // corresponding to the bottom-right of the grid. Thus when a lot is rotated, we need to rotate the imposter
            // from the correct pivot point.
            var frontEdgeDiff = (_lotInfo.CreationFrontEdge - _lotInfo.FrontEdge) % 4;

            Vector3 pivot;
            if (frontEdgeDiff == 0) // No rotation.
                pivot = new Vector3(0, 0, 0);
            else if (frontEdgeDiff == -1 || frontEdgeDiff == 3) // Counter-clockwise 90
                pivot = new Vector3(_lotInfo.WorldWidth, 0, 0);
            else if (frontEdgeDiff == 1 || frontEdgeDiff == -3) // Clockwise 90
                pivot = new Vector3(0, 0, _lotInfo.WorldDepth);
            else if (frontEdgeDiff == 2 || frontEdgeDiff == -2) // Full 180 rotation.
                pivot = new Vector3(_lotInfo.WorldWidth, 0, _lotInfo.WorldDepth);
            else
                throw new IndexOutOfRangeException();

            var position = new GameObject($"imposter_position_{_lotInfo.LotName}")
            {
                transform =
                {
                    position = pivot
                }
            };
            gameObject.transform.SetParent(position.transform, worldPositionStays:true);
            position.transform.position = new Vector3(_lotInfo.WorldLocationX, _lotInfo.NeighborhoodToLotHeightOffset, _lotInfo.WorldLocationY);
            position.transform.Rotate(0, rotation, 0);

            return position;
        }
    }
}