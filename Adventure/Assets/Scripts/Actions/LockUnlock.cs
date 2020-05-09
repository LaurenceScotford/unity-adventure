// LockUnlock
// Excutes the LOCK and UNLOCK commands

public class LockUnlock : Action
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    private ActionController controller;
    private ParserState parserState;
    private GameController gameController;
    private PlayerController playerController;
    private ItemController itemController;
    private TextDisplayController textDisplayController;
    private PlayerMessageController playerMessageController;

    private string itemToLockOrUnlock;      // The ID of the item the player is trying to lock or unlock
    private string location;                // The current location of the player avatar
    private bool locking;                   // True if the action is locking, false if unlocking

    // === CONSTRUCTOR ===

    public LockUnlock(ActionController actionController)
    {
        // Get references to other parts of game engine
        controller = actionController;
        gameController = controller.GC;
        parserState = controller.PS;
        playerController = controller.PC;
        itemController = controller.IC;
        textDisplayController = controller.TDC;
        playerMessageController = controller.PMC;

        // Define behaviour for getting a subject
        carryOverVerb = true;
        subjectOptional = false;
    }

    // ==== PUBLIC METHODS ===

    public override CommandOutcome DoAction()
    {
        location = playerController.CurrentLocation;
        locking = parserState.ActiveCommand.CommandID == "2006Lock";

        // Check whether a subject was identified, or could be identified 
        noSubjectMsg = new NoSubjectMsg("257VerbWhat", parserState.Words);
        itemToLockOrUnlock = controller.GetSubject("lockUnlock");

        // If no item could be identified then we're either done with this command or we need to continue processing to find a subject
        if (itemToLockOrUnlock == null)
        {
            return CommandOutcome.NO_COMMAND;
        }

        string lockUnlockMsg = null;

        // Process the item being locked or unlocked and generate a suitable message
        switch (itemToLockOrUnlock)
        {
            case "14Clam":
            case "15Oyster":
                lockUnlockMsg = ClamOyster();
                break;
            case "9RustyDoor":
                lockUnlockMsg = itemController.GetItemState("9RustyDoor") == 1 ? "54OK" : "111RustyDoor";
                break;
            case "4Cage":
                lockUnlockMsg = "32NoLock";
                break;
            case "1Keys":
                lockUnlockMsg = "55NoUnlockKeys";
                break;
            case "3Grate":
            case "64Chain":
                lockUnlockMsg = "31NoKeys";
                if (playerController.ItemIsPresent("1Keys"))
                {
                    if (itemToLockOrUnlock == "3Grate")
                    {
                        lockUnlockMsg = Grate();
                    }
                    else
                    {
                        lockUnlockMsg = Chain();
                    }
                }
                break;
            default:
                parserState.CurrentCommandState = CommandState.DISCARDED;
                return CommandOutcome.NO_COMMAND;
        }

        textDisplayController.AddTextToLog(playerMessageController.GetMessage(lockUnlockMsg));
        parserState.CommandComplete();
        return CommandOutcome.MESSAGE;
    }

    // If no subject specified, check if an item is present that can be assumed as the subject
    public override string FindSubstituteSubject()
    {
        string subject = null;

        // Try to assume one of several openable items, if present ...
        string[] openables = { "3Grate", "9RustyDoor", "15Oyster", "14Clam", "64Chain" };
        
        for (int i = 0; i < openables.Length; i++)
        {
            if (playerController.ItemIsPresent(openables[i]))
            {
                // if no other openable found, set this item as subject and continue searching
                if (subject == null)
                {
                    subject = openables[i];
                }
                else
                {
                    // There's at least one other openable here, which introduces ambiguity, so erase any previously set item and stop searching
                    subject = null;
                    break;
                }
                
            }
        }

        return subject;
    }

    // === PRIVATE METHODS ===

    // Lock/Unlock the chain - freeing the bear if appropriate
    private string Chain()
    {
        string msg;

        if (locking)
        {
            if (!itemController.ItemIsAt("64Chain", location))
            {
                // The chain is not here
                msg = "173NothingToChain";
            }
            else if (itemController.GetItemState("64Chain") != 0)
            {
                // The chain is already locked
                msg = "34AlreadyLocked";
            }
            else
            {
                // lock the chain
                itemController.SetItemState("64Chain", 2);

                // If the player is carrying the chain, drop it here
                if (playerController.HasItem("64Chain"))
                {
                    itemController.DropItemAt("64Chain", location);
                }

                // Fix the chain at this location
                itemController.MakeItemImmovable("64Chain");

                msg = "172ChainLocked";
            }
        }
        else
        {
            if (itemController.GetItemState("64Chain") == 0)
            {
                // Chain is already unlocked
                msg = "37AlreadyUnlocked";
            }
            else if (itemController.GetItemState("35Bear") == 0)
            {
                // Can't get past bear to unlock chain
                msg = "41NoUnlockBear";
            }
            else
            {
                // Unlock the chain
                itemController.SetItemState("64Chain", 0);

                //Make the chain moveable
                itemController.MakeItemMovable("64Chain");

                // If bear still exists, set it to wandering state
                if (itemController.GetItemState("35Bear") != 3)
                {
                    itemController.SetItemState("35Bear", 2);
                }

                // Set bear to moveable or immoveable based on it's current state
                if (itemController.GetItemState("35Bear") == 2)
                {
                    itemController.MakeItemMovable("35Bear");
                }
                else
                {
                    itemController.MakeItemImmovable("35Bear");
                }

                msg = "171ChainUnlocked";
            }
        }

        return msg;
    }

    // Locks / Unlocks the Clam / Oyster
    private string ClamOyster()
    {
        string msg;
        bool isClam = itemToLockOrUnlock == "14Clam";

        if (locking)
        {
            msg = "61Huh";
        }
        else if (!playerController.HasItem("57Trident"))
        {
            msg = isClam ? "122CantOpenClam" : "123CantOpenOyster";
        }
        else if (playerController.HasItem(itemToLockOrUnlock))
        {
            msg = isClam ? "120ClamAdvice" : "121OysterAdvice";
        }
        else
        {
            msg = isClam ? "124OpenClam" : "125OpenOyster";

            if (isClam)
            {
                // Destroy the clam
                itemController.DestroyItem("14Clam");

                // Substitute the oyster
                itemController.DropItemAt("15Oyster", location);

                // Create the pearl
                itemController.DropItemAt("61Pearl", "105CulDeSac");
            }
        }

        return msg;
    }

    // Lock/Unlo9ck the geate
    private string Grate()
    {
        string msg;

        if (gameController.CurrentCaveStatus == CaveStatus.CLOSING)
        {
            gameController.StartPanic();
            msg = "130ClosedAnnounce";
        }
        else
        {
            int itemState = itemController.GetItemState("3Grate");

            if (locking)
            {
                msg = itemState == 0 ? "34AlreadyLocked" : "35GrateLocked";
                itemController.SetItemState("3Grate", 0);
            }
            else
            {
                msg = itemState == 0 ? "36GrateUnlocked" : "37AlreadyUnlocked";
                itemController.SetItemState("3Grate", 1);
            }
        }

        return msg;
    }
}
