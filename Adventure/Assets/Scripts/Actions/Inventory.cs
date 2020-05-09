// Inventory
// Excutes the INVENTORY command

public class Inventory : Action
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    private ActionController controller;
    private ParserState parserState;
    private PlayerController playerController;
    private TextDisplayController textDisplayController;
    private PlayerMessageController playerMessageController;

    // === CONSTRUCTOR ===
    public Inventory (ActionController actionController)
    {
        // Get references to other parts of game engine
        controller = actionController;
        parserState = controller.PS;
        playerController = controller.PC;
        textDisplayController = controller.TDC;
        playerMessageController = controller.PMC;
    }

    // === PUBLIC METHODS ===

    public override CommandOutcome DoAction()
    {
        // If there was a second word, treat as a find command
        if (parserState.GetOtherWordText() != null)
        {
            return controller.ExecuteAction("find");
        }

        string inventoryText;

        int numItemsCarried = playerController.NumberOfItemsCarried;
        bool bearFollowing = playerController.HasItem("35Bear");

        if (numItemsCarried > 0 && !(numItemsCarried == 1 && bearFollowing))
        {
            inventoryText = playerMessageController.GetMessage("99InventoryList") + playerController.Inventory;
        }
        else
        {
            inventoryText = playerMessageController.GetMessage("98InventoryEmpty");
        }

        if (bearFollowing)
        {
            inventoryText += "\n\n" + playerMessageController.GetMessage("141TameBear");
        }

        textDisplayController.AddTextToLog(inventoryText);
        parserState.CommandComplete();
        return CommandOutcome.MESSAGE;
    }

    // Not used for this command
    public override string FindSubstituteSubject()
    {
        return null;
    }
}
