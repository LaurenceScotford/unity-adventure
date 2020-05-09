// Wake
// Executes WAKE command

public class Wake : Action
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    private ActionController controller;
    private ParserState parserState;
    private GameController gameController;
    private TextDisplayController textDisplayController;
    private PlayerMessageController playerMessageController;

    // === CONSTRUCTOR ===

    public Wake(ActionController actionController)
    {
        // Get references to other parts of game engine
        controller = actionController;
        parserState = controller.PS;
        gameController = controller.GC;
        textDisplayController = controller.TDC;
        playerMessageController = controller.PMC;

        // Define behaviour for getting a subject
        carryOverVerb = true;
        subjectOptional = false;
    }

    // === PUBLIC METHODS ===

    public override CommandOutcome DoAction()
    {
        // Check whether a subject was identified
        noSubjectMsg = new NoSubjectMsg("257VerbWhat", parserState.Words);
        string itemToWake = controller.GetSubject("wake");

        // If no item could be identified then we're either done with this command or we need to continue processing to find a subject
        if (itemToWake == null)
        {
            return CommandOutcome.NO_COMMAND;
        }

        if (itemToWake != "17Dwarf" || gameController.CurrentCaveStatus != CaveStatus.CLOSED)
        {
            // Force defult message
            parserState.CurrentCommandState = CommandState.DISCARDED;
            return CommandOutcome.NO_COMMAND;
        }

        // Player has woken the dwarves, ending the game
        textDisplayController.AddTextToLog(playerMessageController.GetMessage("199WakeDwarf"));
        parserState.CommandComplete();
        return CommandOutcome.DISTURBED;
    }

    // No substitutes for this command
    public override string FindSubstituteSubject()
    {
        return null;
    }
}
