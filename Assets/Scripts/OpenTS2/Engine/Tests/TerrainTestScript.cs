using System.IO;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;
using UnityEngine;

public class TerrainTestScript : MonoBehaviour
{
    public Terrain terrain;

    public MeshFilter terrainMeshFilter;
    
    public string PackageToLoad = "%UserDataDir%/Neighborhoods/N001/N001_Neighborhood.package";

    // Start is called before the first frame update
    void Start()
    {
        var contentProvider = ContentProvider.Get();
        contentProvider.AddPackage(PackageToLoad);

        // Use N001_Neighborhood etc as the group name.
        var groupName = Path.GetFileNameWithoutExtension(PackageToLoad);

        var terrainAsset =
            contentProvider.GetAsset<NeighborhoodTerrainAsset>(
                new ResourceKey(0x0, groupName, TypeIDs.NHOOD_TERRAIN));
        //terrainAsset.ApplyToTerrain(terrain);
        terrainMeshFilter.sharedMesh = terrainAsset.MakeMesh();
    }
    
}