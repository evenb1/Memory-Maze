using UnityEngine;
using UnityEngine.UI;

public class AnimatedBackground : MonoBehaviour
{
    private RawImage rawImage;
    public float scrollSpeed = 0.1f;
    
    void Start()
    {
        rawImage = GetComponent<RawImage>();
        CreateGridTexture();
    }
    
    void Update()
    {
        if(rawImage != null)
        {
            // Animate the grid
            Rect uvRect = rawImage.uvRect;
            uvRect.x += scrollSpeed * Time.deltaTime;
            uvRect.y += scrollSpeed * 0.5f * Time.deltaTime;
            rawImage.uvRect = uvRect;
        }
    }
    
    void CreateGridTexture()
    {
        // Create a simple grid pattern
        Texture2D gridTexture = new Texture2D(64, 64);
        
        for(int x = 0; x < 64; x++)
        {
            for(int y = 0; y < 64; y++)
            {
                Color pixelColor = new Color(0.02f, 0.08f, 0.1f, 0.3f);
                
                // Add grid lines
                if(x % 8 == 0 || y % 8 == 0)
                {
                    pixelColor = new Color(0, 0.3f, 0.5f, 0.2f);
                }
                
                gridTexture.SetPixel(x, y, pixelColor);
            }
        }
        
        gridTexture.Apply();
        rawImage.texture = gridTexture;
        rawImage.color = new Color(1, 1, 1, 0.5f);
    }
}