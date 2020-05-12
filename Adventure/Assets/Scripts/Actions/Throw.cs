// Throw
// Executes THROW command

public class Throw : Action
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    private ActionController controller;
    private ParserState parserState;
    private ItemController itemController;
    private DwarfController dwarfController;
    private GameController gameController;
    private PlayerController playerController;
    private TextDisplayController textDisplayController;
    private PlayerMessageController playerMessageController;

    private string itemToThrow;                     // Item ID for the item the player is trying to throw
    private string location;                        // The current location of the player
    private CommandOutcome outcome;                 // The outcome of the throw command

    // === CONSTRUCTOR === 

    public Throw(ActionController actionController)
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
        location = playerController.CurrentLocation;        // Get player's current location

        // Attempt to get an item to throw
        noSubjectMsg = new NoSubjectMsg("257VerbWhat", parserState.Words);
        itemToThrow = controller.GetSubject("throw");

        // If no item could be identified then we're either done with this command or we need to continue processing to find a subject
        if (itemToThrow == null)
        {
            return CommandOutcome.NO_COMMAND;
        }

        // If subject is real rod, but player only carrying phony rod, then set phony rod as subject
        if (itemToThrow == "5BlackRod" && !playerController.HasItem("5BlackRod") && playerController.HasItem("6BlackRod"))
        {
            itemToThrow = "6BlackRod";
        }

        // If player is not holding the object, show default message
        if (!playerController.HasItem(itemToThrow))
        {
            parserState.CurrentCommandState = CommandState.DISCARDED;
            return CommandOutcome.NO_COMMAND;
        }

        string throwMsg = null; // Will hold the message to be shown to the player as a result of the command
        outcome = CommandOutcome.MESSAGE;       // In most cases, outcome will be message (will be updated if not)

        // If at troll bridge and throwing a treasure, it is assumed to be a toll to cross
        if (itemController.IsTreasure(itemToThrow) && itemController.ItemIsAt("33Troll", location))
        {
            throwMsg = ThrowTreasureToTroll();
        }
        // If throwing food in the presence of the bear...
        else if (itemToThrow == "19Food" && itemController.ItemIsAt("35Bear", location))
        {
            // ... construct a feed command with the bear as subject and execute that instead
            CommandWord feedCommand = new CommandWord("FEED", null);
            CommandWord itemCommand = new CommandWord("BEAR", null);
            parserState.ResetParserState(new CommandWord[2] { feedCommand, itemCommand });
            return CommandOutcome.NO_COMMAND;
        }
        // Throwing the axe
        else if (itemToThrow == "28Axe")
        {
            throwMsg = ThrowAxe();
        }
        // In all other circumstances...
        else
        {
            // ...execute a drop command instead
            return controller.ExecuteAction("drop");
        }

        if (throwMsg != null)
        {
            textDisplayController.AddTextToLog(playerMessageController.GetMessage(throwMsg));
        }

        parserState.CommandComplete();
        return outcome;
    }

    // No substitutes used for this command
    public override string FindSubstituteSubject()
    {
        return null;
    }

    // === PRIVATE METHODS ===

    // Throw the axe and return a message for the player
    private string ThrowAxe()
    {
        string returnMsg = null;
        bool refresh = true;

        int firstDwarf = dwarfController.FirstDwarfAt(location);

        if (firstDwarf >= 0)
        {
            returnMsg = "48AttackDwarf";

            // If player has killed dwarf changed message from attacked to killed
            if (dwarfController.AttackDwarf(firstDwarf))
            {
                // First kill gets a slightly more descriptive message
                returnMsg = dwarfController.DwarvesKilled == 1 ? "149KilledDwarf" : "47KilledDwarf";
            }
        }
        else if (itemController.ItemIsAt("31Dragon", location) && itemController.GetItemState("31Dragon") == 0)
        {
            returnMsg = "152DragonAxe";
        }
        else if (itemController.ItemIsAt("33Troll", location))
        {
            returnMsg = "158TrollAxe";
        }
        else if (itemController.ItemIsAt("41Ogre", location))
        {
            returnMsg = "203OgreDodges";
        }
        else if (itemController.ItemIsAt("35Bear", location) && itemController.GetItemState("35Bear") == 0)
        {
            // Make axe inaccessible
            itemController.SetItemState("28Axe", 1);
            itemController.MakeItemImmovable("28Axe");
            returnMsg = "164BearAxe";
            refresh = false;
        }

        if (refresh)
        {
            outcome = CommandOutcome.DESCRIBE;
        }
       
        itemController.DropItemAt("28Axe", location);
        return returnMsg;
    }

    // Throws a treasure to the troll and returns message to be shown
    private string ThrowTreasureToTroll()
    {
        // Destroy the item and the troll and replace with the phony troll (basically a stand in to trigger the "no sign of the troll" message)
        itemController.DestroyItem(itemToThrow);
        itemController.DestroyItem("33Troll");
        itemController.DropItemAt("34PhonyTroll", "117ChasmSW", "122ChasmNE");

        return "159TrollTreasure";
    }
}
