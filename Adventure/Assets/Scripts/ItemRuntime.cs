// Item Runtime
// Represents a single item at runtime - generated from an Item scriptableobject


public class ItemRuntime
{
    private string[] descriptions;              // An array with a description for each item state - this is what is shown when an item is at a location
    private string initialLocation = null;      // The initial location of the item (null if the item is not in game at start)
    private string initialLocation2 = null;     // The second location for the item (usually null but some fixed items exist in two locations)

    // === PROPERTIES ===

    public string ItemID { get; }
    public int ItemState { get; set; }
    public string Name { get; }
    public bool IsMovable { get; set; } = false;
    public bool IsTreasure { get; } = false;
    public int TreasurePoints { get; } = 0;
    public string CurrentLocation { get; private set; } = null;
    public string CurrentLocation2 { get; private set; } = null;
    public bool CanBeRead { get { return ReadOffset >= 0; } }
    public string Text { get { return ReadOffset >= 0 ? descriptions[ItemState + ReadOffset] : null; } }
    public bool CanBeHeard { get { return ListenOffset >= 0; } }
    public string Sound { get { return ListenOffset >= 0 ? descriptions[ItemState + ListenOffset] : null; } }
    public int ListenOffset { get; set; }
    public int ReadOffset { get; set; }

    // Returns true if the item is currently at its initial location
    public bool AtInitialLocation { get { return CurrentLocation == initialLocation && CurrentLocation2 == initialLocation2; } }

    // Returns appropriate description for the current item state
    public string Description { get { return ItemState >= 0 && descriptions[ItemState] != null ? descriptions[ItemState] : ""; } }

    // === CONSTRUCTOR ===
    public ItemRuntime(Item item)
    {
        ItemID = item.itemID;
        IsMovable = item.isMovable;
        IsTreasure = item.isTreasure;
        TreasurePoints = item.treasurePoints;
        Name = item.itemName;
        descriptions = item.descriptions;
        ListenOffset = item.listenOffset;
        ReadOffset = item.readOffset;
        initialLocation = item.initialLocation;
        initialLocation2 = item.initialLocation2;
        Reset();
    }

    // === PUBLIC METHODS ===

    // Moves this item to the given location (single location only)
    public void DropAt(string location)
    {
        CurrentLocation = location;
        CurrentLocation2 = null;
    }

    // Moves the item to the given locations (dual location)
    public void DropAt(string location, string location2)
    {
        CurrentLocation = location;
        CurrentLocation2 = location2;
    }

    // Returns true if the item is at the current location
    public bool IsAt(string location)
    {
        return CurrentLocation == location || CurrentLocation2 == location;
    }

    // Returns true if the given location is the item's specified initial location
    public bool IsInitialLocation(string locationID, LOCATION_POSITION locPos)
    {
        bool isLocation1 = initialLocation == locationID;
        bool isLocation2 = initialLocation2 == locationID;

        bool outcome = false;

        switch (locPos)
        {
            case LOCATION_POSITION.EITHER_LOCATION:
                outcome = isLocation1 || isLocation2;
                break;
            case LOCATION_POSITION.FIRST_LOCATION:
                outcome = isLocation1;
                break;
            case LOCATION_POSITION.SECOND_LOCATION:
                outcome = isLocation2;
                break;
        }

        return outcome;
    }

    // Resets the item to its initial state and location
    public void Reset()
    {
        ItemState = 0;
        CurrentLocation = initialLocation;
        CurrentLocation2 = initialLocation2;
    }
}

// Used to indicate which of the two locations is being used in an operation
public enum LOCATION_POSITION { EITHER_LOCATION, FIRST_LOCATION, SECOND_LOCATION }

