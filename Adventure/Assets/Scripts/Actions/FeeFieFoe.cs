// Fee Fie Foe
// Excutes the FEE FIE FOE FOO FUM commands

public class FeeFieFoe : Action
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

    // Word order - note FUM is picked up by the command but is not in word order, as saying "FUM" will break the spell
    private readonly string[] wordOrder = new string[] { "FEE", "FIE", "FOE", "FOO" };

    // === CONSTRUCTOR ===
    public FeeFieFoe(ActionController actionController)
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
        // If player tried to use a second word, force default message
        string[] otherword = parserState.GetOtherWordText();

        if (otherword != null && otherword[0].ToUpper() != "SAY")
        {
            parserState.CurrentCommandState = CommandState.DISCARDED;
            return CommandOutcome.NO_COMMAND;
        }

        int nextWord = controller.Foobar;

        // Assume that nothing will happen
        string fooText = playerMessageController.GetMessage("42NothingHappens");

        // Player has used the next word in the sequence
        if (parserState.Words[0].ToUpper() == wordOrder[nextWord])
        {
            // Advance to the next word
            controller.IncrementFoobar();

            // If we're done...
            if (controller.Foobar == wordOrder.Length)
            {
                string eggs = "56Eggs";
                string troll = "33Troll";

                controller.ResetFoobar();

                bool playerAtInitialEggLocation = itemController.IsInitialLocation(eggs, playerController.CurrentLocation, LOCATION_POSITION.FIRST_LOCATION);

                // If the eggs are currently in their initial location or the player is carrying them, but is currently at their initial location, then nothing happens
                if (!itemController.AtInitalLocation(eggs) && !(playerController.HasItem(eggs) && playerAtInitialEggLocation))
                {
                    // Bring back the troll if we're trying to steal back the eggs after using them to pay for a crossing
                    if (!itemController.ItemInPlay(eggs) && !itemController.ItemInPlay(troll) && itemController.GetItemState(troll) == 0)
                    {
                        itemController.SetItemState(troll, 1);
                    }

                    // Transport the eggs back to the giant room
                    int eggMsgState;

                    if (playerAtInitialEggLocation)
                    {
                        eggMsgState = 0;
                    }
                    else if (playerController.ItemIsPresent(eggs))
                    {
                        eggMsgState = 1;
                    }
                    else
                    {
                        eggMsgState = 2;
                    }

                    itemController.ResetItem(eggs);
                    itemController.SetItemState(eggs, eggMsgState);
                    fooText = itemController.DescribeItem(eggs);
                    itemController.SetItemState(eggs, 0);
                }
            }
            else
            {
                // Player has correctly said the next word in the sequence, but we're not at the end yet...
                fooText = playerMessageController.GetMessage("54OK");
            }
        }
        else
        {
            // Player has said a word out of sequence. If mid-sequence, we need to reset and should give the player a hint.
            if (controller.Foobar != 0)
            {
                fooText = playerMessageController.GetMessage("151StartOver");
                controller.ResetFoobar();
            }
        }

        textDisplayController.AddTextToLog(fooText);
        parserState.CommandComplete();
        return CommandOutcome.MESSAGE;
    }

    // Not used for this command
    public override string FindSubstituteSubject()
    {
        return null;
    }
}
