// Read
// Executes the READ command

using System.Collections.Generic;

public class Read : Action
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    private ActionController controller;
    private ParserState parserState;
    private GameController gameController;
    private PlayerController playerController;
    private ItemController itemController;
    private ScoreController scoreController;
    private QuestionController questionController;
    private TextDisplayController textDisplayController;
    private PlayerMessageController playerMessageController;

    private bool isDark;    // True if the player can't see anything at the current location
    
    // === CONSTRUCTOR ===
    public Read(ActionController actionController)
    {
        // Get references to other parts of game engine
        controller = actionController;
        parserState = controller.PS;
        gameController = controller.GC;
        playerController = controller.PC;
        itemController = controller.IC;
        questionController = controller.QC;
        scoreController = controller.SC;
        textDisplayController = controller.TDC;
        playerMessageController = controller.PMC;

        // Define behaviour for getting a subject
        carryOverVerb = true;
        subjectOptional = false;
    }

    // === PUBLIC METHODS ===

    public override CommandOutcome DoAction()
    {
        // Note whether it's dark
        isDark = gameController.IsDark();

        // Check whether a subject was identified
        noSubjectMsg = new NoSubjectMsg("257VerbWhat", parserState.Words);
        string itemToRead = controller.GetSubject("read");

        // If no item could be identified then we're either done with this command or we need to continue processing to find a subject
        if (itemToRead == null)
        {
            return CommandOutcome.NO_COMMAND;
        }

        CommandOutcome outcome = CommandOutcome.MESSAGE;

        if (isDark)
        {
            // If it's dark, can't see object to read it
            textDisplayController.AddTextToLog(playerMessageController.GetMessage("256ISeeNo", parserState.Words));
        }
        else if (itemToRead != "15Oyster" || scoreController.ClosedHintShown)
        {
            textDisplayController.AddTextToLog(itemController.ReadItem(itemToRead));
        }
        else
        {
            // Oyster is a special case 
            questionController.RequestQuestionResponse("192Clue", "193ReadClue", "54OK", OysterYes, null);
            outcome = CommandOutcome.QUESTION;
        }

        parserState.CommandComplete();
        return outcome;
    }

    public override string FindSubstituteSubject()
    {
        // See if there's a single item here that can be read
        string itemToRead = null;

        // If the player can see anything
        if (!isDark)
        {
            // Get all the items currently carried or at player's location
            List<string> itemsHere = playerController.PresentItems();

            // Search the items...
            foreach (string item in itemsHere)
            {
                // .. for any that can be read 
                if (itemController.ItemCanBeRead(item))
                {
                    // We've not found a readable item yet, so set this as the item to read...
                    if (itemToRead == null)
                    {
                        itemToRead = item;
                    }
                    else
                    {
                        // There's more than one item to read, so we can't assume an item
                        itemToRead = null;
                        break;
                    }
                }
            }
        }
        
        return itemToRead;
    }

    // Handler for yes response to oyster question
    public void OysterYes()
    {
        scoreController.ClosedHintShown = true;
    }
}
