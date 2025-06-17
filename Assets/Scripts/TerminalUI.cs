using UnityEngine;
using UnityEngine.UI;

public class TerminalUI : MonoBehaviour
{
    void Start()
    {
        SetupTerminalUI();
    }
    
    void SetupTerminalUI()
    {
        // Removed CreateTerminalBackground() call
        StyleExistingUI();
    }
    
    void StyleExistingUI()
    {
        // Find and style the fragment counter
        Text fragmentText = GameObject.Find("FragmentCounter")?.GetComponent<Text>();
        if(fragmentText != null)
        {
            fragmentText.color = new Color(0, 1, 0, 1); // Terminal green
            fragmentText.fontSize = 20;
        }
        
        // Find and style game status text if it exists
        Text statusText = GameObject.Find("GameStatusText")?.GetComponent<Text>();
        if(statusText != null)
        {
            statusText.color = new Color(0, 1, 0, 1);
            statusText.fontSize = 28;
        }
    }
}