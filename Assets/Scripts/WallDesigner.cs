using UnityEngine;

public class WallDesigner : MonoBehaviour
{
    [Header("Wall Materials")]
    public Material circuitBoardMaterial;
    public Material glowingWallMaterial;
    public Material dataStreamMaterial;
    
    [Header("Visual Effects")]
    public bool addRandomGlow = true;
    public bool addCircuitLines = true;
    public bool addDataPanels = true;
    
    [Header("Colors")]
    public Color primaryColor = new Color(0, 0.8f, 1f); // Cyan
    public Color secondaryColor = new Color(0, 1f, 0.3f); // Green
    public Color accentColor = new Color(1f, 0.2f, 0.8f); // Magenta
    
    void Start()
    {
        if (circuitBoardMaterial == null || glowingWallMaterial == null)
        {
            CreateMaterials();
        }
        ApplyDesignToAllWalls();
    }
    
    void CreateMaterials()
    {
        // Create circuit board material
        circuitBoardMaterial = CreateCircuitMaterial();
        
        // Create glowing wall material
        glowingWallMaterial = CreateGlowMaterial();
        
        // Create data stream material
        dataStreamMaterial = CreateDataStreamMaterial();
    }
    
    Material CreateCircuitMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.name = "CircuitBoard";
        
        // Base color - dark metallic
        mat.color = new Color(0.1f, 0.15f, 0.2f);
        mat.SetFloat("_Metallic", 0.8f);
        mat.SetFloat("_Smoothness", 0.4f);
        
        // Add emission for circuit glow
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", primaryColor * 0.3f);
        
        return mat;
    }
    
    Material CreateGlowMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.name = "GlowWall";
        
        mat.color = new Color(0.2f, 0.2f, 0.3f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", secondaryColor * 0.5f);
        mat.SetFloat("_Metallic", 0.3f);
        mat.SetFloat("_Smoothness", 0.8f);
        
        return mat;
    }
    
    Material CreateDataStreamMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.name = "DataStream";
        
        mat.color = new Color(0.05f, 0.1f, 0.15f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", accentColor * 0.4f);
        
        return mat;
    }
    
    void ApplyDesignToAllWalls()
    {
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        if (walls.Length == 0)
        {
            // If no walls tagged, find by name
            walls = FindWallsByName();
        }
        
        for (int i = 0; i < walls.Length; i++)
        {
            EnhanceWall(walls[i], i);
        }
        
        Debug.Log($"Enhanced {walls.Length} walls with cyberpunk design!");
    }
    
    GameObject[] FindWallsByName()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        System.Collections.Generic.List<GameObject> walls = new System.Collections.Generic.List<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("MazeWall") || obj.name.Contains("Wall"))
            {
                walls.Add(obj);
            }
        }
        
        return walls.ToArray();
    }
    
    void EnhanceWall(GameObject wall, int wallIndex)
    {
        Renderer renderer = wall.GetComponent<Renderer>();
        if (renderer == null) return;
        
        // Apply different materials based on position/index
        if (wallIndex % 3 == 0)
        {
            renderer.material = circuitBoardMaterial;
            if (addCircuitLines) AddCircuitLines(wall);
        }
        else if (wallIndex % 3 == 1)
        {
            renderer.material = glowingWallMaterial;
            if (addRandomGlow) AddRandomGlow(wall);
        }
        else
        {
            renderer.material = dataStreamMaterial;
            if (addDataPanels) AddDataPanels(wall);
        }
    }
    
    void AddCircuitLines(GameObject wall)
    {
        // Create thin glowing lines on the wall surface
        for (int i = 0; i < Random.Range(2, 5); i++)
        {
            GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            line.name = "CircuitLine";
            line.transform.SetParent(wall.transform);
            
            // Position randomly on wall surface
            Vector3 wallSize = wall.transform.localScale;
            float x = Random.Range(-wallSize.x * 0.4f, wallSize.x * 0.4f);
            float y = Random.Range(-wallSize.y * 0.4f, wallSize.y * 0.4f);
            
            line.transform.localPosition = new Vector3(x, y, 0.51f); // Slightly in front
            line.transform.localScale = new Vector3(0.05f, 0.05f, 0.1f);
            
            // Make it glow
            Renderer lineRenderer = line.GetComponent<Renderer>();
            Material lineMat = new Material(Shader.Find("Standard"));
            lineMat.EnableKeyword("_EMISSION");
            lineMat.SetColor("_EmissionColor", primaryColor * 2f);
            lineMat.color = primaryColor;
            lineRenderer.material = lineMat;
            
            // Remove collider
            Destroy(line.GetComponent<Collider>());
        }
    }
    
    void AddRandomGlow(GameObject wall)
    {
        // Add pulsing glow effect
        GlowPulse glowScript = wall.AddComponent<GlowPulse>();
        glowScript.baseColor = secondaryColor;
        glowScript.pulseSpeed = Random.Range(1f, 3f);
    }
    
    void AddDataPanels(GameObject wall)
    {
        // Create small glowing panels
        for (int i = 0; i < Random.Range(1, 3); i++)
        {
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = "DataPanel";
            panel.transform.SetParent(wall.transform);
            
            // Random position on wall
            Vector3 wallSize = wall.transform.localScale;
            float x = Random.Range(-wallSize.x * 0.3f, wallSize.x * 0.3f);
            float y = Random.Range(-wallSize.y * 0.2f, wallSize.y * 0.2f);
            
            panel.transform.localPosition = new Vector3(x, y, 0.52f);
            panel.transform.localScale = new Vector3(0.3f, 0.2f, 0.02f);
            
            // Glowing panel material
            Renderer panelRenderer = panel.GetComponent<Renderer>();
            Material panelMat = new Material(Shader.Find("Standard"));
            panelMat.EnableKeyword("_EMISSION");
            panelMat.SetColor("_EmissionColor", accentColor * 1.5f);
            panelMat.color = accentColor * 0.5f;
            panelRenderer.material = panelMat;
            
            // Remove collider
            Destroy(panel.GetComponent<Collider>());
            
            // Add flickering effect
            DataFlicker flicker = panel.AddComponent<DataFlicker>();
        }
    }
}