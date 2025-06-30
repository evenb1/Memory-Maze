using UnityEngine;

public class Level3VirusSpawner : MonoBehaviour
{
    [Header("Virus Prefabs")]
    public GameObject flyingVirusPrefab;  // Your drone model
    public GameObject groundVirusPrefab;  // BlackTitan prefab
    
    [Header("Level 3 Virus Configuration")]
    public int numberOfFlyingViruses = 1;
    public int numberOfGroundViruses = 2;  // More ground units for Level 3
    public float minimumDistanceFromPlayer = 8f;
    public float minimumDistanceBetweenViruses = 6f;
    
    [Header("Spawn Heights")]
    public float flyingVirusHeight = 2.5f;
    public float groundVirusHeight = 1f;
    
    [Header("Level 3 Difficulty Boosts")]
    public float level3SpeedBoost = 0.5f;     // Make them faster
    public bool enableAggressiveMode = true;   // More aggressive AI
    
    private ProperMazeGenerator mazeGenerator;
    private Transform player;
    private System.Collections.Generic.List<GameObject> spawnedViruses;
    
    void Start()
    {
        spawnedViruses = new System.Collections.Generic.List<GameObject>();
        
        // Find required components
        mazeGenerator = FindFirstObjectByType<ProperMazeGenerator>();
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        // Wait a frame for maze generation to complete, then spawn viruses
        Invoke(nameof(SpawnAllViruses), 0.1f);
    }
    
    void SpawnAllViruses()
    {
        if (mazeGenerator == null)
        {
            Debug.LogError("No ProperMazeGenerator found! Cannot get spawn positions.");
            return;
        }
        
        Debug.Log("Starting Level 3 virus spawning...");
        
        // Get valid spawn positions from maze
        int totalViruses = numberOfFlyingViruses + numberOfGroundViruses;
        Vector3[] validPositions = mazeGenerator.GetValidSpawnPositions(totalViruses + 5); // Get extra positions for choice
        
        if (validPositions.Length < totalViruses)
        {
            Debug.LogWarning($"Only {validPositions.Length} valid positions found for {totalViruses} viruses!");
        }
        
        // Filter positions for optimal placement
        Vector3[] selectedPositions = SelectOptimalVirusPositions(validPositions, totalViruses);
        
        int positionIndex = 0;
        
        // Spawn flying viruses
        for (int i = 0; i < numberOfFlyingViruses && positionIndex < selectedPositions.Length; i++)
        {
            SpawnFlyingVirus(selectedPositions[positionIndex], i);
            positionIndex++;
        }
        
        // Spawn ground viruses
        for (int i = 0; i < numberOfGroundViruses && positionIndex < selectedPositions.Length; i++)
        {
            SpawnGroundVirus(selectedPositions[positionIndex], i);
            positionIndex++;
        }
        
        Debug.Log($"Level 3 virus spawning complete! Spawned {spawnedViruses.Count} viruses total.");
    }
    
    Vector3[] SelectOptimalVirusPositions(Vector3[] availablePositions, int neededCount)
    {
        if (availablePositions.Length <= neededCount)
        {
            return availablePositions;
        }
        
        System.Collections.Generic.List<Vector3> selectedPositions = new System.Collections.Generic.List<Vector3>();
        System.Collections.Generic.List<Vector3> candidatePositions = new System.Collections.Generic.List<Vector3>(availablePositions);
        
        // Remove positions too close to player
        if (player != null)
        {
            candidatePositions.RemoveAll(pos => 
                Vector3.Distance(pos, player.position) < minimumDistanceFromPlayer);
        }
        
        // Select positions with good spacing
        while (selectedPositions.Count < neededCount && candidatePositions.Count > 0)
        {
            Vector3 bestPosition = candidatePositions[0];
            float bestScore = CalculatePositionScore(bestPosition, selectedPositions);
            
            // Find position with best score (farthest from other viruses and player)
            foreach (Vector3 candidate in candidatePositions)
            {
                float score = CalculatePositionScore(candidate, selectedPositions);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestPosition = candidate;
                }
            }
            
            selectedPositions.Add(bestPosition);
            candidatePositions.Remove(bestPosition);
            
            // Remove positions too close to the selected one
            candidatePositions.RemoveAll(pos => 
                Vector3.Distance(pos, bestPosition) < minimumDistanceBetweenViruses);
        }
        
        Debug.Log($"Selected {selectedPositions.Count} optimal virus positions from {availablePositions.Length} candidates");
        
        return selectedPositions.ToArray();
    }
    
    float CalculatePositionScore(Vector3 position, System.Collections.Generic.List<Vector3> existingPositions)
    {
        float score = 0f;
        
        // Prefer positions farther from player
        if (player != null)
        {
            score += Vector3.Distance(position, player.position) * 0.5f;
        }
        
        // Prefer positions farther from center (exit portal)
        score += Vector3.Distance(position, Vector3.zero) * 0.3f;
        
        // Prefer positions farther from existing viruses
        foreach (Vector3 existingPos in existingPositions)
        {
            score += Vector3.Distance(position, existingPos) * 0.2f;
        }
        
        return score;
    }
    
    void SpawnFlyingVirus(Vector3 basePosition, int index)
    {
        if (flyingVirusPrefab == null)
        {
            Debug.LogWarning("Flying virus prefab not assigned!");
            return;
        }
        
        Vector3 spawnPosition = basePosition;
        spawnPosition.y = flyingVirusHeight; // Elevated for flying
        
        GameObject flyingVirus = Instantiate(flyingVirusPrefab, spawnPosition, Quaternion.identity);
        flyingVirus.name = $"Level3_FlyingVirus_{index + 1}";
        flyingVirus.transform.SetParent(transform);
        
        // Configure flying virus for Level 3
        VirusAI virusAI = flyingVirus.GetComponent<VirusAI>();
        if (virusAI != null)
        {
            virusAI.moveSpeed += level3SpeedBoost;
            virusAI.huntSpeed += level3SpeedBoost;
            
            if (enableAggressiveMode)
            {
                virusAI.detectionRange += 3f; // Wider detection range
            }
            
            Debug.Log($"Flying virus {index + 1} spawned at {spawnPosition} with enhanced Level 3 stats");
        }
        
        spawnedViruses.Add(flyingVirus);
    }
    
    void SpawnGroundVirus(Vector3 basePosition, int index)
    {
        if (groundVirusPrefab == null)
        {
            Debug.LogWarning("Ground virus prefab not assigned!");
            return;
        }
        
        Vector3 spawnPosition = basePosition;
        spawnPosition.y = groundVirusHeight; // Ground level
        
        GameObject groundVirus = Instantiate(groundVirusPrefab, spawnPosition, Quaternion.identity);
        groundVirus.name = $"Level3_GroundVirus_{index + 1}";
        groundVirus.transform.SetParent(transform);
        
        // Configure ground virus for Level 3
        GroundPatrolVirus groundAI = groundVirus.GetComponent<GroundPatrolVirus>();
        if (groundAI != null)
        {
            groundAI.moveSpeed += level3SpeedBoost;
            groundAI.huntSpeed += level3SpeedBoost;
            groundAI.patrolSpeed += level3SpeedBoost * 0.5f;
            
            if (enableAggressiveMode)
            {
                groundAI.detectionRange += 2f; // Wider detection range
                groundAI.patrolRadius += 2f;   // Larger patrol area
            }
            
            Debug.Log($"Ground virus {index + 1} spawned at {spawnPosition} with enhanced Level 3 stats");
        }
        
        spawnedViruses.Add(groundVirus);
    }
    
    // Public methods for game events
    public void OnFragmentCollected()
    {
        // Increase virus aggression as player progresses
        foreach (GameObject virus in spawnedViruses)
        {
            if (virus == null) continue;
            
            VirusAI flyingAI = virus.GetComponent<VirusAI>();
            if (flyingAI != null)
            {
                flyingAI.IncreaseSpeed(0.2f);
            }
            
            GroundPatrolVirus groundAI = virus.GetComponent<GroundPatrolVirus>();
            if (groundAI != null)
            {
                groundAI.IncreaseSpeed(0.15f);
            }
        }
        
        Debug.Log("Fragment collected - viruses have become more aggressive!");
    }
    
    public void OnAllFragmentsCollected()
    {
        // Final aggression boost when all fragments collected
        foreach (GameObject virus in spawnedViruses)
        {
            if (virus == null) continue;
            
            VirusAI flyingAI = virus.GetComponent<VirusAI>();
            if (flyingAI != null)
            {
                flyingAI.IncreaseSpeed(0.5f);
            }
            
            GroundPatrolVirus groundAI = virus.GetComponent<GroundPatrolVirus>();
            if (groundAI != null)
            {
                groundAI.IncreaseSpeed(0.3f);
            }
        }
        
        Debug.Log("All fragments collected - viruses are at maximum aggression!");
    }
    
    public void OnGameOver()
    {
        // Stop all virus movement
        foreach (GameObject virus in spawnedViruses)
        {
            if (virus == null) continue;
            
            VirusAI flyingAI = virus.GetComponent<VirusAI>();
            if (flyingAI != null)
            {
                flyingAI.enabled = false;
            }
            
            GroundPatrolVirus groundAI = virus.GetComponent<GroundPatrolVirus>();
            if (groundAI != null)
            {
                groundAI.enabled = false;
            }
        }
    }
    
    // Get virus positions for other systems
    public Vector3[] GetVirusPositions()
    {
        System.Collections.Generic.List<Vector3> positions = new System.Collections.Generic.List<Vector3>();
        
        foreach (GameObject virus in spawnedViruses)
        {
            if (virus != null)
            {
                positions.Add(virus.transform.position);
            }
        }
        
        return positions.ToArray();
    }
    
    public int GetActiveVirusCount()
    {
        int activeCount = 0;
        foreach (GameObject virus in spawnedViruses)
        {
            if (virus != null)
            {
                activeCount++;
            }
        }
        return activeCount;
    }
    
    // Debug visualization
    void OnDrawGizmos()
    {
        if (spawnedViruses != null)
        {
            Gizmos.color = Color.red;
            foreach (GameObject virus in spawnedViruses)
            {
                if (virus != null)
                {
                    Gizmos.DrawWireSphere(virus.transform.position, 1f);
                    
                    // Draw detection range
                    VirusAI virusAI = virus.GetComponent<VirusAI>();
                    if (virusAI != null)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawWireSphere(virus.transform.position, virusAI.detectionRange);
                    }
                    
                    GroundPatrolVirus groundAI = virus.GetComponent<GroundPatrolVirus>();
                    if (groundAI != null)
                    {
                        Gizmos.color = Color.orange;
                        Gizmos.DrawWireSphere(virus.transform.position, groundAI.detectionRange);
                    }
                    
                    Gizmos.color = Color.red;
                }
            }
        }
    }
}