// Off
// Excutes the OFF command

public class Off : Action
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    private ActionController controller;
    private ItemController itemController;
    private PlayerController playerController;
    private LocationController locationController;
    private TextDisplayController textDisplayController;
    private PlayerMessageController playerMessageController;
    private ParserState parserState;

    private string itemToExtinguish;    // Item the player is trying to extinguish
    private string location;            // Current location of player avatar

    private const string LAMP = "2Lantern";
    private const string URN = "42Urn";

    // === CONSTRUCTOR ===

    public Off(ActionController actionController)
    {
        // Get references to other parts of game engine
        controller = actionController;
        itemController = controller.IC;
        playerController = controller.PC;
        locationController = controller.LC;
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
        itemToExtinguish = controller.GetSubject("off");

        // If no item could be identified then we're either done with this command or we need to continue processing to find a subject
        if (itemToExtinguish == null)
        {
            return CommandOutcome.NO_COMMAND;
        }

       string[] offMsg = new string[2] { null, null };

        switch (itemToExtinguish)
        {
            case LAMP:
                // Turn the lamp off
                itemController.SetItemState(LAMP, 0);
                offMsg[0]  = "40LampOff";

                //If the location is dark, warn the player about the danger of wandering about in the dark
                if (locationController.IsDark(location))
                {
                    offMsg[1] = "16PitchDark";
                }
                break;
            case URN:
                // Turn urn off
                int urnState = itemController.GetItemState(URN);
                itemController.SetItemState(URN, urnState == 2 ? 1 : urnState);
                offMsg[0] = "210UrnDark";
                break;
            case "31Dragon":
            case "37Volcano":
                offMsg[0] = "146BeyondPower";
                break;
            default:
                parserState.CurrentCommandState = CommandState.DISCARDED;
                return CommandOutcome.NO_COMMAND;
        }

        textDisplayController.AddTextToLog(playerMessageController.GetMessage(offMsg[0]));

        if (offMsg[1] != null)
        {
            textDisplayController.AddTextToLog(playerMessageController.GetMessage(offMsg[1]));
        }

        parserState.CommandComplete();
        return CommandOutcome.MESSAGE;
    }

    public override string FindSubstituteSubject()
    {
        string subject = null;

        bool lampHere = playerController.ItemIsPresent(LAMP);
        bool urnHere = itemController.ItemIsAt(URN, location);

        // If either the lamp or the urn is here (but not both)  and is currently lit assume that item
        if (lampHere && !urnHere && itemController.GetItemState(LAMP) == 1)
        {
            subject = LAMP;
        }
        else if (urnHere && !lampHere && itemController.GetItemState(URN) == 2)
        {
            subject = URN;
        }

        return subject;
    }
}
