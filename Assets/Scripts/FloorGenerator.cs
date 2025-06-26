using UnityEngine;

public class FloorGenerator : MonoBehaviour
{
    public GameObject floorTilePrefab; // Drag your floor tile here
    public int tilesWide = 5;
    public int tilesDeep = 5;
    public float tileSize = 4f; // Since your tile is 4x4
    
    void Start()
    {
        GenerateFloor();
    }
    
    void GenerateFloor()
    {
        for(int x = 0; x < tilesWide; x++)
        {
            for(int z = 0; z < tilesDeep; z++)
            {
                // Calculate position for each tile
                Vector3 position = new Vector3(
                    (x * tileSize) - (tilesWide * tileSize / 2f) + (tileSize / 2f),
                    0,
                    (z * tileSize) - (tilesDeep * tileSize / 2f) + (tileSize / 2f)
                );
                
                // Create the tile
                GameObject tile = Instantiate(floorTilePrefab, position, Quaternion.identity);
                tile.transform.SetParent(transform);
                tile.name = "FloorTile_" + x + "_" + z;
            }
        }
    }
}