// PloverTransport
// Manages Emerald when telepoting out of the Plover Room

public class PloverTransport : ComputedLocation 
{
    // === MEMBER VARIABLES ===

    // References to other parts of the game engine
    LocationController controller;
    ItemController itemController;
    PlayerController playerController;

    // === CONSTRUCTOR ===

    public PloverTransport(LocationController locationController)
    {
        // Get references to other parts of game engine
        controller = locationController;
        itemController = controller.IC;
        playerController = controller.PC;
    }

    // === PUBLIC METHODS ===

    public override string GetLocation()
    {
        // Drop the emerald at the current location
        itemController.DropItemAt("59Emerald", playerController.CurrentLocation);

        // Return null to force travel to next location in the movement table
        return null;
    }
}
