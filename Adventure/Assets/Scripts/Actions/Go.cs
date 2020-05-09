// Go
// Excutes the GO command

public class Go : Action
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    private ActionController controller;
    private ParserState parserState;
    private PlayerMessageController playerMessageController;
    private TextDisplayController textDisplayController;

    // === CONSTRUCTOR ===
    public Go(ActionController actionController)
    {
        // Get references to other parts of game engine
        controller = actionController;
        parserState = controller.PS;
        playerMessageController = controller.PMC;
        textDisplayController = controller.TDC;
    }

    // === PUBLIC METHODS ===

    public override CommandOutcome DoAction()
    {
        // If there are no further commands to be processed, then force the default message
        if (!parserState.ContainsState(CommandState.NOT_PROCESSED))
        {
            parserState.CurrentCommandState = CommandState.DISCARDED;
        }
        else
        {
            // Otherwise - increment the go count and if it's been used 10 times, give the player a hint about not using it
            if (controller.IncrementGoCount() == 10)
            {
                textDisplayController.AddTextToLog(playerMessageController.GetMessage("276NoGo"));
            }
            
        }

        return CommandOutcome.NO_COMMAND;
    }

    // Not used for this command
    public override string FindSubstituteSubject()
    {
        return null;
    }
}
