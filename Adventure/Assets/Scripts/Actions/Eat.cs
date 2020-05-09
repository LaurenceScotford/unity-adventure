// Eat
// Excutes the EAT command

public class Eat : Action
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
    public Eat(ActionController actionController)
    {
        // Get references to other parts of game engine
        controller = actionController;
        parserState = controller.PS;
        playerController = controller.PC;
        itemController = controller.IC;
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
        string itemToEat = controller.GetSubject("eat");

        // If no item could be identified then we're either done with this command or we need to continue processing to find a subject
        if (itemToEat == null)
        {
            return CommandOutcome.NO_COMMAND;
        }

        string eatMsg;

        switch(itemToEat)
        {
            case "19Food":
                itemController.DestroyItem("19Food");
                eatMsg = "72Delicious";
                break;
            case "8Bird":
            case "11Snake":
            case "14Clam":
            case "15Oyster":
            case "17Dwarf":
            case "31Dragon":
            case "33Troll":
            case "35Bear":
            case "41Ogre":
                eatMsg = "71LostAppetite";
                break;
            default:
             parserState.CurrentCommandState = CommandState.DISCARDED;
             return CommandOutcome.NO_COMMAND;
        }

        textDisplayController.AddTextToLog(playerMessageController.GetMessage(eatMsg));
        parserState.CommandComplete();
        return CommandOutcome.MESSAGE;
    }

    public override string FindSubstituteSubject()
    {
        // Assume the food if it is present
        if (playerController.ItemIsPresent("19Food"))
        {
            return "19Food";
        }
        
        return null;
    }
}
