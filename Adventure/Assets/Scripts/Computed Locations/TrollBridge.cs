// Troll Bridge
// Handles computed movement - crossing of troll bridge

public class TrollBridge : ComputedLocation
{
    // === MEMBER VARIABLES ===

    // References to other parts of the game engine
    LocationController controller;
    ItemController itemController;
    PlayerController playerController;
    TextDisplayController textDisplayController;
    PlayerMessageController playerMessageController;

    // === CONSTRUCTOR ===

    public TrollBridge(LocationController locationController)
    {
        // Get references to other parts of game engine
        controller = locationController;
        itemController = controller.IC;
        playerController = controller.PC;
        textDisplayController = controller.TDC;
        playerMessageController = controller.PMC;
    }

    // === PUBLIC METHODS ===

    public override string GetLocation()
    {
        string location = playerController.CurrentLocation;         // Player avatar's current location
        int trollState = itemController.GetItemState("33Troll");    // Current state of troll

        // If player has already used their purchased passage
        if (trollState == 1)
        {
            // Let player know the troll is blocking them again and restore troll to normal state at bridge (player doesn't move)
            textDisplayController.AddTextToLog(itemController.DescribeItem("33Troll"));
            itemController.DropItemAt("34PhonyTroll", "OutOfPlay");
            itemController.DropItemAt("33Troll", "117ChasmSW", "122ChasmNE");
            itemController.SetItemState("33Troll", 0);
        }
        else
        {
            // Move player to opposite side of chasm
            string newLoc = location == "117ChasmSW" ? "122ChasmNE" : "117ChasmSW";
          //  playerController.GoTo(newLoc, false);

            // Set troll to blocking bridge if he's still around
            if (trollState == 0)
            {
                itemController.SetItemState("33Troll", 1);
            }

            // Check if trying to cross with the bear
            if (playerController.HasItem("35Bear"))
            {
                // Let player know the bridge is destroyed, then destroy bridge and troll, put dead bear at location and kill player
                textDisplayController.AddTextToLog(playerMessageController.GetMessage("162BearWeight"));
                itemController.SetItemState("32Chasm", 1);
                itemController.SetItemState("33Troll", 2);
                itemController.DropItemAt("35Bear", location, newLoc);
                itemController.MakeItemImmovable("35Bear");
                itemController.SetItemState("35Bear", 3);
                playerController.KillPlayer();
                location = "0Death";
            }
            else
            {
                location = newLoc;
            }
            
        }

        return location;
    }
}
