// Brief
// Handles the BRIEF command

public class Brief : Action
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    private ActionController controller;
    private ParserState parserState;
    private LocationController locationController;
    private PlayerMessageController playerMessageController;
    private TextDisplayController textDisplayController;

    // === CONSTRUCTOR ===
    
    public Brief (ActionController actionController)
    {
        // Get references to other parts of game engine
        controller = actionController;
        parserState = controller.PS;
        locationController = controller.LC;
        playerMessageController = controller.PMC;
        textDisplayController = controller.TDC;

        // Define behaviour for getting a subject 
        subjectOptional = true;
    }

    public override CommandOutcome DoAction()
    {
        // If player tried to supply a subject, force default message
        if (controller.GetSubject("brief") != null)
        {
            parserState.CurrentCommandState = CommandState.DISCARDED;
            return CommandOutcome.NO_COMMAND;
        }

        // Acknowledge player's instruction and switch brief mode on
        textDisplayController.AddTextToLog(playerMessageController.GetMessage("156BeBrief"));
        locationController.SetBriefMode();
        parserState.CommandComplete();
        return CommandOutcome.MESSAGE;
    }

    // Not used for this command
    public override string FindSubstituteSubject()
    {
        return null;
    }
}
