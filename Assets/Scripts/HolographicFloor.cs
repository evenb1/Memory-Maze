using UnityEngine;

public class HolographicFloor : MonoBehaviour
{
    void Start()
    {
        Invoke("ApplyFloorMaterial", 0.1f);
    }
    
    void ApplyFloorMaterial()
    {
        GameObject floor = GameObject.Find("MazeFloor");
        if(floor != null)
        {
            Renderer renderer = floor.GetComponent<Renderer>();
            if(renderer != null)
            {
                Material floorMat = new Material(renderer.material);
                
                // Dark grid-like floor
                floorMat.color = new Color(0.05f, 0.1f, 0.2f);
                
                // Subtle blue glow
                floorMat.EnableKeyword("_EMISSION");
                floorMat.SetColor("_EmissionColor", new Color(0f, 0.3f, 0.6f) * 0.1f);
                
                renderer.material = floorMat;
                
                Debug.Log("Applied holographic material to floor");
            }
        }
    }
}