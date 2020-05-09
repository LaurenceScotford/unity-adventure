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

    private int hintCounter;                            // Keeps track of the number of turns at a qualifying location

    // === PROPERTIES ===

    public bool HintActivated { get; set; }                        // Whether the hint has been activated or not
    public string HintID { get { return hintID; } }
    public string QuestionID { get { return questionID; } }
    public string HintMessageID { get { return hintMessageID; } }
    public int PointsCost { get { return pointsCost; } }

    // === PUBLIC METHODS ===

    // Checks if the given location is eligible for this hint. If the hint can be activated, returns the question ID, otherwise returns null
    public string CheckLocation(string locationID)
    {
        hintCounter = locations.Contains(locationID) ? hintCounter + 1 : 0;

        return hintCounter >= turnsTillActivation ? questionID : null;
    }

    // Resets the hint counter
    public void ResetCounter()
    {
        hintCounter = 0;
    }

    // Resets a hint to its intial state
    public void ResetHint()
    {
        HintActivated = false;
        ResetCounter();
    }
}
