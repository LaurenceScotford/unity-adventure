// Break
// Excutes the BREAK command

public class Break : Action
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    private ActionController controller;
    private ParserState parserState;
    private GameController gameController;
    private PlayerController playerController;
    private ItemController itemController;
    private TextDisplayController textDisplayController;
    private PlayerMessageController playerMessageController;

    // === CONSTRUCTOR ===

    public Break (ActionController actionController)
    {
        // Get references to other parts of game engine
        controller = actionController;
        parserState = controller.PS;
        gameController = controller.GC;
        playerController = controller.PC;
        itemController = controller.IC;
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
        string itemToBreak = controller.GetSubject("break");

        // If no item could be identified then we're either done with this command or we need to continue processing to find a subject
        if (itemToBreak == null)
        {
            return CommandOutcome.NO_COMMAND;
        }

        const string VASE = "58Vase";

        string breakMsg;
        CommandOutcome outcome = CommandOutcome.MESSAGE;

        switch (itemToBreak)
        {
            case "23Mirror":
                if (gameController.CurrentCaveStatus == CaveStatus.CLOSED)
                {
                    breakMsg = "197BreakMirror";
                    outcome = CommandOutcome.DISTURBED;
                }
                else
                {
                    breakMsg = "148TooFarUp";
                }
               
                break;
            case VASE:
                breakMsg = "198DropVase";
                itemController.DropItemAt(VASE, playerController.CurrentLocation);
                itemController.SetItemState(VASE, 2);
                itemController.MakeItemImmovable(VASE);
                break;
            default:
                // Force default message
                parserState.CurrentCommandState = CommandState.DISCARDED;
                return CommandOutcome.NO_COMMAND;
        }

        textDisplayController.AddTextToLog(playerMessageController.GetMessage(breakMsg));
        parserState.CommandComplete();
        return outcome;
    }

    // No subsititutes for this command
    public override string FindSubstituteSubject()
    {
        return null;
    }
}
