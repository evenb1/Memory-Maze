using UnityEngine;
using UnityEngine.AI;

public class ProperMazeGenerator : MonoBehaviour
{
    [Header("Level 3 Maze Settings - Correctly Sized")]
    [SerializeField] private int mazeGridWidth = 10;    // 10x10 grid for Level 3
    [SerializeField] private int mazeGridHeight = 10;   // Bigger than Level 2
    [SerializeField] private float cellSize = 4f;       // Match your floor tile size
    
    [Header("Wall Settings")]
    [SerializeField] private float wallHeight = 2f;
    [SerializeField] private float wallThickness = 0.8f;
    
    [Header("Exit Portal")]
    public GameObject exitPortalPrefab;
    
    [Header("Wall Material")]
    public Material wallMaterial;
    
    [Header("Level 3 Theme")]
    public bool useLevel3Theme = true;
    public Color level3WallColor = new Color(0.9f, 0.8f, 0.2f); // Bright gold/yellow for Level 3
    
    [Header("NavMesh Generation")]
    public bool generateNavMesh = true;
    public int navMeshArea = 0; // Walkable area
    
    // Maze grid (true = wall, false = open path)
    private bool[,] mazeGrid;
    
    // Generated objects for NavMesh
    private GameObject floorParent;
    private GameObject wallParent;
    
    void Start()
    {
        // NOTE: Set FloorGenerator to tilesWide = 10, tilesDeep = 10 for Level 3
        GenerateLevel3Maze();
    }
    
    void GenerateLevel3Maze()
    {
        Debug.Log($"Generating Level 3 Maze - {mazeGridWidth}x{mazeGridHeight} cells (Total: {mazeGridWidth * cellSize}x{mazeGridHeight * cellSize} units)");
        
        // Create parent objects for organization
        CreateParentObjects();
        
        InitializeMazeGrid();
        CreateLevel3MazeLayout();
        CreatePhysicalWalls();
        CreateExitPortal();
        
        // IMPORTANT: Bake NavMesh after maze is created
        if (generateNavMesh)
        {
            BakeNavMeshForMaze();
        }
        
        Debug.Log("Level 3 maze generation complete!");
    }
    
    void CreateParentObjects()
    {
        floorParent = new GameObject("Generated_Floors");
        floorParent.transform.SetParent(transform);
        
        wallParent = new GameObject("Generated_Walls");
        wallParent.transform.SetParent(transform);
    }
    
    void InitializeMazeGrid()
    {
        mazeGrid = new bool[mazeGridWidth, mazeGridHeight];
        
        // Start with everything as walls
        for (int x = 0; x < mazeGridWidth; x++)
        {
            for (int z = 0; z < mazeGridHeight; z++)
            {
                mazeGrid[x, z] = true; // true = wall
            }
        }
    }
    
    void CreateLevel3MazeLayout()
    {
        int centerX = mazeGridWidth / 2;
        int centerZ = mazeGridHeight / 2;
        
        // 1. Clear center area for exit portal (2x2)
        ClearArea(centerX - 1, centerZ - 1, 2, 2);
        
        // 2. Create main pathways - Level 3 is more complex than Level 2
        CreateMainPaths(centerX, centerZ);
        
        // 3. Create corner areas and rooms
        CreateLevel3RoomLayout();
        
        // 4. Add strategic walls for Level 3 difficulty
        AddLevel3StrategicWalls();
        
        // 5. Ensure all areas are reachable
        EnsureAllAreasReachable();
        
        Debug.Log("Level 3 maze layout created - more complex than Level 2");
    }
    
    void CreateMainPaths(int centerX, int centerZ)
    {
        // Main horizontal corridor
        for (int x = 1; x < mazeGridWidth - 1; x++)
        {
            mazeGrid[x, centerZ] = false;
        }
        
        // Main vertical corridor
        for (int z = 1; z < mazeGridHeight - 1; z++)
        {
            mazeGrid[centerX, z] = false;
        }
        
        // Level 3: Add more secondary paths for complexity
        int offset1 = 2;
        int offset2 = 3;
        
        // Multiple horizontal paths
        if (centerZ + offset1 < mazeGridHeight - 1)
        {
            for (int x = 2; x < mazeGridWidth - 2; x++)
            {
                mazeGrid[x, centerZ + offset1] = false;
            }
        }
        
        if (centerZ - offset1 > 0)
        {
            for (int x = 2; x < mazeGridWidth - 2; x++)
            {
                mazeGrid[x, centerZ - offset1] = false;
            }
        }
        
        // Multiple vertical paths
        if (centerX + offset1 < mazeGridWidth - 1)
        {
            for (int z = 2; z < mazeGridHeight - 2; z++)
            {
                mazeGrid[centerX + offset1, z] = false;
            }
        }
        
        if (centerX - offset1 > 0)
        {
            for (int z = 2; z < mazeGridHeight - 2; z++)
            {
                mazeGrid[centerX - offset1, z] = false;
            }
        }
    }
    
    void CreateLevel3RoomLayout()
    {
        // Level 3: More complex room patterns
        
        // Corner rooms
        ClearArea(1, 1, 2, 2);                                    // Bottom-left
        ClearArea(mazeGridWidth - 3, 1, 2, 2);                    // Bottom-right
        ClearArea(1, mazeGridHeight - 3, 2, 2);                   // Top-left
        ClearArea(mazeGridWidth - 3, mazeGridHeight - 3, 2, 2);   // Top-right
        
        // Mid-edge rooms
        ClearArea(mazeGridWidth / 2 - 1, 1, 2, 1);               // Bottom center
        ClearArea(mazeGridWidth / 2 - 1, mazeGridHeight - 2, 2, 1); // Top center
        ClearArea(1, mazeGridHeight / 2 - 1, 1, 2);              // Left center
        ClearArea(mazeGridWidth - 2, mazeGridHeight / 2 - 1, 1, 2); // Right center
    }
    
    void AddLevel3StrategicWalls()
    {
        int centerX = mazeGridWidth / 2;
        int centerZ = mazeGridHeight / 2;
        
        // Level 3: More strategic obstacles but maintain reachability
        if (centerX - 4 > 0 && centerZ - 4 > 0)
            mazeGrid[centerX - 4, centerZ - 4] = true;
        if (centerX + 4 < mazeGridWidth && centerZ + 4 < mazeGridHeight)
            mazeGrid[centerX + 4, centerZ + 4] = true;
        
        // Add some maze-like complexity
        mazeGrid[centerX - 3, centerZ + 1] = true;
        mazeGrid[centerX + 3, centerZ - 1] = true;
    }
    
    void EnsureAllAreasReachable()
    {
        int centerX = mazeGridWidth / 2;
        int centerZ = mazeGridHeight / 2;
        
        // Ensure paths from center to each corner
        CreateDirectPath(centerX, centerZ, 1, 1);
        CreateDirectPath(centerX, centerZ, mazeGridWidth - 2, 1);
        CreateDirectPath(centerX, centerZ, 1, mazeGridHeight - 2);
        CreateDirectPath(centerX, centerZ, mazeGridWidth - 2, mazeGridHeight - 2);
    }
    
    void CreateDirectPath(int fromX, int fromZ, int toX, int toZ)
    {
        int currentX = fromX;
        int currentZ = fromZ;
        
        while (currentX != toX || currentZ != toZ)
        {
            mazeGrid[currentX, currentZ] = false;
            
            if (currentX < toX) currentX++;
            else if (currentX > toX) currentX--;
            else if (currentZ < toZ) currentZ++;
            else if (currentZ > toZ) currentZ--;
        }
        mazeGrid[toX, toZ] = false;
    }
    
    void ClearArea(int startX, int startZ, int width, int height)
    {
        for (int x = startX; x < startX + width && x < mazeGridWidth; x++)
        {
            for (int z = startZ; z < startZ + height && z < mazeGridHeight; z++)
            {
                if (x >= 0 && z >= 0)
                {
                    mazeGrid[x, z] = false;
                }
            }
        }
    }
    
    void CreatePhysicalWalls()
    {
        for (int x = 0; x < mazeGridWidth; x++)
        {
            for (int z = 0; z < mazeGridHeight; z++)
            {
                if (mazeGrid[x, z]) // true = wall
                {
                    CreateWallAt(x, z);
                }
                else
                {
                    // Create floor navigation areas
                    CreateFloorNavArea(x, z);
                }
            }
        }
        
        Debug.Log($"Created walls and floor nav areas for {mazeGridWidth}x{mazeGridHeight} maze");
    }
    
    void CreateWallAt(int gridX, int gridZ)
    {
        float worldX = (gridX - mazeGridWidth / 2f) * cellSize + (cellSize / 2f);
        float worldZ = (gridZ - mazeGridHeight / 2f) * cellSize + (cellSize / 2f);
        Vector3 position = new Vector3(worldX, wallHeight / 2f, worldZ);
        
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = $"Level3_Wall_{gridX}_{gridZ}";
        wall.tag = "Wall";
        wall.transform.position = position;
        wall.transform.localScale = new Vector3(cellSize, wallHeight, cellSize);
        wall.transform.SetParent(wallParent.transform);
        
        // Apply Level 3 material
        Renderer renderer = wall.GetComponent<Renderer>();
        if (wallMaterial != null)
        {
            renderer.material = wallMaterial;
        }
        else if (useLevel3Theme)
        {
            Material level3Mat = new Material(Shader.Find("Standard"));
            level3Mat.color = level3WallColor;
            level3Mat.EnableKeyword("_EMISSION");
            level3Mat.SetColor("_EmissionColor", level3WallColor * 0.4f);
            level3Mat.SetFloat("_Metallic", 0.3f);
            level3Mat.SetFloat("_Smoothness", 0.8f);
            renderer.material = level3Mat;
        }
        
        // Important: Walls should NOT be navigation obstacles for NavMesh
        // We'll handle navigation blocking through colliders instead
    }
    
    void CreateFloorNavArea(int gridX, int gridZ)
    {
        // Create invisible floor areas for NavMesh baking
        float worldX = (gridX - mazeGridWidth / 2f) * cellSize + (cellSize / 2f);
        float worldZ = (gridZ - mazeGridHeight / 2f) * cellSize + (cellSize / 2f);
        Vector3 position = new Vector3(worldX, 0.01f, worldZ); // Slightly above ground
        
        GameObject floorArea = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floorArea.name = $"NavFloor_{gridX}_{gridZ}";
        floorArea.transform.position = position;
        floorArea.transform.localScale = new Vector3(cellSize - 0.1f, 0.02f, cellSize - 0.1f);
        floorArea.transform.SetParent(floorParent.transform);
        
        // Make it walkable for NavMesh (simple approach)
        floorArea.layer = 0; // Default layer for NavMesh
        
        // Make invisible
        Renderer renderer = floorArea.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = false; // Invisible but still used for NavMesh
        }
        
        // Remove collider (not needed for NavMesh)
        Collider col = floorArea.GetComponent<Collider>();
        if (col != null)
        {
            Destroy(col);
        }
    }
    
    void BakeNavMeshForMaze()
    {
        Debug.Log("Baking NavMesh for generated maze...");
        
        // Force NavMesh baking using Unity's AI Navigation system
        #if UNITY_EDITOR
        // In editor, we can use UnityEditor.AI.NavMeshBuilder
        UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
        #else
        // At runtime, NavMesh should be pre-baked or use NavMesh Surface component
        Debug.LogWarning("Runtime NavMesh baking not available. NavMesh should be pre-baked.");
        #endif
        
        Debug.Log("NavMesh baking complete!");
    }
    
    void CreateExitPortal()
    {
        if (exitPortalPrefab != null)
        {
            GameObject portal = Instantiate(exitPortalPrefab);
            portal.name = "Level3_ExitPortal";
            portal.transform.position = new Vector3(0, 0.1f, 0);
            portal.transform.localScale = new Vector3(1.3f, 1f, 1.3f); // Slightly larger for Level 3
            
            SetupPortalComponents(portal);
            portal.transform.SetParent(transform);
            
            Debug.Log("Level 3 exit portal created at center");
        }
    }
    
    void SetupPortalComponents(GameObject portal)
    {
        Collider portalCollider = portal.GetComponent<Collider>();
        if (portalCollider == null)
        {
            portalCollider = portal.AddComponent<BoxCollider>();
            portalCollider.isTrigger = true;
            ((BoxCollider)portalCollider).size = new Vector3(4f, 4f, 4f);
        }
        
        ExitTrigger exitTrigger = portal.GetComponent<ExitTrigger>();
        if (exitTrigger == null)
        {
            exitTrigger = portal.AddComponent<ExitTrigger>();
        }
        exitTrigger.currentLevel = 3;
        
        PortalBeacon beacon = portal.GetComponent<PortalBeacon>();
        if (beacon == null)
        {
            beacon = portal.AddComponent<PortalBeacon>();
        }
        
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.exitPlatform = portal;
        }
    }
    
    // Helper methods for other systems
    public Vector3[] GetValidSpawnPositions(int count)
    {
        System.Collections.Generic.List<Vector3> validPositions = new System.Collections.Generic.List<Vector3>();
        
        for (int x = 0; x < mazeGridWidth; x++)
        {
            for (int z = 0; z < mazeGridHeight; z++)
            {
                if (!mazeGrid[x, z]) // false = open path (NOT a wall)
                {
                    float worldX = (x - mazeGridWidth / 2f) * cellSize + (cellSize / 2f);
                    float worldZ = (z - mazeGridHeight / 2f) * cellSize + (cellSize / 2f);
                    Vector3 position = new Vector3(worldX, 1f, worldZ);
                    
                    // Don't spawn too close to center (exit portal area)
                    if (Vector3.Distance(position, Vector3.zero) > 6f)
                    {
                        validPositions.Add(position);
                    }
                }
            }
        }
        
        // Return random selection
        System.Collections.Generic.List<Vector3> selectedPositions = new System.Collections.Generic.List<Vector3>();
        for (int i = 0; i < count && validPositions.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, validPositions.Count);
            selectedPositions.Add(validPositions[randomIndex]);
            validPositions.RemoveAt(randomIndex);
        }
        
        return selectedPositions.ToArray();
    }
    
    public bool IsWallAt(Vector3 worldPosition)
    {
        int gridX = Mathf.RoundToInt((worldPosition.x / cellSize) + mazeGridWidth / 2f - 0.5f);
        int gridZ = Mathf.RoundToInt((worldPosition.z / cellSize) + mazeGridHeight / 2f - 0.5f);
        
        if (gridX < 0 || gridX >= mazeGridWidth || gridZ < 0 || gridZ >= mazeGridHeight)
            return true;
            
        return mazeGrid[gridX, gridZ];
    }
}