// Wave
// Executes WAVE command

public class Wave : Action
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    private ActionController controller;
    private GameController gameController;
    private ParserState parserState;
    private PlayerController playerController;
    private ItemController itemController;
    private TextDisplayController textDisplayController;
    private PlayerMessageController playerMessageController;

    private string itemToWave;  // Item player is trying to wave
    private string location;    // Current location of player avatar

    // === CONSTRUCTOR ===

    public Wave(ActionController actionController)
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

    // === PUBLIC METHODS ===

    public override CommandOutcome DoAction()
    {
        location = playerController.CurrentLocation;

        // Check whether a subject was identified, or could be identified
        noSubjectMsg = new NoSubjectMsg("257VerbWhat", parserState.Words);
        itemToWave = controller.GetSubject("wave");

        // If no item could be identified then we're either done with this command or we need to continue processing to find a subject
        if (itemToWave == null)
        {
            return CommandOutcome.NO_COMMAND;
        }

        string[] waveMsg = new string[2] {null, null};
        bool hasItem = playerController.HasItem(itemToWave);
        bool isRod = itemToWave == "5BlackRod";
        bool birdHere = playerController.ItemIsPresent("8Bird");
        bool atFissure = location == "17EastFissure" || location == "27WestFissure";
        bool closing = gameController.CurrentCaveStatus == CaveStatus.CLOSING;

        if (!hasItem && (!isRod || !playerController.HasItem("6BlackRod")))
        {
            textDisplayController.AddTextToLog(playerMessageController.GetMessage("29DontHaveIt"));
            parserState.CommandComplete();
            return CommandOutcome.MESSAGE;
        }

        // Wave will have no effect in these circumstances
        if (!isRod || !hasItem || (!birdHere && (closing || !atFissure)))
        {
            parserState.CurrentCommandState = CommandState.DISCARDED;
            return CommandOutcome.NO_COMMAND;
        }

        CommandOutcome outcome = CommandOutcome.MESSAGE;

        // If the bird is here...
        if (birdHere)
        {
            switch (itemController.GetItemState("8Bird") % 2)
            {
                // ... and bird is uncaged...
                case 0:
                    // ... if at steps and jade necklace not yet found...
                    if (location == "14TopOfPit" && !itemController.ItemInPlay("66Necklace"))
                    {
                        // ... bird retrieves jade necklace...
                        itemController.DropItemAt("66Necklace", location);
                        // Tally a treasure
                        itemController.TallyTreasure("66Necklace");
                        waveMsg[0] = "208BirdRetrieveNecklace";
                    }
                    else
                    {
                        // ... otherwise bird just got agitated
                        waveMsg[0] = "206BirdAgitated";
                    }
                    break;
                case 1:
                    // ... bird got agitated in cage
                    waveMsg[0] = "207BirdAgitatedCage";
                    break;
            }

            // If cave is closed, this action will wake the dwarves
            if (gameController.CurrentCaveStatus == CaveStatus.CLOSED)
            {
                outcome = CommandOutcome.DISTURBED;
            }
        }
        // If we're at the fissure and it's not closing
        else if (atFissure && !closing)
        {
            //Toggle crystal bridge state and show appropriate message
            int fissureState = itemController.GetItemState("12Fissure");
            itemController.SetItemState("12Fissure", fissureState + 1);
            textDisplayController.AddTextToLog(itemController.DescribeItem("12Fissure"));
            itemController.SetItemState("12Fissure", 1 - fissureState);
        }

        // Show any messages generated and end this command
        for (int i = 0; i < waveMsg.Length; i++)
        {
            if (waveMsg[i] != null)
            {
                textDisplayController.AddTextToLog(playerMessageController.GetMessage(waveMsg[i]));
            }
        }

        parserState.CommandComplete();
        return outcome;
    }

    // No subsitutes for this command
    public override string FindSubstituteSubject()
    {
        return null;
    }
}
