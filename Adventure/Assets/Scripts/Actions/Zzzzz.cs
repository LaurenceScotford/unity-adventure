// Zzzzz
// Executes Z'ZZZ command

public class Zzzzz : Action
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    private ActionController controller;
    private ParserState parserState;
    private GameController gameController;
    private PlayerController playerController;
    private ItemController itemController;
    private PlayerMessageController playerMessageController;
    private TextDisplayController textDisplayController;

    private const string RESERVOIR = "45Reservoir"; // Handy reference to reservoir item

    // === CONSTRUCTOR ===

    public Zzzzz(ActionController actionController)
    {
        // Get references to other parts of game engine
        controller = actionController;
        parserState = controller.PS;
        gameController = controller.GC;
        playerController = controller.PC;
        itemController = controller.IC;
        playerMessageController = controller.PMC;
        textDisplayController = controller.TDC;
    } 

    public override CommandOutcome DoAction()
    {
        string location = playerController.CurrentLocation;
        bool inReservoir = location == "168BottomOfReservoir";

        // Only works if player is at either side of, or in, the reservoir
        if (itemController.ItemIsAt(RESERVOIR, location) || inReservoir)
        {
            CommandOutcome outcome = CommandOutcome.MESSAGE;
            int currentReservoirState = itemController.GetItemState(RESERVOIR);
            itemController.SetItemState(RESERVOIR, currentReservoirState + 1);
            string zzzzzText = itemController.DescribeItem(RESERVOIR);
            itemController.SetItemState(RESERVOIR, 1 - currentReservoirState);

            // If player is in reservoir, they've just killed themself
            if (inReservoir)
            {
                zzzzzText += "\n" + playerMessageController.GetMessage("241NotBright");
                playerController.KillPlayer();
                outcome = CommandOutcome.FULL;
            }

            textDisplayController.AddTextToLog(zzzzzText);
            parserState.CommandComplete();
            return outcome;
        }
        else
        {
            // Everywhere else, nothing happens, so force default message
            parserState.CurrentCommandState = CommandState.DISCARDED;
            return CommandOutcome.NO_COMMAND;
        }
    }

    // Not used for this command
    public override string FindSubstituteSubject()
    {
        return null;
    }
}
