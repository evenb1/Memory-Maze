using UnityEngine;

public class Level2Manager : MonoBehaviour
{
    [Header("Level 2 Settings")]
    public GameObject mapPrefab; // Drag your imported map here
    public Transform playerSpawnPoint;
    public Transform[] virusSpawnPoints;
    public Transform exitPortalLocation;
    
    [Header("Virus Settings")]
    public GameObject[] virusPrefabs; // Different virus types
    public int numberOfViruses = 2; // More than Level 1
    
    [Header("Fragment Settings")]
    public int fragmentCount = 6; // More than Level 1
    public Transform[] fragmentSpawnAreas;
    
    void Start()
    {
        SetupLevel();
        SpawnPlayer();
        SpawnViruses();
        SpawnFragments();
        SpawnExitPortal();
    }
    
    void SetupLevel()
    {
        // Instantiate the map if it's a prefab
        if (mapPrefab != null)
        {
            GameObject map = Instantiate(mapPrefab);
            map.name = "Level2_Map";
            Debug.Log("Level 2 map loaded!");
        }
    }
    
    void SpawnPlayer()
    {
        GameObject player = GameObject.Find("Player");
        if (player != null && playerSpawnPoint != null)
        {
            player.transform.position = playerSpawnPoint.position;
            player.transform.rotation = playerSpawnPoint.rotation;
        }
    }
    
    void SpawnViruses()
    {
        for (int i = 0; i < numberOfViruses && i < virusSpawnPoints.Length; i++)
        {
            if (virusPrefabs.Length > 0)
            {
                // Choose random virus type
                int virusType = Random.Range(0, virusPrefabs.Length);
                GameObject virus = Instantiate(virusPrefabs[virusType], virusSpawnPoints[i].position, virusSpawnPoints[i].rotation);
                virus.name = $"Virus_{i + 1}";
                
                // Slightly randomize their speeds
                VirusAI virusAI = virus.GetComponent<VirusAI>();
                if (virusAI != null)
                {
                    virusAI.moveSpeed += Random.Range(-0.5f, 1f);
                    virusAI.huntSpeed += Random.Range(-0.5f, 1f);
                }
            }
        }
        
        Debug.Log($"Spawned {numberOfViruses} viruses for Level 2");
    }
    
    void SpawnFragments()
    {
        // Use existing FragmentSpawner or create new spawn logic
        FragmentSpawner spawner = FindObjectOfType<FragmentSpawner>();
        if (spawner != null)
        {
            spawner.numberOfFragments = fragmentCount;
        }
    }
    
    void SpawnExitPortal()
    {
        if (exitPortalLocation != null)
        {
            // Create exit portal at designated location
            GameObject portalPrefab = Resources.Load<GameObject>("ExitPortal");
            if (portalPrefab != null)
            {
                GameObject portal = Instantiate(portalPrefab, exitPortalLocation.position, exitPortalLocation.rotation);
                portal.name = "ExitPortal";
                
                // Add components
                portal.AddComponent<ExitTrigger>();
                portal.AddComponent<PortalBeacon>();
            }
        }
    }
}