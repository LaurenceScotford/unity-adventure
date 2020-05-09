// Drink
// Handles the DRINK command

public class Drink : Action
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    private ActionController controller;
    private ParserState parserState;
    private PlayerController playerController;
    private ItemController itemController;
    private LocationController locationController;
    private TextDisplayController textDisplayController;
    private PlayerMessageController playerMessageController;

    // === CONSTRUCTOR ===
    public Drink(ActionController actionController)
    {
        // Get references to other parts of game engine
        controller = actionController;
        parserState = controller.PS;
        playerController = controller.PC;
        itemController = controller.IC;
        locationController = controller.LC;
        textDisplayController = controller.TDC;
        playerMessageController = controller.PMC;

        // Define behaviour for getting a subject
        subjectOptional = false;
        carryOverVerb = true;
    }

    // === PUBLIC METHODS ===

    public override CommandOutcome DoAction()
    {
        noSubjectMsg = new NoSubjectMsg("257VerbWhat", parserState.Words);
        string itemToDrink = controller.GetSubject("drink");

        // If no item could be identified then we're either done with this command or we need to continue processing to find a subject
        if (itemToDrink == null)
        {
            return CommandOutcome.NO_COMMAND;
        }

        string drinkMsg;

        switch (itemToDrink)
        {
            case "44Blood":
                // Destroy the blood and change description of dragon to be sans blood
                itemController.DestroyItem("44Blood");
                itemController.SetItemState("31Dragon", 2);
                itemController.ChangeBirdSound();
                drinkMsg = "240HeadBuzz";
                break;
            case "21Water":
                // If the bottle is here and contains water, drink that
                if (playerController.ItemIsPresent("20Bottle") && itemController.GetItemState("20Bottle") == 0)
                {
                    // Remove liquid from bottle
                    itemController.SetItemState("20Bottle", 1);
                    itemController.DestroyItem("21Water");
                    drinkMsg = "74EmptyBottle";
                }
                else
                {
                    // Otherwise, force the default message
                    parserState.CurrentCommandState = CommandState.DISCARDED;
                    return CommandOutcome.NO_COMMAND;
                }
                break;
            default:
                drinkMsg = "110Ridiculous";
                break;
        }

        textDisplayController.AddTextToLog(playerMessageController.GetMessage(drinkMsg));
        parserState.CommandComplete();
        return CommandOutcome.MESSAGE;
    }

    public override string FindSubstituteSubject()
    {
        // If the player has a bottle containing water, or is at a location with water, then drink water
        if (playerController.HasItem("21Water") || locationController.LiquidAtLocation(playerController.CurrentLocation) == LiquidType.WATER)
        {
            return "21Water";
        }

        return null;
    }
}
