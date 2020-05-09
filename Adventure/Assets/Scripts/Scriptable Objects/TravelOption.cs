using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

[CreateAssetMenu(menuName = "Adventure/Travel Option")]
public class TravelOption : ScriptableObject
{
    private enum ConditionType { NONE, PROBABILITY, CARRYING, PRESENT, ITEMSTATE }

    // Note: Theoretically a travel option could have either a destination or messageID or none (although that wouldn't serve much purpose) or both
    // In the standard game, each option will have one or the other - it either takes you somewhere or shows you a message
    [SerializeField] private string travelOptionID;
    [SerializeField] private string destination = null;           // The location the player will go to if this travel option is successfully selected (can be null if this option does not change location)
    [SerializeField] private string messageID = null;               // A message that is shown to the player (can be null if no message is shown)
    [SerializeField] private MovementWord[] commands = null;        // A list of commands that could trigger this option
    [SerializeField] private string requiredItem = null;             // The item required for conditions CARRYING, PRESENT and ITEMSTATE
    [SerializeField] private int forbiddenItemState = 0;            // The itemstate that is forbidden for condition ITEMSTATE
    [SerializeField, Range(0, 100)] private int percentage = 100;   // The percentage chance of success for condition PROBABILITY
    [SerializeField] private ConditionType conditionType;           // The condition that applies: 
                                                                                // NONE: Unconditional
                                                                                // PROBABILITY: Happens with percentage chance of success
                                                                                // CARRYING: Player must be carrying requiredItem
                                                                                // PRESENT: requiredItem must be either at the current location or carried by player
                                                                                // ITEMSTATE: The item must not have a state of forbiddenItemState

    public string Destination { get { return destination; } }     // We need to query the options destination when calculating possible routes

    // Check to see if the given command is valid for this movement option and return null if it isn't or a move outcome with Location or MessageID if it is
    public MoveOutcome TryMovement(Command command, LocationController controller)
    {
        MoveOutcome outcome = FreshOutcome();

        // If commands are null or the given command is null, this is a forced movement entry, so just get the destination / message ID
        if (commands == null || command == null)
        {
            outcome.locationID = destination;
            outcome.messageID = messageID;
        }
        else
        {
            // Otherwise, search for a the first entry that matches the given command
            for (int i = 0; i < commands.Length; i++)
            {
                if (command == commands[i])
                {
                    outcome.locationID = destination;
                    outcome.messageID = messageID;
                    break;
                }
            }
        }

        // If a match was found, check for conditions
        if (outcome.locationID != null || outcome.messageID != null)
        {
            ItemController itemController = controller.IC;
            PlayerController playerController = controller.PC;

            bool conditionMet = false;
            switch (conditionType)
            {
                case ConditionType.NONE:
                    conditionMet = true;
                    break;
                case ConditionType.PROBABILITY:         // There's a % chance of the movement happening
                    if (Random.Range(0, 100) + 1 <= percentage)
                    {
                        conditionMet = true;
                    }
                    break;
                case ConditionType.PRESENT:             // The required item must be in location or carried
                    if (itemController.ItemsAt(playerController.CurrentLocation).Contains(requiredItem)) 
                    {
                        conditionMet = true;
                        break;
                    }
                    goto case ConditionType.CARRYING;   // Note: falls through to CARRYING check if condition not met
                case ConditionType.CARRYING:            // The required item must be carried
                    if (playerController.HasItem(requiredItem)) 
                    {
                        conditionMet = true;
                    }
                    break;
                case ConditionType.ITEMSTATE:
                    if (itemController.GetItemState(requiredItem) != forbiddenItemState)
                    {
                        conditionMet = true;
                    }
                    break;
                default:
                    Debug.LogErrorFormat("Unknown travel condition: {0}", conditionType);
                    break;
            }

            // If the condition was not met, clear the outcome but indicate a condition was failed
            if (!conditionMet)
            {
                outcome = FreshOutcome();
                outcome.failedCondition = true;
            }
        }
        
        return outcome;
    }

    private MoveOutcome FreshOutcome()
    {
        MoveOutcome outcome = new MoveOutcome();
        outcome.locationID = null;
        outcome.messageID = null;
        outcome.failedCondition = false;
        return outcome;
    }

    // If the given location matches the destination, returns true
    public bool LocationMatchesDestination(string locationID)
    {
        return (locationID == destination);
    }
}
public struct MoveOutcome
{
    public string locationID;
    public string messageID;
    public bool failedCondition;
}