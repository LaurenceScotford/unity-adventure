// Attack
// Handles the ATTACK command

public class Attack : Action
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    private ActionController controller;
    private GameController gameController;
    private ParserState parserState;
    private PlayerController playerController;
    private ItemController itemController;
    private DwarfController dwarfController;
    private TextDisplayController textDisplayController;
    private PlayerMessageController playerMessageController;
    private PlayerInput playerInput;
 
    string itemToAttack;        // The item the player is trying to attack
    private string location;    // The player avatar's current location
    string attackText;          // The text to be displayed as an outcome of the attack

    private const string BEAR = "35Bear";
    private const string BIRD = "8Bird";
    private const string CLAM = "14Clam";
    private const string DRAGON = "31Dragon";
    private const string DWARF = "17Dwarf";
    private const string OGRE = "41Ogre";
    private const string OYSTER = "15Oyster";
    private const string SNAKE = "11Snake";
    private const string TROLL = "33Troll";
    private const string VENDING = "38VendingMachine";

    // === CONSTRUCTOR ===
    public Attack (ActionController actionController)
    {
        // Get references to other parts of game engine
        controller = actionController;
        gameController = controller.GC;
        parserState = controller.PS;
        playerController = controller.PC;
        itemController = controller.IC;
        dwarfController = controller.DC;
        textDisplayController = controller.TDC;
        playerMessageController = controller.PMC;
        playerInput = controller.PI;

        // Define behaviour for getting a subject
        subjectOptional = false;
    }

    // === PUBLIC METHODS ===

    public override CommandOutcome DoAction()
    {
        location = playerController.CurrentLocation;

        noSubjectMsg = new NoSubjectMsg("44NoAttack", null);
        carryOverVerb = false;

        // Check whether a subject was identified, or could be identified and return if not
        itemToAttack = controller.GetSubject("attack");

        // If no item could be identified then we're either done with this command or we need to continue processing to find a subject
        if (itemToAttack == null)
        {
            return CommandOutcome.NO_COMMAND;
        }

        attackText = null;
        CommandOutcome outcome = CommandOutcome.MESSAGE;

        switch(itemToAttack)
        {
            case BIRD:
                outcome = AttackBird();
                break;
            case VENDING:
                outcome = AttackVendingMachine();
                break;
            case CLAM:
            case OYSTER:
                attackText = playerMessageController.GetMessage("150StrongShell");
                break;
            case SNAKE:
                attackText = playerMessageController.GetMessage("46AttackSnake");
                break;
            case DWARF:    
                outcome = AttackDwarf();
                break;
            case DRAGON:
                outcome = AttackDragon();
                break;
            case TROLL:
                attackText = playerMessageController.GetMessage("157TrollDefence");
                break;
            case OGRE:
                outcome = AttackOgre();
                break;
            case BEAR:
                outcome = AttackBear();
                break;
            default:
                // The identified object cannot be attacked, so display default message
                parserState.CurrentCommandState = CommandState.DISCARDED;
                return CommandOutcome.NO_COMMAND;
        }

        if (attackText != null)
        {
            textDisplayController.AddTextToLog(attackText);
        }
        
        parserState.CommandComplete();
        return outcome;
    }

    // Check a range of primary and secondary targets if no explicit subject given
    public override string FindSubstituteSubject()
    {
        string subject = null;
        int subjectCount = 0;

        // These are all potential primary targets if no subject is given
        Substitute[] primarySubstitutes = new Substitute[]
        {
            new Substitute(SNAKE, false),
            new Substitute(DRAGON, true),
            new Substitute(TROLL, false),
            new Substitute(OGRE, false),
            new Substitute(BEAR, true)
        };

        // These are all potential secondary targets if no subject given and no primary target found
        Substitute[] secondarySubstitutes = new Substitute[]
        {
            new Substitute(BIRD, true),
            new Substitute(VENDING, true),
            new Substitute(CLAM, false),
            new Substitute(OYSTER, false)
        };

        // If there's at least one dwarf at this location, set the subject to dwarf
        if (dwarfController.CountDwarvesAt(location) > 0)
        {
            subject = DWARF;
            subjectCount++;
        }

        // Now check other enemies that might be here, that could be attacked
        foreach (Substitute enemy in primarySubstitutes)
        {
            if (itemController.ItemIsAt(enemy.item, location) && (!enemy.conditional || itemController.GetItemState(enemy.item) == 0))
            {
                subject = enemy.item;
                subjectCount++;
            }
        }

        // if no primary substitute was found...
        if (subjectCount == 0)
        {
            // Try the secondary substitutes
            foreach (Substitute target in secondarySubstitutes)
            {
                if (itemController.ItemIsAt(target.item, location) && (!target.conditional || parserState.ActiveCommand != "2017Throw"))
                {
                    subject = target.item;
                    subjectCount++;
                }
            }
        }

        // If we found more than one potential subject, then the subject is ambiguous so indicate no subject
        if (subjectCount > 1)
        {
            subject = null;
            noSubjectMsg = new NoSubjectMsg("257VerbWhat", parserState.Words);
            carryOverVerb = true;
        }

        return subject;
    }

    // === PRIVATE METHODS ===

    private CommandOutcome AttackBear()
    {

        string attackMsg = null;

        switch (itemController.GetItemState(BEAR))
        {
            case 0:
                // Bear is chained to wall
                attackMsg = "165BearKnuckle";
                break;
            case 1:
            case 2:
                // Bear is free
                attackMsg = "166BearConfused";
                break;
            case 3:
                // Bear is dead
                attackMsg = "167AlreadyDead";
                break;
        }

        attackText = playerMessageController.GetMessage(attackMsg);

        return CommandOutcome.MESSAGE;
    }

    private CommandOutcome AttackBird()
    {
        // If cave is closed...
        if (gameController.CurrentCaveStatus == CaveStatus.CLOSED)
        {
            // ... tell player not to bother bird
            attackText = playerMessageController.GetMessage("137LeaveBird");
        }
        else
        {
            // ... otherwise, destory the bird and announce its death
            itemController.DestroyItem(BIRD);
            itemController.SetItemState(BIRD, 0);
            attackText = playerMessageController.GetMessage("45BirdDead");
        }

        return CommandOutcome.MESSAGE;
    }

    private CommandOutcome AttackDragon()
    {
        if (itemController.GetItemState(DRAGON) != 0)
        {
            attackText = playerMessageController.GetMessage("167AlreadyDead");
            return CommandOutcome.MESSAGE;
        }
        else
        {
            // Ask player if they plan to attack with bare hands and temporarily divert command processing to a bespoke method to monitor for a response to this question
            gameController.SuspendCommandProcessing();
            PlayerInput.commandsEntered = DragonResponse;
            attackText = playerMessageController.GetMessage("49BareHands");
            return CommandOutcome.QUESTION;
        }
    }

    private CommandOutcome AttackDwarf()
    {
        if (gameController.CurrentCaveStatus == CaveStatus.CLOSED)
        {
            // If cave is closed, attacking a dwarf results in a violent and deadly response
            attackText = null;
            return CommandOutcome.DISTURBED;
        }
        else
        {
            attackText = playerMessageController.GetMessage("49BareHands");
            return CommandOutcome.MESSAGE;
        }
    }

    private CommandOutcome AttackOgre()
    {
        int numDwarvesHere = dwarfController.CountDwarvesAt(location);

        // If there is at least one dwarf here...
        if (numDwarvesHere > 0)
        {
            // ... tell the player that the ogre dodges their attack, but they are attacked by the dwarf/dwarves
            textDisplayController.AddTextToLog(playerMessageController.GetMessage("203OgreDodges"));
            textDisplayController.AddTextToLog(playerMessageController.GetMessage("6KnifeThrow"));

            // Destroy the ogre
            itemController.DestroyItem(OGRE);

            // Move any dwarves at this location to the west end of long hall and make them lose sight of the player
            string westEnd = "61LongHallWest";
            int nextDwarf = dwarfController.FirstDwarfAt(location);

            while (nextDwarf != -1)
            {
                dwarfController.MoveDwarfTo(nextDwarf, westEnd);
                nextDwarf = dwarfController.FirstDwarfAt(location);
            }

            attackText = playerMessageController.GetMessage(numDwarvesHere > 1 ? "204OgreChaseDwarves" : "205OgreChaseDwarf");
        }
        else
        {
            attackText = playerMessageController.GetMessage("203OgreDodges");
        }
        return CommandOutcome.MESSAGE;
    }

    private CommandOutcome AttackVendingMachine()
    {
        int vendingState = itemController.GetItemState(VENDING);

        // Describe what happens to vending machine
        itemController.SetItemState(VENDING, vendingState + 2);
        attackText = itemController.DescribeItem(VENDING);

        // Put vending machine into new stable state
        itemController.SetItemState(VENDING, 3 - vendingState);

        return CommandOutcome.MESSAGE;
    }

    // Temporary handler for commands - monitors immediate response after player is asked if they want to attack dragon with their bare hands
    private void DragonResponse()
    {
        const string RUG = "62Rug";

        // Remove this special case handler
        PlayerInput.commandsEntered = null;

        // Now resume normal command processing
        gameController.ResumeCommandProcessing();

        // Get player's response
        string response = playerInput.Words[0].activeWord.ToUpper();

        // If they've answered the question positively...
        if (response == "Y" || response == "YES")
        {
            // Tell them they've killed the dragon
            itemController.SetItemState(DRAGON, 3);
            textDisplayController.AddTextToLog(itemController.DescribeItem(DRAGON));

            // Actually kill the dragon
            itemController.SetItemState(DRAGON, 1);

            // Move rug here and make it movable
            itemController.DropItemAt(RUG, location);
            itemController.MakeItemMovable(RUG);
            itemController.SetItemState(RUG, 0);

            // Move the blood here
            itemController.DropItemAt("44Blood", location);

            // Force the location to be described again
            gameController.ProcessTurn(CommandOutcome.DESCRIBE);
        }
        else
        {
            // The player did not give a positive response to the question, so process it like any other command
            gameController.GetPlayerCommand();
        }
    }

    // === STRUCTS ===

    // Holds a potential substitute item. item = the item ID that must be present, conditional indicates whether an additional condition must be met (the condition is contextual)
    private struct Substitute
    {
        public string item;
        public bool conditional;

        public Substitute(string p1, bool p2)
        {
            item = p1;
            conditional = p2;
        }
    }
}


