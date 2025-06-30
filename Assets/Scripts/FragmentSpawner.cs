using UnityEngine;

public class FragmentSpawner : MonoBehaviour
{
    [Header("Fragment Settings")]
    public GameObject fragmentPrefab;
    public int numberOfFragments = 6; // Level 2 has 6 fragments
    public float spawnHeight = 1f;
    
    [Header("Spawn Method")]
    public bool useSmartSpawning = true; // Use maze-aware spawning
    
    private ProperMazeGenerator mazeGenerator;
    
    void Start()
    {
        mazeGenerator = FindFirstObjectByType<ProperMazeGenerator>();
        
        if (useSmartSpawning && mazeGenerator != null)
        {
            SpawnFragmentsInValidPositions();
        }
        else
        {
            SpawnFragmentsLegacyMethod();
        }
    }
    
    void SpawnFragmentsInValidPositions()
    {
        Debug.Log("Using smart spawning - getting valid positions from maze generator");
        
        // Get valid spawn positions from the maze generator
        Vector3[] validPositions = mazeGenerator.GetValidSpawnPositions(numberOfFragments);
        
        if (validPositions.Length < numberOfFragments)
        {
            Debug.LogWarning($"Only found {validPositions.Length} valid positions, but need {numberOfFragments}. Using what we have.");
        }
        
        // Spawn fragments at valid positions
        for (int i = 0; i < validPositions.Length; i++)
        {
            Vector3 spawnPosition = validPositions[i];
            spawnPosition.y = spawnHeight; // Set correct height
            
            if (fragmentPrefab != null)
            {
                GameObject fragment = Instantiate(fragmentPrefab, spawnPosition, Quaternion.identity);
                fragment.name = $"MemoryFragment_{i + 1}";
                fragment.transform.SetParent(transform);
                
                Debug.Log($"Fragment {i + 1} spawned at: {spawnPosition}");
            }
        }
        
        Debug.Log($"Successfully spawned {validPositions.Length} fragments using smart spawning!");
    }
    
    void SpawnFragmentsLegacyMethod()
    {
        Debug.Log("Using legacy spawning method");
        
        // Fallback to manual positions if maze generator not available
        Vector3[] manualSpawnPoints = {
            new Vector3(-6, spawnHeight, 6),    // Top left
            new Vector3(6, spawnHeight, 6),     // Top right
            new Vector3(-6, spawnHeight, -6),   // Bottom left
            new Vector3(6, spawnHeight, -6),    // Bottom right
            new Vector3(-8, spawnHeight, 0),    // Left side
            new Vector3(8, spawnHeight, 0),     // Right side
            new Vector3(0, spawnHeight, 8),     // Top center
            new Vector3(0, spawnHeight, -8)     // Bottom center
        };
        
        int fragmentsSpawned = 0;
        for (int i = 0; i < manualSpawnPoints.Length && fragmentsSpawned < numberOfFragments; i++)
        {
            Vector3 spawnPosition = manualSpawnPoints[i];
            
            // Check if position is valid (not in a wall)
            if (!IsPositionBlocked(spawnPosition))
            {
                if (fragmentPrefab != null)
                {
                    GameObject fragment = Instantiate(fragmentPrefab, spawnPosition, Quaternion.identity);
                    fragment.name = $"MemoryFragment_{fragmentsSpawned + 1}";
                    fragment.transform.SetParent(transform);
                    fragmentsSpawned++;
                    
                    Debug.Log($"Fragment {fragmentsSpawned} spawned at: {spawnPosition}");
                }
            }
            else
            {
                Debug.LogWarning($"Spawn position {spawnPosition} is blocked, skipping...");
            }
        }
        
        Debug.Log($"Legacy spawning complete - {fragmentsSpawned} fragments spawned");
    }
    
    bool IsPositionBlocked(Vector3 position)
    {
        // Check if there's a wall at this position
        Collider[] colliders = Physics.OverlapSphere(position, 0.5f);
        
        foreach (Collider col in colliders)
        {
            if (col.gameObject.name.Contains("Wall") || col.gameObject.tag == "Wall")
            {
                return true; // Position is blocked by a wall
            }
        }
        
        // If maze generator is available, also check with it
        if (mazeGenerator != null)
        {
            return mazeGenerator.IsWallAt(position);
        }
        
        return false; // Position is clear
    }
    
    // Debug method to show spawn points in scene view
    void OnDrawGizmos()
    {
        if (mazeGenerator != null && useSmartSpawning)
        {
            // Show valid spawn positions as green spheres
            Vector3[] validPositions = mazeGenerator.GetValidSpawnPositions(numberOfFragments);
            
            Gizmos.color = Color.green;
            foreach (Vector3 pos in validPositions)
            {
                Vector3 gizmoPos = pos;
                gizmoPos.y = spawnHeight;
                Gizmos.DrawWireSphere(gizmoPos, 0.5f);
            }
        }
    }
}