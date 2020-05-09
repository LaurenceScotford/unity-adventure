// Pour
// Executes the POUR command

public class Pour : Action
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    private ActionController controller;
    private ParserState parserState;
    private PlayerController playerController;
    private ItemController itemController;
    private TextDisplayController textDisplayController;
    private PlayerMessageController playerMessageController;

    private string itemToPour;          // Item player is trying to pour
    private string liquidInBottle;      // The liquid type currently in the bottle
    private string location;            // The current location of the player avatar

    // === CONSTRUCTOR ===

    public Pour(ActionController actionController)
    {
        // Get references to other parts of game engine
        controller = actionController;
        parserState = controller.PS;
        playerController = controller.PC;
        itemController = controller.IC;
        textDisplayController = controller.TDC;
        playerMessageController = controller.PMC;

        // Define behaviour for getting a subject
        carryOverVerb = true;
        subjectOptional = false;
    }

    // === PUBLIC METHODS ===

    public override CommandOutcome DoAction()
    {
        location = playerController.CurrentLocation;

        // Determine the liquid in the botttle, if any
        liquidInBottle = null;

        switch (itemController.GetItemState("20Bottle"))
        {
            case 0:
                liquidInBottle = "21Water";
                break;
            case 2:
                liquidInBottle = "22Oil";
                break;
        }

        // Check whether a subject was identified, or could be identified 
        noSubjectMsg = new NoSubjectMsg("257VerbWhat", parserState.Words);
        itemToPour = controller.GetSubject("pour");

        // If no item could be identified then we're either done with this command or we need to continue processing to find a subject
        if (itemToPour == null)
        {
            return CommandOutcome.NO_COMMAND;
        }

        // If the item to pour is the bottle, substitute the liquid, if any
        if (itemToPour == "20Bottle" && liquidInBottle != null)
        {
            itemToPour = liquidInBottle;
        }

        // If player doesn't have the object, show the default message
        if (!playerController.HasItem(itemToPour))
        {
            parserState.CurrentCommandState = CommandState.DISCARDED;
            return CommandOutcome.NO_COMMAND;
        }

        string pourMsg = null;
        CommandOutcome outcome = CommandOutcome.MESSAGE;

        // If the item being poured is not oil or water, object
        if (itemToPour != "21Water" && itemToPour != "22Oil")
        {
            pourMsg = "78NoPour";
        }
        else
        {
            // If the urn is here and empty - create a new command to fill the urn and execute that instead
            if (itemController.ItemIsAt("42Urn", location))
            {
                CommandWord fillCommand = new CommandWord("FILL", null);
                CommandWord urnCommand = new CommandWord("URN", null);
                parserState.ResetParserState(new CommandWord[2] { fillCommand, urnCommand });
                return CommandOutcome.NO_COMMAND;
            }
            else
            {
                // Set the bottle to empty and destroy the liquid
                itemController.SetItemState("20Bottle", 1);
                itemController.DestroyItem(itemToPour);

                // Check for special actions at the rusty door
                if (itemController.ItemIsAt("9RustyDoor", location))
                {
                    if (itemToPour == "22Oil")
                    {
                        // If oil used, hinges are lubricated
                        itemController.SetItemState("9RustyDoor", 1);
                        pourMsg = "114OiledHinges";
                    } 
                    else
                    {
                        // Water is used, so hinges are rusted up
                        itemController.SetItemState("9RustyDoor", 0);
                        pourMsg = "113RustyHinges";
                    }
                }
                // Check for special actions at plant
                else if (itemController.ItemIsAt("24Plant", location))
                {
                    if (itemToPour == "21Water")
                    {
                        int plantState = itemController.GetItemState("24Plant");

                        // Describe what happens to plant
                        itemController.SetItemState("24Plant", plantState + 3);
                        textDisplayController.AddTextToLog(itemController.DescribeItem("24Plant"));

                        // Set a new stable state for the plant and the phony plant
                        plantState = (plantState + 1) % 3;
                        itemController.SetItemState("24Plant", plantState);
                        itemController.SetItemState("25PhonyPlant", plantState);

                        // Force location to be redescribed
                        outcome = CommandOutcome.FULL;
                    }
                    else
                    {
                        // Player has watered plant with oil
                        pourMsg = "112PlantOil";
                    }
                }
                else
                {
                    // The player has emptied the bottle onto the ground
                    pourMsg = "77PourGround";
                }
            }
        }

        if (pourMsg != null)
        {
            textDisplayController.AddTextToLog(playerMessageController.GetMessage(pourMsg));
        }
        parserState.CommandComplete();
        return outcome;
    }

    public override string FindSubstituteSubject()
    {
        // If the player currently has the liquid in the bottle, assume that as the item to pour 
        return playerController.HasItem(liquidInBottle) ? liquidInBottle : null;
    }
}
