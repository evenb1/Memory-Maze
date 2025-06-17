using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int totalFragments = 5;
    public int collectedFragments = 0;
    public Text fragmentCounterText;
    public Text winText;
    
    void Start()
    {
        UpdateUI();
    }
    
    public void CollectFragment()
    {
        collectedFragments++;
        UpdateUI();
        
        if(collectedFragments >= totalFragments)
        {
            WinGame();
        }
    }
    
    void UpdateUI()
    {
        if(fragmentCounterText != null)
        {
            fragmentCounterText.text = "Memory Fragments: " + collectedFragments + "/" + totalFragments;
        }
    }
    
    void WinGame()
    {
        Debug.Log("YOU WIN! All memory fragments collected!");
        Time.timeScale = 0;
        if(winText != null)
        {
            winText.text = "YOU WIN! Bit-27 has recovered all memory fragments!";
        }
    }
    
    public void GameOver()
    {
        Debug.Log("GAME OVER! Bit-27 was caught by the virus!");
        Time.timeScale = 0;
        
        if(winText != null)
        {
            winText.text = "GAME OVER! Virus caught Bit-27!";
        }
    }
}