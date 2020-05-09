// Score
// Executes the SCORE command

public class Score : Action
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    private ActionController controller;
    private ParserState parserState;
    private ScoreController scoreController;

    // === CONSTRUCTOR ===

    public Score(ActionController actionController)
    {
        // Get references to other parts of game engine
        controller = actionController;
        parserState = controller.PS;
        scoreController = controller.SC;

        // Define behaviour for getting a subject
        subjectOptional = true;
    }

    // === PUBLIC METHODS ===

    public override CommandOutcome DoAction()
    {
        // If player tried to supply a subject, force default message
        if (controller.GetSubject("score") != null)
        {
            parserState.CurrentCommandState = CommandState.DISCARDED;
            return CommandOutcome.NO_COMMAND;
        }

        scoreController.DisplayScore(ScoreMode.SCORING);
        parserState.CommandComplete();
        return CommandOutcome.MESSAGE;
    }

    // No substitutes for this command
    public override string FindSubstituteSubject()
    {
        return null;
    }
}
