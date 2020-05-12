// Carry
// Executes CARRY command

using System.Collections.Generic;

public class Carry : Action
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    private ActionController controller;
    private ParserState parserState;
    private PlayerController playerController;
    private ItemController itemController;
    private TextDisplayController textDisplayController;
    private PlayerMessageController playerMessageController;
    
    private string itemToCarry;         // The item the player is trying to carry
    private string location;            // The current location of the player avatar
    private CommandOutcome outcome;     // The outcome of this command

    // === CONSTRUCTOR ===
    public Carry(ActionController actionController)
    {
        // Get references to other parts of game engine
        controller = actionController;
        playerController = controller.PC;
        itemController = controller.IC;
        textDisplayController = controller.TDC;
        parserState = controller.PS;
        playerMessageController = controller.PMC;

        // Define behaviour for getting a subject
        carryOverVerb = true;
        subjectOptional = false;
    }

    // === PUBLIC METHODS ===

    public override CommandOutcome DoAction()
    {
        itemToCarry = null;
        location = playerController.CurrentLocation;

        // Check whether a subject was identified, or could be identified
        noSubjectMsg = new NoSubjectMsg("257VerbWhat", parserState.Words);
        itemToCarry = controller.GetSubject("carry");

        // If no item could be identified then we're either done with this command or we need to continue processing to find a subject
        if (itemToCarry == null)
        {
            return CommandOutcome.NO_COMMAND;
        }

        outcome = CommandOutcome.MESSAGE;

        // Check for a condition that prevents us from processing the carry command further
        if (AlreadyHasItem() || CustomCarryResponse() || ItemNotMovable() || !ValidLiquidItem() || InventoryFull() || CantCatchBird())
        {
            return outcome;
        }

        // Remove any negative item state (possibly set when cave closed)
        int itemState = itemController.GetItemState(itemToCarry);

        if (itemState < 0)
        {
            itemState = -1 - itemState;
            itemController.SetItemState(itemToCarry, itemState);
        }

        // Carry the item
        playerController.CarryItem(itemToCarry);

        // For certain objects, there's some tidying up to do
        switch(itemToCarry)
        {
            case "4Cage": 
            case "8Bird":
                // If the player has carried the bird or cage and the bird is currently in the cage...
                if (itemState == 1)
                {
                    // ... then carry the other item as well
                    string otherItemToCarry = itemToCarry == "4Cage" ? "8Bird" : "4Cage";
                    playerController.CarryItem(otherItemToCarry);
                }
                break;
            case "20Bottle":
                // If the player has carried the bottle, and the bottle has liquid in it...
                if ( itemState == 0 || itemState == 2)
                {
                    // ... then carry the relevant liquid as well
                    string otherItemToCarry = itemState == 0 ? "21Water" : "22Oil";
                    playerController.CarryItem(otherItemToCarry);
                }
                break;
            case "59Emerald":
            case "65Ruby":
            case "67Amber":
            case "68Sapphire":
                // If the player has carried a gemstone and the gemstone was in the cavity...
                if (itemState != 0)
                {
                    //... then indicate the gemstone is no longer in the cavity and the cavity is now empty
                    itemController.SetItemState(itemToCarry, 0);
                    itemController.SetItemState("43Cavity", 1);
                }
                break;
        }

        // Mark the command as complete
        parserState.CommandComplete();

        // Ackowledge completion of the command
        textDisplayController.AddTextToLog(playerMessageController.GetMessage("54OK"));

        return outcome;
    }

    // Called by the generic GetSubject method in ActionController to attempt to find a substitute subject for this command, if none was supplied
    public override string FindSubstituteSubject()
    {
        string subject = null;

        // Check to see if this location has a single item ...
        if (itemController.NumberOfItemsAt(location) == 1)
        {
            // ... and if so, try to carry that
            subject = itemController.FirstItemAt(location);
        }

        return subject;
    }

    // === PRIVATE METHODS ===

    // Checks to see if the player already has the item and returns true if they do and processing must end or false if not
    private bool AlreadyHasItem()
    {
        if (playerController.HasItem(itemToCarry))
        {
            parserState.CurrentCommandState = CommandState.DISCARDED;
            outcome = CommandOutcome.NO_COMMAND;
            return true;
        }

        return false;
    }

    // Checks to see if the player is trying to catch the bird and returns true if they are unable to catch it or false otherwise
    private bool CantCatchBird()
    {
        int itemState = itemController.GetItemState(itemToCarry);

        // Check to see if the player is trying to get the uncaged bird
        if (itemToCarry == "8Bird" && itemState != 1)
        {
            bool birdProblem = false;

            // If the player is trying to recapture the bird after releasing it in the forest...
            if (itemState == 2)
            {
                // Tell the player they get crapped on and detroy the bird
                textDisplayController.AddTextToLog(playerMessageController.GetMessage("238BirdPoop"));
                itemController.DestroyItem(itemToCarry);
                birdProblem = true;
            }
            // Check if player is carrying the cage...
            else if (!playerController.HasItem("4Cage"))
            {
                //... and if not, tell them they can catch the bird but not carry it
                textDisplayController.AddTextToLog(playerMessageController.GetMessage("27NotCarryBird"));
                birdProblem = true;
            }
            // Check if the player is carrying the black rod..
            else if (playerController.HasItem("5BlackRod"))
            {
                // ... and if so, let them know the bird is frightened away
                textDisplayController.AddTextToLog(playerMessageController.GetMessage("26NotCatchBird"));
                birdProblem = true;
            }

            if (birdProblem)
            {
                // Things didn't go to plan, so we're done with this command
                parserState.CommandComplete();
                return true;
            }
            else
            {
                // We're going to carry the bird, so indicate that it is now caged
                itemController.SetItemState(itemToCarry, 1);
            }
        }

        return false;
    }

    // Checks for items that receive a custom response to attempts to carry and return true if processing can't continue or false if it can
    private bool CustomCarryResponse()
    {
        // Responses to trying to carry certain fixed items
        Dictionary<string, FixedItemResponse> fixedItemResponses = new Dictionary<string, FixedItemResponse>
        {
            { "24Plant", new FixedItemResponse("24Plant", 0, false, "115DeepRoots") },
            { "35Bear",  new FixedItemResponse("35Bear", 1, false, "169BearChained")},
            { "64Chain", new FixedItemResponse("35Bear", 0, true, "170ChainStillLocked")},
            { "42Urn", new FixedItemResponse(null, 0, false, "215CantMoveUrn")},
            { "43Cavity", new FixedItemResponse(null, 0, false, "217CollectHoles")},
            { "44Blood", new FixedItemResponse(null, 0, false, "239FewDrops")},
            { "62Rug", new FixedItemResponse("62Rug", 2, false, "222RugStationary")},
            { "49Sign", new FixedItemResponse(null, 0, false, "196NonCorporeal")},
            { "36MazeMessage", new FixedItemResponse(null, 0, false, "190WipeMessage")}
        };

        if (!itemController.ItemCanBeMoved(itemToCarry) && fixedItemResponses.ContainsKey(itemToCarry))
        {
            FixedItemResponse response = fixedItemResponses[itemToCarry];
            string itemToTest = response.requiredItemID != null ? response.requiredItemID : null;
            bool itemTest = itemToTest != null ? itemController.GetItemState(itemToTest) == response.requiredItemState : true;
            itemTest = itemTest ^ response.negated;

            // If there's no condition or there is and the conditional item has been found in the required state...
            if (itemToTest == null || itemTest)
            {
                textDisplayController.AddTextToLog(playerMessageController.GetMessage(response.messageID));

                // If the item is the maze message - attempting to carry it also destroys the message
                if (itemToCarry == "36MazeMessage")
                {
                    itemController.DestroyItem(itemToCarry);
                }
                parserState.CommandComplete(); // Indicate we're done with this command
                return true;
            }
        }
        return false;
    }
    
    // Check if the player's inventory is already full and returns true if it is or false if there's still room
    private bool InventoryFull()
    {
        // Check that the player's inventory is not full
        if (playerController.NumberOfItemsCarried >= 7)
        {
            textDisplayController.AddTextToLog(playerMessageController.GetMessage("92InventoryFull"));
            parserState.CommandComplete();
            return true;
        }

        return false;
    }

    // Checks to see if player is trying to carry an item that's not movable and returns true of processing must end and false otherwise
    private bool ItemNotMovable()
    {
        if (!itemController.ItemCanBeMoved(itemToCarry))
        {
            textDisplayController.AddTextToLog(playerMessageController.GetMessage("25NotSerious"));
            parserState.CommandComplete();
            return true;
        }

        return false;
    }

    // Checks to see if player is carrying a valid liquid item and returns true if processing can continue and false otherwise
    private bool ValidLiquidItem()
    {
        // Check if the player is trying to get oil or water
        if (itemToCarry == "21Water" || itemToCarry == "22Oil")
        {
            // Make a copy of the liquid item we're atfer and set the item to carry as the bottle
            string liquidItem = itemToCarry;
            itemToCarry = "20Bottle";

            // Check for cases where the bottle is not at this location with the correct liquid
            if (!(playerController.ItemIsPresent(itemToCarry) && itemController.GetItemState(itemToCarry) == (liquidItem == "21Water" ? 0 : 2)))
            {
                // Check if the player is carrying the bottle
                if (playerController.HasItem(itemToCarry))
                {
                    // Check if the bottle is empty
                    if (itemController.GetItemState(itemToCarry) == 1)
                    {
                        // If so, change the command to "fill bottle" and process that instead
                        CommandWord fillCommand = new CommandWord("FILL", null);
                        CommandWord bottleCommand = new CommandWord("BOTTL", null);
                        parserState.ResetParserState(new CommandWord[2] { fillCommand, bottleCommand });
                        outcome = CommandOutcome.NO_COMMAND;
                    }
                    else
                    {
                        // The bottle is not empty so let the player know
                        textDisplayController.AddTextToLog(playerMessageController.GetMessage("105BottleFull"));
                        parserState.CommandComplete();
                    }
                }
                else
                {
                    // The player is not carrying the bottle, so let them know they don't have a suitable container
                    textDisplayController.AddTextToLog(playerMessageController.GetMessage("104NoContainer"));
                    parserState.CommandComplete();
                }

                return false;
            }
        }

        return true;
    }

    // Structure used for custom carry responses
    private struct FixedItemResponse 
    {
        public string requiredItemID;   // The item required to trigger the response
        public int requiredItemState;   // The state the item should be in (or not if negated)
        public bool negated;            // True if we're checking that the item is not in that state
        public string messageID;        // The message to show if the condition is true

        public FixedItemResponse(string p1, int p2, bool p3, string p4)
        {
            requiredItemID = p1;
            requiredItemState = p2;
            negated = p3;
            messageID = p4;
        }
    }
}

