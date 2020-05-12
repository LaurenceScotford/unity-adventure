// Listen
// Excutes the LISTEN command

using System.Collections.Generic;

public class Listen : Action
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    private ActionController controller;
    private ParserState parserState;
    private PlayerController playerController;
    private ItemController itemController;
    private LocationController locationController;
    private CommandsController commandsController;
    private PlayerMessageController playerMessageController;
    private TextDisplayController textDisplayController;

    private const string BIRD = "8Bird";    // Handy reference for the Bird item

    // === CONSTRUCTOR ===

    public Listen(ActionController actionController)
    {
        // Get references to other parts of game engine
        controller = actionController;
        parserState = controller.PS;
        playerController = controller.PC;
        itemController = controller.IC;
        locationController = controller.LC;
        commandsController = controller.CC;
        playerMessageController = controller.PMC;
        textDisplayController = controller.TDC;
    }

    // === PUBLIC METHODS ===

    public override CommandOutcome DoAction()
    {
        // If player tried to supply a subject, force default message 
        if (parserState.GetOtherWordText() != null)
        {
            parserState.CurrentCommandState = CommandState.DISCARDED;
            return CommandOutcome.NO_COMMAND;
        }

        // Create a new list for sounds that can be heard here
        List<string> soundTexts = new List<string>();
        LocationSound locSound = locationController.SoundAtLocation(playerController.CurrentLocation);

        // Add the location sound if there is one
        if (locSound.soundMessage != null && locSound.soundMessage != "")
        {
            soundTexts.Add(locSound.soundMessage);
        }

        // If the location sound doesn't drown out other sounds...
        if (!locSound.drownsOutOtherSounds)
        {
            // Get all the items currently carried or at player's location
            List<string> itemsHere = playerController.PresentItems();

            // Search the items...
            foreach (string item in itemsHere)
            {
                // .. for any that can be heard 
                if (itemController.ItemCanBeHeard(item))
                {
                    string soundText = itemController.ListenToItem(item);

                    // If the bird is singing about its freedom in the forest, we need to add magic word and the bird disappears forever
                    if (item == BIRD && soundText.Contains("~"))
                    {
                        soundText = playerMessageController.AssembleTextWithParams(soundText, new string[] { commandsController.MagicWordText });
                        itemController.DestroyItem(BIRD);
                    }

                    soundTexts.Add(soundText);
                }
            }
        }

        // If there were no sounds, add the silence message
        if (soundTexts.Count == 0)
        {
            soundTexts.Add(playerMessageController.GetMessage("228Silent"));
        }

        // Display all the sounds that can be heard here
        string outString = "";

        for (var i = 0; i < soundTexts.Count; i++)
        {
            if (i > 0)
            {
                outString += "\n";
            }

            outString += soundTexts[i];
        }

        textDisplayController.AddTextToLog(outString);
        parserState.CommandComplete();
        return CommandOutcome.MESSAGE;
    }

    // Not used for this command
    public override string FindSubstituteSubject()
    {
        return null;
    }
}
