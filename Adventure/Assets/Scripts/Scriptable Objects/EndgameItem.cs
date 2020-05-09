// Endgame Item
// Holds the information about the placement and state of an endgame item

using UnityEngine;

[CreateAssetMenu(menuName = "Adventure/Endgame Item")]

public class EndgameItem : ScriptableObject
{
    public string itemID;   // The ID of the item to be placed
    public int itemState;   // The item state the item should have
    public bool neEnd;      // True if the item is to be placed at the NE End (note an item can be placed at either the NW end or the SW end or both)
    public bool swEnd;      // True if the item is to be placed at the SW End
}
