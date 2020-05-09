// Rub
// Executes the RUB command

public class Rub : Action
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    private ActionController controller;
    private ParserState parserState;
    private PlayerController playerController;
    private ItemController itemController;
    private TextDisplayController textDisplayController;
    private PlayerMessageController playerMessageController;

    // === CONSTRUCTOR ===

    public Rub(ActionController actionController)
    {
        // Get references to other parts of game engine
        controller = actionController;
        parserState = controller.PS;
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
        noSubjectMsg = new NoSubjectMsg("257VerbWhat", parserState.Words);
        string itemToRub = controller.GetSubject("rub");

        // If no item could be identified then we're either done with this command or we need to continue processing to find a subject
        if (itemToRub == null)
        {
            return CommandOutcome.NO_COMMAND;
        }

        string rubMsg;

        switch (itemToRub)
        {
            case "2Lantern":
                // Force default message
                parserState.CurrentCommandState = CommandState.DISCARDED;
                return CommandOutcome.NO_COMMAND;
            case "42Urn":
                if (itemController.GetItemState("42Urn") == 2)
                {
                    string location = playerController.CurrentLocation;
                    itemController.DestroyItem("42Urn");
                    itemController.DropItemAt("43Cavity", location);
                    itemController.DropItemAt("67Amber", location);
                    itemController.SetItemState("67Amber", 1);
                    itemController.TallyTreasure("67Amber");
                    rubMsg = "216UrnGenie";
                }
                else
                {
                    goto default;
                }
                break;
            default:
                rubMsg = "76Peculiar";
                break;
        }

        textDisplayController.AddTextToLog(playerMessageController.GetMessage(rubMsg));
        parserState.CommandComplete();
        return CommandOutcome.MESSAGE;
    }

    // No substitutes for this command
    public override string FindSubstituteSubject()
    {
        return null;
    }
}
