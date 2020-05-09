// Find
// Excutes the FIND command

public class Find : Action
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    private ActionController controller;
    private ParserState parserState;
    private GameController gameController;
    private PlayerController playerController;
    private ItemController itemController;
    private LocationController locationController;
    private DwarfController dwarfController;
    private TextDisplayController textDisplayController;
    private PlayerMessageController playerMessageController;

    // === CONSTRUCTOR ===
    public Find(ActionController actionController)
    {
        // Get references to other parts of game engine
        controller = actionController;
        parserState = controller.PS;
        gameController = controller.GC;
        playerController = controller.PC;
        itemController = controller.IC;
        locationController = controller.LC;
        dwarfController = controller.DC;
        playerMessageController = controller.PMC;
        textDisplayController = controller.TDC;

        // Define behaviour for getting a subject
        carryOverVerb = true;
        subjectOptional = false;
    }

    // === PUBLIC METHODS ===

    public override CommandOutcome DoAction()
    {
        noSubjectMsg = new NoSubjectMsg("257VerbWhat", parserState.Words);
        string itemToFind = controller.GetSubject("find");

        // If no item could be identified then we're either done with this command or we need to continue processing to find a subject
        if (itemToFind == null)
        {
            return CommandOutcome.NO_COMMAND;
        }

        string location = playerController.CurrentLocation;
        string findMsg = null;

        if (gameController.CurrentCaveStatus == CaveStatus.CLOSED)
        {
            findMsg = "138AroundSomewhere";
        }
        else if (playerController.HasItem(itemToFind))
        {
            findMsg = "24AlreadyHaveIt";
        }
        else
        {
            bool foundDwarf = itemToFind == "17Dwarf" && dwarfController.CountDwarvesAt(location) > 0;
            bool foundWater = itemToFind == "21Water" && ((itemController.GetItemState("20Bottle") == 0 && itemController.ItemIsAt("20Bottle", location)) || (locationController.LiquidAtLocation(location) == LiquidType.WATER));
            bool foundOil = itemToFind == "22Oil" && ((itemController.GetItemState("20Bottle") == 2 && itemController.ItemIsAt("20Bottle", location)) || (locationController.LiquidAtLocation(location) == LiquidType.OIL));
            bool foundItem = itemController.ItemIsAt(itemToFind, location);

            if (foundDwarf || foundWater || foundOil || foundItem)
            {
                findMsg = "94HaveWhatNeed";
            }
            else
            {
                // Show default message if nothing found
                parserState.CurrentCommandState = CommandState.DISCARDED;
                return CommandOutcome.NO_COMMAND;
            }
        }

        textDisplayController.AddTextToLog(playerMessageController.GetMessage(findMsg));
        parserState.CommandComplete();
        return CommandOutcome.MESSAGE;
    }

    // No substitutions for this command
    public override string FindSubstituteSubject()
    {
        return null;
    }
}
