using UnityEngine;
using UnityEngine.SceneManagement;

public class FragmentSpawner : MonoBehaviour
{
    [Header("Fragment Settings")]
    public GameObject fragmentPrefab;
    public int numberOfFragments = 6; // Default - will auto-adjust based on level
    public float spawnHeight = 1f;
    
    [Header("Spawn Method")]
    public bool useSmartSpawning = true;
    
    [Header("Auto Level Detection")]
    public bool autoDetectLevel = true; // NEW: Automatically set fragment count based on scene name
    
    private ProperMazeGenerator mazeGenerator;
    
    void Start()
    {
        // Auto-detect level and set fragment count
        if (autoDetectLevel)
        {
            DetectLevelAndSetFragmentCount();
        }
        
        Debug.Log($"FragmentSpawner starting - Need to spawn {numberOfFragments} fragments");
        
        // Wait a moment for maze to generate
        Invoke(nameof(SpawnFragments), 0.5f);
    }
    
    void DetectLevelAndSetFragmentCount()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"Detected scene: {sceneName}");
        
        if (sceneName.Contains("Level1"))
        {
            numberOfFragments = 5;
            Debug.Log("🎯 Level 1 detected - Setting to 5 fragments");
        }
        else if (sceneName.Contains("Level2"))
        {
            numberOfFragments = 6;
            Debug.Log("🎯 Level 2 detected - Setting to 6 fragments");
        }
        else if (sceneName.Contains("Level3"))
        {
            numberOfFragments = 7;
            Debug.Log("🎯 Level 3 detected - Setting to 7 fragments");
        }
        else if (sceneName.Contains("Level4"))
        {
            numberOfFragments = 8;
            Debug.Log("🎯 Level 4 detected - Setting to 8 fragments");
        }
        else
        {
            Debug.Log($"⚠️ Unknown level '{sceneName}' - Using manual setting: {numberOfFragments} fragments");
        }
    }
    
    void SpawnFragments()
    {
        mazeGenerator = FindFirstObjectByType<ProperMazeGenerator>();
        
        if (fragmentPrefab == null)
        {
            Debug.LogError("Fragment Prefab is NULL! Assign MemoryFragment prefab in FragmentSpawner!");
            return;
        }
        
        if (useSmartSpawning && mazeGenerator != null)
        {
            SpawnFragmentsInValidPositions();
        }
        else
        {
            Debug.LogWarning("Maze generator not found - using legacy method");
            SpawnFragmentsLegacyMethod();
        }
    }
    
    void SpawnFragmentsInValidPositions()
    {
        Debug.Log("Using smart spawning - getting valid positions from maze generator");
        
        // Get valid spawn positions from the maze generator
        Vector3[] validPositions = mazeGenerator.GetValidSpawnPositions(numberOfFragments + 2); // Get a few extra
        
        if (validPositions == null || validPositions.Length == 0)
        {
            Debug.LogError("No valid positions returned from maze generator! Using legacy method.");
            SpawnFragmentsLegacyMethod();
            return;
        }
        
        if (validPositions.Length < numberOfFragments)
        {
            Debug.LogWarning($"Only found {validPositions.Length} valid positions, but need {numberOfFragments}. Using what we have.");
        }
        
        // Spawn fragments at valid positions
        int fragmentsSpawned = 0;
        for (int i = 0; i < validPositions.Length && fragmentsSpawned < numberOfFragments; i++)
        {
            Vector3 spawnPosition = validPositions[i];
            spawnPosition.y = spawnHeight; // Set correct height
            
            GameObject fragment = Instantiate(fragmentPrefab, spawnPosition, Quaternion.identity);
            fragment.name = $"MemoryFragment_{fragmentsSpawned + 1}";
            fragment.transform.SetParent(transform);
            
            // Make sure fragment has the right components
            MemoryFragment memoryComp = fragment.GetComponent<MemoryFragment>();
            if (memoryComp == null)
            {
                memoryComp = fragment.AddComponent<MemoryFragment>();
            }
            
            fragmentsSpawned++;
            Debug.Log($"✅ Fragment {fragmentsSpawned} spawned at: {spawnPosition}");
        }
        
        Debug.Log($"🎯 Successfully spawned {fragmentsSpawned} fragments using smart spawning!");
        
        // Verify they're actually in the scene
        MemoryFragment[] allFragments = FindObjectsOfType<MemoryFragment>();
        Debug.Log($"📊 Total fragments found in scene: {allFragments.Length}");
    }
    
    void SpawnFragmentsLegacyMethod()
    {
        Debug.Log($"Using legacy spawning method for {numberOfFragments} fragments");
        
        // Different spawn patterns based on fragment count
        Vector3[] spawnPoints = GetSpawnPointsForFragmentCount(numberOfFragments);
        
        int fragmentsSpawned = 0;
        for (int i = 0; i < spawnPoints.Length && fragmentsSpawned < numberOfFragments; i++)
        {
            Vector3 spawnPosition = spawnPoints[i];
            
            // Check if position is valid (not in a wall)
            if (!IsPositionBlocked(spawnPosition))
            {
                GameObject fragment = Instantiate(fragmentPrefab, spawnPosition, Quaternion.identity);
                fragment.name = $"MemoryFragment_{fragmentsSpawned + 1}";
                fragment.transform.SetParent(transform);
                
                // Make sure fragment has the right components
                MemoryFragment memoryComp = fragment.GetComponent<MemoryFragment>();
                if (memoryComp == null)
                {
                    memoryComp = fragment.AddComponent<MemoryFragment>();
                }
                
                fragmentsSpawned++;
                Debug.Log($"✅ Fragment {fragmentsSpawned} spawned at: {spawnPosition}");
            }
            else
            {
                Debug.LogWarning($"❌ Spawn position {spawnPosition} is blocked, skipping...");
            }
        }
        
        Debug.Log($"🎯 Legacy spawning complete - {fragmentsSpawned} fragments spawned");
        
        // Verify they're actually in the scene
        MemoryFragment[] allFragments = FindObjectsOfType<MemoryFragment>();
        Debug.Log($"📊 Total fragments found in scene: {allFragments.Length}");
    }
    
    Vector3[] GetSpawnPointsForFragmentCount(int count)
    {
        // Return appropriate spawn points based on how many fragments we need
        if (count <= 5) // Level 1
        {
            return new Vector3[] {
                new Vector3(-6, spawnHeight, 6),
                new Vector3(6, spawnHeight, 6),
                new Vector3(-6, spawnHeight, -6),
                new Vector3(6, spawnHeight, -6),
                new Vector3(0, spawnHeight, 0)
            };
        }
        else if (count <= 6) // Level 2
        {
            return new Vector3[] {
                new Vector3(-8, spawnHeight, 8),
                new Vector3(8, spawnHeight, 8),
                new Vector3(-8, spawnHeight, -8),
                new Vector3(8, spawnHeight, -8),
                new Vector3(-10, spawnHeight, 0),
                new Vector3(10, spawnHeight, 0)
            };
        }
        else if (count <= 7) // Level 3
        {
            return new Vector3[] {
                new Vector3(-10, spawnHeight, 10),
                new Vector3(10, spawnHeight, 10),
                new Vector3(-10, spawnHeight, -10),
                new Vector3(10, spawnHeight, -10),
                new Vector3(-12, spawnHeight, 0),
                new Vector3(12, spawnHeight, 0),
                new Vector3(0, spawnHeight, 12)
            };
        }
        else // Level 4 (8+ fragments)
        {
            return new Vector3[] {
                new Vector3(-15, spawnHeight, 15),   // Far corners
                new Vector3(15, spawnHeight, 15),
                new Vector3(-15, spawnHeight, -15),
                new Vector3(15, spawnHeight, -15),
                new Vector3(-20, spawnHeight, 0),    // Far sides
                new Vector3(20, spawnHeight, 0),
                new Vector3(0, spawnHeight, 20),
                new Vector3(0, spawnHeight, -20),
                new Vector3(-10, spawnHeight, 10),   // Mid positions
                new Vector3(10, spawnHeight, -10)
            };
        }
    }
    
    bool IsPositionBlocked(Vector3 position)
    {
        // Check if there's a wall at this position
        Collider[] colliders = Physics.OverlapSphere(position, 1f);
        
        foreach (Collider col in colliders)
        {
            if (col.gameObject.name.Contains("Wall") || col.gameObject.tag == "Wall")
            {
                return true;
            }
        }
        
        // If maze generator is available, also check with it
        if (mazeGenerator != null)
        {
            return mazeGenerator.IsWallAt(position);
        }
        
        return false;
    }
    
    // Method to manually spawn fragments if needed
    [ContextMenu("Force Spawn Fragments")]
    public void ForceSpawnFragments()
    {
        // Clear existing fragments first
        MemoryFragment[] existingFragments = FindObjectsOfType<MemoryFragment>();
        for (int i = 0; i < existingFragments.Length; i++)
        {
            if (Application.isPlaying)
            {
                Destroy(existingFragments[i].gameObject);
            }
            else
            {
                DestroyImmediate(existingFragments[i].gameObject);
            }
        }
        
        Debug.Log("🔄 Force spawning fragments...");
        SpawnFragments();
    }
    
    // Debug method to show spawn points in scene view
    void OnDrawGizmos()
    {
        if (mazeGenerator != null && useSmartSpawning)
        {
            Vector3[] validPositions = mazeGenerator.GetValidSpawnPositions(numberOfFragments);
            
            if (validPositions != null)
            {
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
}