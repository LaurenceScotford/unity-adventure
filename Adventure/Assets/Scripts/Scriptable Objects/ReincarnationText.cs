// Reincarnation Text
// Holds references to player messages to be used during reincarnation sequence

using UnityEngine;

[CreateAssetMenu(menuName = "Adventure/Reincarnation Text")]

public class ReincarnationText : ScriptableObject
{
    public string questionMessageID;    // The question to ask the player
    public string responseMessageID;    // The response the player receives if they answer yes
}
