// Resume
// Executes RESUME command

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Resume : Action
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    private ParserState parserState;
    private ActionController controller;
    private GameController gameController;
    private LocationController locationController;
    private QuestionController questionController;
    private TextDisplayController textDisplayController;
    private PlayerMessageController playerMessageController;

    // === CONSTRUCTOR ===

    public Resume(ActionController actionController)
    {
        // Get references to other parts of game engine
        controller = actionController;
        parserState = controller.PS;
        gameController = controller.GC;
        locationController = controller.LC;
        questionController = controller.QC;
        textDisplayController = controller.TDC;
        playerMessageController = controller.PMC;

        // Define behaviour for getting a subject
        subjectOptional = true;

        // Create question used by this action
        questionController.AddQuestion("resume", new Question("200Acceptable", "54OK", "54OK", false, YesResume, NoDontResume));
    }

    // === PUBLIC METHODS ===

    public override CommandOutcome DoAction()
    {
        // If player tried to supply a subject, force default message
        if (controller.GetSubject("resume") != null)
        {
            parserState.CurrentCommandState = CommandState.DISCARDED;
            return CommandOutcome.NO_COMMAND;
        }

        CommandOutcome outcome = CommandOutcome.MESSAGE;
        parserState.CommandComplete();

        // If the player has been anywhere other than the first location, ask for confirmation before resuming
        if (locationController.MovedBeyondStart)
        {
            textDisplayController.AddTextToLog(playerMessageController.GetMessage("268ResumeInstruction"));
            questionController.RequestQuestionResponse("resume");
            outcome = CommandOutcome.QUESTION;
        }
        else
        {
            // Otherwise just resume a saved game
            YesResume();
        }

        return outcome;
    }

    // No substitutes for this command
    public override string FindSubstituteSubject()
    {
        return null;
    }

    // Handler for yes reponse to resume question - Makes a continuation save first, in case player cancels and wants to resume current game
    public void YesResume()
    {
        gameController.OpenLoadSaveDialogue("load");
    }

    // Handler for no response to resume question
    public void NoDontResume()
    {
        if (gameController.CurrentGameStatus == GameStatus.PLAYING)
        {
            gameController.ProcessTurn(CommandOutcome.DESCRIBE);
        }
        else
        {
            gameController.ResumeCommandProcessing();
        }
    }
}
