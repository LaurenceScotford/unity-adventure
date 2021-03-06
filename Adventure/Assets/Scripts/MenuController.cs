﻿// Menu Controller
// Manages player actions on the menu

using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    // === MEMBER VARIABLES ===
    [SerializeField] private GameObject warningDialogue;
    [SerializeField] private Text warningText;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Text loadText;
    [SerializeField] private Text[] playerTexts;
    [SerializeField] private Button[] continueButtons;
    [SerializeField] private Button[] newButtons;
    [SerializeField] private Button[] loadButtons;

    // === MONOBEHAVIOUR METHODS ===

    private void Start()
    {
        // Update the player texts to reflect the current status of each player
        for (int i = 0; i < playerTexts.Length; i++)
        {
            int player = i + 1;
            string contPath = Path.Combine(Application.persistentDataPath, "Player" + player + ".cont");
            string textOut = PlayerCanContinue(player, false) ? PlayerPrefs.GetString("Player" + player + "Status") : "No active game";
            playerTexts[i].text = "Player " + player + ": " + textOut;
        }

        EnableButtons();
    }

    // === PUBLIC METHODS ===

    // Close the warning panel
    public void CloseWarningPanel()
    {
        PlayerPrefs.DeleteKey("CurrentMode");
        PlayerPrefs.DeleteKey("CurrentPlayer");
        warningDialogue.SetActive(false);
        EnableButtons();
    }

    // Continue a game
    public void ContinueGame(int player)
    {
        PlayerPrefs.SetString("CurrentMode", "continue");
        PlayerPrefs.SetInt("CurrentPlayer", player);
        StartGame();
    }

    // Exits the application
    public void ExitGame()
    {
        Application.Quit();
    }

    // Opens the load game dialogue
    public void LoadGame(int player)
    {
        PlayerPrefs.SetString("CurrentMode", "load");
        PlayerPrefs.SetInt("CurrentPlayer", player);
        PlayerPrefs.SetString("OriginatingScene", "Menu");

        // Show warning if there's an existing game in progress
        if (PlayerCanContinue(player, true))
        {
            ShowWarningPanel();
        }
        else
        {
            PlayerPrefs.SetString("OriginatingScene", "Menu");
            StartCoroutine(GoToScene("LoadSaveGame"));
        }
    }

    // Start a new game
    public void NewGame(int player)
    {
        PlayerPrefs.SetString("CurrentMode", "new");
        PlayerPrefs.SetInt("CurrentPlayer", player);

        // Show warning if there's an existing game in progress
        if (PlayerCanContinue(player, true))
        {
            ShowWarningPanel();
        }
        else
        {
           StartGame();
        }
    }

    // Start a game
    public void StartGame()
    {
        StartCoroutine(GoToScene("Game"));
    }

    // Called when the player confirms the choice on the warning panel
    public void WarningPanelConfirm()
    {
        if (PlayerPrefs.GetString("CurrentMode") == "new")
        {
            StartGame();
        }
        else
        {
            PlayerPrefs.SetString("OriginatingScene", "Menu");
            StartCoroutine(GoToScene("LoadSaveGame"));
        }
    }

    // === PRIVATE METHODS ===

    // Disable the buttons
    private void DisableButtons()
    {
        foreach (Button button in continueButtons)
        {
            button.interactable = false;
        }

        foreach (Button button in newButtons)
        {
            button.interactable = false;
        }

        foreach (Button button in loadButtons)
        {
            button.interactable = false;
        }
    }

    // Enable the buttons
    private void EnableButtons()
    {
        for (int i = 0; i < playerTexts.Length; i++)
        {
            int player = i + 1;
            string loadPath = "*.p" + player;
            continueButtons[i].interactable = PlayerCanContinue(player, false);
            newButtons[i].interactable = true;
            loadButtons[i].interactable = Directory.GetFiles(Application.persistentDataPath, loadPath).Length > 0;
        }
    }

    // Returns true if the player has a continuation game, in play or ended (if notEnded is false) or true only if there is a continuation game that is not ended (if notEnded is true)
    private bool PlayerCanContinue(int player, bool notEnded)
    {
        string contPath = Path.Combine(Application.persistentDataPath, "Player" + player + ".cont");
        return PlayerPrefs.HasKey("Player" + player + "Status") && File.Exists(contPath) && !(notEnded && PlayerPrefs.GetString("Player" + player + "Status").Contains("Ended"));
    }

    // Shows a warning panel with a message based on the current operation
    private void ShowWarningPanel()
    {
        DisableButtons();
        string insertText = PlayerPrefs.GetString("CurrentMode") == "new" ? "start a new game" : "load a game";
        warningText.text = "If you " + insertText + " it will replace your current game.\nDo you want to continue?";
        warningDialogue.SetActive(true);
    }

    // === COROUTINES ===

    private IEnumerator GoToScene(string sceneName)
    {
        menuPanel.SetActive(false);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            string loadMsg = "Loading";

            for (int i = 0; i < asyncLoad.progress * 10; i++)
            {
                loadMsg += " .";
            }

            loadText.text = loadMsg;

            yield return null;
        }
    }
}
