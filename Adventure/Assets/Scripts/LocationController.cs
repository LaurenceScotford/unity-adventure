// Location Controller
// Manages locations, their attributes and connections

using System.Collections.Generic;
using UnityEngine;

public class LocationController : MonoBehaviour
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    [SerializeField] private GameController gameController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private ItemController itemController;
    [SerializeField] private TextDisplayController textDisplayController;
    [SerializeField] private PlayerMessageController playerMessageController;

    // Array holding all the locations (used to generate a dictionary)
    [SerializeField] private Location[] locations;
    
    // The dictionary used to look up individual locations 
    private Dictionary<string, Location> locationsDict = new Dictionary<string, Location>();

    // A dictionary mapping computed locations to the class that handles them
    private Dictionary<string, ComputedLocation> computedLocations; 

    // A list of special locations that are either unreachable by the player avatar or treated differently to normal locations
    private readonly List<string> specialLocations = new List<string>()
    {
        "0Death", "OutOfPlay", "Player"
    };
    
    // === PROPERTIES ===

    // References to parts of the game engine used by locations
    public PlayerController PC { get { return playerController; } }
    public ItemController IC { get { return itemController; } }
    public TextDisplayController TDC { get { return textDisplayController; } }
    public PlayerMessageController PMC { get { return playerMessageController; } }

    // Returns true if the player avatar has moved beyond the start location
    public bool MovedBeyondStart
    {
        get
        {
            return LocViews.Count > 1;
        }
    }

    public int TimesToAbbreviateLocation { get ; private set; } // The number of times a short location description should be shown before the long description is shown again
    public int DetailMsgCount { get; private set; }             // Keeps track of how many times player has been warned about not giving more detail
    public Dictionary<string, int> LocViews { get; private set; } = new Dictionary<string, int>();  // Keeps track of number of times each location is viewed (used to determine whether long or short description is shown



    // === MONOBEHAVIOUR METHODS ===

    // On waking, generate the dictionary of locations
    private void Awake()
    {
        foreach(Location location in locations)
        {
            locationsDict.Add(location.LocationID, location);
        }

        // And set up the computed locations
        computedLocations = new Dictionary<string, ComputedLocation>()
        {
            {"301ComputedLocationPloverAlcove", new PloverAlcove(this)},
            {"302ComputedLocationPloverTransport", new PloverTransport(this)},
            {"303ComputedLocationTrollBridge", new TrollBridge(this)}
        };
    }

    // === PUBLIC METHODS ===

    // Returns true if the player avatar can retreat from the current location to a previous location
    public bool CanMoveBack(string locationID)
    {
        if (LocationExists(locationID, "CanMoveBack"))
        {
            return locationsDict[locationID].CanMoveBack;
        }

        return false;
    }

    // Returns a long or short description of the given location based on what's available and how many times it has been described
    public string DescribeLocation(string locationID)
    {
        string description = null;

        if (LocationExists(locationID, "DescribeLocation"))
        {
            // Check if this location has been visited before
            if (!LocViews.ContainsKey(locationID))
            {
                // This is the first visit, so start tracking number of views
                LocViews.Add(locationID, 0);
            }

            // Show long or short description based on number of views
            description = locationsDict[locationID].ShortDescription;

            if (LocViews[locationID] % TimesToAbbreviateLocation == 0)
            {
                description = locationsDict[locationID].LongDescription;
            }

            // Increment number of views for this location
            LocViews[locationID]++;
        }

        return description;
    }
    
    // Returns true if the given destination can be reached from the given location
    public bool DestinationCanBeReached(string locationID, string destinationID)
    {
        if (LocationExists(locationID, "DestinationCanBeReached"))
        {
            return locationsDict[locationID].DestinationCanBeReached(destinationID, this);
        }

        return false;
    }

    // Returns a list of unique destinations reachable from this location (does not include Computed or Special Locations)
    public List<string> Destinations(string locationID)
    {
        if (LocationExists(locationID, "Destinations"))
        {
            // Get a list of unique destinations
            List<string> destinations = locationsDict[locationID].Destinations;
            List<string> finalDestinations = new List<string>();

            // Remove any computed or special locations from the list
            foreach (string destination in destinations)
            {
                if (!computedLocations.ContainsKey(destination) && !specialLocations.Contains(destination))
                {
                    finalDestinations.Add(destination);
                }
            }

            return finalDestinations;
        }

        return null;
    }

    // Computes and returns the final destination for the given computed location
    public string GetComputedLocation(string locationID)
    {
        if (computedLocations.ContainsKey(locationID))
        {
            return computedLocations[locationID].GetLocation();
        }
        else
        {
            Debug.LogErrorFormat("LocationController.GetComputedLocation was passed an invalid travel option ID: \"{0}\"", locationID);
            return null;
        }
    }

    // Returns true if the given location is one where the final destination is computed
    public bool IsComputedLocation(string locationID)
    {
        return computedLocations.ContainsKey(locationID);
    }

    // Returns true if the given location is dark
    public bool IsDark(string locationID)
    {
        if (LocationExists(locationID, "IsDark"))
        {
            return locationsDict[locationID].IsDark;
        }

        return false;
    }

    // Returns true if the given location is outside the cave
    public bool IsOutside(string locationID)
    {
        if (LocationExists(locationID, "IsOutside"))
        {
            LocationType locType = locationsDict[locationID].LocType;
            return locType == LocationType.OUTSIDE || locType == LocationType.FOREST;
        }

        return false;
    }

    // Returns the liquid type at the given location
    public LiquidType LiquidAtLocation(string locationID)
    {
        if (LocationExists(locationID, "LiquidAtLocation"))
        {
            return locationsDict[locationID].LiquidAtLocation;
        }

        return LiquidType.NONE;
    }

    // Returns true if the given location exists (note there's a private version of this with a different signature)
    public bool LocationExists(string locationID)
    {
        return locationsDict.ContainsKey(locationID);
    }

    // Returns the location type of the given location
    public LocationType LocType(string locationID)
    {
        if (LocationExists(locationID, "LocType"))
        {
            return locationsDict[locationID].LocType;
        }

        return LocationType.NONE;
    }

    // Responds to a LOOK command, potentially showinga  message to the player about not showing more detial, then instructs the game controller to process a new turn showing the long description
    public CommandOutcome Look()
    {
        // If it has not yet been shown three times, show the no more detail warning and increase the count of times shown
        if (DetailMsgCount < 3)
        {
            textDisplayController.AddTextToLog(playerMessageController.GetMessage("15NoMoreDetail"));
            DetailMsgCount++;
        }

        LocViews[playerController.CurrentLocation] = 0;
        return CommandOutcome.DESCRIBE;
    }

    // Resets the trcked locations to a new game state
    public void ResetLocations()
    {
        DetailMsgCount = 0;
        TimesToAbbreviateLocation = 5;
        LocViews.Clear();
    }

    // Restores Location Controller from saved game data
    public void Restore(GameData gameData)
    {
        DetailMsgCount = gameData.detailMsgCount;
        LocViews = gameData.locViews;
        TimesToAbbreviateLocation = gameData.timesToAbbreviateLocation;
    }

    // Surpresses use of long descriptions except when player uses a LOOK command
    public void SetBriefMode()
    {
        TimesToAbbreviateLocation = 10000;
        DetailMsgCount = 3;
    }

    // Returns a structure detailing the sound that can be heard at the location and whether it drowns out other sounds
    public LocationSound SoundAtLocation(string locationID)
    {
        LocationSound locSound = new LocationSound(null, false);

        if (LocationExists(locationID, "SoundAtLocation") && locationsDict[locationID].CanBeHeard)
        {
            locSound.soundMessage = playerMessageController.GetMessage(locationsDict[locationID].Sound);
            locSound.drownsOutOtherSounds = locationsDict[locationID].DrownsOutOtherSounds;
        }

        return locSound;
    }

    // Returns true if travel is forced from the given location
    public bool TravelIsForced(string locationID)
    {
        if (LocationExists(locationID, "TravelIsForced"))
        {
            return locationsDict[locationID].TravelIsForced;
        }

        return false;
    }

    // Determines if the given command triggers movement from the player avatar's current location and returns detials of the movement
    public MoveOutcome TryMovement(MovementWord command) 
    {
        return locationsDict[playerController.CurrentLocation].TryMovement(command, this);
    }

    // === PRIVATE METHODS ===

    // Returns true if the given location exists. If not, returns false and generates an error message (NOTE: There's a public version of this with a different signature
    private bool LocationExists(string locationID, string methodName)
    {
        if (locationsDict.ContainsKey(locationID))
        {
            return true;
        }
        else
        {
            Debug.LogErrorFormat("LocationController.{0} was passed a non-existant location: \"{1}\"", methodName, locationID);
            return false;
        }
    }
}

// A structure holding a description of the sound at a location and whether that sound drowns out other sounds
public struct LocationSound
{
    public string soundMessage;
    public bool drownsOutOtherSounds;

    public LocationSound(string p1, bool p2)
    {
        soundMessage = p1;
        drownsOutOtherSounds = p2;
    }
}
