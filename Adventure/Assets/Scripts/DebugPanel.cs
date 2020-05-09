// Debug Panel
// Handlers for the various fields and buttons on the debug panel

using System;
using UnityEngine;
using UnityEngine.UI;

public class DebugPanel : MonoBehaviour
{
    // === MEMBER VARIABLES ===

    // References to elements of the debug panel
    [SerializeField] private InputField location;
    [SerializeField] private InputField item;
    [SerializeField] private InputField numericValue;
    [SerializeField] private Text message;
    [SerializeField] private Text toggleButtonText;
    [SerializeField] private Text debugLoc;
    [SerializeField] private Text debugCave;
    [SerializeField] private Text debugTurns;
    [SerializeField] private Text debugLamp;
    [SerializeField] private Text debugTreasures;
    [SerializeField] private GameObject lowerPanel;

    // References to other parts of game engine
    [SerializeField] private GameController gameController;
    [SerializeField] private LocationController locationController;
    [SerializeField] private ItemController itemController;
    [SerializeField] private PlayerController playerController;

    private string locationID;  // ID of location entered
    private string itemID;      // Item ID entered
    private int numVal;         // numeric value entered 

    // === PUBLIC METHODS ===

    // Give the player the lamp and turn it on
    public void GetLitLamp()
    {
        ShowMessage("Getting lit lamp", false);
        const string LAMP = "2Lantern";
        itemController.SetItemState(LAMP, 1);
        itemController.DropItemAt(LAMP, "Player");
        gameController.ProcessTurn(CommandOutcome.DESCRIBE);
    }

    // If a valid location ID has been entered, moves the player to that location and shows a confirmatory message
    public void GoToLocation()
    {
        if (ValidLocation())
        {
            ShowMessage("Moving to " + locationID, false);
            playerController.GoTo(locationID, true);
            gameController.ProcessTurn(CommandOutcome.DESCRIBE);
        }
    }

    // If a valid location ID and item ID have been entered, moves the item to that location and shows a confirmatory message
    public void MoveItem()
    {
        if (ValidLocation() && ValidItem())
        {
            ShowMessage("Moving " + itemID + " to " + locationID, false);
            itemController.DropItemAt(itemID, locationID);
        }
    }

    // If a valid item ID and state have been entered, sets the item to that state and shows a confirmatory message
    public void SetItemState()
    {
        if (ValidItem() && ValidNumber(true))
        {
            ShowMessage(itemID + " set to " + numVal, false);
            itemController.SetItemState(itemID, numVal);
        }
    }

    // If a valid number has been entered, set the lamp life
    public void SetLampLife()
    {
        if (ValidNumber(false))
        {
            ShowMessage("Lamp Life set to " + numVal, false);
            gameController.LampLife = numVal;
            UpdateDebugPanel();
        }
    }

    // If a valid number has been entered, set the number of turns
    public void SetTurns()
    {
        if (ValidNumber(false))
        {
            ShowMessage("Turns set to " + numVal, false);
            gameController.Turns = numVal;
            UpdateDebugPanel();
        }
    }

    // Close the cave
    public void CloseCave()
    {
        ShowMessage("Closing Cave", false);
        gameController.StartEndGame();
        UpdateDebugPanel();
    }

    // Starts the cave closing sequence
    public void StartClosing() 
    {
        ShowMessage("Starting Closing Sequence", false);
        gameController.StartClosing();
        UpdateDebugPanel();
    }

    public void ToggleBottomPanel()
    {
        lowerPanel.SetActive(!lowerPanel.activeSelf);

        if (lowerPanel.activeSelf)
        {
            toggleButtonText.text = "Minimise";
        }
        else
        {
            toggleButtonText.text = "Maximise";
        }
    }

    // Updates the debug panel with current information
    public void UpdateDebugPanel()
    {
        debugLoc.text = "Location: " + playerController.CurrentLocation;
        debugCave.text = "Cave Status: " + gameController.CurrentCaveStatus;
        debugTurns.text = "Turns Taken: " + gameController.Turns;
        debugLamp.text = "Lamp Life Remaining: " + gameController.LampLife;
        debugTreasures.text = "Treasures Remaining: " + itemController.TreasuresRemaining;
     }

    // === PRIVATE METHODS ===

    // Clears the status message
    private void ClearMessage()
    {
        message.text = "";
    }

    // Displays status message msg in black or red if isWarning = true
    private void ShowMessage(string msg, bool isWarning)
    {
        message.color = isWarning ? Color.red : Color.black;
        message.text = msg;
    }

    // Returns true if the item ID entered is valid and sets this as the item to use, otherwise shows a warning message and returns false 
    private bool ValidItem()
    {
        string itemStr = item.text.Trim();

        if (itemStr != "" && itemController.ItemExists(itemStr))
        {
            ClearMessage();
            itemID = itemStr;
            return true;
        }
        else
        {
            ShowMessage("Invalid item ID", true);
            return false;
        }
    }

    // Returns true if the number entered is valid and sets this as the value to use, otherwise shows a warning message and returns false
    private bool ValidNumber(bool negAllowed)
    {
        int enteredVal;

        if (Int32.TryParse(numericValue.text.Trim(), out enteredVal) && (negAllowed || enteredVal >=0))
        {
            ClearMessage();
            numVal = enteredVal;
            return true;
        }
        else
        {
            ShowMessage("Invalid number", true);
            return false;
        }
    }

    // Returns true if the location ID entered is valid and sets this as the location to use, otherwise shows a warning message and returns false 
    private bool ValidLocation()
    {
        string locStr = location.text.Trim();

        if (locStr != "" && locationController.LocationExists(locStr))
        {
            ClearMessage();
            locationID = locStr;
            return true;
        }
        else
        {
            ShowMessage("Invalid location ID", true);
            return false;
        }
    }
}
