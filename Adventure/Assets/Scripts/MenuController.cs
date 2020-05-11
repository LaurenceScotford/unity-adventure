// Menu Controller
// Manages player actions on the menu

using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // === PUBLIC METHODS ===
    
    // Exits the application
    public void ExitGame()
    {
        Application.Quit();
    }


    // Starts a new game
    public void New(int player)
    {
        PlayerPrefs.SetInt("CurrentPlayer", player);
        PlayerPrefs.SetString("CurrentMode", "new");
        SceneManager.LoadScene("Game");
    }
}
