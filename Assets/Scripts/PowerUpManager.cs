using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    [Header("Power-up Prefabs")]
    public GameObject speedBoostPrefab;         // Lightning asset
    public GameObject empPulsePrefab;           // Thunder asset  
    public GameObject fragmentMagnetPrefab;     // Magnet asset
    public GameObject shieldBoostPrefab;        // Shield asset
    
    [Header("Spawn Settings")]
    public int maxActivePowerUps = 4;           
    public float minSpawnInterval = 8f;         
    public float maxSpawnInterval = 20f;
    public float spawnHeight = 1f;
    
    [Header("Power-up Distribution")]
    [Range(0, 100)]
    public float speedBoostChance = 35f;        
    [Range(0, 100)]
    public float empPulseChance = 35f;          
    [Range(0, 100)]
    public float fragmentMagnetChance = 20f;    
    [Range(0, 100)]
    public float shieldBoostChance = 10f;       
    
    [Header("Debug")]
    public bool enableDebugMode = false;
    
    private ProperMazeGenerator mazeGenerator;
    private int currentActivePowerUps = 0;
    private float nextSpawnTime;
    private bool isSpawningActive = true;
    
    void Start()
    {
        mazeGenerator = FindFirstObjectByType<ProperMazeGenerator>();
        CalculateNextSpawnTime();
        Debug.Log($"PowerUpManager initialized - Max active: {maxActivePowerUps}");
    }
    
    void Update()
    {
        if (isSpawningActive && Time.time >= nextSpawnTime)
        {
            if (currentActivePowerUps < maxActivePowerUps)
            {
                SpawnRandomPowerUp();
                CalculateNextSpawnTime();
            }
            else
            {
                nextSpawnTime = Time.time + 2f;
            }
        }
    }
    
    void CalculateNextSpawnTime()
    {
        float spawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
        nextSpawnTime = Time.time + spawnInterval;
    }
    
    void SpawnRandomPowerUp()
    {
        Vector3 spawnPosition = GetValidSpawnPosition();
        if (spawnPosition == Vector3.zero)
        {
            Debug.LogWarning("No valid spawn position found for power-up!");
            return;
        }
        
        GameObject powerUpToSpawn = DeterminePowerUpType();
        
        if (powerUpToSpawn != null)
        {
            GameObject spawnedPowerUp = Instantiate(powerUpToSpawn, spawnPosition, Quaternion.identity);
            spawnedPowerUp.transform.SetParent(transform);
            
            // Add cleanup component
            PowerUpCleanup cleanup = spawnedPowerUp.AddComponent<PowerUpCleanup>();
            cleanup.powerUpManager = this;
            cleanup.lifetime = 30f;
            
            currentActivePowerUps++;
            
            string powerUpName = GetPowerUpName(powerUpToSpawn);
            Debug.Log($"✨ Spawned {powerUpName} ({currentActivePowerUps}/{maxActivePowerUps} active)");
        }
    }
    
    GameObject DeterminePowerUpType()
    {
        float randomValue = Random.Range(0f, 100f);
        float cumulativeChance = 0f;
        
        cumulativeChance += speedBoostChance;
        if (randomValue <= cumulativeChance && speedBoostPrefab != null)
        {
            return speedBoostPrefab;
        }
        
        cumulativeChance += empPulseChance;
        if (randomValue <= cumulativeChance && empPulsePrefab != null)
        {
            return empPulsePrefab;
        }
        
        cumulativeChance += fragmentMagnetChance;
        if (randomValue <= cumulativeChance && fragmentMagnetPrefab != null)
        {
            return fragmentMagnetPrefab;
        }
        
        cumulativeChance += shieldBoostChance;
        if (randomValue <= cumulativeChance && shieldBoostPrefab != null)
        {
            return shieldBoostPrefab;
        }
        
        return speedBoostPrefab; // Fallback
    }
    
    string GetPowerUpName(GameObject powerUpPrefab)
    {
        if (powerUpPrefab == speedBoostPrefab) return "Speed Boost ⚡";
        if (powerUpPrefab == empPulsePrefab) return "EMP Pulse ⚡";
        if (powerUpPrefab == fragmentMagnetPrefab) return "Fragment Magnet 🧲";
        if (powerUpPrefab == shieldBoostPrefab) return "Shield Boost 🛡️";
        return "Unknown Power-up";
    }
    
    Vector3 GetValidSpawnPosition()
    {
        if (mazeGenerator != null)
        {
            Vector3[] validPositions = mazeGenerator.GetValidSpawnPositions(5);
            
            if (validPositions != null && validPositions.Length > 0)
            {
                Vector3 chosenPosition = validPositions[Random.Range(0, validPositions.Length)];
                chosenPosition.y = spawnHeight;
                return chosenPosition;
            }
        }
        
        return GetFallbackSpawnPosition();
    }
    
    Vector3 GetFallbackSpawnPosition()
    {
        Vector3[] fallbackPositions = {
            new Vector3(-10f, spawnHeight, 10f),
            new Vector3(10f, spawnHeight, 10f),
            new Vector3(-10f, spawnHeight, -10f),
            new Vector3(10f, spawnHeight, -10f),
            new Vector3(-15f, spawnHeight, 0f),
            new Vector3(15f, spawnHeight, 0f)
        };
        
        foreach (Vector3 pos in fallbackPositions)
        {
            if (!IsPositionBlocked(pos))
            {
                return pos;
            }
        }
        
        return new Vector3(5f, spawnHeight, 5f);
    }
    
    bool IsPositionBlocked(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, 1f);
        
        foreach (Collider col in colliders)
        {
            if (col.gameObject.name.Contains("Wall") || col.gameObject.tag == "Wall")
            {
                return true;
            }
        }
        
        return false;
    }
    
    public void OnPowerUpRemoved()
    {
        currentActivePowerUps = Mathf.Max(0, currentActivePowerUps - 1);
    }
    
    public void OnFragmentCollected()
    {
        if (minSpawnInterval > 6f)
        {
            minSpawnInterval = Mathf.Max(6f, minSpawnInterval - 0.5f);
            maxSpawnInterval = Mathf.Max(15f, maxSpawnInterval - 0.5f);
        }
    }
    
    public void OnLevelComplete()
    {
        isSpawningActive = false;
        Debug.Log("Level complete - power-up spawning stopped");
    }
    
    public void OnPlayerCaught()
    {
        isSpawningActive = false;
        Debug.Log("Player caught - power-up spawning stopped");
    }
}

// Helper component for power-up cleanup
public class PowerUpCleanup : MonoBehaviour
{
    public PowerUpManager powerUpManager;
    public float lifetime = 30f;
    
    void Start()
    {
        Invoke(nameof(ExpirePowerUp), lifetime);
    }
    
    void ExpirePowerUp()
    {
        if (powerUpManager != null)
        {
            powerUpManager.OnPowerUpRemoved();
        }
        
        Debug.Log($"Power-up {gameObject.name} expired");
        Destroy(gameObject);
    }
    
    void OnDestroy()
    {
        if (powerUpManager != null)
        {
            powerUpManager.OnPowerUpRemoved();
        }
    }
}