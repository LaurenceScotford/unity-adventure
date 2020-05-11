// GameData
// Serializable game data for saving and loading game state

using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public int activationLevel;
    public int bonusPoints;
    public bool closedHintShown;
    public int commandBeingProcessed;
    public CaveStatus currentCaveStatus;
    public GameStatus currentGameStatus;
    public int clock1;
    public int clock2;
    public CommandToProcess[] commandsToProcess;
    public string currentLocation;
    public int detailMsgCount;
    public Dwarf[] dwarves;
    public int dwarvesKilled;
    public int foobar;
    public Dictionary<string, HintActivation> hintActivations;
    public Dictionary<string, ItemRuntime> itemDict;
    public int lampLife;
    public bool lampWarning;
    public Dictionary<string, int> locViews;
    public string magicWordText;
    public int numDeaths;
    public string oldItem;
    public string[] oldLocations;
    public bool panic;
    public int savePenaltyPoints;
    public string subjectCarriedOver;
    public List <string> textLog;
    public int thresholdIndex;
    public int timesToAbbreviateLocation;
    public int turns;
    public List<string> treasuresSeen;
    public int turnsPointsLost;
    public string verbCarriedOver;
    public bool wasDark;
    public int westCount;

    public GameData(GameController controller)
    {
        // Add text display data
        textLog = controller.textDisplayController.textLog;

        // Add parser state
        ParserState parserState = controller.parserState;
        commandBeingProcessed = parserState.CommandBeingProcessed;
        commandsToProcess = parserState.commandsToProcess;
        subjectCarriedOver = parserState.SubjectCarriedOver;
        verbCarriedOver = parserState.VerbCarriedOver;

        // Add game controller data
        clock1 = controller.Clock1;
        clock2 = controller.Clock2;
        currentCaveStatus = controller.CurrentCaveStatus;
        currentGameStatus = controller.CurrentGameStatus;
        lampLife = controller.LampLife;
        lampWarning = controller.LampWarning;
        numDeaths = controller.NumDeaths;
        panic = controller.Panic;
        turns = controller.Turns;
        wasDark = controller.WasDark;

        // Add player data
        PlayerController playerController = controller.playerController;
        currentLocation = playerController.CurrentLocation;
        oldLocations = playerController.OldLocations;

        // Add commands data
        CommandsController commandsController = controller.commandsController;
        magicWordText = commandsController.MagicWordText;
        oldItem = commandsController.OldItem;
        westCount = commandsController.WestCount;

        // Add location data
        LocationController locationController = controller.locationController;
        detailMsgCount = locationController.DetailMsgCount;
        locViews = locationController.LocViews;
        timesToAbbreviateLocation = locationController.TimesToAbbreviateLocation;

        // Add items data
        ItemController itemController = controller.itemController;
        itemDict = itemController.ItemDict;
        treasuresSeen = itemController.TreasuresSeen;

        // Add actions data
        ActionController actionController = controller.actionController;
        foobar = actionController.Foobar;

        // Add dwarf data
        DwarfController dwarfController = controller.dwarfController;
        activationLevel = dwarfController.ActivationLevel;
        dwarves = dwarfController.Dwarves;
        dwarvesKilled = dwarfController.DwarvesKilled;

        // Add score data
        ScoreController scoreController = controller.scoreController;
        bonusPoints = scoreController.BonusPoints;
        closedHintShown = scoreController.ClosedHintShown;
        savePenaltyPoints = scoreController.SavePenaltyPoints;
        thresholdIndex = scoreController.ThresholdIndex;
        turnsPointsLost = scoreController.TurnsPointsLost;

        // Add hints data
        hintActivations = controller.hintController.hintActivations;
    }
}
