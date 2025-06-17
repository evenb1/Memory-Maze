using UnityEngine;

public class ProperMazeGenerator : MonoBehaviour
{
    void Start()
    {
        GenerateRealMaze();
    }
    
    void GenerateRealMaze()
    {
        // Create the floor first
        CreateFloor();
        
        // Create the outer boundary walls (completely enclosed)
        CreateBoundaryWalls();
        
        // Create inner maze walls
        CreateInnerMazeWalls();
        
        // Create the single exit
        CreateExit();
    }
    
    void CreateFloor()
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "MazeFloor";
        floor.transform.position = new Vector3(0, 0, 0);
        floor.transform.localScale = new Vector3(2, 1, 2); // 20x20 unit floor
    }
    
void CreateBoundaryWalls()
{
    // North wall (top) - completely solid
    for(int x = -9; x <= 9; x += 2)
    {
        CreateWall(x, 1, 9, new Vector3(2.1f, 2, 0.5f)); // Made slightly bigger to avoid gaps
    }
    
    // South wall (bottom) - completely solid
    for(int x = -9; x <= 9; x += 2)
    {
        CreateWall(x, 1, -9, new Vector3(2.1f, 2, 0.5f));
    }
    
    // East wall (right) - completely solid
    for(int z = -9; z <= 9; z += 2)
    {
        CreateWall(9, 1, z, new Vector3(0.5f, 2, 2.1f));
    }
    
    // West wall (left) - with ONE exit gap
    for(int z = -9; z <= 9; z += 2)
    {
        if(z != 1) // This creates the exit at position z = 1
        {
            CreateWall(-9, 1, z, new Vector3(0.5f, 2, 2.1f));
        }
    }
}
    
    void CreateInnerMazeWalls()
    {
        // Horizontal inner walls
        CreateWall(-4, 1, 6, new Vector3(4, 2, 0.3f));
        CreateWall(2, 1, 4, new Vector3(6, 2, 0.3f));
        CreateWall(-6, 1, 2, new Vector3(2, 2, 0.3f));
        CreateWall(4, 1, -2, new Vector3(4, 2, 0.3f));
        CreateWall(-2, 1, -4, new Vector3(4, 2, 0.3f));
        CreateWall(6, 1, -6, new Vector3(2, 2, 0.3f));
        
        // Vertical inner walls  
        CreateWall(-6, 1, -6, new Vector3(0.3f, 2, 4));
        CreateWall(0, 1, 6, new Vector3(0.3f, 2, 2));
        CreateWall(6, 1, 2, new Vector3(0.3f, 2, 6));
        CreateWall(-2, 1, 0, new Vector3(0.3f, 2, 4));
        CreateWall(2, 1, -6, new Vector3(0.3f, 2, 2));
    }
    
    void CreateExit()
    {
        // Exit is already created by skipping a wall section in the West boundary
        // Let's mark it with a different colored wall
        GameObject exitMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        exitMarker.name = "EXIT";
        exitMarker.transform.position = new Vector3(-9, 0.1f, 1);
        exitMarker.transform.localScale = new Vector3(0.3f, 0.2f, 2);
        
        // Make it green so it's obvious
        Renderer renderer = exitMarker.GetComponent<Renderer>();
        renderer.material.color = Color.green;
    }
    
    void CreateWall(float x, float y, float z, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "MazeWall";
        wall.transform.position = new Vector3(x, y, z);
        wall.transform.localScale = scale;
        
        // Make walls gray
        Renderer renderer = wall.GetComponent<Renderer>();
        renderer.material.color = Color.gray;
    }
}