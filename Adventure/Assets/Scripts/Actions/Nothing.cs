// Nothing
// Excutes the NOTHING command

public class Nothing : Action
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    private ActionController controller;
    private TextDisplayController textDisplayController;
    private PlayerMessageController playerMessageController;
    private ParserState parserState;

    // === CONSTRUCTOR ===

    public Nothing(ActionController actionController)
    {
        // Get references to other parts of game engine
        controller = actionController;
        textDisplayController = controller.TDC; ;
        playerMessageController = controller.PMC;
        parserState = controller.PS;
    }

    // === PUBLIC METHODS ===

    public override CommandOutcome DoAction()
    {
        // In all cases simply acknowledge action
        textDisplayController.AddTextToLog(playerMessageController.GetMessage("54OK"));

        // We're done with this command
        parserState.CommandComplete();
        return CommandOutcome.MESSAGE;
    }

    // Not used for this command
    public override string FindSubstituteSubject()
    {
        return null;
    }
}
