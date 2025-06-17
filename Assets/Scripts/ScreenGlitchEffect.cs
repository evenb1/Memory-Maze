using UnityEngine;
using UnityEngine.UI;

public class RetroGlitch : MonoBehaviour
{
    void Start()
    {
        InvokeRepeating("DoRandomGlitch", 3f, Random.Range(6f, 10f));
    }
    
    void DoRandomGlitch()
    {
        Debug.Log("Retro glitch happening!");
        
        int glitchType = Random.Range(0, 4);
        
        switch(glitchType)
        {
            case 0:
                StartCoroutine(StaticNoise());
                break;
            case 1:
                StartCoroutine(ScanLines());
                break;
            case 2:
                StartCoroutine(ColorDistortion());
                break;
            case 3:
                StartCoroutine(PixelCorruption());
                break;
        }
    }
    
    System.Collections.IEnumerator StaticNoise()
    {
        // Longer TV static effect
        GameObject staticObj = new GameObject("Static");
        staticObj.transform.SetParent(FindObjectOfType<Canvas>().transform);
        
        RawImage staticImg = staticObj.AddComponent<RawImage>();
        
        // Create static texture
        Texture2D staticTexture = new Texture2D(100, 100);
        for(int x = 0; x < 100; x++)
        {
            for(int y = 0; y < 100; y++)
            {
                float noise = Random.Range(0f, 1f);
                Color pixelColor = noise > 0.5f ? Color.white : Color.black;
                staticTexture.SetPixel(x, y, pixelColor * 0.4f);
            }
        }
        staticTexture.Apply();
        staticImg.texture = staticTexture;
        
        // Full screen
        RectTransform rect = staticImg.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        yield return new WaitForSeconds(0.4f); // Much longer
        Destroy(staticObj);
    }
    
    System.Collections.IEnumerator ScanLines()
    {
        // Slower scan line effect
        GameObject lineObj = new GameObject("ScanLine");
        lineObj.transform.SetParent(FindObjectOfType<Canvas>().transform);
        
        Image line = lineObj.AddComponent<Image>();
        line.color = new Color(0, 1, 0, 0.8f); // Brighter green scan line
        
        RectTransform lineRect = line.GetComponent<RectTransform>();
        lineRect.anchorMin = new Vector2(0, 0);
        lineRect.anchorMax = new Vector2(1, 0);
        lineRect.offsetMin = new Vector2(0, 0);
        lineRect.offsetMax = new Vector2(0, 5); // Thicker line
        
        // Slower animation from top to bottom
        float timer = 0f;
        while(timer < 1.2f) // Much slower
        {
            timer += Time.deltaTime;
            float progress = timer / 1.2f;
            lineRect.anchorMin = new Vector2(0, 1f - progress);
            lineRect.anchorMax = new Vector2(1, 1f - progress);
            yield return null;
        }
        
        Destroy(lineObj);
    }
    
    System.Collections.IEnumerator ColorDistortion()
    {
        // Longer color corruption
        TMPro.TextMeshProUGUI[] allTMPTexts = FindObjectsOfType<TMPro.TextMeshProUGUI>();
        
        // Store original colors
        Color[] originalColors = new Color[allTMPTexts.Length];
        for(int i = 0; i < allTMPTexts.Length; i++)
        {
            originalColors[i] = allTMPTexts[i].color;
        }
        
        // Multiple flashes of corruption
        for(int flash = 0; flash < 4; flash++)
        {
            // Corrupt colors
            for(int i = 0; i < allTMPTexts.Length; i++)
            {
                allTMPTexts[i].color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            }
            
            yield return new WaitForSeconds(0.15f);
            
            // Brief normal
            for(int i = 0; i < allTMPTexts.Length; i++)
            {
                allTMPTexts[i].color = originalColors[i];
            }
            
            yield return new WaitForSeconds(0.05f);
        }
    }
    
    System.Collections.IEnumerator PixelCorruption()
    {
        // Multiple corruption blocks
        GameObject[] corruptBlocks = new GameObject[5];
        
        for(int i = 0; i < 5; i++)
        {
            GameObject corruptObj = new GameObject("Corruption");
            corruptObj.transform.SetParent(FindObjectOfType<Canvas>().transform);
            
            Image corrupt = corruptObj.AddComponent<Image>();
            corrupt.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 0.6f);
            
            // Random position and size
            RectTransform rect = corrupt.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(Random.Range(-400f, 400f), Random.Range(-300f, 300f));
            rect.sizeDelta = new Vector2(Random.Range(80f, 250f), Random.Range(30f, 120f));
            
            corruptBlocks[i] = corruptObj;
        }
        
        yield return new WaitForSeconds(0.6f); // Much longer
        
        // Clean up
        for(int i = 0; i < 5; i++)
        {
            if(corruptBlocks[i] != null)
                Destroy(corruptBlocks[i]);
        }
    }
}