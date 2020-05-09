using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Adventure/PlayerMessage")]

public class PlayerMessage : ScriptableObject
{
    [SerializeField] private string messageID;
    [SerializeField, TextArea(minLines: 3, maxLines: 10)] private string message; 

    public string Message { get { return message; } }
    public string MessageID { get { return messageID; } }
}
