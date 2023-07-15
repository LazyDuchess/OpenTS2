using System;
using System.Collections;
using System.Collections.Generic;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ScenegraphGMDCTest : MonoBehaviour
{
    
    public string PackageToLoad = "TestAssets/Scenegraph/teapot_model.package";
    public string ModelName = "teapot_tslocator_gmdc";
    public string PrimitiveToShow = "teapot";
    
    private void Start()
    {
        Debug.Log($"Loading package from {PackageToLoad}");
        var contentProvider = ContentProvider.Get();
        contentProvider.AddPackage(PackageToLoad);
        
        var scenegraphModel = contentProvider.GetAsset<ScenegraphModelAsset>(
            new ResourceKey(ModelName, 0x1C0532FA, TypeIDs.SCENEGRAPH_GMDC));
        
        Debug.Log($"scenegraphModel: {scenegraphModel.GlobalTGI}");
        Debug.Log($"primitives: {string.Join(" ", scenegraphModel.Primitives.Keys)}");

        if (scenegraphModel.Primitives.ContainsKey(PrimitiveToShow))
        {
            GetComponent<MeshFilter>().mesh = scenegraphModel.Primitives[PrimitiveToShow];
        }
        else
        {
            GetComponent<MeshFilter>().mesh = scenegraphModel.StaticBoundMesh;
        }
    }
}
