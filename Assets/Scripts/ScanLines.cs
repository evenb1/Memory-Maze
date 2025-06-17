using UnityEngine;
using UnityEngine.UI;

public class SubtleScanLines : MonoBehaviour
{
    public float scrollSpeed = 1f;
    private RawImage scanLineImage;
    
    void Start()
    {
        CreateSubtleScanLines();
    }
    
    void Update()
    {
        if(scanLineImage != null)
        {
            Rect uvRect = scanLineImage.uvRect;
            uvRect.y += scrollSpeed * Time.deltaTime;
            scanLineImage.uvRect = uvRect;
        }
    }
    
    void CreateSubtleScanLines()
    {
        // Much more subtle scan lines
        Texture2D scanTexture = new Texture2D(1, 8);
        for(int i = 0; i < 8; i++)
        {
            if(i == 2 || i == 6)
                scanTexture.SetPixel(0, i, new Color(0, 1, 0, 0.02f)); // Very faint green
            else
                scanTexture.SetPixel(0, i, Color.clear); // Transparent
        }
        scanTexture.Apply();
        
        GameObject scanLineObj = new GameObject("SubtleScanLines");
        scanLineObj.transform.SetParent(transform);
        
        scanLineImage = scanLineObj.AddComponent<RawImage>();
        scanLineImage.texture = scanTexture;
        
        RectTransform rect = scanLineImage.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}