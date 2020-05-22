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
    
    // === PROPERTIES ===

    public List<string> TextLog { get; private set; } = new List<string>();            // All the text shown in the narrative is logged here

    // === PUBLIC  METHODS ===


    // Adds new text to the narrative log and displays it
    public void AddTextToLog(string textToAdd)
    {
        if (textToAdd != null && textToAdd != "")
        {
            TextLog.Add(textToAdd);
        }

        StartCoroutine(UpdateTextDisplay());
    }

    // Clear text display
    public void ResetTextDisplay()
    {
        TextLog.Clear();
        textLogView.text = "";
        StartCoroutine(UpdateTextDisplay());
    }

    // Restore TextDisplayController from saved game data
    public void Restore(GameData gameData)
    {
        TextLog = gameData.textLog;
        StartCoroutine(UpdateTextDisplay());
    }

    // === PRIVATE METHODS

    // Update text display (should be called after any change)
    private IEnumerator UpdateTextDisplay()
    {
        textLogView.text = string.Join("\n\n", TextLog.ToArray()) + "\n";

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
