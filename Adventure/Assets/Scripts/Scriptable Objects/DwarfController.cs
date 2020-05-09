// Dwarf Controller
// Manages dwarf status and movement

using System.Collections.Generic;
using UnityEngine;

public class DwarfController : MonoBehaviour
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    [SerializeField] private PlayerController playerController;
    [SerializeField] private ItemController itemController;
    [SerializeField] private LocationController locationController;
    [SerializeField] private TextDisplayController textDisplayController;
    [SerializeField] private PlayerMessageController playerMessageController;

    private Dwarf[] dwarves;        // Array holding the dwarves
    /*
     * Dwarf activation levels:
     * 0 - Dwarves not yet active (player avatar hasn't reached the Hall of Mists)
     * 1 - Player avatar has reached the Hall of Mists, so dwarves active but player hasn't encountered one yet
     * 2 - Player has encountered first dwarf (axe attack that misses) - other dwarves start moving
     * 3 - First knife has been thrown (and missed player avatar) - things start becoming more deadly from now on
     * 4+ - Dwarves are getting madder and their accuracy is increasing
     */
    private bool knifeMessageShown;
    private const string KNIFE = "18Knife";
    private const string CHEST = "55Chest";
    private const int PIRATE = 5;
    private const int NUM_DWARVES = 5;

    // A list of locations forbidden to the pirate, unless he's following the player
    private readonly List<string> forbiddenToPirate = new List<string>()
    {
        "46DeadEnd", "47DeadEnd", "48DeadEnd", "54DeadEnd", "56DeadEnd", "58DeadEnd", "82DeadEnd", "85DeadEnd", "86DeadEnd",
        "122ChasmNE", "123LongCorridor", "124ForkInPath", "125JunctionWithWarmWalls", "126BreathTakingView", "127ChamberOfBoulders",
        "128LimestonePassage", "129FrontOfBarrenRoom", "130BarrenRoom"
    };

    private readonly string[] chestLocations = new string[] { "114DeadEnd", "140DeadEnd" };  // Locations of chest and maze message

    // Start locations for the dwarves (Note dwarf six is pirate - starts at chest location - 7th location is an alternate location in case a dwarf starts at player avatar location
    private readonly string[] dwarfStartLocations = new string[] { "19HallOfMountainKing", "27WestFissure", "33Y2", "44MazeAlike", "64ComplexJunction", "114DeadEnd", "18NuggetOfGoldRoom" };

    // === PROPERTIES ===

    // Keeps track of current activation level
    public int ActivationLevel { get; private set; }

    // Return number of dwarves killed by player
    public int DwarvesKilled { get; private set; }

     // === MONOBEHAVIOUR METHODS ===

    private void Awake()
    {
        dwarves = new Dwarf[6];

        for (int i = 0; i < dwarves.Length; i++)
        {
            dwarves[i] = new Dwarf();
        }
    }

    // === PUBLIC METHODS ===

    // Launches attack on dwarf and returns true if the attack killed the dwarf
    public bool AttackDwarf(int dwarf)
    {
        if (DwarfExists(dwarf, "AttackDwarf"))
        {
            if (Random.Range(0, 7) < ActivationLevel)
            {
                KillDwarf(dwarf);
                DwarvesKilled++;
                return true;
            }
        }

        return false;
    }
    
    // Returns true if a dwarf has blocked the player avatar's movement
    public bool Blocked()
    {
        // If the player avatar is moving, the movement hasn't been forced and they are not coming from a location forbidden to the pirate...
        if (playerController.IsMoving && !playerController.MovementIsForced && !forbiddenToPirate.Contains(playerController.CurrentLocation))
        {
            // Cycle through all the dwarves (except pirate)
            for (int i = 0; i < NUM_DWARVES; i++)
            {
                // If dwarf has come from where the player is trying to get to and has seen the player ...
                if (dwarves[i].OldDwarfLocation == playerController.PotentialLocation && dwarves[i].SeenPlayer)
                {
                    return true;
                }
            }
        }
        return false;
    }

    // Returns the number of dwarves at the given location
    public int CountDwarvesAt(string location)
    {
        int dwarfCount = 0;

        if (ActivationLevel > 1)
        {
            // Cycle through dwarves excluding pirate
            for (int i = 0; i < NUM_DWARVES; i++)
            {
                if (dwarves[i].Status == DwarfStatus.ALIVE && dwarves[i].DwarfLocation == location)
                {
                    dwarfCount++;
                }
            }
        }
        
        return dwarfCount;
    }

    // Move dwarves and attack player avatar when appropriate. Returns true if player avatar was attacked and killed, or false otherwise
    public bool DoDwarfActions()
    {
        string location = playerController.CurrentLocation;

        // Only move dwarves if player is alive and movement is not forced from this location, and player is not at a location forbidden to the pirate
        if (playerController.IsAlive && !playerController.MovementIsForced && !forbiddenToPirate.Contains(location))
        {
            switch (ActivationLevel)
            {
                case 0:
                    // If player has reached a deep location in the caves, activate the dwarves
                    if (locationController.LocType(location) == LocationType.DEEP)
                    {
                        ActivationLevel = 1;
                    }
                    break;
                case 1:
                    FirstEncounter();
                    break;
                default:
                    if (MoveDwarves())
                    {
                        return true;
                    }
                    break;
            }
        }

        return false;
    }

    // Returns the index of the first dwarf at a given location or -1 if there are none there
    public int FirstDwarfAt(string location)
    {
        if (ActivationLevel > 1)
        {
            // Cycle through dwarves excluding pirate
            for (int i = 0; i < NUM_DWARVES; i++)
            {
                if (dwarves[i].Status == DwarfStatus.ALIVE && dwarves[i].DwarfLocation == location)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    // Steps up dwarf activation to the next level
    public void IncreaseActivationLevel()
    {
        ActivationLevel++;
    }

    // Kill all dwarves
    public void KillAllDwarves()
    {
        for (int i = 0; i < dwarves.Length; i++)
        {
            KillDwarf(i);
        }
    }

    // Called when the knife message has been shown (knife is destroyed an no longer created)
    public void KnifeMessageShown()
    {
        knifeMessageShown = true;
        itemController.DestroyItem(KNIFE);
    } 

    // Moves a dwarf to the given location (used for direct translocation only, not normal movement)
    public void MoveDwarfTo(int dwarf, string location)
    {
        if (DwarfExists(dwarf, "MoveDwarfTo"))
        {
            dwarves[dwarf].DwarfLocation = location;
            dwarves[dwarf].OldDwarfLocation = location;
            dwarves[dwarf].SeenPlayer = false;
        }
    }

    // Resets the dwarves
    public void ResetDwarves()
    {
        // Put dwarves at initial locations and set their state
        for (int i = 0; i < dwarves.Length; i++)
        {
            dwarves[i].Status = DwarfStatus.ALIVE;
            dwarves[i].SeenPlayer = false;
            dwarves[i].DwarfLocation = dwarfStartLocations[i];
        }

        ActivationLevel = 0;
        DwarvesKilled = 0;

        // Set the knife to be non-existant
        knifeMessageShown = false;
        itemController.DestroyItem(KNIFE);
    }

    // === PRIVATE METHODS ===

    // Returns true if the given dwarf exists (dead or alive). If not returns false and generates error message
    private bool DwarfExists(int dwarf, string methodName)
    {
        if (dwarf >= 0 && dwarf < dwarves.Length)
        {
            return true;
        }
        else
        {
            Debug.LogErrorFormat("DwarfController.{0} was passed a non-existant dwarf index: {1}", methodName, dwarf);
            return false;
        }
    }

    private void FirstEncounter()
    {
        string location = playerController.CurrentLocation;

        // Determine if the player will encounter the first dwarf on this turn and return if not
        if (!(locationController.LocType(location) == LocationType.DEEP) || (Random.value < .95 && (!locationController.CanMoveBack(location) || Random.value < .85)))
        {
            return;
        }

        // Indicate player has met the first dwarf
        ActivationLevel = 2;

        // On first encountering the dwarves, randomly kill up to two of them (there's a 50% chance a dwarf will die - note it's possible the same dwarf could be killed twice - not a bug)
        for (int i = 0; i < 2; i++)
        {
            if (Random.value < .5)
            {
                KillDwarf(Random.Range(0, 4));
            }
        }

        // If any of the dwarves is at the player avatar's current location, move them to the alternative location
        for (int i = 0; i < NUM_DWARVES; i++)
        {
            if (dwarves[i].DwarfLocation == location)
            {
                dwarves[i].DwarfLocation = dwarfStartLocations[6];
                dwarves[i].OldDwarfLocation = dwarfStartLocations[6];
            }
        }

        // Tell the player about the encounter
        textDisplayController.AddTextToLog(playerMessageController.GetMessage("3DwarfThrow"));

        // Drop the axe here
        itemController.DropItemAt("28Axe", location);
    }

    // Hides the pirate chest in the maze (and the message in the other maze) and moves the pirate to his chest
    private void HidePirateChest()
    {


        // If the treasure chest is not yet in play, bring it into existance, along with the message in the other maze
        if (!itemController.ItemInPlay(CHEST))
        {
            itemController.DropItemAt(CHEST, chestLocations[0]);
            itemController.DropItemAt("36MazeMessage", chestLocations[1]);
        }

        // Move the pirate to the chest location
        MoveDwarfTo(PIRATE, chestLocations[0]);
    }

    // Kills the given dwarf
    private void KillDwarf(int dwarf)
    {
        dwarves[dwarf].SeenPlayer = false;
        dwarves[dwarf].DwarfLocation = "OutOfPlay";
        dwarves[dwarf].OldDwarfLocation = "OutOfPlay";
        dwarves[dwarf].Status = DwarfStatus.DEAD;
    }

    // Move the dwarves. Return true if the dwarves attacked and killed the player, or false otherwise
    private bool MoveDwarves()
    {
        int dwarvesHere = 0;
        int dwarvesAttacking = 0;
        int dwarvesSticking = 0;

        string location = playerController.CurrentLocation;

        for (int i = 0; i < dwarves.Length; i++)
        {
            // Get a list of potential destinations and select one at random
            List<string> potentialDestinations = PossibleMoves(dwarves[i].DwarfLocation, i);
            string destination = potentialDestinations[Random.Range(0, potentialDestinations.Count - 1)];

            // Move the dwarf to the new location
            dwarves[i].OldDwarfLocation = dwarves[i].DwarfLocation;
            dwarves[i].DwarfLocation = destination;

            // Dwarf has seen player avatar if they were seen on previous turn and are still deep in the cave, or if they are currently at the dwarf's current or previous location
            if ((dwarves[i].SeenPlayer && locationController.LocType(location) == LocationType.DEEP) || (location == dwarves[i].DwarfLocation || location == dwarves[i].OldDwarfLocation))
            {
                // Dwarf has seen player avatar so moves to player avatar's location
                dwarves[i].SeenPlayer = true;
                dwarves[i].DwarfLocation = location;

                // If this dwarf is the pirate, do special pirate actions at this point
                if (i == PIRATE)
                {
                    PirateActions();
                }
                else
                {
                    // Increment number of dwarves at player avatar's location
                    dwarvesHere++;

                    // If dwarf has not moved...
                    if (dwarves[i].DwarfLocation == dwarves[i].OldDwarfLocation)
                    {
                        //... attack the player
                        dwarvesAttacking++;

                        // If the knife message has not yet been shown drop the phony knife here (not a real item but purely a way to trigger the message if the player tries to interact with the thrown knives)
                        if (!knifeMessageShown)
                        {
                            itemController.DropItemAt(KNIFE, location);
                        }

                        // Caluclate if the attack hits the player avatar
                        if (Random.Range(0, 1000) < 95 * (ActivationLevel - 2))
                        {
                            dwarvesSticking++;
                        }
                    }
                }
            }
        }

        // If there are any dwarves at the player avatar's location, let the player know what's happening
        if (dwarvesHere > 0)
        {
            string dwarfMessage = dwarvesHere == 1 ? playerMessageController.GetMessage("5OneDwarf") : playerMessageController.GetMessage("4SeveralDwarves", new string[] { dwarvesHere.ToString() });
            textDisplayController.AddTextToLog(dwarfMessage);

            // If any of the dwarves are attacking...
            if (dwarvesAttacking > 0)
            {
                // If this is the first attack, step up the activation level so that future attacks are potentially deadly
                if (ActivationLevel == 2)
                {
                    ActivationLevel = 3;
                }

                if (dwarvesAttacking > 1)
                {
                    textDisplayController.AddTextToLog(playerMessageController.GetMessage("250NumKnivesThrown", new string[] { dwarvesAttacking.ToString() }));
                }
                else
                {
                    textDisplayController.AddTextToLog(playerMessageController.GetMessage("6KnifeThrow"));
                }
               
                if (dwarvesSticking > 0)
                {
                    switch (dwarvesSticking)
                    {
                        case 0:
                            dwarfMessage = playerMessageController.GetMessage("253NoKnivesHit");
                            break;
                        case 1:
                            dwarfMessage = playerMessageController.GetMessage("252OneKnifeHit");
                            break;
                        default:
                            dwarfMessage = playerMessageController.GetMessage("251NumKnivesHit", new string[] { dwarvesSticking.ToString() });
                            break;
                    }

                    textDisplayController.AddTextToLog(dwarfMessage);
                    return true;
                }
            }
        }

        return false;
    }

    private void PirateActions()
    {
        const string LAMP = "2Lantern";

        string location = playerController.CurrentLocation;

        // Pirate leaves player alone if they have already discovered the treasure chest
        if (location == chestLocations[0] || itemController.TreasureWasSeen(CHEST))
        {
            // Create new emptry list of treasures the pirate could steal
            List<string> treasuresToSteal = new List<string>();

            // Get all the items carried or at the player avatar's location
            List<string> itemsHere = playerController.PresentItems();

            bool treasureAtLocation = false;
            bool carriedTreasure = false;

            // Identify any stealable treasures 
            foreach (string item in itemsHere)
            {
                if (itemController.IsTreasure(item))
                {
                    if (playerController.HasItem(item))
                    {
                        carriedTreasure = true;
                    }
                    else
                    {
                        treasureAtLocation = true;
                    }
                    
                    // Don't steal pyramid from start location or start location of emerald or any treasure that's not currently movable
                    if (itemController.ItemCanBeMoved(item) && !(item == "60Pyramid" && location == "100PloverRoom" || location == "101DarkRoom"))
                    {
                        treasuresToSteal.Add(item);
                    }
                }
            }

            // If the player is carrying a treasure...
            if (carriedTreasure)
            {
                
                // Alert the player that their treasure is being stolen
                textDisplayController.AddTextToLog(playerMessageController.GetMessage("128Pirate"));

                // Now move all the treasures that can be stolen to the chest location
                foreach (string item in treasuresToSteal)
                {
                    itemController.DropItemAt(item, chestLocations[0]);
                }

                HidePirateChest();
               
            }
            else if (itemController.TreasuresRemaining == 1 && !treasureAtLocation && itemsHere.Contains(LAMP) && itemController.GetItemState(LAMP) == 1)
            {
                // Let the player know they've spotted the pirate
                textDisplayController.AddTextToLog(playerMessageController.GetMessage("186SpotPirate"));

                HidePirateChest();
            }
            // If nothing else has happened, but the pirate has moved, there's a 20% chance the player avatar will hear him
            else if (dwarves[PIRATE].DwarfLocation != dwarves[PIRATE].OldDwarfLocation && Random.value < .2)
            {
                textDisplayController.AddTextToLog(playerMessageController.GetMessage("127NoisesBehind"));
            }
        } 
    }

     // Returns a list of locations that a dwarf could move to from the given location
    private List<string> PossibleMoves(string locationID, int dwarf)
    {
        // Get previous location as we'll need this later
        string previous = dwarves[dwarf].OldDwarfLocation;

        List<string> possibleDestinations = locationController.Destinations(locationID);
        List<string> usableDestinations = new List<string>();

        // Prune unsuitable locations from the list
        foreach (string destination in possibleDestinations)
        {
            bool inDeep = locationController.LocType(destination) == LocationType.DEEP;
            bool notCurrentOrPrevious = destination != locationID && destination != previous;
            bool notForced = !locationController.TravelIsForced(destination);
            bool notForbidden = !(locationID == "61LongHallWest" && destination == "107MazeDifferent");
            bool notForbiddenPirateMove = !(dwarf == 5 && forbiddenToPirate.Contains(destination));

            if (inDeep && notCurrentOrPrevious && notForced && notForbidden && notForbiddenPirateMove)
            {
                usableDestinations.Add(destination);
            }
        }

        // Add dwarf's previous location to the list, if the list would otherwise be empty
        // Note we might have removed it earlier
        if (usableDestinations.Count == 0)
        {
            usableDestinations.Add(previous);
        } 

        return usableDestinations;
    }
}
