// Game Controller
// The master controller that is the entry point for a new game and controls the game sequence

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    // === MEMBER VARIABLES ===

    // When debug mode is on, a panel is available that enables navigation directly to locations and to move object and set their state
    public bool debugMode = false;  
    [SerializeField] private GameObject debugPanel;
    [SerializeField] private DebugPanel debugPanelScript;

    // References to other parts of the game engine
    [SerializeField] private PlayerInput playerInput;             
    [SerializeField] private PlayerMessageController playerMessageController;      
    public CommandsController commandsController;
    public ActionController actionController;
    public TextDisplayController textDisplayController;          
    public PlayerController playerController;                    
    public ItemController itemController;
    public LocationController locationController;
    public ParserState parserState;
    public HintController hintController;
    [SerializeField] private QuestionController questionController;
    public ScoreController scoreController;
    public DwarfController dwarfController;
    [SerializeField] private PersistenceController persistenceController;

    // Hold questions and messages to be used during reincarnation sequence
    [SerializeField] private ReincarnationText[] reincarnationTexts;                                                                                     

    // Constant values
   
    private const int LAMP_LIFE_NOVICE = 1000;          // Lamp life given to novice players
    private const int LAMP_LIFE_EXPERIENCED = 330;      // Lamp life given to experienced players
    private const int PANIC_CLOCK = 15;                 // Turns till cave closure once player has found out they are locked in
    private const int CLOCK_1_INITIAL = 30;
    private const int CLOCK_2_INITIAL = 50;
    private const string LAMP = "2Lantern";

    // === PROPERTIES ===

    public bool WasDark { get; set; }                                   // Keeps track of whether the player is moving from a dark location
    public CaveStatus CurrentCaveStatus { get; set; }                   // Whether the cave is currently open, closing or closed
    public int LampLife { get; set; }                                   // Keeps track of remaining lamp life                
    public int NumDeaths { get; set; }                                  // Keeps track of number of times player avatar has died
    public int MaxDeaths { get { return reincarnationTexts.Length; } }  // Maximum times player avatar can die
    public int Turns { get ; set; }                                     // Number of turns taken
    public int Clock1 { get; set; }                                     // Countdown number of turns to closing after finding last treasure
    public int Clock2 { get; set; }                                     // Countdowns number of turns from first warning to blnding flash
    public bool LampWarning { get; set; }                               // Whether the player has been warned about the lamp dimmiong
    public bool Panic { get; set; }                                     // Set to true when player finds out they are locked in
    public GameStatus CurrentGameStatus { get; set; }                   // Whether the game is currently playing or is over

    // ========= MONOBEHAVIOUR METHODS =========

    // Sets up the debug panel, if needed, initialises a new game and offers the player the choice of seeing the instructions or not
    private void Start()
    {
        // Start the game in the selected mode
        switch (PlayerPrefs.GetString("CurrentMode"))
        {
            case "new":
                // Set up parameters for a new game
                NewGame();

                // Clear text view and show welcome message and instructions question
                textDisplayController.ResetTextDisplay();
                questionController.RequestQuestionResponse("65Welcome", "1Instructions", null, InstructionsResponseYes, InstructionsResponseNo);
                break;
            case "continue":
                break;
            case "load":
                break;
        }
    }

    // ========= PUBLIC METHODS =========

    // Brings the game to a close isQuitting is set to true if player has quit and false if game has come to a natural end
    public void EndGame(bool isQuitting)
    {
        SuspendCommandProcessing();

        if (!isQuitting)
        {
            scoreController.ReachedEnd = true;
        }

        scoreController.DisplayScore(ScoreMode.FINAL);
        CurrentGameStatus = GameStatus.OVER;
        textDisplayController.AddTextToLog(playerMessageController.GetMessage("201GameOver"));
        ResumeCommandProcessing();
    }

    // Extends the lamp life by the given amount
    public void ExtendLampLife (int extension)
    {
        LampLife += extension;
    }

    // Get a new command from the player
    public void GetPlayerCommand()
    {
        CommandOutcome outcome = CommandOutcome.FULL;
        bool hintOrDeathQuestion = false;

        SuspendCommandProcessing(); // Suspend further command processing until command has been executed

        // Remind the player of two word limit, if they have entered more than a two word command
        if (playerInput.NumberOfWords > 2)
        {
            textDisplayController.AddTextToLog(playerMessageController.GetMessage("53LongCommand"));
        }

        // Count a new turn and deal with any fallout from that
        AddTurn();

        CaveStatus oldCaveStatus = CurrentCaveStatus;

        // Update the clocks and check for closing
        UpdateClocks();

        // Only do lamp stuff if the cave status hasn't just changed
        if (oldCaveStatus == CurrentCaveStatus)
        {
            // Update the lamp and check for dimming/out of power
            UpdateLamp();
        }

        // Only process the command if the cave hasn't just closed
        if (!(oldCaveStatus != CurrentCaveStatus && CurrentCaveStatus == CaveStatus.CLOSED))
        {
            // Process the players's command
            parserState.ResetParserState(playerInput.Words);
            outcome = commandsController.ProcessCommand();
        }

        // If the outcome allows, process a turn following the player command
        if (outcome == CommandOutcome.FULL || outcome == CommandOutcome.DESCRIBE || outcome == CommandOutcome.MESSAGE)
        {
            hintOrDeathQuestion = ProcessTurn(outcome);
        }
        else if (outcome == CommandOutcome.DISTURBED)
        {
            // Player has disturbed the dwarves, so show message before ending game
            textDisplayController.AddTextToLog(playerMessageController.GetMessage("136DwarvesAttack"));
        }

        // If the command has resulted int eh end of game, then end game now
        if (outcome == CommandOutcome.DISTURBED || outcome == CommandOutcome.ENDED)
        {
            EndGame(false);
        }
        // Otherwise, wait for the next player command, unless we're waiting for a question response
        if (outcome != CommandOutcome.QUESTION && !hintOrDeathQuestion)
        {
            ResumeCommandProcessing();
        }
    }

     // Callback when learner declines instructions at start of game
    public void InstructionsResponseNo()
    {
        ActivateDebugPanel();
        LampLife = LAMP_LIFE_EXPERIENCED;                     // Expert players get a shorter lamp life
        ProcessTurn(CommandOutcome.FULL);
    }

    // Callback when learner requests instructions at start of game
    public void InstructionsResponseYes()
    {
        ActivateDebugPanel();
        LampLife = LAMP_LIFE_NOVICE;                    // Novice players get a longer lamp life
        scoreController.IsNovice = true;
        ProcessTurn(CommandOutcome.FULL);
    }

    // Determines if the player can see anything, based on whether the location is dark and whether lamp is present and switched on
    public bool IsDark()
    {
        bool locDark = locationController.IsDark(playerController.CurrentLocation);

        bool lampOff = itemController.GetItemState(LAMP) == 0;
        bool lampHere = playerController.ItemIsPresent(LAMP);

        return locDark && (lampOff || !lampHere);
    }

    // End game after player declines ressurection
    public void NoReincarnation()
    {
        EndGame(false);
    }

    // Monitors for a RESUME command and shows a message for any other command (this is the standard processer used acfter the game is over)
    public void PostGameCommand()
    {
        SuspendCommandProcessing();
        parserState.ResetParserState(playerInput.Words);

        if (parserState.NextCommandForProcessing())
        {
            List<string> currentCommands = parserState.CurrentCommands;

            if (currentCommands.Contains("2031Resume"))
            {
                actionController.ExecuteAction("resume");
            }
            else if (currentCommands.Contains("2024Score"))
            {
                scoreController.DisplayScore(ScoreMode.FINAL);
                ResumeCommandProcessing();
            }
            else
            {
                textDisplayController.AddTextToLog(playerMessageController.GetMessage("201GameOver"));
                ResumeCommandProcessing();
            }
        }
    }

    // Begin a new turn
    public bool ProcessTurn(CommandOutcome turnType)
    {
        bool hintOrDeathQuestion = false;
        bool forced = true;

        // This loop is repeated while the player is alive until the player reaches a location where movement isn't forced
        while (playerController.IsAlive && forced)
        {
            forced = false; // Ensure we break out of the loop, unless the current location is forced 

            if (turnType == CommandOutcome.FULL)
            {
                // If the player has left the cave during closing ...
                if (playerController.IsOutside && CurrentCaveStatus == CaveStatus.CLOSING)
                {
                    // ... force them back inside and give them warning about cave closing
                    playerController.RevokeMovement();
                    textDisplayController.AddTextToLog(playerMessageController.GetMessage("130ClosedAnnounce"));

                    // Start the panic clock, if it hasn't already started
                    StartPanic();
                }

                // CHeck if player's path is blocked by a dwarf before moving player to new location
                if (dwarfController.Blocked())
                {
                    // Passage was blocked, so tell player
                    textDisplayController.AddTextToLog(playerMessageController.GetMessage("2DwarfBlock"));
                    playerController.RevokeMovement();
                }
                else
                {
                    playerController.CommitMovement();
                }

                // Now process the dwarves
                if (dwarfController.DoDwarfActions())
                {
                    // The dwarves killed the player avatar
                    playerController.KillPlayer();
                }
            }

            // If player is still alive after any dwarf encounters...
            if (playerController.IsAlive)
            {
                // If the location is to be shown or reshown, show it now
                if (turnType != CommandOutcome.MESSAGE)
                {
                    // Now show the current location 
                    DisplayLocation();
                }

                // Show useful info in the debug panel (only visible if debug mode is on)
                debugPanelScript.UpdateDebugPanel();

                // Check to see if movement from this location is forced and move if so
                forced = playerController.CheckForcedMove();

                // Forced locations can't have items left there and don't have hints, so if movement was forced, we can skip this section
                if (!forced)
                {
                    // Don't show item descriptions again if the command outcome was just a message 
                    if (turnType != CommandOutcome.MESSAGE)
                    {
                        // If at Y2 before closing, there's a 25% chance the player hears someone say "PLUGH"
                        if (playerController.CurrentLocation == "33Y2" && CurrentCaveStatus != CaveStatus.CLOSING && Random.value < .25)
                        {
                            textDisplayController.AddTextToLog(playerMessageController.GetMessage("7Plugh"));
                        }

                        DisplayItems();
                    }

                    // If cave is closed, adjust state of any carried items so they will be described again after they are put down
                    if (CurrentCaveStatus == CaveStatus.CLOSED)
                    {
                        AdjustCarriedItemsAfterClosing();
                    }

                    // Get rid of the knife if it exists
                    itemController.CleanUpKnife();
                }
                else
                {
                    // Ensure we process a full turn for any forced movement
                    turnType = CommandOutcome.FULL;
                }
            }
        }
   
        // If the player has died
        if (!playerController.IsAlive)
        {
            PlayerDeath();
            hintOrDeathQuestion = true;
        }
        else
        {
            // Show any hints for the current location
            hintOrDeathQuestion = hintController.CheckForHints(playerController.CurrentLocation);
        }

        // Check to see if a continuation save is due
        persistenceController.CheckContinuationSave();

        return hintOrDeathQuestion;
    }

    // If possible reincarnate player on request
    public void ReincarnatePlayer()
    {
        // If at the final text, no reincarnation possible
        if (NumDeaths == MaxDeaths)
        {
            EndGame(false);
        }
        else
        {
            // Make sure the bottle is empty
            itemController.DestroyItem("21Water");
            itemController.DestroyItem("22Oil");

            // Drop all carried items at the last safe location before the player died (except lamp)
            playerController.DropAllAtDeathLocation();

            // Move player to start location and redescribe location and items
            playerController.GoToStart();
            ProcessTurn(CommandOutcome.DESCRIBE);
        }
    }

    // Returns to menu (saves the Continue state first)
    public void ReturnToMenu()
    {
        persistenceController.SaveGame(false);
        SceneManager.LoadScene("Menu");
    }

    // Resume interpreting input from the player as commands
    public void ResumeCommandProcessing()
    {
        if (CurrentGameStatus == GameStatus.PLAYING)
        {
            PlayerInput.commandsEntered = GetPlayerCommand;
        }
        else
        {
            PlayerInput.commandsEntered =  PostGameCommand;
        }
    }

    // Start the closure of the cave
    public void StartClosing()
    {
        // Lock the cave
        itemController.SetItemState("3Grate", 0);

        // Remove the bridge across the fissure
        itemController.SetItemState("12Fissure", 0);

        // Kill off the active dwarves
        dwarfController.KillAllDwarves();

        // Retire the troll
        itemController.DestroyItem("33Troll");
        itemController.DropItemAt("34PhonyTroll", "117ChasmSW", "122ChasmNE");

        // If the bear isn't dead, remove it from play
        if (itemController.GetItemState("35Bear") != 3)
        {
            itemController.DestroyItem("35Bear");
        }

        // Ensure chain and axe are retrievable now that the bear is no longer around
        itemController.SetItemState("64Chain", 0);
        itemController.MakeItemMovable("64Chain");
        itemController.MakeItemMovable("28Axe");

        // indicate we're closing
        Clock1 = -1;
        CurrentCaveStatus = CaveStatus.CLOSING;

        // Finally, let the player know what's happening
        textDisplayController.AddTextToLog(playerMessageController.GetMessage("129ClosingAnnounce"));
    }

    // Set up the final puzzle and transport the player there
    public void StartEndGame()
    {
        // Put items required for the end game in the required positions and states
        itemController.EndGameStatesandPositions();

        // Destroy any items still being carried
        playerController.EmptyBackpack();

        // Put player at NE Repository
        playerController.GoTo("115RepositoryNE", true);

        // Close the cave
        CurrentCaveStatus = CaveStatus.CLOSED;

        // Let player know about the blinding flash and display their new location
        textDisplayController.AddTextToLog(playerMessageController.GetMessage("132ClosedTransport"));
        DisplayLocation();
        DisplayItems();
    }

    // Called when the player finds out they are locked in (i.e. after closing starts) to set panic mode
    public void StartPanic()
    {
        if (!Panic)
        {
            Clock2 = PANIC_CLOCK;
            Panic = true;
        }
    }

    // Temprorily prevent input from the player being interpreted as commands
    public void SuspendCommandProcessing()
    {
        PlayerInput.commandsEntered = null;
    }

    // ========= PRIVATE METHODS =========
   
    // If debug mode is on, activate the debug panel
    private void ActivateDebugPanel()
    {
        if (debugMode)
        {
            debugPanel.SetActive(true);
        }
    }
        
    // Adds 1 to number of turns taken and, if past next threshold, deducts points and shows a message 
    private void AddTurn()
    {
        // Add one to number of turns taken
        Turns++;

        // If the player has reached a turns threshold, show a message and deduct points
        scoreController.CheckTurnThresholds(Turns);
    }

    // Adjusts the state of any carried items after closign so they are properly described again (and shows oyster message if oyster picked up)
    private void AdjustCarriedItemsAfterClosing()
    {
        const string OYSTER = "15Oyster";

        List<string> carriedItems = playerController.CarriedItems;

        foreach (string item in carriedItems)
        {
            int itemState = itemController.GetItemState(item);

            if (itemState < 0)
            {
                // Special case for oyster - we need to let player know about message
                if (item == OYSTER)
                {
                    itemController.SetItemState(OYSTER, 1);
                    textDisplayController.AddTextToLog(itemController.DescribeItem(OYSTER));
                    itemController.SetItemState(OYSTER, 1);
                }
                else
                {
                    itemController.SetItemState(item, -1 - itemState);
                }
            }
        }
    }

    // Display items at current location
    private void DisplayItems()
    {
        // We can't see items in the dark
        if (!IsDark())
        {
            string itemList = itemController.DescribeItemsAt(playerController.CurrentLocation);

            if (itemList != null && itemList != "")
            {
                textDisplayController.AddTextToLog(itemList);
            }
        }
    }

    // Display the current location
    private void DisplayLocation()
    {
        // If player is being followed by the bear, remind them of this
        if (playerController.HasItem("35Bear"))
        {
            textDisplayController.AddTextToLog(playerMessageController.GetMessage("141TameBear"));
        }

        if (IsDark() && !locationController.TravelIsForced(playerController.CurrentLocation))
        {
            // It's dark (and movement from here is not forced) so first check if player has moved from another dark location and falls into a pit (35% chance)
            if (WasDark && Random.value <= 0.35)
            {
                textDisplayController.AddTextToLog(playerMessageController.GetMessage("23PitFall"));
                playerController.KillPlayer();
            }
            else
            {
                // Player is alive so give them a warning about wandering about in the dark
                textDisplayController.AddTextToLog(playerMessageController.GetMessage("16PitchDark"));
                WasDark = true;
            }
        }
        else
        {
            // Player can see here, so describe the location
            string description = locationController.DescribeLocation(playerController.CurrentLocation);

            if (description != null && description != "")
            {
                textDisplayController.AddTextToLog(description);
            }
            WasDark = false;
        }
    }

    // Set up parameters for a new game
    private void NewGame()
    {
        // Initialise variables
        CurrentGameStatus = GameStatus.PLAYING;
        Turns = 0;
        Panic = false;
        Clock1 = CLOCK_1_INITIAL;
        Clock2 = CLOCK_2_INITIAL;
        LampWarning = false;
        NumDeaths = 0;
        CurrentCaveStatus = CaveStatus.OPEN;

        // Reset all items to their initial locations
        itemController.ResetItems();

        // Reset scores
        scoreController.ResetScores();

        // Reset hints
        hintController.ResetHints();

        // Reset action command trackers
        actionController.ResetTrackers();

        // Clear the visited locations
        locationController.ResetLocations();

        // Generate a new magic word
        commandsController.ResetCommands();

        // Reset dwarves
        dwarfController.ResetDwarves();

        // Reset the player
        playerController.ResetPlayer();

        // Reset the persistence mechanism
        persistenceController.ResetLastSave();
    }

    // Player has died, offer reincarnation or wrap things up
    private void PlayerDeath()
    {
        NumDeaths++;

        // Can't be reincarnated if cave is closing
        if (CurrentCaveStatus == CaveStatus.CLOSING)
        {
            // Let the player know it's closing and we're wrapping things up
            textDisplayController.AddTextToLog(playerMessageController.GetMessage("131DeadClosing"));
            EndGame(false);
        }
        else
        {
            // Otherwise, ask the reincarnation question
            questionController.RequestQuestionResponse(reincarnationTexts[NumDeaths - 1].questionMessageID, reincarnationTexts[NumDeaths - 1].responseMessageID, "54OK", ReincarnatePlayer, NoReincarnation);
        }
    }

    // Update the clocks and check for closing/blinding flash
    private void UpdateClocks()
    {
        string location = playerController.CurrentLocation;

        // If all treasures collected, and in deep and not at Y2, countdown clock to closing
        if (itemController.TreasuresRemaining == 0 && locationController.LocType(location) == LocationType.DEEP && location != "33Y2")
        {
            Clock1--;

            if (Clock1 == 0)
            {
                StartClosing();
            }
            else if (Clock1 < 0)
            {
                // We're already closing, so countdown clock to blinding flash
                Clock2--;

                if (Clock2 == 0)
                {
                    StartEndGame();
                }
            }
        }
    }

    // Update the lamp and check for loss of power/battery change etc
    private void UpdateLamp()
    {
        const string BATTERIES = "39Batteries";
        string lampMessage = null;

        // If lamp is currently on...
        if (itemController.GetItemState(LAMP) == 1)
        {
            LampLife--;

            bool lampIsHere = playerController.ItemIsPresent(LAMP);

            // If lamp is out of power ...
            if (LampLife == 0)
            {
                // Switch lamp off permanantly
                LampLife = -1;
                itemController.SetItemState(LAMP, 0);

                // If the player has the lamp or it's at current location, warn them it's out of power
                if (lampIsHere)
                {
                    lampMessage = "184LampNoPower";
                }
            }
            // If lamp is getting dim...
            else if (LampLife <= 30)
            {
                // If there are fresh batteries here, replace them
                if (playerController.ItemIsPresent(BATTERIES) && itemController.GetItemState(BATTERIES) == 0 && lampIsHere)
                {
                    itemController.SetItemState(BATTERIES, 1);

                    if (playerController.HasItem(BATTERIES))
                    {
                        itemController.DropItemAt(BATTERIES, playerController.CurrentLocation);
                    }

                    LampLife += 2500;
                    LampWarning = false;
                    lampMessage = "188BatteryReplacement";
                }
                // Otherwise show a message based on whether the batteries exist, have been used, etc.
                else if (!LampWarning && lampIsHere)
                {
                    LampWarning = true;
                    lampMessage = "187BatteriesWarning";
                   
                    if (!itemController.ItemInPlay(BATTERIES))
                    {
                        lampMessage = "183LampWarning";
                    }
                    else if (itemController.GetItemState(BATTERIES) == 1)
                    {
                        lampMessage = "189NoMoreBatteries";
                    }
                }
            }
        }

        // If any of that generated a message, show it now
        if (lampMessage != null)
        {
            textDisplayController.AddTextToLog(playerMessageController.GetMessage(lampMessage));
        }
    }
}

// Cave Status shows whether the cave is currently open, closing or closed
public enum CaveStatus { OPEN, CLOSING, CLOSED };

// Game status shows whether the game is currently playing or is over
public enum GameStatus { PLAYING, OVER };




