﻿// Hint Controller
// Manages hints to the player

using System.Collections.Generic;
using UnityEngine;

public class HintController : MonoBehaviour
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    [SerializeField] GameController gameController;
    [SerializeField] PlayerController playerController;
    [SerializeField] ItemController itemController;
    [SerializeField] CommandsController commandsController;
    [SerializeField] DwarfController dwarfController;
    [SerializeField] QuestionController questionController;
    [SerializeField] PlayerMessageController playerMessageController;
    [SerializeField] TextDisplayController textDisplayController;

    [SerializeField] Hint[] hints;      // Array holding all the available hints

    private int currentHint;                                     // Keepos track of hint being checked
    private string locationToCheck;                              // Keeps track of location currently being checked

    // === PROPERTIES ===
    public Dictionary<string, HintActivation> HintActivations { get; private set; }   // Tracks the hint activation and count for each hint

    public int PointsDeductedForHints
    {
        get
        {
            int pointsDeducted = 0;

            foreach (Hint hint in hints)
            {
                if (HintActivations[hint.HintID].hintActivated)
                {
                    pointsDeducted += hint.PointsCost;
                }
            }

            return pointsDeducted;
        }
    }

    // === MONOBEHAVIOUR METHODS ===
    private void Start()
    {
        // Add questions used by this controller
        for (int i = 0; i < hints.Length; i++)
        {
            questionController.AddQuestion("hint" + i, new Question(hints[i].QuestionID, null, "54OK", false, HintQuestionYesResponse, null));
            questionController.AddQuestion("hintConfirm" + i, new Question("175WantHint", hints[i].HintMessageID, "54OK", false, HintWantedResponse, null));
        }
    }

    // === PUBLIC METHODS ===

    // Checks if the given location is eligible for any of the hints on this turn. If so asks the hint quesiton and returns true, otherwise returns false
    public bool CheckForHints(string locationID)
    {
        currentHint = -1;

        // Check against all the hints
        for (var i = 0; i < hints.Length; i++)
        {
            string hintID = hints[i].HintID;

            // If the hint has not already been activated...
            if (!HintActivations[hintID].hintActivated)
            {
                // Check if this location is eligible for a hint on this turn
                if (hints[i].Locations.Contains(locationID))
                {
                    HintActivations[hintID].hintCounter++;

                    if (HintActivations[hintID].hintCounter >= hints[i].TurnsTillActivation)
                    {
                        HintActivations[hintID].hintCounter = 0;

                        if (QuickChecks(hintID))
                        {
                            currentHint = i;
                            break;
                        }
                    }    
                }
                else
                {
                    HintActivations[hintID].hintCounter = 0;
                }
            }
        }

        // If a hint was identified...
        if (currentHint >= 0)
        {
            // ... ask the hint question
            questionController.RequestQuestionResponse("hint" + currentHint);
            return true;
        }

        return false;
    }

    // Callback if player answers positively to hint question
    public void HintQuestionYesResponse()
    {
        // Let the player know how much this hint will cost them and ask for confirmation
        int points = hints[currentHint].PointsCost;
        string pointsMsg = playerMessageController.GetMessage("261HintCost", new string[] { points.ToString(), points != 1 ? "s" : "" });
        textDisplayController.AddTextToLog(pointsMsg);

        questionController.RequestQuestionResponse("hintConfirm" + currentHint);
    }

    // Callback if player answers positively to the hint confirmation question
    public void HintWantedResponse()
    {
        HintActivations[hints[currentHint].HintID].hintActivated = true;

        // If the lamp is not already dimming, extend the lamp life based on the cost of the hint
        if (gameController.LampLife > 30)
        {
            gameController.ExtendLampLife(30 * hints[currentHint].PointsCost);
        }
    }

    // A series of checks the might be applied to a given hint before askign the hint question
    public bool QuickChecks(string hintID)
    {
        const string BIRD = "8Bird";
        string location = playerController.CurrentLocation;
        bool noItemsAtLocations = itemController.ItemsAt(location).Count == 0 && itemController.ItemsAt(playerController.OldLocations[0]).Count == 0 && itemController.ItemsAt(playerController.OldLocations[1]).Count == 0;

        // Whether the checks have been passed
        bool checkPassed;

        switch (hintID)
        {
            case "11TryingToGetIntoCave":
                checkPassed = itemController.GetItemState("3Grate") == 0 && !playerController.ItemIsPresent("1Keys");
                break;
            case "12TryingToCatchBird":
                checkPassed = itemController.ItemIsAt(BIRD, location) && playerController.HasItem("5BlackRod") && commandsController.OldItem == BIRD;
                break;
            case "13TryingToDealWithSnake":
                checkPassed = itemController.ItemIsAt("11Snake", location) && !playerController.ItemIsPresent(BIRD);
                break;
            case "14LostInMaze":
                checkPassed = noItemsAtLocations && playerController.NumberOfItemsCarried > 1;
                break;
            case "15PonderingDarkRoom":
                checkPassed = itemController.TreasureWasSeen("59Emerald") && !itemController.TreasureWasSeen("60Pyramid");
                break;
            case "17CliffWithUrn":
                checkPassed = dwarfController.ActivationLevel == 0;
                break;
            case "18LostInForest":
                checkPassed = noItemsAtLocations;
                break;
            case "19TryingToDealWithOgre":
                checkPassed = itemController.ItemIsAt("41Ogre", location) && dwarfController.CountDwarvesAt(location) == 0;
                break;
            case "20FoundAllTreasuresExceptJade":
                checkPassed = itemController.TreasuresRemaining == 1 && !itemController.TreasureWasSeen("66Necklace");
                break;
            default:
                checkPassed = true;
                break;
        }

        return checkPassed;
    }

    public void ResetHints()
    {
        HintActivations = new Dictionary<string, HintActivation>();

        foreach (Hint hint in hints)
        {
            HintActivations.Add(hint.HintID, new HintActivation());
        }
    }

    // Restore Hint Controller from saved game data
    public void Restore(GameData gameData)
    {
        HintActivations = gameData.hintActivations;
    }
}

// Used for keeping track of hint activations
[System.Serializable]
public class HintActivation
{
    public bool hintActivated;
    public int hintCounter;

    public HintActivation()
    {
        hintActivated = false;
        hintCounter = 0;
    }
}


