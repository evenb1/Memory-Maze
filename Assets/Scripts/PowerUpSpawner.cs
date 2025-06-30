using UnityEngine;
using System.Collections.Generic;

public class PowerUpSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject speedBoostPrefab;        // Drag SpeedBoost prefab here
    public int maxActiveBoosts = 2;            // Max speed boosts on map at once
    public float minSpawnInterval = 15f;       // Minimum time between spawns
    public float maxSpawnInterval = 30f;       // Maximum time between spawns
    public float spawnHeight = 1.2f;           // Height above ground
    
    [Header("Spawn Timing")]
    public float gameStartDelay = 20f;         // Wait before first spawn
    public bool spawnOnlyAfterFirstFragment = true; // Wait until player gets first fragment
    
    [Header("Debug")]
    public bool enableDebugLogs = true;
    
    private List<GameObject> activeSpeedBoosts;
    private ProperMazeGenerator mazeGenerator;
    private GameManager gameManager;
    private float nextSpawnTime;
    private bool canSpawn = false;
    private int lastFragmentCount = 0;
    
    void Start()
    {
        activeSpeedBoosts = new List<GameObject>();
        mazeGenerator = FindFirstObjectByType<ProperMazeGenerator>();
        gameManager = FindFirstObjectByType<GameManager>();
        
        // Set initial spawn time
        nextSpawnTime = Time.time + gameStartDelay;
        
        if (enableDebugLogs)
        {
            Debug.Log($"PowerUp Spawner initialized. First spawn in {gameStartDelay} seconds.");
        }
    }
    
    void Update()
    {
        CheckSpawnConditions();
        
        if (ShouldSpawnPowerUp())
        {
            SpawnSpeedBoost();
        }
        
        CleanupDestroyedBoosts();
    }
    
    void CheckSpawnConditions()
    {
        if (!canSpawn)
        {
            if (spawnOnlyAfterFirstFragment)
            {
                // Check if player has collected at least one fragment
                if (gameManager != null && gameManager.GetCollectedFragments() > 0)
                {
                    canSpawn = true;
                    if (enableDebugLogs)
                    {
                        Debug.Log("Player collected first fragment - power-up spawning enabled!");
                    }
                }
            }
            else
            {
                // Just wait for the initial delay
                if (Time.time >= nextSpawnTime)
                {
                    canSpawn = true;
                }
            }
        }
    }
    
    bool ShouldSpawnPowerUp()
    {
        // Check all conditions for spawning
        return canSpawn && 
               Time.time >= nextSpawnTime && 
               activeSpeedBoosts.Count < maxActiveBoosts &&
               speedBoostPrefab != null &&
               mazeGenerator != null;
    }
    
    void SpawnSpeedBoost()
    {
        // Get a valid spawn position
        Vector3[] validPositions = mazeGenerator.GetValidSpawnPositions(1);
        
        if (validPositions.Length > 0)
        {
            Vector3 spawnPosition = validPositions[0];
            spawnPosition.y = spawnHeight;
            
            // Create the speed boost
            GameObject speedBoost = Instantiate(speedBoostPrefab, spawnPosition, Quaternion.identity);
            speedBoost.name = $"SpeedBoost_{activeSpeedBoosts.Count + 1}";
            speedBoost.transform.SetParent(transform);
            
            // Add to active list
            activeSpeedBoosts.Add(speedBoost);
            
            // Schedule next spawn
            float nextInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
            nextSpawnTime = Time.time + nextInterval;
            
            if (enableDebugLogs)
            {
                Debug.Log($"Speed boost spawned at {spawnPosition}. Next spawn in {nextInterval:F1} seconds.");
                Debug.Log($"Active speed boosts: {activeSpeedBoosts.Count}/{maxActiveBoosts}");
            }
            
            // Update UI
            if (gameManager != null)
            {
                gameManager.UpdateStatusMessage("A speed boost has appeared in the maze!");
            }
        }
        else
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning("No valid spawn positions found for speed boost!");
            }
            
            // Try again soon
            nextSpawnTime = Time.time + 5f;
        }
    }
    
    void CleanupDestroyedBoosts()
    {
        // Remove null references (collected/destroyed boosts)
        for (int i = activeSpeedBoosts.Count - 1; i >= 0; i--)
        {
            if (activeSpeedBoosts[i] == null)
            {
                activeSpeedBoosts.RemoveAt(i);
                
                if (enableDebugLogs)
                {
                    Debug.Log($"Speed boost removed from active list. Active count: {activeSpeedBoosts.Count}");
                }
            }
        }
    }
    
    // Public methods for external control
    public void ForceSpawnSpeedBoost()
    {
        if (activeSpeedBoosts.Count < maxActiveBoosts)
        {
            SpawnSpeedBoost();
        }
    }
    
    public void ClearAllSpeedBoosts()
    {
        foreach (GameObject boost in activeSpeedBoosts)
        {
            if (boost != null)
            {
                Destroy(boost);
            }
        }
        activeSpeedBoosts.Clear();
        
        if (enableDebugLogs)
        {
            Debug.Log("All speed boosts cleared!");
        }
    }
    
    public int GetActiveBoostCount()
    {
        CleanupDestroyedBoosts();
        return activeSpeedBoosts.Count;
    }
    
    // Adjust spawn rate based on game state
    public void AdjustSpawnRate(float multiplier)
    {
        minSpawnInterval *= multiplier;
        maxSpawnInterval *= multiplier;
        
        if (enableDebugLogs)
        {
            Debug.Log($"Spawn rate adjusted by {multiplier}x. New interval: {minSpawnInterval}-{maxSpawnInterval}s");
        }
    }
    
    // Event responses
    public void OnFragmentCollected()
    {
        // Speed up spawning slightly as player progresses
        if (gameManager != null)
        {
            int currentFragments = gameManager.GetCollectedFragments();
            if (currentFragments > lastFragmentCount)
            {
                lastFragmentCount = currentFragments;
                
                // Reduce spawn intervals slightly with each fragment
                float reductionFactor = 0.95f;
                minSpawnInterval *= reductionFactor;
                maxSpawnInterval *= reductionFactor;
                
                // Ensure minimum intervals don't get too short
                minSpawnInterval = Mathf.Max(minSpawnInterval, 8f);
                maxSpawnInterval = Mathf.Max(maxSpawnInterval, 15f);
                
                if (enableDebugLogs)
                {
                    Debug.Log($"Fragment collected! Spawn rate increased. New interval: {minSpawnInterval:F1}-{maxSpawnInterval:F1}s");
                }
            }
        }
    }
    
    public void OnPlayerCaught()
    {
        // Clear all boosts when player is caught
        ClearAllSpeedBoosts();
    }
    
    public void OnLevelComplete()
    {
        // Stop spawning when level is complete
        canSpawn = false;
        ClearAllSpeedBoosts();
    }
    
    // Debug visualization
    void OnDrawGizmos()
    {
        if (enableDebugLogs && activeSpeedBoosts != null)
        {
            Gizmos.color = Color.yellow;
            foreach (GameObject boost in activeSpeedBoosts)
            {
                if (boost != null)
                {
                    Gizmos.DrawWireSphere(boost.transform.position, 1f);
                }
            }
        }
    }
}