using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using OpenTS2.Content;
using OpenTS2.Files;
using System.IO;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF;

public class LotImposterGMDCTest : MonoBehaviour
{
    public string NeighborhoodPrefix = "N001";
    public int LotID = 11;
    private void Start()
    {
        var contentManager = ContentManager.Instance;
        var lotsFolderPath = Path.Combine(Filesystem.GetUserPath(), $"Neighborhoods/{NeighborhoodPrefix}/Lots");
        var lotFilename = $"{NeighborhoodPrefix}_Lot{LotID}.package";
        var lotFullPath = Path.Combine(lotsFolderPath, lotFilename);
        contentManager.AddPackage(lotFullPath);

        var lotImposterResource = contentManager.GetAssetsOfType<ScenegraphResourceAsset>(TypeIDs.SCENEGRAPH_CRES)[0];

        var gameObject = lotImposterResource.CreateRootGameObject();
    }
}
