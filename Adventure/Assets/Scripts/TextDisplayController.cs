// Text Display Controller
// Manages the scrolling text display

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextDisplayController : MonoBehaviour
{
    // === MEMBER VARIABLES ===

    [SerializeField] private Text textLogView;                    // Reference to text field that the narrative appears in
    [SerializeField] private ScrollRect scrollView;               // Reference to scrolling view on narrative text
    public List<string> textLog = new List<string>();            // All the text shown in the narrative is logged here

    // === PUBLIC  METHODS ===


    // Adds new text to the narrative log and displays it
    public void AddTextToLog(string textToAdd)
    {
        if (textToAdd != null && textToAdd != "")
        {
            textLog.Add(textToAdd);
        }
        
        // Only the last 200 entries are kept
        if (textLog.Count > 200)
        {
            textLog.RemoveAt(0);
        }
        textLogView.text = string.Join("\n\n", textLog.ToArray()) + "\n";
        StartCoroutine(UpdateTextDisplay());
    }

    // Clear text display
    public void ResetTextDisplay()
    {
        textLog.Clear();
        textLogView.text = "";
        StartCoroutine(UpdateTextDisplay());
    }

    // === PRIVATE METHODS

    // Update text display (should be called after any change)
    private IEnumerator UpdateTextDisplay()
    {
        // Wait for the next frame
        yield return null;

        // Make sure canvas is updated
        Canvas.ForceUpdateCanvases();

        // Wait for the next frame
        yield return null;

        // Then scroll view to the bottom of the text
        scrollView.verticalNormalizedPosition = 0;    
    }
}
