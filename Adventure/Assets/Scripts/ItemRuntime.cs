// Item Runtime
// Represents a single item at runtime - generated from an Item scriptableobject


public class ItemRuntime
{
    // === PROPERTIES ===

    public int ItemState { get; set; }
    public bool IsMovable { get; set; } = false;
    public string CurrentLocation { get; set; } = null;
    public string CurrentLocation2 { get; set; } = null;
    public bool CanBeRead { get { return ReadOffset >= 0; } }
    public bool CanBeHeard { get { return ListenOffset >= 0; } }
    public int ListenOffset { get; set; }
    public int ReadOffset { get; set; }

    // === CONSTRUCTOR ===

    public ItemRuntime(Item item)
    {
        IsMovable = item.isMovable;
        ListenOffset = item.listenOffset;
        ReadOffset = item.readOffset;
        CurrentLocation = item.initialLocation;
        CurrentLocation2 = item.initialLocation2;
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
}



