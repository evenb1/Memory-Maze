using UnityEngine;
using UnityEngine.SceneManagement;

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
    
    [Header("Level Settings")]
    public bool enableForLevel1 = false;        // Level 1 is tutorial - no power-ups
    public bool enableForLevel2 = true;         // Level 2+ get power-ups
    public bool enableForLevel3 = true;
    public bool enableForLevel4 = true;
    
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
    public bool enableDebugMode = true;
    
    private ProperMazeGenerator mazeGenerator;
    private int currentActivePowerUps = 0;
    private float nextSpawnTime;
    private bool isSpawningActive = false;  // Start disabled, enable based on level
    private int currentLevel = 1;
    
    void Start()
    {
        Debug.Log("🔋 PowerUpManager Start() called!");
        
        // Detect current level
        DetectCurrentLevel();
        
        // Check if power-ups should be enabled for this level
        CheckLevelSettings();
        
        mazeGenerator = FindFirstObjectByType<ProperMazeGenerator>();
        
        if (isSpawningActive)
        {
            CalculateNextSpawnTime();
            Debug.Log($"🔋 PowerUpManager initialized for Level {currentLevel}");
            Debug.Log($"🔋 Max active: {maxActivePowerUps}, First spawn in: {nextSpawnTime - Time.time:F1}s");
        }
        else
        {
            Debug.Log($"🔋 PowerUpManager disabled for Level {currentLevel}");
        }
        
        Debug.Log($"🔋 Maze generator found: {mazeGenerator != null}");
    }
    
    void DetectCurrentLevel()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        
        if (sceneName.Contains("Level1"))
        {
            currentLevel = 1;
        }
        else if (sceneName.Contains("Level2"))
        {
            currentLevel = 2;
        }
        else if (sceneName.Contains("Level3"))
        {
            currentLevel = 3;
        }
        else if (sceneName.Contains("Level4"))
        {
            currentLevel = 4;
        }
        else
        {
            currentLevel = 1; // Default to level 1
        }
        
        Debug.Log($"🔋 Detected Level: {currentLevel} (Scene: {sceneName})");
    }
    
    void CheckLevelSettings()
    {
        switch (currentLevel)
        {
            case 1:
                isSpawningActive = enableForLevel1;
                break;
            case 2:
                isSpawningActive = enableForLevel2;
                break;
            case 3:
                isSpawningActive = enableForLevel3;
                break;
            case 4:
                isSpawningActive = enableForLevel4;
                break;
            default:
                isSpawningActive = false;
                break;
        }
        
        Debug.Log($"🔋 Level {currentLevel} power-ups enabled: {isSpawningActive}");
    }
    
    void Update()
    {
        // Manual test - Press P to force spawn (only if debug enabled)
        if (enableDebugMode && Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("🔋 MANUAL SPAWN TEST!");
            SpawnRandomPowerUp();
        }
        
        // Debug info when spawn time approaches
        if (enableDebugMode && isSpawningActive && Time.time >= nextSpawnTime - 1f && Time.time <= nextSpawnTime)
        {
            Debug.Log($"🔋 Spawn in {nextSpawnTime - Time.time:F1}s | Active: {currentActivePowerUps}/{maxActivePowerUps}");
        }
        
        // Main spawning logic
        if (isSpawningActive && Time.time >= nextSpawnTime)
        {
            if (currentActivePowerUps < maxActivePowerUps)
            {
                if (enableDebugMode)
                {
                    Debug.Log("🔋 Attempting to spawn power-up...");
                }
                SpawnRandomPowerUp();
                CalculateNextSpawnTime();
            }
            else
            {
                if (enableDebugMode)
                {
                    Debug.Log("🔋 Max power-ups reached, waiting 2s...");
                }
                nextSpawnTime = Time.time + 2f;
            }
        }
    }
    
    void CalculateNextSpawnTime()
    {
        float spawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
        nextSpawnTime = Time.time + spawnInterval;
        
        if (enableDebugMode)
        {
            Debug.Log($"🔋 Next spawn scheduled in {spawnInterval:F1}s (at {nextSpawnTime:F1})");
        }
    }
    
    void SpawnRandomPowerUp()
    {
        if (enableDebugMode)
        {
            Debug.Log("🔋 Getting spawn position...");
        }
        
        Vector3 spawnPosition = GetValidSpawnPosition();
        if (spawnPosition == Vector3.zero)
        {
            Debug.LogWarning("🔋 No valid spawn position found for power-up!");
            return;
        }
        
        if (enableDebugMode)
        {
            Debug.Log($"🔋 Spawn position found: {spawnPosition}");
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
            Debug.Log($"✨ Spawned {powerUpName} at {spawnPosition} ({currentActivePowerUps}/{maxActivePowerUps} active)");
        }
        else
        {
            Debug.LogWarning("🔋 No power-up prefab selected!");
        }
    }
    
    GameObject DeterminePowerUpType()
    {
        float randomValue = Random.Range(0f, 100f);
        float cumulativeChance = 0f;
        
        if (enableDebugMode)
        {
            Debug.Log($"🔋 Random value: {randomValue:F1}");
        }
        
        cumulativeChance += speedBoostChance;
        if (randomValue <= cumulativeChance && speedBoostPrefab != null)
        {
            if (enableDebugMode) Debug.Log("🔋 Selected: Speed Boost");
            return speedBoostPrefab;
        }
        
        cumulativeChance += empPulseChance;
        if (randomValue <= cumulativeChance && empPulsePrefab != null)
        {
            if (enableDebugMode) Debug.Log("🔋 Selected: EMP Pulse");
            return empPulsePrefab;
        }
        
        cumulativeChance += fragmentMagnetChance;
        if (randomValue <= cumulativeChance && fragmentMagnetPrefab != null)
        {
            if (enableDebugMode) Debug.Log("🔋 Selected: Fragment Magnet");
            return fragmentMagnetPrefab;
        }
        
        cumulativeChance += shieldBoostChance;
        if (randomValue <= cumulativeChance && shieldBoostPrefab != null)
        {
            if (enableDebugMode) Debug.Log("🔋 Selected: Shield Boost");
            return shieldBoostPrefab;
        }
        
        // Fallback
        if (speedBoostPrefab != null)
        {
            if (enableDebugMode) Debug.Log("🔋 Selected: Speed Boost (fallback)");
            return speedBoostPrefab;
        }
        
        return null;
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
            if (enableDebugMode)
            {
                Debug.Log("🔋 Requesting spawn positions from maze generator...");
            }
            
            Vector3[] validPositions = mazeGenerator.GetValidSpawnPositions(5);
            
            if (validPositions != null && validPositions.Length > 0)
            {
                Vector3 chosenPosition = validPositions[Random.Range(0, validPositions.Length)];
                chosenPosition.y = spawnHeight;
                
                if (enableDebugMode)
                {
                    Debug.Log($"🔋 Got {validPositions.Length} positions from maze, chose: {chosenPosition}");
                }
                return chosenPosition;
            }
            else
            {
                if (enableDebugMode)
                {
                    Debug.Log("🔋 Maze generator returned no valid positions, using fallback");
                }
            }
        }
        else
        {
            if (enableDebugMode)
            {
                Debug.Log("🔋 No maze generator found, using fallback positions");
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
            new Vector3(15f, spawnHeight, 0f),
            new Vector3(0f, spawnHeight, 15f),
            new Vector3(0f, spawnHeight, -15f)
        };
        
        if (enableDebugMode)
        {
            Debug.Log("🔋 Checking fallback positions...");
        }
        
        foreach (Vector3 pos in fallbackPositions)
        {
            if (!IsPositionBlocked(pos))
            {
                if (enableDebugMode)
                {
                    Debug.Log($"🔋 Found clear fallback position: {pos}");
                }
                return pos;
            }
        }
        
        // Last resort
        Vector3 lastResort = new Vector3(5f, spawnHeight, 5f);
        if (enableDebugMode)
        {
            Debug.Log($"🔋 Using last resort position: {lastResort}");
        }
        return lastResort;
    }
    
    bool IsPositionBlocked(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, 1f);
        
        foreach (Collider col in colliders)
        {
            if (col.gameObject.name.Contains("Wall") || col.gameObject.tag == "Wall")
            {
                if (enableDebugMode)
                {
                    Debug.Log($"🔋 Position {position} blocked by: {col.gameObject.name}");
                }
                return true;
            }
        }
        
        return false;
    }
    
    public void OnPowerUpRemoved()
    {
        currentActivePowerUps = Mathf.Max(0, currentActivePowerUps - 1);
        if (enableDebugMode)
        {
            Debug.Log($"🔋 Power-up removed. Active count: {currentActivePowerUps}");
        }
    }
    
    public void OnFragmentCollected()
    {
        if (!isSpawningActive)
        {
            if (enableDebugMode)
            {
                Debug.Log("🔋 Fragment collected but spawning is disabled for this level");
            }
            return;
        }
        
        if (minSpawnInterval > 6f)
        {
            minSpawnInterval = Mathf.Max(6f, minSpawnInterval - 0.5f);
            maxSpawnInterval = Mathf.Max(15f, maxSpawnInterval - 0.5f);
            
            if (enableDebugMode)
            {
                Debug.Log($"🔋 Fragment collected! Spawn interval reduced to {minSpawnInterval}-{maxSpawnInterval}s");
            }
        }
    }
    
    public void OnLevelComplete()
    {
        isSpawningActive = false;
        if (enableDebugMode)
        {
            Debug.Log("🔋 Level complete - power-up spawning stopped");
        }
    }
    
    public void OnPlayerCaught()
    {
        isSpawningActive = false;
        if (enableDebugMode)
        {
            Debug.Log("🔋 Player caught - power-up spawning stopped");
        }
    }
    
    // Enable power-ups for testing (call from console or other script)
    [ContextMenu("Force Enable Spawning")]
    public void ForceEnableSpawning()
    {
        isSpawningActive = true;
        CalculateNextSpawnTime();
        Debug.Log("🔋 Power-up spawning force enabled!");
    }
    
    [ContextMenu("Force Spawn Power-up")]
    public void ForceSpawnPowerUp()
    {
        SpawnRandomPowerUp();
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
        Debug.Log($"🔋 Power-up {gameObject.name} will expire in {lifetime}s");
    }
    
    void ExpirePowerUp()
    {
        if (powerUpManager != null)
        {
            powerUpManager.OnPowerUpRemoved();
        }
        
        Debug.Log($"🔋 Power-up {gameObject.name} expired");
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