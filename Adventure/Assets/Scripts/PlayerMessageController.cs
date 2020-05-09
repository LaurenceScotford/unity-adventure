using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMessageController: MonoBehaviour
{
    [SerializeField]private PlayerMessage[] playerMessages;

    // This dictionary holds miscellaneous player messages, all referenced with a short descriptive key
   private Dictionary<string, string> messages = new Dictionary<string, string>();

    // Build dictionary on waking
    private void Awake()
    {
        foreach (PlayerMessage playerMessage in playerMessages)
        {
            messages.Add(playerMessage.MessageID, playerMessage.Message);
        }
    }

    // Returns the appropriate string for the given key
    public string GetMessage(string messageKey)
    {
        if (messages.ContainsKey(messageKey))
        {
            return messages[messageKey];
        }
        else
        {
            Debug.LogErrorFormat("Player message {0} does not exist.", messageKey);
            return null;
        }
    }

    // Returns the appropriate string for the given key, substituting a parameter for each instance of ~
    public string GetMessage(string messageKey, string[] messageParams)
    {
        if (messages.ContainsKey(messageKey))
        {
            return AssembleTextWithParams(messages[messageKey], messageParams);
        }
        else
        {
            Debug.LogErrorFormat("Player message \"{0}\" does not exist.", messageKey);
            return null;
        }
    }

    public string AssembleTextWithParams(string text, string[] textParams)
    {
        string[] textParts = text.Split('~');

        string output = "";

        for (int i = 0; i < textParts.Length; i++)
        {
            output += textParts[i];

            if (i + 1 < textParts.Length || text.LastIndexOf('~') == text.Length - 1)
            {
                if (i < textParams.Length)
                {
                    output += textParams[i];
                }
                else
                {
                    Debug.LogError("Insufficient parameters passed to AssembleTextWithParams.");
                }
            }
        }

        return output;
    }
}
