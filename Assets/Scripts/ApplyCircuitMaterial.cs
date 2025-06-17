using UnityEngine;

public class ApplyCircuitMaterial : MonoBehaviour
{
    public Material circuitBoardMaterial; // We'll drag your material here
    
    void Start()
    {
        Invoke("ApplyToAllWalls", 0.1f);
    }
    
    void ApplyToAllWalls()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach(GameObject obj in allObjects)
        {
            if(obj.name == "MazeWall" && circuitBoardMaterial != null)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if(renderer != null)
                {
                    renderer.material = circuitBoardMaterial;
                    Debug.Log("Applied circuit material to: " + obj.name);
                }
            }
        }
    }
}