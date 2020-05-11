// Persistence Controller
// Manages the saving and loading of games and continue states

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class PersistenceController : MonoBehaviour
{
    // === MEMBER VARIABLES ===
    [SerializeField] private GameController gameController;

    private int lastSaveTurnCount;      // Number of turns that had elapsed when the last continuation save took place
    private DateTime lastSaveTimeStamp; // Time at which last continuation save took place

    private const int TIME_THRESHOLD = 15;
    private const int TURNS_THRESHOLD = 25;

    // === PUBLIC METHODS ===

    // Checks if we're ready for a new continuation save and saves if we are
    public void CheckContinuationSave()
    {
        TimeSpan elapsedTime = DateTime.Now - lastSaveTimeStamp;
        bool timeThreshold = elapsedTime.TotalMinutes >= TIME_THRESHOLD;
        int elapsedTurns = gameController.Turns - lastSaveTurnCount;
        bool turnsThreshold = elapsedTurns >= TURNS_THRESHOLD;

        // If the required number of turns have taken place or the required time has passed (with at least one turn happening during that time), make a continuation save
        if (turnsThreshold || (timeThreshold && elapsedTurns >0))
        {
            SaveGame(false);
            ResetLastSave();
        }
    }

    // Saves the game data, playerSave = true if it's a player initiated save and we need a filename, or false if its a continuation save
    public bool SaveGame(bool playerSave)
    {
        FileStream stream = null;
        bool saveSuccess = true;
        int playerNum = PlayerPrefs.GetInt("CurrentPlayer");

        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            string path = Path.Combine(Application.persistentDataPath, "Player" + playerNum + ".cont");
            stream = new FileStream(path, FileMode.Create);

            GameData data = new GameData(gameController);

            formatter.Serialize(stream, data);
        }
        catch
        {
            saveSuccess = false;
        } 
        finally
        {
            stream.Close();
        }

        return saveSuccess;
    }

    // Loads data into a GameData object and returns that object (or null if the loading was not successful), playerResume if it's a player initiated resume and we need a filename, or false if it's a continuation resume
    public GameData LoadGame(bool playerResume)
    {
        int playerNum = PlayerPrefs.GetInt("CurrentPlayer");
        string path = Path.Combine(Application.persistentDataPath, "Player" + playerNum + ".cont");
        GameData data = null;
        FileStream stream = null;

        if (File.Exists(path))
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                stream = new FileStream(path, FileMode.Open);
                data = formatter.Deserialize(stream) as GameData;
            }
            catch
            {
                data = null;
            }
            finally
            {
                stream.Close();
            }
        }

        return data;
    }

    // Resets the last save values
    public void ResetLastSave()
    {
        lastSaveTurnCount = gameController.Turns;
        lastSaveTimeStamp = DateTime.Now;
    }
}
