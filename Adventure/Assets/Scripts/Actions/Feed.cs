// Feed
// Excutes the FEED command

public class Feed : Action
{
    // === MEMBER VARIABLES ===

    private ActionController controller;
    private ParserState parserState;
    private GameController gameController;
    private PlayerController playerController;
    private ItemController itemController;
    private DwarfController dwarfController;
    private TextDisplayController textDisplayController;
    private PlayerMessageController playerMessageController;

    // === CONSTRUCTOR ===
    public Feed(ActionController actionController)
    {
        // Get references to other parts of game engine
        controller = actionController;
        parserState = controller.PS;
        gameController = controller.GC;
        playerController = controller.PC;
        itemController = controller.IC;
        dwarfController = controller.DC;
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
        string itemToFeed = controller.GetSubject("feed");

        // If no item could be identified then we're either done with this command or we need to continue processing to find a subject
        if (itemToFeed == null)
        {
            return CommandOutcome.NO_COMMAND;
        }

        // Assume trying to feed one of the monsters (will be replaced if not appropriate)
        string feedMsg = "102NothingToEat";

        bool foodHere = playerController.ItemIsPresent("19Food");

        switch (itemToFeed)
        {
            case "8Bird":
                feedMsg = "100MontyBird";
                break;
            case "11Snake":
                // If the bird is here and the cave is not closed, the snake eats the bird
                if (!(gameController.CurrentCaveStatus == CaveStatus.CLOSED) && playerController.ItemIsPresent("8Bird"))
                {
                    feedMsg = "101SnakeAteBird";
                    itemController.SetItemState("8Bird", 0);
                    itemController.DestroyItem("8Bird");
                }
                break;
            case "31Dragon":
                // Trying to feed the dragon's corpse
                if (itemController.GetItemState("31Dragon") != 0)
                {
                    feedMsg = "110Ridiculous";
                }
                break;
            case "33Troll":
                feedMsg = "182TrollSins";
                break;
            case "17Dwarf":
                // If trying to feed food to dwarf...
                if (foodHere)
                {
                    // ... gets really mad (two increases to activation level)
                    feedMsg = "103MadDwarf";
                    dwarfController.IncreaseActivationLevel();
                    dwarfController.IncreaseActivationLevel();
                }
                else
                {
                    // Force default message
                    parserState.CurrentCommandState = CommandState.DISCARDED;
                    return CommandOutcome.NO_COMMAND;
                }
                break;
            case "35Bear":

                int bearState = itemController.GetItemState("35Bear");

                if (foodHere)
                {
                    itemController.DestroyItem("19Food");
                    itemController.SetItemState("35Bear", 1);

                    // Ensure axe is now accessible
                    itemController.MakeItemMovable("28Axe");
                    itemController.SetItemState("28Axe", 0);
                    feedMsg = "168BearFood";
                }
                else if (bearState == 3)
                {
                    // Trying to feed dead bear
                    feedMsg = "110Ridiculous";
                }
                break;
            case "41Ogre":
                if (foodHere)
                {
                    feedMsg = "202OgreNotHungry";
                }
                break;
            default:
                feedMsg = "14Explain";
                break;
        }

        textDisplayController.AddTextToLog(playerMessageController.GetMessage(feedMsg));
        parserState.CommandComplete();
        return CommandOutcome.MESSAGE;
    }

    // No substitutes for this command
    public override string FindSubstituteSubject()
    {
        return null;
    }
}
