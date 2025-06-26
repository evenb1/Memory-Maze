using UnityEngine;
using UnityEngine.UI;

public class TerminalStatic : MonoBehaviour
{
    [Header("Static Settings")]
    public float staticIntensity = 0.1f;
    public float staticSpeed = 10f;
    public Vector2 staticScale = new Vector2(100f, 100f);
    
    [Header("Color")]
    public Color staticColor = Color.white;
    
    private Image staticImage;
    private Material staticMaterial;
    private float timeOffset;
    
    void Start()
    {
        staticImage = GetComponent<Image>();
        if (staticImage == null)
        {
            staticImage = gameObject.AddComponent<Image>();
        }
        
        // Create static material
        CreateStaticMaterial();
        
        // Random time offset for variation
        timeOffset = Random.Range(0f, 100f);
    }
    
    void CreateStaticMaterial()
    {
        // Create a simple static texture
        Texture2D staticTexture = CreateStaticTexture(128, 128);
        
        // Create material
        staticMaterial = new Material(Shader.Find("UI/Default"));
        staticMaterial.mainTexture = staticTexture;
        
        // Apply to image
        staticImage.material = staticMaterial;
        staticImage.color = new Color(staticColor.r, staticColor.g, staticColor.b, staticIntensity);
    }
    
    Texture2D CreateStaticTexture(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        
        for (int i = 0; i < pixels.Length; i++)
        {
            float noise = Random.Range(0f, 1f);
            pixels[i] = new Color(noise, noise, noise, noise > 0.8f ? 1f : 0f);
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
    
    void Update()
    {
        if (staticMaterial != null)
        {
            // Animate the static by offsetting UV coordinates
            float offsetX = (Time.time + timeOffset) * staticSpeed * 0.1f;
            float offsetY = (Time.time + timeOffset) * staticSpeed * 0.07f; // Different speed for Y
            
            staticMaterial.mainTextureOffset = new Vector2(offsetX, offsetY);
            staticMaterial.mainTextureScale = staticScale;
        }
    }
    
    public void SetStaticIntensity(float intensity)
    {
        staticIntensity = intensity;
        if (staticImage != null)
        {
            Color color = staticImage.color;
            color.a = intensity;
            staticImage.color = color;
        }
    }
}