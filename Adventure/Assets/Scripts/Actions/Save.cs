// Save
// Executes SAVE command

using UnityEngine;
using UnityEngine.SceneManagement;

public class Save : Action
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    private ActionController controller;
    private ParserState parserState;
    private ScoreController scoreController;
    private GameController gameController;
    private QuestionController questionController;
    private TextDisplayController textDisplayController;
    private PlayerMessageController playerMessageController;

    // === CONSTRUCTOR ===

    public Save(ActionController actionController)
    {
        // Get references to other parts of game engine
        controller = actionController;
        parserState = controller.PS;
        gameController = controller.GC;
        questionController = controller.QC;
        scoreController = controller.SC;
        textDisplayController = controller.TDC;
        playerMessageController = controller.PMC;

        // Define behaviour for getting a subject
        subjectOptional = true;

        // Add question used by this action
        questionController.AddQuestion("save", new Question("200Acceptable", "54OK", "54OK", false, YesSave, NoDontSave));
    }

    // === PUBLIC METHODS ===

    public override CommandOutcome DoAction()
    {
        // If player tried to supply a subject, force default message
        if (controller.GetSubject("save") != null)
        {
            parserState.CurrentCommandState = CommandState.DISCARDED;
            return CommandOutcome.NO_COMMAND;
        }

        textDisplayController.AddTextToLog(playerMessageController.GetMessage("260Suspend"));
        parserState.CommandComplete();
        questionController.RequestQuestionResponse("save");
        return CommandOutcome.QUESTION;
    }

    // No substitutes for this command
    public override string FindSubstituteSubject()
    {
        return null;
    }

    // Handler for the yes reponse to save question - Makes a continuation save first, so player can continue current game after the save completes
    public void YesSave()
    {
        if (gameController.ContinuationSave())
        {
            PlayerPrefs.SetString("OriginatingScene", "Game");
            PlayerPrefs.SetString("LoadSaveMode", "save");
            SceneManager.LoadScene("LoadSaveGame");
        }
    }

    // === Handler for the no response to save question ===
    public void NoDontSave()
    {
        gameController.ProcessTurn(CommandOutcome.DESCRIBE);
    }
}
