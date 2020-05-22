
// Question Controller
// Asks the player a question and processes the response appropriately

using System.Collections.Generic;
using UnityEngine;

public class QuestionController : MonoBehaviour
{
    // === MEMBER VARIABLES ===

    // References to other parts of game engine
    [SerializeField] private GameController gameController;
    [SerializeField] private TextDisplayController textDisplayController;
    [SerializeField] private PlayerMessageController playerMessageController;
    [SerializeField] private PlayerInput playerInput;

    private Dictionary<string, Question> questions;

    public delegate void OnYesResponse();
    public delegate void OnNoResponse();

    // === PROPERTIES ===

    public string CurrentQuestion { get; private set; }

    // === MONOBEHAVIOUR METHODS ===
    private void Awake()
    {
        questions = new Dictionary<string, Question>();
        CurrentQuestion = null;
    }

    // === PUBLIC METHODS ===

    // Add a new question to the controller
    public void AddQuestion(string questionID, Question question)
    {
        if (!questions.ContainsKey(questionID))
        {
            questions.Add(questionID, question);
        }
    }

    // Process the response to a question
    public void GetQuestionResponse()
    {
        string response = playerInput.Words[0].activeWord.ToUpper();

        string questionID = CurrentQuestion; 

        // If the player wants instructions, show them and give them a more generous battery life for the lamp
        if (response == "Y" || response == "YES")
        {
            if (questions[questionID].YesMessage != null)
            {
                textDisplayController.AddTextToLog(playerMessageController.GetMessage(questions[CurrentQuestion].YesMessage));
            }

            ResetParams();
            questions[questionID].yesResponse?.Invoke();
            gameController.ResumeCommandProcessing();
        }
        // Otherwise give them the default battery life for the lamp
        else if (questions[questionID].AnyNonYesResponseForNo || response == "N" || response == "NO")
        {
            if (questions[questionID].NoMessage != null)
            {
                textDisplayController.AddTextToLog(playerMessageController.GetMessage(questions[CurrentQuestion].NoMessage));
            }

            ResetParams();
            questions[questionID].noResponse?.Invoke();
            gameController.ResumeCommandProcessing();
        }
        // Learner has responded with something other than yes or no answer so prompt them and continue waiting
        else
        {
            textDisplayController.AddTextToLog(playerMessageController.GetMessage("185Answer"));
        }
    }

    // Creates a new question and responses and asks the question
    public void RequestQuestionResponse(string questionID)
    {
        if (questions.ContainsKey(questionID))
        {
            CurrentQuestion = questionID;
            textDisplayController.AddTextToLog(playerMessageController.GetMessage(questions[questionID].QuestionMessageID));

            SetUpQuestion();
        }
        else
        {
            Debug.LogErrorFormat("QuestionController.RequestQuestionResponse was passed a non-existant question ID: {0}", questionID);
        }
    }

    // Restores a question from saved game data
    public void Restore(GameData gameData)
    {
        CurrentQuestion = gameData.currentQuestion;

        SetUpQuestion();
    }

    // === PRIVATE METHODS ===

    // Sets the paramaters for the current question
    private void SetUpQuestion()
    {
        // Change placeholder, unless this question will accept any non-yes response as negative
        if (!questions[CurrentQuestion].AnyNonYesResponseForNo)
        {
            playerInput.Placeholder = "Enter a Yes or No response ...";
        }
        
        gameController.SuspendCommandProcessing();
        PlayerInput.commandsEntered = GetQuestionResponse;
    }

    // Resumes normal command processing after the response to a question has been received
    private void ResetParams()
    {
        CurrentQuestion = null;
        PlayerInput.commandsEntered = null;
    }
}
