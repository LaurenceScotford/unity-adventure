// Blast
// Handles BLAST command

public class Blast : Action
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    private ActionController controller;
    private ParserState parserState;
    private GameController gameController;
    private PlayerController playerController;
    private ItemController itemController;
    private ScoreController scoreController;
    private TextDisplayController textDisplayController;
    private PlayerMessageController playerMessageController;

    // === CONSTRUCTOR ===

    public Blast (ActionController actionController)
    {
        // Get references to other parts of game engine
        controller = actionController;
        parserState = controller.PS;
        gameController = controller.GC;
        playerController = controller.PC;
        itemController = controller.IC;
        scoreController = controller.SC;
        textDisplayController = controller.TDC;
        playerMessageController = controller.PMC;
    }

    // === PUBLIC METHODS ===

    public override CommandOutcome DoAction()
    {
        // If the phony rod hasn't been picked up yet or the cave is not yet closed, force default message
        if (gameController.CurrentCaveStatus != CaveStatus.CLOSED || itemController.GetItemState("6BlackRod") < 0)
        {
            parserState.CurrentCommandState = CommandState.DISCARDED;
            return CommandOutcome.NO_COMMAND;
        }

        string blastMsg;
        int bonusPoints;

        if (playerController.ItemIsPresent("6BlackRod"))
        {
            bonusPoints = 25;
            blastMsg = "135HoistedPetard";
        }
        else if (playerController.CurrentLocation == "115RepositoryNE")
        {
            bonusPoints = 30;
            blastMsg = "134ExplodeLava";
        }
        else
        {
            bonusPoints = 45;
            blastMsg = "133EndWithBang";
        }

        scoreController.AddBonusPoints(bonusPoints);
        textDisplayController.AddTextToLog(playerMessageController.GetMessage(blastMsg));
        parserState.CommandComplete();
        return CommandOutcome.ENDED;
    }

    // Not used for this command
    public override string FindSubstituteSubject()
    {
        return null;
    }
}
