// Drop
// Excutes DROP command

public class Drop : Action
{
    // === MEMBER VARIABLES ===

    // References to other parts of gamne engine
    private ActionController controller;
    private ParserState parserState;
    private GameController gameController;
    private TextDisplayController textDisplayController;
    private PlayerMessageController playerMessageController;
    private PlayerController playerController;
    private ItemController itemController;
    private LocationController locationController;
    private string itemToDrop;
    private string location;
    private CommandOutcome outcome;

    // === CONSTRUCTOR ===

    public Drop (ActionController actionController)
    {
        controller = actionController;
        parserState = controller.PS;
        gameController = controller.GC;
        playerMessageController = controller.PMC;
        textDisplayController = controller.TDC;
        playerController = controller.PC;
        itemController = controller.IC;
        locationController = controller.LC;

        carryOverVerb = true;
        subjectOptional = false;
    }

    // === PUBLIC METHODS ===

    public override CommandOutcome DoAction()
    {
        itemToDrop = null;
        location = playerController.CurrentLocation;

        // Check whether a subject was identified, or could be identified 
        noSubjectMsg = new NoSubjectMsg("257VerbWhat", parserState.Words);
        itemToDrop = controller.GetSubject("drop");

        // If no item could be identified then we're either done with this command or we need to continue processing to find a subject
        if (itemToDrop == null)
        {
            return CommandOutcome.NO_COMMAND;
        }

        // Check if player is trying to drop the rod but they have the phony rod, not the real rod
        DroppingRod();

        outcome = CommandOutcome.MESSAGE;

        // Check for a number of conditions that might stop processing
        if (DoesNotHaveItem() || BirdAttackSnakeEndsGame() || Vending() || DragonKillsBird())
        {
            return outcome;
        }

        // Check for custom gem interactions
        GemInteractions();

        // Check for bear v troll interaction
        BearTrollInteraction();

        // Check if vase dropped safely
        VaseDropped();

        // Ackowledge completion of the command
        textDisplayController.AddTextToLog(playerMessageController.GetMessage("54OK"));

        // Check if we're trying to drop a liquid
        LiquidDropped();
 
        // Check if we're dropping the cage with the bird in it
        CageDropped();

        // Drop the item
        itemController.DropItemAt(itemToDrop, location);

        // Check if we've dropped the bird in the forest
        BirdReleased();

        // Mark the command as complete
        parserState.CommandComplete();
        return outcome;
    }

    // There are no possible subject substitutes for this command
    public override string FindSubstituteSubject()
    {
        return null;
    }

    // === PRIVATE METHODS ===

    // Checks if the bear is being dropped at the troll's location
    private void BearTrollInteraction()
    {
        if (itemToDrop == "35Bear" && itemController.ItemIsAt("33Troll", location))
        {
            // Destroy the troll
            itemController.DestroyItem("33Troll");
            itemController.SetItemState("33Troll", 2);

            // Replace with the phony troll
            itemController.DropItemAt("34PhonyTroll", location);

            // Let the player know the bear scared the troll away
            textDisplayController.AddTextToLog(playerMessageController.GetMessage("163BearTroll"));
        }
    }

    // Checks if the bird is released in a location containing the snake. if this happens and triggers end of game, returns true, otherwise returns false
    private bool BirdAttackSnakeEndsGame()
    {
        // Check if the player is dropping the bird in a location where the snake is present
        if (itemToDrop == "8Bird" && itemController.ItemIsAt("11Snake", location))
        {
            // Let the player know that the snake has been driven away
            textDisplayController.AddTextToLog(playerMessageController.GetMessage("30BirdAtttacksSnake"));

            // Destroy the snake
            itemController.DestroyItem("11Snake");
            itemController.SetItemState("11Snake", 1);

            // If this happens after the cave is closed, trigger the "dwarves disturbed" ending
            if (gameController.CurrentCaveStatus == CaveStatus.CLOSED)
            {
                parserState.CommandComplete();
                outcome = CommandOutcome.DISTURBED;
                return true;
            }
        }

        return false;
    }

    // Check if we've relased the bird in the forest
    private void BirdReleased()
    {
        if (itemToDrop == "8Bird")
        {
            int birdState = locationController.LocType(location) == LocationType.FOREST ? 2 : 0;
            itemController.SetItemState("8Bird", birdState);
        }
    }

    // Check if we're dropping the cage while the bird is in it
    private void CageDropped()
    {
        if (itemToDrop == "4Cage" && itemController.GetItemState("8Bird") == 1)
        {
            // Drop the bird as well
            itemController.DropItemAt("8Bird", location);
        }
    }

    // Checks to see if the player does not have the item and returns true if they don't and processing must end or false if not
    private bool DoesNotHaveItem()
    {
        if (!playerController.HasItem(itemToDrop))
        {
            parserState.CurrentCommandState = CommandState.DISCARDED;
            outcome = CommandOutcome.NO_COMMAND;
            return true;
        }

        return false;
    }

    // Checks if the bird is released at the location with the dragon, returns true if so or false otherwise
    private bool DragonKillsBird()
    {
        if (itemToDrop == "8Bird" && itemController.ItemIsAt("31Dragon", location) && itemController.GetItemState("31Dragon") == 0)
        {
            // Destroy the bird and set it to its normal state
            itemController.DestroyItem("8Bird");
            itemController.SetItemState("8Bird", 0);

            // Let the player know the bird was killed
            textDisplayController.AddTextToLog(playerMessageController.GetMessage("154BirdAttackDragon"));
            return true;
        }

        return false;
    }

    // Checks if the player is trying to drop the rod but has the phony rod instead
    private void DroppingRod()
    {
        if (itemToDrop == "5BlackRod" && !playerController.HasItem(itemToDrop) && playerController.HasItem("6BlackRod"))
        {
            itemToDrop = "6BlackRod";
        }
    }


    // Checks for various gemstone interactions with cavity and rug
    private void GemInteractions()
    {
        // If dropping a gemstone at the cavity location and the cavity is currently empty...
        if ((itemToDrop == "59Emerald" || itemToDrop == "65Ruby" || itemToDrop == "67Amber" || itemToDrop == "68Sapphire") && itemController.ItemIsAt("43Cavity", location) && itemController.GetItemState("43Cavity") != 0)
        {
            // Tell the player the gem fits in the cavity and set the correct states for each object
            textDisplayController.AddTextToLog(playerMessageController.GetMessage("218GemCavity"));
            itemController.SetItemState("43Cavity", 0);
            itemController.SetItemState(itemToDrop, 1);

            // If the rug is here and the object being dropped is the emerald while the rug is not flying or the object being dropped is the ruby while the rug is flying...
            int rugState = itemController.GetItemState("62Rug");
            bool playerHasRug = playerController.HasItem("62Rug");
            if (playerController.ItemIsPresent("62Rug") && ((itemToDrop == "59Emerald" && rugState != 2) || (itemToDrop == "65Ruby" && rugState == 2)))
            {
                string rugMsg;
                // If the rug is being carried...
                if (playerHasRug)
                {
                    // It just wriggles on the player's shoulder
                    rugMsg = "220RugShoulder";
                }
                else
                {
                    // Otherwise select a message to say the rig begins or ends flight based on the gem dropped in the cavity
                    rugMsg = itemToDrop == "59Emerald" ? "219RugRises" : "221RugGround";

                    // Set the rug to its new state
                    itemController.SetItemState("62Rug", 2 - rugState);

                    // If the rug is flying,set its second location to the ledge and make it immovable
                    if (itemController.GetItemState("62Rug") == 2)
                    {
                        itemController.DropItemAt("62Rug", location, "167Ledge");
                        itemController.MakeItemImmovable("62Rug");
                    }
                    else
                    {
                        itemController.DropItemAt("62Rug", location);
                        itemController.MakeItemMovable("62Rug");
                    }
                }

                // Tell the player the effect on the rug
                textDisplayController.AddTextToLog(playerMessageController.GetMessage(rugMsg));
            }
        }
    }

    // Checks if it is liquid being dropped or the bottle containing a liquid
    private void LiquidDropped()
    {
        const string BOTTLE = "20Bottle";
        const string WATER = "21Water";
        const string OIL = "22Oil";

        if (itemToDrop == WATER || itemToDrop == OIL)
        {
            // If we're dropping a liquid, we need to actually drop the bottle
            itemToDrop = BOTTLE;
        }

        int bottleState = itemController.GetItemState(BOTTLE);

        // If we're dropping the bottle and it's not empty
        if (itemToDrop == BOTTLE && bottleState != 1)
        {
            // Destroy the liquid item it contains
            itemController.DestroyItem(bottleState == 0 ? WATER : OIL);
        }
    }

    // Check if vase is dropped safely
    private void VaseDropped()
    {
        if (itemToDrop == "58Vase")
        {
            // Set the item state based on whether the pillow is here
            int initialState = itemController.ItemIsAt("10Pillow", location) ? 1 : 3;
            itemController.SetItemState("58Vase", initialState);

            // Describe the vase
            textDisplayController.AddTextToLog(itemController.DescribeItem("58Vase"));

            // Set it to its final state
            itemController.SetItemState("58Vase", initialState - 1);

            // If the vase was broken, its location is now fixed
            if (initialState == 3)
            {
                itemController.MakeItemImmovable("58Vase");
            }
        }
    }

    // Checks if the player is buying batteries from the vending machine - returns true if they are or false otherwise
    private bool Vending()
    {
        // If the player is dropping coins in the room with the vending machine...
        if (itemToDrop == "54Coins" && itemController.ItemIsAt("38VendingMachine", location))
        {
            // Destroy the coins
            itemController.DestroyItem("54Coins");

            // Drop the batteries here and let the player know they are here
            itemController.DropItemAt("39Batteries", location);
            textDisplayController.AddTextToLog(itemController.DescribeItem("39Batteries"));
            return true;
        }

        return false;
    }
 }
