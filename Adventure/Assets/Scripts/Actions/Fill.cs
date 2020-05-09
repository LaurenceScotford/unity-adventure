// Fill
// Excutes the FILL command

public class Fill : Action
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
    public Fill(ActionController actionController)
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
        carryOverVerb = true;
        subjectOptional = false;
    }

    // === PUBLIC METHODS ===

    public override CommandOutcome DoAction()
    {
        noSubjectMsg = new NoSubjectMsg("257VerbWhat", parserState.Words);
        string itemToFill = controller.GetSubject("fill");

        // If no item could be identified then we're either done with this command or we need to continue processing to find a subject
        if (itemToFill == null)
        {
            return CommandOutcome.NO_COMMAND;
        }

        string location = playerController.CurrentLocation;
        string fillMsg;
        int bottleState = itemController.GetItemState("20Bottle");
        LiquidType liquidAtLocation = locationController.LiquidAtLocation(location);

        switch (itemToFill)
        {
            case "58Vase":
                if (!playerController.HasItem("58Vase"))
                {
                    fillMsg = "29DontHaveIt";
                }
                else if (liquidAtLocation == LiquidType.NONE && !(playerController.HasItem("21Water") || playerController.HasItem("22Oil")))
                {
                    fillMsg = "144NoLiquid";
                }
                else
                {
                    // Vase breaks when filled with liquid
                    itemController.SetItemState("58Vase", 2);
                    itemController.MakeItemImmovable("58Vase");
                    itemController.DropItemAt("58Vase", location);
                    fillMsg = "145TemperatureVase";
                }
                break;
            case "42Urn":
                if (itemController.GetItemState("42Urn") != 0)
                {
                    fillMsg = "213UrnFullOil";
                }
                else
                {
                    // If the bottle is available and it contains a liquid...
                    if (playerController.ItemIsPresent("20Bottle") && bottleState != 1)
                    {
                        string liquidType = bottleState == 0 ? "21Water" : "22Oil";

                        // Empty the bottle
                        itemController.DestroyItem(liquidType);
                        itemController.SetItemState("20Bottle", 1);

                        if (liquidType == "21Water")
                        {
                            // Water gets squirted back
                            fillMsg = "211UrnSquirt";
                        }
                        else 
                        {
                            // Oil stys in the urn
                            itemController.SetItemState("42Urn", 1);
                            fillMsg = "212BottleOilToUrn";
                        }
                    }
                    else
                    {
                        // No liquid here to fill urn with
                        fillMsg = "144NoLiquid";
                    }
                }
                break;
            case "20Bottle":
                // bottle not empty
                if (bottleState != 1)
                {
                    fillMsg = "105BottleFull";
                }
                else if (liquidAtLocation != LiquidType.NONE)
                {
                    bool isWater = liquidAtLocation == LiquidType.WATER;
                    itemController.SetItemState("20Bottle", isWater ? 0 : 2);
                    playerController.CarryItem(isWater ? "21Water" : "22Oil");
                    fillMsg = isWater ? "107BottleWater" : "108BottleOil";
                }
                else if (itemController.ItemIsAt("42Urn", location) && itemController.GetItemState("42Urn") != 0)
                {
                    fillMsg = "214NoOilFromUrn";
                }
                else
                {
                    fillMsg = "106NoFill";
                }
                break;
            default:
                // Force default message
                parserState.CurrentCommandState = CommandState.DISCARDED;
                return CommandOutcome.NO_COMMAND;
        }

        textDisplayController.AddTextToLog(playerMessageController.GetMessage(fillMsg));
        parserState.CommandComplete();
        return CommandOutcome.MESSAGE;
    }

    public override string FindSubstituteSubject()
    {
        // if the bottle is present, assume the bottle
        return playerController.ItemIsPresent("20Bottle") ? "20Bottle" : null;
    }
}
