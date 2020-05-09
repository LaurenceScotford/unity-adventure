// Action Word
// Holds the data for an action command

using UnityEngine;

[CreateAssetMenu(menuName = "Adventure/Commands/Action")]
public class ActionWord : Command
{
    // === MEMBER VARIABLES ===
    [SerializeField] private string actionID;

    // === PROPERTIES ===
    public string ActionID { get { return actionID; } }
}
