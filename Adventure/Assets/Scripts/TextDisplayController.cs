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
    [SerializeField] private Button fontSmallerButton;
    [SerializeField] private Button fontLargerButton;

    private readonly int[] fontSizes = { 14, 16, 18, 20, 24, 32, 40, 52, 64, 80 };
    private int currentFontSizeIndex;
    private string playerFontSizeKey;

    // === PROPERTIES ===

    public List<string> TextLog { get; private set; } = new List<string>();            // All the text shown in the narrative is logged here


    // MONOBEHAVIOUR METHODS
    private void Start()
    {
        playerFontSizeKey = "p" + PlayerPrefs.GetInt("CurrentPlayer") + "FontSizeIndex";

        if (PlayerPrefs.HasKey(playerFontSizeKey))
        {
            currentFontSizeIndex = PlayerPrefs.GetInt(playerFontSizeKey);
        }
        else
        {
            currentFontSizeIndex = 5;
            PlayerPrefs.SetInt(playerFontSizeKey, currentFontSizeIndex);
        }

        SetNewFontSize();
    }

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

    // Decrease size of text
    public void AdjustFontSize(bool increasing)
    {
        if (increasing && currentFontSizeIndex < fontSizes.Length - 1)
        {
            currentFontSizeIndex++;
        }
        else if (currentFontSizeIndex > 0)
        {
            currentFontSizeIndex--;
        }

        SetNewFontSize();
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

    // === PRIVATE METHODS ===

    // Sets a new font size, saves it in player prefs, makes font size buttons avialable or unavailable depending on the current index and refreshes the view
    private void SetNewFontSize()
    {
        textLogView.fontSize = fontSizes[currentFontSizeIndex];
        PlayerPrefs.SetInt(playerFontSizeKey, currentFontSizeIndex);
        fontSmallerButton.interactable = currentFontSizeIndex == 0 ? false : true;
        fontLargerButton.interactable = currentFontSizeIndex == fontSizes.Length - 1 ? false : true;
        StartCoroutine(UpdateTextDisplay());
    }

    // === COROUTINES ===

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
