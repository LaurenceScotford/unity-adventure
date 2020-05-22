// Question
// Represents a question that the player can be asked and the actions performed based on outcome 

public class Question 
{
    // === MEMBER VARIABLES ===
    public QuestionController.OnYesResponse yesResponse;
    public QuestionController.OnNoResponse noResponse;

    // === PROPERTIES ===

    public string QuestionMessageID { get; private set; }       // ID of question to ask player
    public string YesMessage { get; private set; }              // ID of message to show following Yes response
    public string NoMessage { get; private set; }               // ID of message to show following No response
    public bool AnyNonYesResponseForNo { get; private set; }    // True if any response other than yes will be accepted as no
    

    // === CONSTRUCTOR ===

    public Question (string questionMessageID, string yesMessage, string noMessage, bool anyNonYesResponseForNo, QuestionController.OnYesResponse yesResponse, QuestionController.OnNoResponse noResponse)
    {
        QuestionMessageID = questionMessageID;
        YesMessage = yesMessage;
        NoMessage = noMessage;
        AnyNonYesResponseForNo = anyNonYesResponseForNo;
        this.yesResponse = yesResponse;
        this.noResponse = noResponse;
    }
}
