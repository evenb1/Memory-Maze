using UnityEngine;
using System.Collections.Generic;

public class FragmentSpawner : MonoBehaviour
{
    public GameObject fragmentPrefab;
    public int numberOfFragments = 5;
    public float spawnHeight = 1f;
    public LayerMask wallLayer = 1; // Default layer for walls
    
    // Better spawn points that avoid walls in our maze
   private Vector3[] possibleSpawnPoints = {
    new Vector3(-7, 1, 7),    // Top left corner
    new Vector3(7, 1, 7),     // Top right corner
    new Vector3(-7, 1, -7),   // Bottom left corner
    new Vector3(7, 1, -7),    // Bottom right corner
    new Vector3(-5, 1, 3),    // Left side paths
    new Vector3(-3, 1, 5),    // Top side paths
    new Vector3(3, 1, 5),     
    new Vector3(5, 1, 3),     // Right side paths
    new Vector3(5, 1, -3),    
    new Vector3(3, 1, -5),    // Bottom side paths
    new Vector3(-3, 1, -5),   
    new Vector3(-5, 1, -3),   
    new Vector3(-1, 1, 3),    // Near center but not (0,0,0)
    new Vector3(1, 1, -3),    
    new Vector3(-3, 1, -1),   
    new Vector3(3, 1, 1)      
};
    
    void Start()
    {
        SpawnRandomFragments();
    }
    
    void SpawnRandomFragments()
    {
        List<Vector3> validSpawnPoints = new List<Vector3>();
        
        // Check each spawn point to make sure it's not inside a wall
        foreach(Vector3 point in possibleSpawnPoints)
        {
            if(IsSpawnPointValid(point))
            {
                validSpawnPoints.Add(point);
            }
        }
        
        Debug.Log("Found " + validSpawnPoints.Count + " valid spawn points");
        
        // Spawn fragments at valid points
        int fragmentsSpawned = 0;
        while(fragmentsSpawned < numberOfFragments && validSpawnPoints.Count > 0)
        {
            int randomIndex = Random.Range(0, validSpawnPoints.Count);
            Vector3 spawnPosition = validSpawnPoints[randomIndex];
            
            if(fragmentPrefab != null)
            {
                GameObject fragment = Instantiate(fragmentPrefab, spawnPosition, Quaternion.identity);
                fragment.name = "MemoryFragment_" + fragmentsSpawned;
                fragmentsSpawned++;
            }
            
            validSpawnPoints.RemoveAt(randomIndex);
        }
        
        Debug.Log("Successfully spawned " + fragmentsSpawned + " memory fragments!");
    }
    
    bool IsSpawnPointValid(Vector3 point)
    {
        // Check if there's a wall at this position
        Collider[] colliders = Physics.OverlapSphere(point, 0.5f);
        
        foreach(Collider col in colliders)
        {
            if(col.gameObject.name.Contains("Wall"))
            {
                return false; // There's a wall here
            }
        }
        
        return true; // Safe to spawn here
    }
}