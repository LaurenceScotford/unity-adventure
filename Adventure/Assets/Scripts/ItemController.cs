// Item Controller
// Manages the items in the game

using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    // === MEMBER VARIABLES ===

    // References to other parts of the game engine
    [SerializeField] private GameController gameController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private LocationController locationController;
    [SerializeField] private PlayerMessageController playerMessageController;

    // An array of items - this is used to generate the items dictionary
    [SerializeField] private Item[] items;

    // The items to be set up in the end game
    [SerializeField] private EndgameItem[] endgameItems;

    private const string DEADDROP = "OutOfPlay";                // The unreachable location that items are sent to when destroyed 

    private List<string> treasuresSeen = new List<string>();    // Keeps track of treasures found by player
                                    
    // A dictionary used to lookup items
    private Dictionary<string, ItemRuntime> itemDict;

    // === PROPERTIES ===
    public int TreasuresRemaining
    {
        get
        {
            return MaxTreasures - treasuresSeen.Count;
        }
    }

    public int MaxTreasures { get; private set; }   // Holds the number of treasures in the game
    public int MaxTreasurePoints { get; private set; }  // Holds the maximum points attainable by leaving treasures in the building
    public List<string> TreasuresSeen { get { return treasuresSeen; } }

    // === MONOBEHAVIOUR METHODS ===

    // On waking count treasures and calculate maximum treasure points
    private void Awake()
    {
        MaxTreasures = 0;
        MaxTreasurePoints = 0;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].isTreasure)
            {
                MaxTreasures++;
                MaxTreasurePoints += items[i].treasurePoints;
            }
        }
    }

    // === PUBLIC METHODS ===

    // Retursn true if the given item is currently at its initial location
    public bool AtInitalLocation(string itemID)
    {

        return ItemExists(itemID, "AtInitialLocation") && itemDict[itemID].AtInitialLocation;
    }

    // Used to change the bird sound after drinking dragon blood
    public void ChangeBirdSound()
    {
        itemDict["8Bird"].ListenOffset = 6;
    }

    // If the knife is in play but not at player avatar's current location, then remove it from play
    public void CleanUpKnife()
    {
        const string KNIFE = "18Knife";
        string knifeLoc = itemDict[KNIFE].CurrentLocation;
        
        if (knifeLoc != DEADDROP && knifeLoc != playerController.CurrentLocation)
        {
            DropItem(KNIFE, DEADDROP, null);
        }
    }

    // Describe the given item
    public string DescribeItem(string itemID)
    {
        if (ItemExists(itemID, "DescribeItem"))
        {
            // There's a special case for steps
            bool isSteps = itemID == "7Steps";
            bool carryingGold = playerController.HasItem("50Gold");

            // Temporarily change steps description when coming down steps
            if (isSteps)
            {
                int describeStepsState = itemDict[itemID].IsInitialLocation(playerController.CurrentLocation, LOCATION_POSITION.FIRST_LOCATION) ? 0 : 1;
                itemDict[itemID].ItemState = describeStepsState;
            }

            TallyTreasure(itemID);
            return !(isSteps && carryingGold) ? itemDict[itemID].Description : null;
        }

        return null;
    }

    // Return a list of descriptions of items at a given location
    public string DescribeItemsAt(string location)
    {
        string resultString = GetItemDescriptions(location, false); // Describe fixed items first

        if (resultString != "")
        {
            resultString += "\n";
        }

        resultString += GetItemDescriptions(location, true);        // Then describe any movable items that are here
        return resultString;
    }

    // Destroy item
    public void DestroyItem(string itemToDestroy)
    {
        if (ItemExists(itemToDestroy, "DestroyItem"))
        {
            itemDict[itemToDestroy].DropAt(DEADDROP);
        }
    }

    // Drops item at a single location - the version with this signature is normally used for movable items that will exist at a single location
    public void DropItemAt(string itemID, string location)
    {
        if (ItemExists(itemID, "DropItemAt [single location]") && LocationExists(location, "DropItemAt [single location]"))
        {
            DropItem(itemID, location, null);
        }
    }

    // Drops the item at two locations - the version withy this signature is used to place immovable items that span two locations
    public void DropItemAt(string itemID, string location, string location2)
    {
        if (ItemExists(itemID, "DropItemAt [dual location]") && LocationExists(location, "DropItemAt [dual location]") && LocationExists(location2, "DropItemAt [dual location]"))
        {
            DropItem(itemID, location, location2);
        }
    }

    // Puts required objects in the correct location and state for the end game
    public void EndGameStatesandPositions()
    {
        // Place all the items needed for the end game
        foreach (EndgameItem endgameItem in endgameItems)
        {
            if (endgameItem.neEnd && endgameItem.swEnd)
            {
                DropItemAt(endgameItem.itemID, "115RepositoryNE", "116RepositorySW");
            }
            else
            {
                DropItemAt(endgameItem.itemID, endgameItem.neEnd ? "115RepositoryNE" : "116RepositorySW");
            }
            
            SetItemState(endgameItem.itemID, endgameItem.itemState);
        }

        // Now take care of a couple of special cases
        itemDict["15Oyster"].ReadOffset = 3;
        itemDict["49Sign"].ReadOffset++;
    }

    // Return ID of first item at location or null if none
    public string FirstItemAt(string location)
    {
        foreach (ItemRuntime item in itemDict.Values)
        {
            if (item.IsAt(location))
            {
                return item.ItemID;
            }
        }

        return null;
    }

    // Gets the current state for this given item
    public int GetItemState(string itemID)
    {
        if (ItemExists(itemID, "GetItemState"))
        {
            return itemDict[itemID].ItemState;
        }

        return -1;
    }

    // Returns number of treasure points for this item
    public int GetTreasurePoints(string itemID)
    {
        if (ItemExists(itemID, "GetTreasurePoints"))
        {
            return itemDict[itemID].IsTreasure ? itemDict[itemID].TreasurePoints : 0;
        }

        return 0;
    }

    // Returns true if the given location is the requested initial location of the given item
    public bool IsInitialLocation(string itemID, string locationID, LOCATION_POSITION locPos)
    {
        return ItemExists(itemID, "IsInitialLocation") && LocationExists(locationID, "IsInitialLocation") && itemDict[itemID].IsInitialLocation(locationID, locPos);
    }

    // Returns true if the given item is a treasure item
    public bool IsTreasure(string itemID)
    {
        return ItemExists(itemID, "IsTreasure") && itemDict[itemID].IsTreasure;
    }

    // Returns a list of item IDs at the given location
    public List<string> ItemsAt(string location)
    {
        List<string> itemsList = new List<string>();

        foreach (ItemRuntime item in itemDict.Values)
        {
            if (item.IsAt(location))
            {
                itemsList.Add(item.ItemID);
            }
        }

        return itemsList;
    }

    // Returns true if the given item creates a sound
    public bool ItemCanBeHeard(string itemID)
    {
        if (ItemExists(itemID, "ItemCanBeHeard"))
        {
            return itemDict[itemID].CanBeHeard;
        }

        return false;
    }

    // Returns true if the given item is currently movable
    public bool ItemCanBeMoved(string itemID)
    {
        return ItemExists(itemID, "ItemCanBeMoved") && itemDict[itemID].IsMovable;
    }

    // Returns true if the given item can be read
    public bool ItemCanBeRead(string itemID)
    {
        if (ItemExists(itemID, "ItemCanBeRead"))
        {
            return itemDict[itemID].CanBeRead;
        }

        return false;
    }

    // Returns true if the given item exists - note there is a private version of this with a different signature
    public bool ItemExists(string itemID)
    {
        return itemDict.ContainsKey(itemID);
    }

    // Returns true if the given item is currently in play
    public bool ItemInPlay(string itemToCheck)
    {
        return ItemExists(itemToCheck, "ItemInPlay") && !itemDict[itemToCheck].IsAt(DEADDROP);
    }

    // Returns true if the item with the given itemID is at the given location
    public bool ItemIsAt(string itemID, string location)
    {
        if (ItemExists(itemID, "ItemIsAt"))
        {
            return itemDict[itemID].IsAt(location);
        }

        return false;
    }

    // Returns a string describing what can be heard when listening to the given item
    public string ListenToItem(string itemID)
    {
        if (ItemExists(itemID, "ListenToItem"))
        {
            return itemDict[itemID].Sound;
        }

        return null;
    }

    // Returns a list of names of items at the location 
    public string ListItems(string locationID)
    {
        string resultString = "";

        foreach (ItemRuntime item in itemDict.Values)
        {
            string name = item.Name;

            if ((item.CurrentLocation == locationID || item.CurrentLocation2 == locationID) && name != null && name != "")
            {
                resultString += "\n" + name;
            }

        }

        return resultString;
    }

    // Make the given item immovable
    public void MakeItemImmovable(string itemID)
    {
        if (ItemExists(itemID, "MakeItemImmovable"))
        {
            itemDict[itemID].IsMovable = false;
        }
    }

    // Make the give item movable
    public void MakeItemMovable(string itemID)
    {
        if (ItemExists(itemID, "MakeItemMovable"))
        {
            itemDict[itemID].IsMovable = true;
        }
    }

    // Returns number of items at this location
    public int NumberOfItemsAt(string location)
    {
        int count = 0;

        foreach (ItemRuntime item in itemDict.Values)
        {
            if (item.IsAt(location))
            {
                count++;
            }
        }

        return count;
    }

    // Returns a string with the outcome of reading the given item
    public string ReadItem(string itemID)
    {
        if (ItemExists(itemID, "ReadItem"))
        {
            return playerMessageController.GetMessage(itemDict[itemID].Text);
        }

        return null;
    }

    // Puts the given item back at its initial location and state
    public void ResetItem(string itemID)
    {
        if (ItemExists(itemID, "ResetItem"))
        {
            itemDict[itemID].Reset();
        }
    }

    // Creates a fresh dictionary of runtime items and clears treasure seen
    public void ResetItems()
    {
        // Reset treasures seen
        treasuresSeen.Clear();

        itemDict = new Dictionary<string, ItemRuntime>();

        for (int i = 0; i < items.Length; i++)
        {
            itemDict.Add(items[i].itemID, new ItemRuntime(items[i]));
        }

        // Adjust initial item state for rug and chain
        itemDict["62Rug"].ItemState = 1;
        itemDict["64Chain"].ItemState = 1;
    }

    // Sets the state for the given item
    public void SetItemState(string itemID, int state)
    {
        if (ItemExists(itemID, "SetItemState"))
        {
            itemDict[itemID].ItemState = state;
        }
    }

    // If the given item is a trasure, tally it off
    public void TallyTreasure(string itemID)
    {
        // Only tally items if they are treasure, not already seen and the cave is not closed
        if (ItemExists(itemID, "TallyTreasure") && itemDict[itemID].IsTreasure && !treasuresSeen.Contains(itemID) && gameController.CurrentCaveStatus != CaveStatus.CLOSED)
        {
            treasuresSeen.Add(itemID);
        }
    }

    // Returns true if the given item is a treasure and has been seen by the player
    public bool TreasureWasSeen(string itemID)
    {
        return ItemExists(itemID, "TreasureWasSeen") && treasuresSeen.Contains(itemID);
    }

    // Returns the requested location of the given item
    public string Where(string itemID, LOCATION_POSITION position)
    {
        if (ItemExists(itemID, "Where"))
        {
            return position == LOCATION_POSITION.SECOND_LOCATION ? itemDict[itemID].CurrentLocation2 : itemDict[itemID].CurrentLocation;
        }

        return null;
    }

    // === PRIVATE METHODS ===

    // Places the item at the given location(s), location2 can be null
    private void DropItem(string itemID, string location, string location2)
    {
        if (location2 == null)
        {
            itemDict[itemID].DropAt(location);
            
        }
        else
        {
            itemDict[itemID].DropAt(location, location2);
        }
    }

    // Returns a string with a description of the items at the given location that match the required moveable attribute
    private string GetItemDescriptions(string location, bool moveableState)
    {
        string resultString = "";

        foreach (ItemRuntime item in itemDict.Values)
        {
            if (item.IsMovable == moveableState && item.IsAt(location))
            {
                string description = DescribeItem(item.ItemID);
                if (description != null && description != "")
                {
                    // If there was a previous item, add a carriage return first
                    if (resultString != "")
                    {
                        resultString += "\n";
                    }

                    resultString += description;
                }
            }
        }

        return resultString;
    }

    // Returns true if the given item exists. if not, returns false and generates an error message
    // Note: there is a public version of this with a different signature
    private bool ItemExists(string itemID, string methodName)
    {
        if (itemDict.ContainsKey(itemID))
        {
            return true;
        }
        else
        {
            Debug.LogErrorFormat("ItemController.{0} was passed a non-existant item: \"{1}\"", methodName, itemID);
            return false;
        }
    }

    // Returns true if the given location exists. If not, returns false and generates an error message
    private bool LocationExists(string locationID, string methodName)
    {
        if (locationController.LocationExists(locationID))
        {
            return true;
        }
        else
        {
            Debug.LogErrorFormat("ItemController.{0} was passed a non-existant location: \"{1}\"", methodName, locationID);
            return false;
        }
    }
}
