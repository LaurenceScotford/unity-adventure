// CommandsController
// Manages commands and their interpretation

using System.Collections.Generic;
using UnityEngine;

public class CommandsController : MonoBehaviour
{
    // === MEMBER VARIABLES ===

    // References to other parts of the game engine
    [SerializeField] private GameController gameController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private TextDisplayController textDisplayController;
    [SerializeField] private PlayerMessageController playerMessageController;
    [SerializeField] private ActionController actionController;
    [SerializeField] private DwarfController dwarfController;
    [SerializeField] private ItemController itemController;
    [SerializeField] private LocationController locationController;
    [SerializeField] private ParserState parserState;

    // An array that holds the commands - used to generate the commands dictionary
    [SerializeField] private Command[] commands;

    // A dictionary created on waking that is used to lookup individual commands 
    private Dictionary<string, Command> commandsDict = new Dictionary<string, Command>();

    // === PROPERTIES ===

    // The text for this magic word command gets randomised at the start of each new game - this property is used to access the generated text 
    public string MagicWordText { get; set; }

    // Keeps track of item referenced in previous command
    public string OldItem { get; set; }
    public int WestCount { get; set; }  // Keeps trak of the number of times player has used the word "WEST" in full rather than "W"

    // ==== MONOBEHAVIOR METHODS ===

    // On waking generate the commands dictionary from the array of commands
    private void Awake()
    {
        foreach (Command command in commands)
        {
            commandsDict.Add(command.CommandID, command);
        }
    }

    // ==== PUBLIC METHODS ===

    // Returns a list of commands IDs matching the given word (list will be empty if no matches were found)
    public List<string> FindMatch(string wordToMatch)
    {
        List<string> matchedCommands = new List<string>();
        wordToMatch = wordToMatch.ToUpper();

        // If the word to match is the generated magic word then change it to match the default pattern
        if (wordToMatch == MagicWordText)
        {
            matchedCommands.Add("2034Z'zzz");
        }
        else
        {
            foreach (Command command in commandsDict.Values)
            {
                if (command.matchesWord(wordToMatch.ToUpper()))
                {
                    matchedCommands.Add(command.CommandID);
                }
            }
        }

        return matchedCommands;
    }

    // Process a new command from the player

    public CommandOutcome ProcessCommand()
    {
        OldItem = null;

        // If there's a command to process, then process it
        if (parserState.NextCommandForProcessing())
        {
            List<string> currentCommands = parserState.CurrentCommands;

            // Prioritise the first (and possibly only) command matched
            Command command = GetCommandWithID(currentCommands[0]);

            // If there's more one match and we've already identified a verb...
            if (currentCommands.Count > 1 && parserState.ContainsState(CommandState.VERB_IDENTIFIED))
            {
                for (int i = 0; i < currentCommands.Count; i++)
                {
                    // ... then prioritise matches with item commands
                    Command nextCommand = GetCommandWithID(currentCommands[i]);

                    if (nextCommand is ItemWord)
                    {
                        command = nextCommand;
                        parserState.ActiveCommandIndex = i;
                        break;
                    }
                }
            }

            // Any command other than the FEE FIE FOE sequence or SAY will break the sequence
            if (command.CommandID != "2025FeeFieFoe" && command.CommandID != "2003Say")
            {
                actionController.ResetFoobar();
            }

            // Process the command based on its type
            if (command is MovementWord)
            {
                return ProcessMovement(command);
            }
            else if (command is ActionWord)
            {
                return ProcessAction(command);
            }
            else if (command is ItemWord)
            {
                return ProcessItem(command);
            }
            else if (command is SpecialWord)
            {
                return ProcessSpecial(command);
            }
        }

        return CommandOutcome.NO_COMMAND;
    }

    public void ResetCommands()
    {
        WestCount = 0;   //  Resets the west count that tracks use of full term "WEST"

        // Generate a new randomised magic word of the form ?'??? where ? is a random letter
        MagicWordText = "";

        for (int i = 0; i < 5; i++)
        {
            char letter = '\'';

            if (i != 1)
            {
                letter = (char)('A' + Random.Range(0, 26));
            }

            MagicWordText += letter;
        }
    }

    // ==== PRIVATE METHODS ===

    // Returns the command with the given ID, or null if not found
    private Command GetCommandWithID(string commandID)
    {
        return commandsDict.ContainsKey(commandID) ? commandsDict[commandID] : null;
    }

    // Process an action command
    private CommandOutcome ProcessAction( Command command)
    {
        parserState.CurrentCommandState = CommandState.VERB_IDENTIFIED;

        ActionWord action = (ActionWord)command;
        CommandOutcome actionOutcome = actionController.ExecuteAction(action.ActionID);

        CommandState commandState = parserState.CurrentCommandState;

        if (commandState == CommandState.DISCARDED)
        {
            if (action.DefaultMessageID != null || action.DefaultMessageID != "")
            {
                // If the action couldn't be executed and there's a default message, show the default message for that action
                textDisplayController.AddTextToLog(playerMessageController.GetMessage(action.DefaultMessageID));
            }

            // Mark this command as complete
            parserState.CommandComplete();
        }
        else if (commandState == CommandState.VERB_IDENTIFIED || commandState == CommandState.NOT_PROCESSED || (commandState == CommandState.NO_COMMAND && parserState.ContainsState(CommandState.NOT_PROCESSED)))
        {
            // If the processing of the command is suspended, pending the processing of another command, or has been reset to not processed, then continue processing 
            return ProcessCommand();
        }

        return actionOutcome;
    }

    // Process an item word
    private CommandOutcome ProcessItem(Command itemCommand)
    {
        // Select the command to use
        ItemWord command = (ItemWord)itemCommand;

        OldItem = command.AssociatedItem;

        // Get the other word entered, if any
        string[] otherWord = parserState.GetOtherWordText();
        string otherWordActive = otherWord != null ? otherWord[0].ToUpper() : null;

        // Check for use of certain item words as verbs
        if ((command.CommandID == "1021Water" || command.CommandID == "1022Oil") && (otherWordActive == "PLANT" || otherWordActive == "DOOR"))
        {
            // Player is trying to water/oil the plant/door, so construct a POUR command with this item as the subject and execute that instead
            CommandWord pourCommand = new CommandWord("POUR", null);
            CommandWord newItemCommand = new CommandWord(parserState.Words[0], parserState.Words[1]);
            parserState.ResetParserState(new CommandWord[2] { pourCommand, newItemCommand });
            return ProcessCommand();
        }
        else if (command.CommandID == "1004Cage" && otherWordActive == "BIRD" && playerController.ItemIsPresent("4Cage") && playerController.ItemIsPresent("8Bird"))
        {
            // Player is trying to cage the bird, so construct a CATCH command with the bird as the subject and execute that instead
            CommandWord catchCommand = new CommandWord("CATCH", null);
            CommandWord newItemCommand = new CommandWord(otherWord[0], otherWord[1]);
            parserState.ResetParserState(new CommandWord[2] { catchCommand, newItemCommand });
            return ProcessCommand();
        }

        // Get the matching item
        string item = command.AssociatedItem;

        bool continueProcessing = true;

        // Check if item is present
        if (!playerController.ItemIsPresent(item))
        {
            // Item is not present so check for special cases
            string loc = playerController.CurrentLocation;    // Make short ref to current location
            continueProcessing = false;        // Assume we won't continue processing 

            switch (command.CommandID)
            {
                case "1003Grate":

                    // If GRATE used on its own, treat it like a movement word, if in an appropriate location
                    if (parserState.GetOtherWordText() == null)
                    {
                        if (playerController.PlayerCanBeFoundAt(new List<string> { "1Road", "4Valley", "7SlitInStreambed" }))
                        {
                            parserState.SubstituteCommand("63Depression");
                            continueProcessing = true;
                        }
                        else if (playerController.PlayerCanBeFoundAt(new List<string> { "10CobbleCrawl", "11DebrisRoom", "12AwkwardCanyon", "13BirdChamber", "14TopOfPit" }))
                        {
                            parserState.SubstituteCommand("64Entrance");
                            continueProcessing = true;
                        }
                    }
                    
                    break;

                case "1017Dwarf":

                    // If dwarf has been referenced, check that there is actually a dwarf here
                    if (dwarfController.FirstDwarfAt(loc) != -1)
                    {
                        // There is, so process next command
                        parserState.Subject = item;
                        parserState.CurrentCommandState = CommandState.SUBJECT_IDENTIFIED;
                        continueProcessing = true;
                    }

                    break;

                case "1021Water":
                case "1022Oil":
                    // Check to see if the bottle is present with the correct liquid, or the correct liquid is present at this location
                    //Item bottle = itemController.GetItemWithID("20Bottle");
                    bool bottleHere = playerController.ItemIsPresent("20Bottle");
                    int requiredBottleState = command.CommandID == "1021Water" ? 0 : 2;
                    LiquidType requiredLiquidType = command.CommandID == "1021Water" ? LiquidType.WATER : LiquidType.OIL;

                    if ((bottleHere && itemController.GetItemState("20Bottle") == requiredBottleState) || locationController.LiquidAtLocation(loc) == requiredLiquidType)
                    {
                        // There is, so process next command
                        parserState.Subject = item;
                        parserState.CurrentCommandState = CommandState.SUBJECT_IDENTIFIED;
                        continueProcessing = true;
                    }
                    else
                    {
                        // Otherwise check to see if the player is trying to manipulate oil and the urn is here and contains oil
                        if (command.CommandID == "1022Oil" && itemController.ItemIsAt("42Urn", loc) && itemController.GetItemState("42Urn") != 0)
                        {
                            // If so, set the subject to be the urn and continue processing
                            parserState.Subject = "42Urn";
                            parserState.CurrentCommandState = CommandState.SUBJECT_IDENTIFIED;
                            continueProcessing = true;
                        }
                    }

                    break;

                case "1024Plant":
                    // Check if player is trying to manipulate the plant, but is actually at a location where the phony plant is present
                    if (itemController.ItemIsAt("25PhonyPlant", loc) && itemController.GetItemState("25PhonyPlant") != 0)
                    {
                        // If so, set the subject to be the phony plant and continue processing
                        parserState.Subject = "25PhonyPlant";
                        parserState.CurrentCommandState = CommandState.SUBJECT_IDENTIFIED;
                        continueProcessing = true;
                    }

                    break;

                case "1005Rod":
                    // If the player is trying to do something with the rod, check if they are at the phony rod location
                    if (playerController.ItemIsPresent("6BlackRod"))
                    {
                        // If so, set the subject to be the phony rod and continue processing
                        parserState.Subject = "6BlackRod";
                        parserState.CurrentCommandState = CommandState.SUBJECT_IDENTIFIED;
                        continueProcessing = true;
                    }
                    break;
            }

            // If we have a verb and it's FIND or INVENT, then it's OK that the subject is not present
            if (!continueProcessing && (parserState.VerbIs("2019Find") || parserState.VerbIs("2020Inventory")))
            {
                parserState.Subject = item;
                parserState.CurrentCommandState = CommandState.SUBJECT_IDENTIFIED;
                continueProcessing = true;
            }

            if (!continueProcessing)
            {
                // Mark this as discarded...
                parserState.CurrentCommandState = CommandState.DISCARDED;

                // ... but check if there's still more commands to be processed
                if (parserState.ContainsState(CommandState.NOT_PROCESSED))
                {
                    // ... if so, mark this as pending and process other command
                    parserState.CurrentCommandState = CommandState.PENDING;
                    continueProcessing = true;
                }
                else
                {
                    // Object not present and isn't a special case so let player know that object is not here
                    textDisplayController.AddTextToLog(playerMessageController.GetMessage("256ISeeNo", parserState.Words));
                    return CommandOutcome.MESSAGE;
                }
            }
        }
        else
        {
            // Special case for knife
            if (item == "18Knife")
            {
                dwarfController.KnifeMessageShown();
                textDisplayController.AddTextToLog(playerMessageController.GetMessage("116KnivesVanish"));
                parserState.CommandComplete();
                return CommandOutcome.MESSAGE;
            }

            // The item is here, so set it as the subject
            parserState.Subject = item;
            parserState.CurrentCommandState = CommandState.SUBJECT_IDENTIFIED;
        }

        // Now check to see if we have a verb or an unprocessed command, or a carried over verb, and if so, go back to processing
        if (parserState.ContainsState(CommandState.VERB_IDENTIFIED) || parserState.ContainsState(CommandState.NOT_PROCESSED) || parserState.SetCarriedOverCommand(false))
        {
            return ProcessCommand();
        }

        // If we get here, the player has just given us the name of the item, so ask them what they want to do with it
        parserState.CarryOverSubject();
        textDisplayController.AddTextToLog(playerMessageController.GetMessage("255WantToDo", parserState.Words));
        return CommandOutcome.MESSAGE;
    }


    // Process a movement command
    private CommandOutcome ProcessMovement(Command movementCommand)
    {
        Command command = (MovementWord)movementCommand;
        parserState.CommandComplete();  // We have a movement command so we don't need to process anything else

        // if the player has used the full command "WEST" ...
        if (parserState.Words[0].ToUpper() == "WEST")
        {
            if (++WestCount == 10)
            {
                textDisplayController.AddTextToLog(playerMessageController.GetMessage("17ShortWest"));
            }
        }
        else if (command.CommandID == "3Enter")
        {
            // Get the other word entered, if any
            string[] otherWord = parserState.GetOtherWordText();
            string otherWordActive = otherWord != null ? otherWord[0].ToUpper() : null;

            // If the player is trying to enter water or the stream and there is water at the player avatar's current location...
            if ((otherWordActive == "WATER" || otherWordActive == "STREA") && locationController.LiquidAtLocation(playerController.CurrentLocation) == LiquidType.WATER)
            {
                // Let the player know their feet are now wet
                textDisplayController.AddTextToLog(playerMessageController.GetMessage("70FeetWet"));
                return CommandOutcome.MESSAGE;
            }
        }

        switch (command.CommandID)
        {
            case "21Null":
                return CommandOutcome.FULL;
            case "8Back":
                return playerController.TryBack();
            case "57Look":
                return locationController.Look();
            case "67Cave":
                // Player is trying to go directly to CAVE. Show an appropriate message
                string loc = playerController.CurrentLocation;
                string caveMsg = (locationController.IsOutside(loc) && loc != "8OutsideGrate") ? "57WhereCave" : "58MoreInstruction";
                textDisplayController.AddTextToLog(playerMessageController.GetMessage(caveMsg));
                return CommandOutcome.FULL;
            default:
                // Standard movement
                return playerController.MovePlayer((MovementWord)command);
        }
    }


    // Process a special word
    private CommandOutcome ProcessSpecial(Command specialCommand)
    {
        // Select the command to use
        SpecialWord command = (SpecialWord)specialCommand;

        // Display the default message
        textDisplayController.AddTextToLog(playerMessageController.GetMessage(command.DefaultMessageID));

        parserState.CommandComplete();
        return CommandOutcome.MESSAGE;
    }

}

// Possible outcomes of a command used to determine engine behaviour following command execution
// NO_COMMAND - the command could not be executed on this pass (can be temporary)
// FULL - start the next full turn
// DESCRIBE - Just redescribe the location and items present then wait for next player command
// MESSAGE - the command showed a message, so just wait for next player command
// DISTURBED - The command resulted in the dwarves being disturbed, so the game will end
// QUESTION - the command resulted in the player being asked a YES?NO question so just wait for the reponse handler to restore normal service
// ENDED - the command ended the game
public enum CommandOutcome { NO_COMMAND, FULL, DESCRIBE, MESSAGE, DISTURBED, QUESTION, ENDED }
