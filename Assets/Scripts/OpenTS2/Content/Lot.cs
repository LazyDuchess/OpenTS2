using System;
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
        /// <summary>
        /// Path to lot file like ".../Neighborhoods/G001/Lots/G001_Lot3.package"
        /// </summary>
        public string LotPackage { get; }

        public Lot(string lotPackage)
        {
            LotPackage = lotPackage;
        }

        /// <summary>
        /// Gets the scenegraph resource that represents this lot's imposter shown on the neighborhood view.
        /// </summary>
        public ScenegraphResourceAsset GetLotImposterResource()
        {
            var package = ContentProvider.Get().GetPackageByPath(LotPackage);
            foreach (var packageEntry in package.Entries)
            {
                if (packageEntry.TGI.TypeID == TypeIDs.SCENEGRAPH_CRES)
                {
                    return packageEntry.GetAsset<ScenegraphResourceAsset>();
                }
            }
            throw new ArgumentException("Lot does not have imposter resource");
        }
    }
}