using UnityEngine;

public class CircuitWallGenerator : MonoBehaviour
{
    void Start()
    {
        Invoke("ApplySimpleMaterials", 0.1f);
    }
    
    void ApplySimpleMaterials()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach(GameObject obj in allObjects)
        {
            if(obj.name == "MazeWall")
            {
                ApplyBasicCircuitLook(obj);
            }
        }
    }
    
    void ApplyBasicCircuitLook(GameObject wall)
    {
        Renderer renderer = wall.GetComponent<Renderer>();
        if(renderer != null)
        {
            // Just modify the existing material - no new shaders
            Material mat = renderer.material;
            
            // Dark green circuit board base
            mat.color = new Color(0.1f, 0.4f, 0.1f);
            
            Debug.Log("Applied simple circuit color to: " + wall.name);
        }
    }
}