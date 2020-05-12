// Say
// Executes SAY command

using System.Collections.Generic;

public class Say : Action
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    private ActionController controller;
    private ParserState parserState;
    private TextDisplayController textDisplayController;
    private PlayerMessageController playerMessageController;
    private CommandsController commandsController;

    private readonly List<string> magicWords = new List<string>() { "62Xyzzy", "65Plugh", "71Plover", "2025FeeFieFoe", "2034Z'zzz" };

    // === CONSTRUCTOR ===

    public Say(ActionController actionController)
    {
        // Get references to other parts of game engine
        controller = actionController;
        parserState = controller.PS;
        playerMessageController = controller.PMC;
        textDisplayController = controller.TDC;
        commandsController = controller.CC;
    }

    // === PUBLIC METHODS ===

    public override CommandOutcome DoAction()
    {
        // Get the word to be said, if any
        string[] otherWord = parserState.GetOtherWordText();

        CommandOutcome outcome = CommandOutcome.NO_COMMAND;
        bool resetFoobar = true;

        // If there were any 
        if (otherWord != null)
        {
            bool foundMagicWord = false;

            List<string> otherCommands = commandsController.FindMatch(otherWord[0]);

            foreach (string command in otherCommands)
            {
                
               if (magicWords.Contains(command))
                {
                    if (command == "2025FeeFieFoe")
                    {
                        resetFoobar = false;
                    }

                    foundMagicWord = true;
                    break;
                }
            }

            // If it's one of the magic words, discard the SAY verb and just continue to process the magic word
            if (foundMagicWord)
            {
                parserState.CurrentCommandState = CommandState.NO_COMMAND;
            }
            else
            {
                textDisplayController.AddTextToLog(playerMessageController.GetMessage("258SayWord", otherWord));
                parserState.CommandComplete();
                outcome = CommandOutcome.MESSAGE;
            }
        }
        else
        {
            parserState.CarryOverVerb();
            textDisplayController.AddTextToLog(playerMessageController.GetMessage("257VerbWhat", parserState.Words));
            parserState.CurrentCommandState = CommandState.NO_COMMAND; // Indicate we're done with this command
        }

        // If used with anything other than the Fee Fie Foe sequence, then reset foobar
        if (resetFoobar)
        {
            controller.ResetFoobar();
        }

        return outcome;
    }

    // Not used for this command
    public override string FindSubstituteSubject()
    {
        return null;
    }
}
