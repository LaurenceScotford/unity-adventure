// Action Controller
// Manages processing of verbs

using System.Collections.Generic;
using UnityEngine;

public class ActionController : MonoBehaviour
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    [SerializeField] private GameController gameController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private ItemController itemController;
    [SerializeField] private TextDisplayController textDisplayController;
    [SerializeField] private CommandsController commandsController;
    [SerializeField] private ParserState parserState;
    [SerializeField] private PlayerMessageController playerMessageController;
    [SerializeField] private LocationController locationController;
    [SerializeField] private DwarfController dwarfController;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private QuestionController questionController;
    [SerializeField] private ScoreController scoreController;

    // A dictionary mapping verbs to the class that handles them
    private Dictionary<string, Action> actions; 

    

    // === PROPERTIES ===

    public GameController GC { get { return gameController; } }
    public PlayerController PC { get { return playerController; } }
    public ItemController IC { get { return itemController; } }
    public TextDisplayController TDC { get { return textDisplayController; } }
    public CommandsController CC { get { return commandsController; } }
    public ParserState PS { get { return parserState; } }
    public PlayerMessageController PMC { get { return playerMessageController; } }
    public LocationController LC { get { return locationController; } }
    public DwarfController DC { get { return dwarfController; } }
    public PlayerInput PI { get { return playerInput; } }
    public QuestionController QC { get { return questionController; } }
    public ScoreController SC { get { return scoreController; } }

    public int Foobar { get; private set; }     // Used to track the FEE FIE FOE sequence
    public int GoCount { get; private set; }    // Used to keep track of number of times player uses "GO"

    // === MONOBEHAVIOUR METHODS ===

    private void Start()
    {
        actions  = new Dictionary<string, Action>()
        {
            {"carry", new Carry(this)},
            {"drop", new Drop(this)},
            {"say", new Say(this)},
            {"lockUnlock", new LockUnlock(this)},
            {"nothing", new Nothing(this)},
            {"on", new On(this)},
            {"off", new Off(this)},
            {"wave", new Wave(this)},
            {"calm", new Calm(this)},
            {"go", new Go(this)},
            {"attack", new Attack(this)},
            {"pour", new Pour(this)},
            {"eat", new Eat(this)},
            {"drink", new Drink(this)},
            {"rub", new Rub(this) },
            {"throw", new Throw(this)},
            {"quit", new Quit(this)},
            {"find", new Find(this)},
            {"inventory", new Inventory(this)},
            {"feed", new Feed(this)},
            {"fill", new Fill(this)},
            {"blast", new Blast(this)},
            {"score", new Score(this)},
            {"feefiefoe", new FeeFieFoe(this)},
            {"brief", new Brief(this)},
            {"read", new Read(this)},
            {"break", new Break(this)},
            {"wake", new Wake(this)},
            {"save", new Save(this)},
            {"resume", new Resume(this)},
            {"fly", new Fly(this)},
            {"listen", new Listen(this)},
            {"z'zzz", new Zzzzz(this)}
        };
    }

    // === PUBLIC METHODS ===

    // Executes the handler for the given action command
    public CommandOutcome ExecuteAction(string actionID)
    {
        if (actions.ContainsKey(actionID))
        {
            return actions[actionID].DoAction();
        }
        else
        {
            parserState.CommandComplete();
            Debug.LogErrorFormat("ActionController.DoAction was passed an unknown action ID: \"{0}\"", actionID);
            return CommandOutcome.NO_COMMAND;
        }
    }

    // Tries to identify a subject for the command and returns the ID of the item or null, if none could be found
    public string GetSubject(string actionID)
    {
        string subject = null;

        // If no subject has been identified and there are no further commands to be processed
        if (!parserState.ContainsState(CommandState.SUBJECT_IDENTIFIED) && !parserState.ContainsState(CommandState.NOT_PROCESSED) && !parserState.ContainsState(CommandState.PENDING) && !parserState.SetCarriedOverCommand(true))
        {
            // Otherwise check to see if the command can assume a subject based on the current context
            subject = actions[actionID].FindSubstituteSubject();
        }

        // If the subject has not yet been set but a subject has been identified
        if (subject == null && parserState.ContainsState(CommandState.SUBJECT_IDENTIFIED))
        {
            // ... set it
            subject = parserState.Subject;
        }

        // If we've still not found a subject and there's nothing further to process, it is ambiguous, so ask player for clarification
        if (subject == null && !parserState.ContainsState(CommandState.NOT_PROCESSED) && !parserState.ContainsState(CommandState.PENDING) && !actions[actionID].SubjectOptional)
        {
            if (actions[actionID].CarryOverVerb)
            {
                parserState.CarryOverVerb();
            }

            NoSubjectMsg noSubjectMsg = actions[actionID].NoSubjectMessage;

            if (noSubjectMsg.messageParams != null)
            {
                textDisplayController.AddTextToLog(playerMessageController.GetMessage(noSubjectMsg.messageID, noSubjectMsg.messageParams));
            }
            else
            {
                textDisplayController.AddTextToLog(playerMessageController.GetMessage(noSubjectMsg.messageID));
            }

            parserState.CurrentCommandState = CommandState.NO_COMMAND; // Indicate we're done with this command
        }

        return subject;
    }

    // increments index tracking FEE FIE FOE sequence
    public void IncrementFoobar()
    {
        Foobar++;
    }

    // Increments Go Count and returns the new count
    public int IncrementGoCount()
    {
        return ++GoCount;
    }

    // Resets index tracking FEE FIE FOE sequence
    public void ResetFoobar()
    {
        Foobar = 0;
    }

    // Resets trackers for new game
    public void ResetTrackers()
    {
        ResetFoobar();
        GoCount = 0;
    }

    // Restore Action Controller from saved game data
    public void Restore(GameData gameData)
    {
        Foobar = gameData.foobar;
        GoCount = gameData.goCount;
    }
}


