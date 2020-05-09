// PloverAlcove
// Manages movement between the Plover Room and the Alcove

public class PloverAlcove : ComputedLocation
{
    // === MEMBER VARIABLES ===

    // References to other parts of the game engine
    LocationController controller;
    PlayerController playerController;
    TextDisplayController textDisplayController;
    PlayerMessageController playerMessageController;

    // === CONSTRUCTOR ===

    public PloverAlcove(LocationController locationController)
    {
        // Get references to other parts of game engine
        controller = locationController;
        playerController = controller.PC;
        textDisplayController = controller.TDC;
        playerMessageController = controller.PMC;
    }

    // === PUBLIC METHODS ===

    public override string GetLocation()
    {
        string locationID = playerController.CurrentLocation;
        int numItems = playerController.NumberOfItemsCarried;

        // If the player is not carrying anything (or only carrying the emerald), they can pass through the passage
        if ( numItems == 0  || (numItems == 1 && playerController.HasItem("59Emerald")))
        {
            locationID = locationID == "99Alcove" ? "100PloverRoom" : "99Alcove";
        }
        else
        {
            // Otherwise tell them something they are carrying won't fit through the passage
            textDisplayController.AddTextToLog(playerMessageController.GetMessage("117ObjectWontFit"));
        }        

        return locationID;
    }
}
