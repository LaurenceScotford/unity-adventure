// Off
// Excutes the OFF command

public class Off : Action
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    private ActionController controller;
    private GameController gameController;
    private ItemController itemController;
    private PlayerController playerController;
    private LocationController locationController;
    private TextDisplayController textDisplayController;
    private PlayerMessageController playerMessageController;
    private ParserState parserState;

    private string itemToExtinguish;    // Item the player is trying to extinguish
    private string location;            // Current location of player avatar

    // === CONSTRUCTOR ===
    
    public Off(ActionController actionController)
    {
        // Get references to other parts of game engine
        controller = actionController;
        gameController = controller.GC;
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
            case "2Lantern":
                // Turn the lamp off
                itemController.SetItemState("2Lantern", 0);
                offMsg[0]  = "40LampOff";

                //If the location is dark, warn the player about the danger of wandering about in the dark
                if (locationController.IsDark(location))
                {
                    offMsg[1] = "16PitchDark";
                }
                break;
            case "42Urn":
                // Turn urn off
                itemController.SetItemState("42Urn", 1);
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

        bool lampHere = playerController.ItemIsPresent("2Lantern");
        bool urnHere = itemController.ItemIsAt("42Urn", location);

        // If either the lamp or the urn is here (but not both)  and is currently lit assume that item
        if (lampHere && !urnHere && itemController.GetItemState("2Lantern") == 1)
        {
            subject = "2Lantern";
        }
        else if (urnHere && !lampHere && itemController.GetItemState("42Urn") == 2)
        {
            subject = "42Urn";
        }

        return subject;
    }
}
