// Calm
// Handles the CALM command

public class Calm : Action
{

    // === MEMBER VARIABLES ===

    // Reference to other parts of game engine 
    private ActionController controller;
    private ParserState parserState; 

    // === CONSTRUCTOR ===

    public Calm (ActionController actionController)
    {
        // Get references to other parts of game engine
        controller = actionController;
        parserState = controller.PS;

        // Define behaviour for getting a subject
        carryOverVerb = true;
        subjectOptional = false;
    }

    // === PUBLIC METHODS ===

    public override CommandOutcome DoAction()
    {
        // Check whether a subject was identified, or could be identified
        noSubjectMsg = new NoSubjectMsg("257VerbWhat", parserState.Words);
        string itemToCalm = controller.GetSubject("calm");

        // If a subject was provided, force the default message to be shown
        if (itemToCalm != null)
        {
            parserState.CurrentCommandState = CommandState.DISCARDED;
        }

        return CommandOutcome.NO_COMMAND;
    }

    // No substitutes for this command
    public override string FindSubstituteSubject()
    {
        return null;
    }
}
