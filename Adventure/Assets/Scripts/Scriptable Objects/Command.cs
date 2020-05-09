// Command
// Object representing a single command - This is the base class for commands of specific types (MovementWord, ItemWord, ActionWord and SpecialWord)

using UnityEngine;

public abstract class Command : ScriptableObject
{
    [SerializeField] private string commandID;              // Unique ID for this command
    [SerializeField] private string[] words;                // Array of words that will trigger this command
    [SerializeField] private string defaultMessageID;       // The ID of the default message that is shown if the command does not generate a bespoke response
    
    public string CommandID { get { return commandID; } }                               // Read only property for the command ID
    public string DefaultMessageID { get { return defaultMessageID; } }                 // Read only property for the default message ID

    // Returns true if wordToMatch matches any of the permitted words for this command
    public bool matchesWord(string wordToMatch) 
    {
        foreach (var word in words)
        {
            if (word == wordToMatch)
            {
                return true;
            }
        }

        return false;
    }
}

