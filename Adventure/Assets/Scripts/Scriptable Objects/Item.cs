// Item 
// Class representing a single item (used to create the ItemRuntime class when the game starts)

using UnityEngine;

[CreateAssetMenu(menuName = "Adventure/Item")]
public class Item : ScriptableObject
{
    // === MEMBER VARIABLES ===
    public string itemID;                                               // Unique ID for item
    public bool isMovable = false;                                      // Whether item can be carried (true) or is fixed in place (false)
    public bool isTreasure = false;                                     // Whether item qualifies as a treasure (true)
    public int treasurePoints = 0;                                      // Number of points if this item is left in building (only applies if item is a treasure)
    public string itemName;                                             // The name for the item (this is what appears in the Inventory list if the item is movable 
    [TextArea(minLines: 1, maxLines: 10)] public string[] descriptions; // An array with a description for each item state - this is what is shown when an item is at a location
    public int listenOffset = -1;                                       // The item state if the player listens to the item (-1 = no sound)
    public int readOffset = -1;                                         // The item state if the player reads the item (-1 = no text)
    public string initialLocation = null;                               // The initial location of the item (null if the item is not in game at start)
    public string initialLocation2 = null;                              // The second location for the item (usually null but some fixed items exist in two locations)
}


