// Quit
// Executes the QUIT command

public class Quit : Action
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    ActionController controller;
    ParserState parserState;
    GameController gameController;
    QuestionController questionController;

    // === CONSTRUCTOR ===

    public Quit(ActionController actionController)
    {
        // Get references to other parts of game engine
        controller = actionController;
        parserState = controller.PS;
        gameController = controller.GC;
        questionController = controller.QC;
    }

    public override CommandOutcome DoAction()
    {
        // If player tried to use a second word, force default message
        if (parserState.GetOtherWordText() != null)
        {
            parserState.CurrentCommandState = CommandState.DISCARDED;
            return CommandOutcome.NO_COMMAND;
        }

        // Ask for confirmation before quitting
        parserState.CommandComplete();
        questionController.RequestQuestionResponse("22QuitQuestion", "54OK", "54OK", YesQuit, NoDontQuit);
        return CommandOutcome.QUESTION;
    }

    // Not used for this command
    public override string FindSubstituteSubject()
    {
        return null;
    }

    // Handler for yes response to quit question
    public void YesQuit()
    {
        gameController.EndGame(true);
    }

    // Handler for no response to quit question
    public void NoDontQuit()
    {
        gameController.ProcessTurn(CommandOutcome.DESCRIBE);
    }
}
