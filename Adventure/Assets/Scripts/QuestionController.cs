using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;

public class QuestionController : MonoBehaviour
{
    [SerializeField] private GameController gameController;
    [SerializeField] private TextDisplayController textDisplayController;
    [SerializeField] private PlayerMessageController playerMessageController;
    [SerializeField] private PlayerInput playerInput;
    private string yesMessage;
    private string noMessage;

    public delegate void OnYesResponse ();
    public OnYesResponse yesResponse;
    public delegate void OnNoResponse ();
    public OnNoResponse noResponse;

    public void RequestQuestionResponse(string questionID, string yesMsg, string noMsg,  OnYesResponse yesMethod, OnNoResponse noMethod)
    {
        yesMessage = yesMsg;
        noMessage = noMsg;
        yesResponse = yesMethod;
        noResponse = noMethod;

        textDisplayController.AddTextToLog(playerMessageController.GetMessage(questionID));

        gameController.SuspendCommandProcessing();
        PlayerInput.commandsEntered = GetQuestionResponse;
    }

    public void GetQuestionResponse()
    {
        string response = playerInput.Words[0].activeWord.ToUpper();

        // If the player wants instructions, show them and give them a more generous battery life for the lamp
        if (response == "Y" || response == "YES")
        {
            ResumeCommandProcessing();

            if (yesMessage != null)
            {
                textDisplayController.AddTextToLog(playerMessageController.GetMessage(yesMessage));
            }

            yesResponse?.Invoke();
        }
        // Otherwise give them the default battery life for the lamp
        else if (response == "N" || response == "NO")
        {
            ResumeCommandProcessing();

            if (noMessage != null)
            {
                textDisplayController.AddTextToLog(playerMessageController.GetMessage(noMessage));
            }

            noResponse?.Invoke();
        }
        // Learner has responded with something other than yes or no answer so prompt them and continue waiting
        else
        {
            textDisplayController.AddTextToLog(playerMessageController.GetMessage("185Answer"));
        }
    }

    private void ResumeCommandProcessing()
    {
        PlayerInput.commandsEntered = null;
        gameController.ResumeCommandProcessing();
    }
}
