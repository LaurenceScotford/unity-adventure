// Hint
// Represents a single hint

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Adventure/Hint")]
public class Hint : ScriptableObject
{
    // === MEMBER VARIABLES ===

    [SerializeField] private string hintID;             // A unique ID for this hint
    [SerializeField] private int turnsTillActivation;   // Number of turns spent at any of the locations before the hint activates
    [SerializeField] private int pointsCost;            // Number of points lost for taking the hint 
    [SerializeField] private string questionID;         // The message ID used to ask the player about the hint
    [SerializeField] private string hintMessageID;      // The message ID with the hint for the player
    [SerializeField] private List<string> locations;    // A list of location IDs where this hint applied

    // === PROPERTIES ===  
    public string HintID { get => hintID; }
    public string QuestionID { get => questionID; }
    public string HintMessageID { get => hintMessageID; }
    public int PointsCost { get => pointsCost; }
    public List<string> Locations { get => locations; }
    public int TurnsTillActivation { get => turnsTillActivation; }
}
