// PlayerController
// Manages player avatar location, poossessions and movement

using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // === MEMBER VARIABLES ===

    [SerializeField] private GameController gameController;
    [SerializeField] private ItemController itemController;
    [SerializeField] private LocationController locationController;
    [SerializeField] private PlayerMessageController playerMessageController;
    [SerializeField] private TextDisplayController textDisplayController;

    [SerializeField] private string startLocation;  // Reference to starting location for player avatar
   // private string[] oldLocations = new string[2];  // Previous locations of player avatar

    // References to locations for player avatar's backpacks and death
    private const string BACKPACK = "Player";
    private const string DEATH = "0Death";

    // === PROPERTIES ===

    public string CurrentLocation { get; set; } // The current location of the player avatar
    public string[] OldLocations { get; private set; }

    // Returns a string with a list of items carried by the player avatar
    public string Inventory
    {
        get
        {
            return itemController.ListItems(BACKPACK);
        }
    }

    // True if the player avatar is currently outside the cave
    public bool IsOutside 
    { 
        get 
        {
            return locationController.IsOutside(CurrentLocation);
        } 
    }

    // True if the player avatar is currently alive
    public bool IsAlive
    {
        get
        {
            return CurrentLocation != DEATH;
        }
    }

    // True if the player avatar is trying to move on the current turn
    public bool IsMoving
    {
        get
        {
            return PotentialLocation != CurrentLocation;
        }
    }

    // Returns true if player avatar's current movement is forced
    public bool MovementIsForced
    {
        get
        {
            return locationController.TravelIsForced(CurrentLocation);
        }
    }

    // Returns the number of items currently carried by the player avatar
    public int NumberOfItemsCarried
    {
        get
        {
            return itemController.ItemsAt(BACKPACK).Count;
        }
    }

    // Returns a list of items currently carried
    public List<string>CarriedItems { get { return itemController.ItemsAt(BACKPACK); } }

    public string PotentialLocation { get; private set; }   // Location player avatar is trying to move to

    // === PUBLIC METHODS === 

    // Adds the item to the player avatar's backpack
    public void CarryItem(string itemID)
    {
        itemController.DropItemAt(itemID, BACKPACK);
    }

    // Checks to see if movement is forced from current location and moves if it is
    public bool CheckForcedMove()
    {
        bool wasForced = locationController.TravelIsForced(CurrentLocation);

        if (wasForced)
        {
            MovePlayer(null);
        }

        return wasForced;
    }

    // Commits a potential movement
    public void CommitMovement()
    {
        CurrentLocation = PotentialLocation;
    }

    public void EmptyBackpack()
    {
        DropCarriedItemsAt("OutOfPlay");
    }

    // Drop all carried items at the location the player was last safe before death
    public void DropAllAtDeathLocation()
    {
        const string LAMP = "2lantern";

        // Lamp is a special case  - it gets switched off and left at building
        if (HasItem(LAMP))
        {
            itemController.SetItemState(LAMP, 0);
            itemController.DropItemAt(LAMP, "3Building");
        }

        DropCarriedItemsAt(OldLocations[1]);
    }

     // Makes a potential movement directly to the given location. If aboslute is true, sets old locations to the given locaiton as well
    public void GoTo(string locationID, bool absolute)
    {
        if (locationController.LocationExists(locationID))
        {
            PotentialLocation = locationID;

            if (absolute)
            {
                OldLocations[0] = locationID;
                OldLocations[1] = locationID;
                CurrentLocation = locationID;
            }
            else
            {
                UpdateOldLocations();
                PotentialLocation = locationID;
            }
        }
    }

    // Sends player to start location
    public void GoToStart()
    {
        GoTo(startLocation, true);
    }

    // Returns true if player avatar is carrying item passed in
    public bool HasItem(string item)
    {
        return itemController.ItemsAt(BACKPACK).Contains(item);
    }

    // Returns true if the given item is either in the player avatar's backpack or at the player avatar's current location
    public bool ItemIsPresent(string itemID)
    {
        return itemController.ItemIsAt(itemID, CurrentLocation) || HasItem(itemID);
    }

    // Kills the player avatar by moving it to the Death location
    public void KillPlayer()
    {
        OldLocations[1] = CurrentLocation;
        PotentialLocation = DEATH;
        CurrentLocation = DEATH;
    }

    // Attempts to move player avatar to a new location and returns the outcome of the movement - potential location will be updated with the location the player avatar is trying to move to
    public CommandOutcome MovePlayer(MovementWord command)
    {
        // Start with potential location being the same as the current location
        PotentialLocation = CurrentLocation;

        // Keep track of where player avatar is moving from
        UpdateOldLocations();

        // See if the command is valid for movement from this location 
        MoveOutcome outcome = locationController.TryMovement(command);

        string moveMsg;

        if (outcome.locationID != null && outcome.locationID != "")
        {
            // The command attempts to move player avatar to a new location
            PotentialLocation = outcome.locationID;
            return CommandOutcome.FULL;
        }
        else if (outcome.messageID != null && outcome.messageID != "")
        {
            // The command triggers a message for the player instead of a movement
            moveMsg = outcome.messageID;
        }
        else
        {
            // The command doesn't do anything at this location, so show a default message for that command
            moveMsg = command.DefaultMessageID;
        }

        textDisplayController.AddTextToLog(playerMessageController.GetMessage(moveMsg));
        return CommandOutcome.MESSAGE;
    }

    // Returns true if the player avatar can currently be found at any of the given locations
    public bool PlayerCanBeFoundAt(List<string> locations)
    {
        foreach (string location in locations)
        {
            if (location == CurrentLocation)
            {
                return true;
            }
        }

        return false;
    }

    // Returns a list of items that are either carried by the player avatar or at the player avatar's current location
    public List<string> PresentItems()
    {
        List<string> presentItems = itemController.ItemsAt(CurrentLocation);
        presentItems.AddRange(itemController.ItemsAt(BACKPACK));
        return presentItems;
    }

    // Revokes a potential movement (e.g. if the player avatar's path is blocked)
    public void RevokeMovement()
    {
        PotentialLocation = CurrentLocation;
    }

    // Reset player avatar to starting state
    public void ResetPlayer()
    {
        // Set current location to starting location
        OldLocations = new string[2];
        CurrentLocation = startLocation;
        PotentialLocation = startLocation;
    }

    //  Tries to move the player back to previous location and if possible, sets potential location to the new location. Returns the outcome of the movement
    public CommandOutcome TryBack()
    {
        // Get intended destination (use previous old loc if travel was forced from old loc)
        string destination = locationController.TravelIsForced(OldLocations[0]) ? OldLocations[1] : OldLocations[0];

        // Keep track of where player avatar is coming from
        UpdateOldLocations();

        string backMsg = null;

        // Check to see if we actually moved from this location
        if (destination == CurrentLocation)
        {
            // We didn't, so claim a memory lapse
            backMsg = "91MemoryLapse";
        }
        // Check to see if moving back is allowed from this location
        else if (!locationController.CanMoveBack(CurrentLocation))
        {
            // It isn't so let player no there's no way back
            backMsg = "274NoWayBack";
        }
        // Check to see if the destination can be reached from the current location
        else if (!locationController.DestinationCanBeReached(CurrentLocation, destination))
        {
            // It can't, so let the player know
            backMsg = "140NoWayThere";
        }

        if (backMsg == null)
        {
            // Destination can be reached, so mark that as potential location
            PotentialLocation = destination;
        }
        else
        {
            textDisplayController.AddTextToLog(playerMessageController.GetMessage(backMsg));
        }
        
        return CommandOutcome.FULL;
    }

    // === PRIVATE METHODS ===

    // Drops all carried items at the given location
    public void DropCarriedItemsAt(string locationID)
    {
        List<string> carriedItems = itemController.ItemsAt(BACKPACK);

        foreach (string item in carriedItems)
        {
            itemController.DropItemAt(item, locationID);
        }
    }

    // Update old locations just before moving to new location
    private void UpdateOldLocations()
    {
        OldLocations[1] = OldLocations[0];
        OldLocations[0] = CurrentLocation;
    }
 }
