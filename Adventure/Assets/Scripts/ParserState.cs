// Parser State
// Managers the state of the command parser

using System;
using System.Collections.Generic;
using UnityEngine;

public class ParserState : MonoBehaviour
{
    // === MEMBER VARIABLES ===
    [SerializeField] private CommandsController commandsController;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private TextDisplayController textDisplayController;
    [SerializeField] private PlayerMessageController playerMessageController;
    public CommandToProcess[] commandsToProcess;       // The commands given by the player                  

    // === PROPERTIES ===

    // Returns a list of command IDs currently being processed
    public List<string> CurrentCommands
    {
        get
        {
            if (CommandBeingProcessed != -1)
            {
                return commandsToProcess[CommandBeingProcessed].commands;
            }
            else
            {
                return null;
            }
        }
    }

    // Gets and sets the current command state of the command being processed
    public CommandState CurrentCommandState
    {
        get
        {
            if (CommandBeingProcessed != -1)
            {
                return commandsToProcess[CommandBeingProcessed].commandState;
            }
            else
            {
                return CommandState.NO_COMMAND;
            }
        }

        set
        {
            if (CommandBeingProcessed != -1)
            {
                commandsToProcess[CommandBeingProcessed].commandState = value;

                if (value == CommandState.VERB_IDENTIFIED)
                {
                    VerbCarriedOver = null;
                }
            }
        }
    }

    // Gets and sets the currently identified subject or null if one has not been identified
    public string Subject
    {
        get
        {
            int index = FindCommandWithState(CommandState.SUBJECT_IDENTIFIED);
            if (index != -1)
            {
                return commandsToProcess[index].subject;
            }
            else
            {
                return null;
            }
        }

        set
        {
            if (CommandBeingProcessed != -1)
            {
                commandsToProcess[CommandBeingProcessed].subject = value;
                SubjectCarriedOver = null;
            }
        }
    }

    // Returns the words entered that triggered the command currently being processed
    public string[] Words
    {
        get
        {
            if (CommandBeingProcessed != -1)
            {
                return commandsToProcess[CommandBeingProcessed].words;
            }
            else
            {
                return null;
            }
        }
    }

    // Gets and sets the index of the command being processed
    public int ActiveCommandIndex
    {
        get
        {
            if (CommandBeingProcessed != -1)
            {
                return commandsToProcess[CommandBeingProcessed].activeCommand;
            }
            else
            {
                return -1;
            }
        }

        set
        {
            if (CommandBeingProcessed != -1)
            {
                commandsToProcess[CommandBeingProcessed].activeCommand = value;
            }
        }
    }

    // Returns the ID of the active command
    public string ActiveCommand
    {
        get
        {
            if (CommandBeingProcessed != -1)
            {
                return commandsToProcess[CommandBeingProcessed].commands[commandsToProcess[CommandBeingProcessed].activeCommand];
            }
            else
            {
                return null;
            }
        }
    }
    // Gets and sets the verb/subject carried over
    public string VerbCarriedOver { get; set; }
    public string SubjectCarriedOver { get; set; }
    public int CommandBeingProcessed { get; set; } // The index of the command currently being processed

    // === PUBLIC METHODS ===

    // Set the current command to be carried over as subject
    public void CarryOverSubject()
    {
        if (CommandBeingProcessed != -1 && commandsToProcess[CommandBeingProcessed].subject != null)
        {
            SubjectCarriedOver = commandsToProcess[CommandBeingProcessed].commands[commandsToProcess[CommandBeingProcessed].activeCommand];
        }
    }

    // Set current command to be carried over as verb
    public void CarryOverVerb()
    {
        if (CommandBeingProcessed != -1 && commandsToProcess[CommandBeingProcessed].commandState == CommandState.VERB_IDENTIFIED)
        {
            VerbCarriedOver = commandsToProcess[CommandBeingProcessed].commands[commandsToProcess[CommandBeingProcessed].activeCommand];
        }
    }

    // Marks this command as complete and ensures no further processing can take place in this or the next turn
    public void CommandComplete()
    {
        VerbCarriedOver = null;
        SubjectCarriedOver = null;

        for (int i = 0; i < commandsToProcess.Length; i++)
        {
            commandsToProcess[i].commandState = CommandState.NO_COMMAND;
        }
    }

    // Returns true if any of the commands currently being processed has the given state
    public bool ContainsState(CommandState state)
    {
        return FindCommandWithState(state) != -1;
    }

    // Returns the words entered for the command not currently being processed, or null if there was no second command
    public string[] GetOtherWordText()
    {
        if (CommandBeingProcessed != -1)
        {
            return commandsToProcess[1 - CommandBeingProcessed].words;
        }

        return null;
    }

    // Sets up the next command for processing. Returns true if there is processing to be done and false otherwise
    public bool NextCommandForProcessing()
    {
        // Find the first command that has not yet been processed
        CommandBeingProcessed = FindCommandWithState(CommandState.NOT_PROCESSED);

        // If all commands have been processed, then finf the first command with a pending process that needs completion
        if (CommandBeingProcessed == -1)
        {
            CommandBeingProcessed = FindCommandWithState(CommandState.PENDING);
        }

        // If all commands have been processed and there are no pending commands then try to reprocess the verb
        if (CommandBeingProcessed == -1)
        {
            CommandBeingProcessed = FindCommandWithState(CommandState.VERB_IDENTIFIED);
        }

        // Indicate if there is still more processing to do
        return CommandBeingProcessed != -1;
    }

    // Resets the parser state with new commands. Returns true if there is processing to be done and false otherwise
    public bool ResetParserState(CommandWord[] words)
    {
        commandsToProcess = new CommandToProcess[2];
        CommandBeingProcessed = -1;

        // Look for a match with available commands
        for (int i = 0; i < 2; i++)
        {
            CommandToProcess commandToProcess = new CommandToProcess();

            // If no word was entered for this command...
            if (words[i].activeWord == null)
            {
                // ... discard it
                commandToProcess.commandState = CommandState.NO_COMMAND;
            } 
            else
            {
                // Otherise, Store the words for this command
                commandToProcess.words = new string[2] { words[i].activeWord, words[i].wordTail };

                // Try to find one or more matching commands for the word
                commandToProcess.commands = commandsController.FindMatch(words[i].activeWord);
                commandToProcess.activeCommand = 0;

                // If no match was found...
                if (commandToProcess.commands.Count == 0)
                {
                    // .. discard it
                    commandToProcess.commandState = CommandState.DISCARDED;
                }
                else
                {
                    // Otherwise, mark it ready for processing
                    commandToProcess.commandState = CommandState.NOT_PROCESSED;
                }
            }

            // Add this command to the ParserState
            commandsToProcess[i] = commandToProcess;
        }

        // Now find the first unprocessed command and mark that as the current command
        CommandBeingProcessed = FindCommandWithState(CommandState.NOT_PROCESSED);

        // If there's nothing to process
        if (CommandBeingProcessed == -1)
        {
            // If nothing's been found to process but the carried over verb was Say, construct a new say verb and process that
            if (VerbCarriedOver != null && VerbCarriedOver == "2003Say")
            {
                commandsToProcess[1] = commandsToProcess[0];
                CommandToProcess sayCommand = new CommandToProcess();
                sayCommand.commands = new List<String>() { VerbCarriedOver };
                sayCommand.commandState = CommandState.NOT_PROCESSED;
                commandsToProcess[0] = sayCommand;
                VerbCarriedOver = null;
                CommandBeingProcessed = 0;
            }
            else
            {
                // ... otherwise tell the player they used a word not in the vocabulary
                textDisplayController.AddTextToLog(playerMessageController.GetMessage("254UnknownWord", new string[] { words[0].activeWord, words[0].wordTail }));
            }
        }
        
        // Return true if we found a command ready for processing and false otherwise
        return CommandBeingProcessed != -1;
    }

    // Attempts to set a carried over command for processing and returns true if the processing was successful and false otherwise, isItemWord is true if the command to be carried over is an ItemWord or false if an ActionWord
    public bool SetCarriedOverCommand(bool isItemWord)
    {
        // Check whether the require command type has been carried over
        if (isItemWord ? SubjectCarriedOver != null : VerbCarriedOver != null)
        {
            CommandState commandStateToLeaveIntact = isItemWord ? CommandState.VERB_IDENTIFIED : CommandState.SUBJECT_IDENTIFIED;

            // Find first command slot that's not the command state to leave intact...
            for (int i = 0; i < 2; i++)
            {
                if (commandsToProcess[i].commandState != commandStateToLeaveIntact)
                {
                    // ... and set it up to process the carried over command
                    commandsToProcess[i].commands = new List<string>() { isItemWord ? SubjectCarriedOver : VerbCarriedOver };
                    commandsToProcess[i].commandState = CommandState.NOT_PROCESSED;
                    CommandBeingProcessed = i;
                    VerbCarriedOver = null;
                    SubjectCarriedOver = null;
                    return true;
                }
            }
        }

        return false;
    }

    // Substitutes current verb, if any, for a new command
    public void SubstituteCommand(string command)
    {
        commandsToProcess[CommandBeingProcessed].commands = new List<string>() { command };
        commandsToProcess[CommandBeingProcessed].commandState = CommandState.VERB_IDENTIFIED;
    }

      // Checks to see if the supplied term has been identfied as the verb and returns true if yes or false otherwise
    public bool VerbIs(string verbID)
    {
        int index = FindCommandWithState(CommandState.VERB_IDENTIFIED);

        return index != -1 && commandsToProcess[index].commands[commandsToProcess[index].activeCommand] == verbID;
    }

     // =========== PRIVATE METHODS =======

    // Returns the index of the first command with the given state or -1 if none found
    private int FindCommandWithState (CommandState commandState)
    {
        for (int i = 0; i < commandsToProcess.Length; i++)
        {
            if (commandsToProcess[i].commandState == commandState)
            {
                return i;
            }
        }

        return -1;
    }
}

[Serializable]
public struct CommandToProcess
{
    public List<string> commands;           // The IDs of potential commands for processing
    public string[] words;                  // The word and word tail entered by the player that generated that command(s)
    public CommandState commandState;       // The current status of the command
    public string subject;                  // An item identfied as the subject of the command, or null if none / not an item word
    public int activeCommand;               // The index of the command to use       
}

// Used to indicate the current status of a command being processed
public enum CommandState { NO_COMMAND, NOT_PROCESSED, DISCARDED, PENDING, VERB_IDENTIFIED, SUBJECT_IDENTIFIED };
