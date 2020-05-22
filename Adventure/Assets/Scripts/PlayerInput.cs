// Player Input
// Manages input of commands from the player

using UnityEngine;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine

    [SerializeField] private InputField inputField;
    [SerializeField] private GameController gameController;
    [SerializeField] private GameObject lowerDebugPanel;
    [SerializeField] private TextDisplayController textDisplayController;

    private Text placeholder;   // Reference to placeholder text

    // Delegate to hold call back when a new command is entered
    public delegate void OnCommandsEntered();
    public static OnCommandsEntered commandsEntered;

    // === PROPERTIES ===

    public int NumberOfWords { get; private set; }
    public CommandWord[] Words { get; } = new CommandWord[2];
    public string Placeholder 
    {
        set 
        {
            placeholder.text = value;
        }
    }

    // === MONOBEHAVIOUR METHODS ===
    private void Awake()
    {
        // Get reference to placeholder
        placeholder = inputField.placeholder.GetComponent<Text>();
    }

    private void Start()
    {
        // Set up for accepting player commands
        inputField.onEndEdit.AddListener(ProcessInput);
        inputField.ActivateInputField();
    }

    // === PUBLIC METHODS ===
    public void DisableInput()
    {
        inputField.interactable = false;
    }

    public void EnableInput()
    {
        inputField.interactable = true;
    }

    // === PRIVATE METHODS ===

    // Process the input from the player
    private void ProcessInput(string userInput)
    {
        // Clear the text field and give it the focus back ready for the next command
        inputField.text = null;

        // Only give the input field automatic focus if the lower part of debug panel is not active
        if (!gameController.debugMode || !lowerDebugPanel.activeSelf)
        {
            inputField.ActivateInputField();
        }

        // Create array with inidvidual words
        char[] delimiters = { ' ' };
        string[] inputWords = userInput.Split(delimiters, System.StringSplitOptions.RemoveEmptyEntries);
        
        // Nothing to process if learner simply hit return
        if (inputWords.Length > 0)
        {
            // Add the full text entered to the narrative in its original form
            textDisplayController.AddTextToLog(">" + userInput);

            // Calculate number of words entered and set word1 and word2 (if present) 
            NumberOfWords = inputWords.Length;
            Words[0] = formatCommandWord(inputWords[0]);
            Words[1] = NumberOfWords > 1 ? formatCommandWord(inputWords[1]) : new CommandWord();
            
            // Run the current delegate for a new command being entered
            commandsEntered();
        }
    }

    // Processes commands to a usable format (upper case and first five letters only)
    private CommandWord formatCommandWord(string rawCommandWord)
    {
        string formattedWord = rawCommandWord;
        string wordTail = null;

        if (formattedWord.Length > 5)
        {
            formattedWord = formattedWord.Substring(0, 5);
            wordTail = rawCommandWord.Substring(5);
        }

        CommandWord commandWord = new CommandWord(formattedWord, wordTail);

        return commandWord;
    }
}

public struct CommandWord
{
    public string activeWord;
    public string wordTail;

    public CommandWord(string p1, string p2)
    {
        activeWord = p1;
        wordTail = p2;
    }
}