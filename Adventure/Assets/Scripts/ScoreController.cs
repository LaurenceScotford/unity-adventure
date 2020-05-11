// Score Controller
// Manages and displays the player's score

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreController : MonoBehaviour
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    [SerializeField] private GameController gameController;
    [SerializeField] private ItemController itemController;
    [SerializeField] private DwarfController dwarfController;
    [SerializeField] private HintController hintController;
    [SerializeField] private TextDisplayController textDisplayController;
    [SerializeField] private PlayerMessageController playerMessageController;

    private int score;                                            // Used to calculate total score
    private int maxScore;                                         // Used to calculate maximum score
    private ScoreMode mode;                                       // The scoring mode
    
    // Set up turn thresholds at which points are lost and a message shown
    private readonly TurnThreshold[] turnThresholds = new TurnThreshold[]
        {
            new TurnThreshold(350, 2, "Thresh350"),
            new TurnThreshold(500, 3, "Thresh500"),
            new TurnThreshold(1000, 5, "Thresh1000"),
            new TurnThreshold(2500, 10, "Thresh2500")
        };

    // Set up rank thresholds for final score
    private readonly RankThreshold[] rankThresholds = new RankThreshold[]
    {
        new RankThreshold(9, "Rank0"),
        new RankThreshold(44, "Rank10"),
        new RankThreshold(119, "Rank45"),
        new RankThreshold(169, "Rank120"),
        new RankThreshold(249, "Rank170"),
        new RankThreshold(319, "Rank250"),
        new RankThreshold(374, "Rank320"),
        new RankThreshold(409, "Rank375"),
        new RankThreshold(425, "Rank410"),
        new RankThreshold(428, "Rank426"),
        new RankThreshold(9998, "Rank429"),
    };

    // === PROPERTIES ===

    
    public bool ClosedHintShown { get; set; }   // Keeps track of whether the closed hint on the oyster was shown
    public bool IsNovice { get; set; }
    public int BonusPoints { get; set; }        // Keeps track of bonus points added
    public int SavePenaltyPoints { get; set; }  // keeps track of points spent on saving game
    public int ThresholdIndex { get; set; }     // Index of next turn threshold
    public int TurnsPointsLost { get; set; }    // Keeps track of points lost for using too many turns

    // === PUBLIC METHODS ===

    // Adds bonus points to the existing total
    public void AddBonusPoints(int points)
    {
        BonusPoints += points;
    }

    // Adds save penalty points for saving the game
    public void AddSavePenalty()
    {
        SavePenaltyPoints += 5;
    }

    // Checks if player has passed a number of turns threshold, and shows message and deducts points if so
    public void CheckTurnThresholds(int turns)
    {
        if (ThresholdIndex < turnThresholds.Length && turns == turnThresholds[ThresholdIndex].threshold)
        {
            TurnsPointsLost += turnThresholds[ThresholdIndex].pointsLost;

            textDisplayController.AddTextToLog(playerMessageController.GetMessage(turnThresholds[ThresholdIndex].messageID));
            ThresholdIndex++;
        }
    }

    // Calculates and displays the player's current score. isFFinal should be true if this is a final score
    public void DisplayScore(ScoreMode scoreMode)
    {
        mode = scoreMode; 

        // Initialise counts
        score = 0;
        maxScore = 0;

        // Now add the scores for treasures and progress
        TreasureScore();
        ProgressScore();
        
        // Deduct any penalties for saving, using hints or taking too many turns
        DeductPenalties();

        string scoreMsg;
        string[] ScoreMsgParams = new string[] { score.ToString(), maxScore.ToString(), gameController.Turns.ToString(), gameController.Turns != 1 ? "s" : "" };

        // Now report the score in a format based on the mode
        if (mode == ScoreMode.SCORING)
        {
            // For interim score, just show the current score
            textDisplayController.AddTextToLog(playerMessageController.GetMessage("259Score", ScoreMsgParams));
        }
        else
        {
            // Show message if player missed maxing out their score by taking too long
            if (score + TurnsPointsLost + 1 >= maxScore && TurnsPointsLost != 0)
            {
                textDisplayController.AddTextToLog(playerMessageController.GetMessage("242SoLong"));
            }

            // Show message if player missed maxing out their score by saving the game
            if (score + SavePenaltyPoints + 1 >= maxScore && SavePenaltyPoints != 0)
            {
                textDisplayController.AddTextToLog(playerMessageController.GetMessage("143NoSuspense"));
            }

            // Show the final score
            textDisplayController.AddTextToLog(playerMessageController.GetMessage("262Score", ScoreMsgParams));

            // Calculate and display the player's rank
            bool rankFound = false;

            for (int i = 0; i < rankThresholds.Length; i++)
            {
                if (score <= rankThresholds[i].threshold)
                {
                    rankFound = true;
                    textDisplayController.AddTextToLog(playerMessageController.GetMessage(rankThresholds[i].messageID));

                    // Show a message about the next rank based on whether the best rank has been achieved or not
                    if (i < rankThresholds.Length - 1)
                    {
                        int pointsNeeded = rankThresholds[i].threshold + 1 - score;
                        textDisplayController.AddTextToLog(playerMessageController.GetMessage("263NextRating", new string[] {pointsNeeded.ToString(), pointsNeeded > 1 ? "s" : "" }));
                    }
                    else
                    {
                        textDisplayController.AddTextToLog(playerMessageController.GetMessage("264NeatTrick"));
                    }

                    break;
                }
            }

            // If, for some reason, score is higher then top threshold, show a message to that effect
            if (!rankFound)
            {
                textDisplayController.AddTextToLog(playerMessageController.GetMessage("264NeatTrick"));
            }
        }
    }

    // Reset scores ready for a new game
    public void ResetScores()
    {
        ThresholdIndex = 0;
        TurnsPointsLost = 0;
        BonusPoints = 0;
        SavePenaltyPoints = 0;
        ClosedHintShown = false;
        IsNovice = false;
    }

    // === PRIVATE METHODS ===

    // Deduct any accrued penalties from the score
    private void DeductPenalties()
    {
        // Deduct points for hints used and help given
        score -= hintController.PointsDeductedForHints;

        if (ClosedHintShown)
        {
            score -= 10;
        }

        if (IsNovice)
        {
            score -= 5;
        }

        // Deduct points for taking too long
        score -= TurnsPointsLost;

        // Deduct points for saving the game
        score -= SavePenaltyPoints;
    }

    // Calculate score for progress
    private void ProgressScore()
    {
        // Add score for number of deaths
        maxScore += gameController.MaxDeaths * 10;
        score += (gameController.MaxDeaths - gameController.NumDeaths) * 10;

        // Add points for reaching end of game, i.e. not quitting
        maxScore += 4;
        if (mode == ScoreMode.ENDING)
        {
            score += 4;
        }

        // Add points for getting deep in cave
        maxScore += 25;
        if (dwarfController.ActivationLevel != 0)
        {
            score += 25;
        }

        CaveStatus caveStatus = gameController.CurrentCaveStatus;

        // Add points for reaching closing
        maxScore += 25;
        if (caveStatus == CaveStatus.CLOSING || caveStatus == CaveStatus.CLOSED)
        {
            score += 25;
        }

        maxScore += 45;
        // Add bonus points if reached closed
        if (caveStatus == CaveStatus.CLOSED)
        {
            score += BonusPoints;
        }

        // Add a point for leaving the magazine at Witt's End
        maxScore++;
        if (itemController.ItemIsAt("16SpelunkerToday", "108WittsEnd"))
        {
            score++;
        }

        // Round off the score
        maxScore += 2;
        score += 2;
    }

    // Calculate score for treasures (just 2 points if treasure has only been seen, but more if treasure has been left at building)
    private void TreasureScore()
    {
        maxScore += itemController.MaxTreasurePoints;

        // Firts add points for treasures left in building
        List<string> itemsInBuilding = itemController.ItemsAt("3Building");

        foreach (string item in itemsInBuilding)
        {
            if (itemController.IsTreasure(item))
            {
                score += itemController.GetTreasurePoints(item);
            }
        }

        // Now add points for treasures seen but not collected
        List<string> treasuresSeen = itemController.TreasuresSeen;

        foreach (string item in treasuresSeen)
        {
            // Only add the seen points if the item hasn'tr already scored for being in the building
            if (!itemsInBuilding.Contains(item))
            {
                score += 2;
            }
        }
    }
 }

// Used to hold details of each turns threshold at which the player incurs penalty points
public struct TurnThreshold
{
    public int threshold;
    public int pointsLost;
    public string messageID;

    public TurnThreshold(int p1, int p2, string p3)
    {
        threshold = p1;
        pointsLost = p2;
        messageID = p3;
    }
}

// Used to hold details of each rank threshold for final score
public struct RankThreshold
{
    public int threshold;
    public string messageID;

    public RankThreshold(int p1, string p2)
    {
        threshold = p1;
        messageID = p2;
    }
}

// The scoring mode SCORING = interim score, QUITTING = score after quitting the game, ENDING = score after game ended
public enum ScoreMode { SCORING, QUITTING, ENDING }
