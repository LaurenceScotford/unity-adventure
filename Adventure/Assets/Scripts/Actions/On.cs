// On
// Excutes the ON command

public class On : Action
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    private ActionController controller;
    private GameController gameController;
    private PlayerController playerController;
    private ItemController itemController;
    private TextDisplayController textDisplayController;
    private PlayerMessageController playerMessageController;
    private ParserState parserState;

    private string itemToLight;     // Item player is trying to light
    private string location;        // Current location of player avatar

    private const string LAMP = "2Lantern";
    private const string URN = "42Urn";

    // === CONSTRUCTOR ===
    public On(ActionController actionController)
    {
        // Get references to other parts of game engine
        controller = actionController;
        gameController = controller.GC;
        playerController = controller.PC;
        itemController = controller.IC;
        textDisplayController = controller.TDC;
        playerMessageController = controller.PMC;
        parserState = controller.PS;

        // Define behaviour for getting a subject
        carryOverVerb = true;
        subjectOptional = false;
    }

    // === PUBLIC METHODS ===

    public override CommandOutcome DoAction()
    {
        location = playerController.CurrentLocation;

        // Check whether a subject was identified, or could be identified
        noSubjectMsg = new NoSubjectMsg("257VerbWhat", parserState.Words);
        itemToLight = controller.GetSubject("on");

        // If no item could be identified then we're either done with this command or we need to continue processing to find a subject
        if (itemToLight == null)
        {
            return CommandOutcome.NO_COMMAND;
        }

        string onMsg = null;
        CommandOutcome outcome = CommandOutcome.MESSAGE;

        switch (itemToLight)
        {
            case LAMP:
                if (gameController.LampLife < 0)
                {
                    // Lamp is out of power
                    onMsg = "184LampNoPower";
                }
                else
                {
                    // Turn lamp on
                    itemController.SetItemState(LAMP, 1);
                    onMsg = "39LampOn";

                    // If it had been dark, describe the location now there's light to see by
                    if (gameController.WasDark)
                    {
                        outcome = CommandOutcome.DESCRIBE;
                    }
                }
                break;
            case URN:
                if (itemController.GetItemState(URN) == 0)
                {
                    onMsg = "38UrnEmpty";
                }
                else
                {
                    // Light the urn
                    itemController.SetItemState(URN, 2);
                    onMsg = "209UrnLit";
                }
                break;
            default:
                parserState.CurrentCommandState = CommandState.DISCARDED;
                return CommandOutcome.NO_COMMAND;
        }

        textDisplayController.AddTextToLog(playerMessageController.GetMessage(onMsg));
        parserState.CommandComplete();
        return outcome;
    }

    // Substitute lamp or urn if possible, when no subject given
    public override string FindSubstituteSubject()
    {
        string subject = null;

        bool lampHere = playerController.ItemIsPresent(LAMP);
        bool urnHere = itemController.ItemIsAt(URN, location);

        // If either the lamp or the urn is here (but not both) assume the item that is present
        if (lampHere && !urnHere && gameController.LampLife >= 0)
        {
            subject = LAMP;
        } 
        else if (urnHere && !lampHere) 
        {
            subject = URN;
        }

        return subject;
    }
}
