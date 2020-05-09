// Fly
// Excutes the FLY command

public class Fly : Action
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    private ActionController controller;
    private ParserState parserState;
    private PlayerController playerController;
    private ItemController itemController;
    private PlayerMessageController playerMessageController;
    private TextDisplayController textDisplayController;

    private bool rugHere;                   // Whether the rug is present

    private const string RUG = "62Rug";     // Handy reference to the rug item

    // === CONSTRUCTOR ===
    public Fly(ActionController actionController)
    {
        // Get references to other parts of game engine
        controller = actionController;
        parserState = controller.PS;
        playerController = controller.PC;
        itemController = controller.IC;
        playerMessageController = controller.PMC;
        textDisplayController = controller.TDC;

        // Define behaviour for getting a subject
        subjectOptional = true;
    }

    // === PUBLIC METHODS ===

    public override CommandOutcome DoAction()
    {
        // Note whether rug is here
        rugHere = playerController.ItemIsPresent(RUG);

        // Check whether a subject was identified
        string itemToFly = controller.GetSubject("fly");

        // If there was no subject but there's more to process, carry on processing
        if (itemToFly == null && (parserState.ContainsState(CommandState.NOT_PROCESSED) || parserState.ContainsState(CommandState.PENDING)))
        {
            return CommandOutcome.NO_COMMAND;
        }

        string flyMsg;
        CommandOutcome outcome = CommandOutcome.MESSAGE;

        if (itemToFly == null)
        {
            // Trying to fly without a subject
            flyMsg = rugHere ? "224CantUseRug" : "225FlapArms";
        }
        else if (itemToFly == RUG)
        {
            // If rug is hovering...
            if (itemController.GetItemState(RUG) == 2)
            {
                // fly across chasm on rug
                string rugLoc = itemController.Where(RUG, LOCATION_POSITION.FIRST_LOCATION);
                playerController.GoTo(playerController.CurrentLocation == rugLoc ? itemController.Where(RUG, LOCATION_POSITION.SECOND_LOCATION) : rugLoc, false);

                flyMsg = itemController.TreasureWasSeen("68Sapphire")  ? "227RugBack" : "226BoardRug";
                outcome = CommandOutcome.FULL;
            }
            else
            {
                // Rug won't fly
                flyMsg = "223RugUncooperative";
            }
        }
        else
        {
            // Trying to fly something other than the rug, so force defult message
            parserState.CurrentCommandState = CommandState.DISCARDED;
            return CommandOutcome.NO_COMMAND;
        }

        textDisplayController.AddTextToLog(playerMessageController.GetMessage(flyMsg));
        parserState.CommandComplete();
        return outcome;
    }

    public override string FindSubstituteSubject()
    {
        string itemToFly = null;

        // If no subject given, but the rug is here, assume that
        if (rugHere && itemController.GetItemState(RUG) == 2)
        {
            itemToFly = RUG;
        }

        return itemToFly;
    }
}
