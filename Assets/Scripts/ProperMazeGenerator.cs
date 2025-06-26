using UnityEngine;

public class ProperMazeGenerator : MonoBehaviour
{
    [Header("Exit Portal")]
    public GameObject exitPortalPrefab;
    
    [Header("Wall Material")]
    public Material wallMaterial; // Drag TL_SS_Wall_01_Damage.mat here
    
    void Start()
    {
        GenerateRealMaze();
    }
    
    void GenerateRealMaze()
    {
        CreateFloor();
        CreateBoundaryWalls();
        CreateInnerMazeWalls();
        CreateExitPortal();
    }
    
    void CreateFloor()
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "MazeFloor";
        floor.transform.position = new Vector3(0, 0, 0);
        floor.transform.localScale = new Vector3(2, 1, 2);
    }
    
    void CreateBoundaryWalls()
    {
        // North wall
        for(int x = -9; x <= 9; x += 2)
        {
            CreateWall(x, 1, 9, new Vector3(2.1f, 2, 0.8f));
        }
        
        // South wall
        for(int x = -9; x <= 9; x += 2)
        {
            CreateWall(x, 1, -9, new Vector3(2.1f, 2, 0.8f));
        }
        
        // East wall
        for(int z = -9; z <= 9; z += 2)
        {
            CreateWall(9, 1, z, new Vector3(0.8f, 2, 2.1f));
        }
        
        // West wall
        for(int z = -9; z <= 9; z += 2)
        {
            CreateWall(-9, 1, z, new Vector3(0.8f, 2, 2.1f));
        }
    }
    
    void CreateInnerMazeWalls()
    {
        CreateWall(-4, 1, 6, new Vector3(4, 2, 0.4f));
        CreateWall(2, 1, 4, new Vector3(6, 2, 0.4f));
        CreateWall(-6, 1, 2, new Vector3(2, 2, 0.4f));
        CreateWall(4, 1, -2, new Vector3(4, 2, 0.4f));
        CreateWall(-2, 1, -4, new Vector3(4, 2, 0.4f));
        CreateWall(6, 1, -6, new Vector3(2, 2, 0.4f));
        CreateWall(-6, 1, -6, new Vector3(0.4f, 2, 4));
        CreateWall(0, 1, 6, new Vector3(0.4f, 2, 2));
        CreateWall(6, 1, 2, new Vector3(0.4f, 2, 6));
        CreateWall(-2, 1, 0, new Vector3(0.4f, 2, 4));
        CreateWall(2, 1, -6, new Vector3(0.4f, 2, 2));
    }
    
    void CreateExitPortal()
    {
        if(exitPortalPrefab != null)
        {
            GameObject portal = Instantiate(exitPortalPrefab);
            portal.name = "ExitPortal";
portal.transform.position = new Vector3(0, 0.1f, 0);
            portal.transform.localScale = new Vector3(1f, 1f, 1f);
            
            Collider portalCollider = portal.GetComponent<Collider>();
            if (portalCollider == null)
            {
                portalCollider = portal.AddComponent<BoxCollider>();
                portalCollider.isTrigger = true;
                ((BoxCollider)portalCollider).size = new Vector3(3f, 4f, 3f);
            }
            else
            {
                portalCollider.isTrigger = true;
            }
            
            ExitTrigger exitTrigger = portal.AddComponent<ExitTrigger>();
            PortalBeacon beacon = portal.AddComponent<PortalBeacon>();
            
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.exitPlatform = portal;
            }
        }
    }
    
    void CreateWall(float x, float y, float z, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "MazeWall";
        wall.transform.position = new Vector3(x, y, z);
        wall.transform.localScale = scale;
        
        // Apply your wall material
        Renderer renderer = wall.GetComponent<Renderer>();
        if (wallMaterial != null)
        {
            renderer.material = wallMaterial;
        }
        else
        {
            renderer.material.color = Color.gray;
        }
    }
}