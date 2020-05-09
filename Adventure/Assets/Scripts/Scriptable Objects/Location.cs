// Location
// Represents a single location

using System.Collections.Generic;
using UnityEngine;

public enum LocationType { OUTSIDE, FOREST, INSIDE, DEEP, NONE };   // The type of location
public enum LiquidType { NONE, WATER, OIL };                        // The liquid present at the location

[CreateAssetMenu(menuName = "Adventure/Location")]
public class Location : ScriptableObject
{
    // === MEMBER VARIABLES ===
    [SerializeField] private string locationID;                                             // Unique ID for this location
    [SerializeField, TextArea(minLines: 3, maxLines: 10)] private string longDescription;   // The long description (shown on first visit then periodically or when requested)
    [SerializeField] private string shortDescription = null;                                // The optional short description (if present, shown instead of long description for repeat visits)
    [SerializeField] private LocationType locationType;                                     // Whether this location is OUTSIDE, INSIDE the cave or DEEP inside the cave
    [SerializeField] private bool isDark = true;                                            // Whether this location is dark without artificial illumination
    [SerializeField] private LiquidType liquidType = LiquidType.NONE;                       // Whether the type of liquid present at this location is NONE, OIL or WATER
    [SerializeField] private TravelOption[] travelOptions;                                  // The travel options available from this location (applied in strict order)
    [SerializeField] private bool travelIsForced = false;                                   // Whether travel is forced from this location (i.e. on visiting this location, player is immediately moved to another)
    [SerializeField] private bool canMoveBack = true;                                       // Whether the player can use the BACK / RETURN / RETREAT command from this location to go back to the previous location
    [SerializeField] private string sound = null;                                           // The ID of the message that describes the sound at this location
    [SerializeField] bool drownsOutOtherSounds = false;                                     // Whether the sound at this location drowns out other sounds


    // === PROPERTIES ===
    public string LocationID { get { return locationID; } }
    public string LongDescription { get { return longDescription; } }
    public string ShortDescription { get { return shortDescription != null && shortDescription != "" ? shortDescription : longDescription; } }
    public LocationType LocType { get { return locationType; } }
    public bool IsDark { get { return isDark; } }
    public LiquidType LiquidAtLocation { get { return liquidType; } }
    public bool TravelIsForced { get { return travelIsForced; } }
    public bool CanMoveBack { get { return canMoveBack; } }
    public bool CanBeHeard { get { return sound != null && sound != ""; } }
    public string Sound { get { return sound; } }
    public bool DrownsOutOtherSounds { get { return drownsOutOtherSounds; } }

    // Returns a list of unique destinations reachable from this location
    public List<string> Destinations
    {
        get
        {
            List<string> destinations = new List<string>();

            for (int i = 0; i < travelOptions.Length; i++)
            {
                string destination = travelOptions[i].Destination; 
                
                if (destination != null && destination != "" && !destinations.Contains(destination))
                {
                    destinations.Add(destination);
                }
            }

            return destinations;
        }
    }

    // === PUBLIC METHODS == 

    // Checks to see if there is an unconditional command that will move the player to the given location from this one
    public bool DestinationCanBeReached(string destination, LocationController controller)
    {
        if (destination != null && destination != "")
        {
            for (int i = 0; i < travelOptions.Length; i++)
            {
                string optionDestination = travelOptions[i].Destination;
                bool validOptDest = optionDestination != null && optionDestination != "";
                if (validOptDest && (travelOptions[i].LocationMatchesDestination(destination) || (controller.TravelIsForced(optionDestination) && controller.DestinationCanBeReached(optionDestination, destination))))
                {
                    return true;
                }
            }
        }

        return false;
    }

    // Check to see if it is possible to move from this location using the given command and returns the outcome of the movement 
    public MoveOutcome TryMovement(Command command, LocationController controller)
    {
        MoveOutcome outcome = new MoveOutcome();
        outcome.locationID = null;
        outcome.messageID = null;

        // If travel is forced, set command to null, which will find the first option that is unconditional, or where the condition is met
        if (travelIsForced)
        {
            command = null;
        }

        bool failedCondition = false;

        // Find and return the first option matching the given command
        for (int i = 0; i < travelOptions.Length; i++)
        {
            outcome = travelOptions[i].TryMovement(command, controller);

            // Check if a computed location needs to be translated to final location
            if (!outcome.failedCondition && outcome.locationID != null && controller.IsComputedLocation(outcome.locationID))
            {
                outcome.locationID = controller.GetComputedLocation(outcome.locationID);

                // If the new location is null, it means we need to get the next location from the movement table
                if (outcome.locationID == null)
                {
                    outcome.failedCondition = true;
                }
            }

            if (outcome.failedCondition)
            {
                // The current option matched but failed a condition, so indicate we should match the next item with a condition that passes or which is unconditional
                command = null;
                failedCondition = true;
            } 
            else if (outcome.locationID != null || outcome.messageID != null)
            {
                failedCondition = false;
                break;
            }
        }

        // If we get here with failedCondition set then there's been an error as no unconditional alternative was found for one or more conditional entries
        if (failedCondition)
        {
            Debug.LogErrorFormat("Location \"{0}\" has at least one conditional travel option without an unconditional alternative", ShortDescription);
        }

        return outcome;
    }
}

