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
            SaveGame(null);
            ResetLastSave();
        }
    }

    // Creates the correct filepath for the current player and either a continuation save/load (filename is null) or player save/load
    public string CreateFilePath(string filename)
    {
        int playerNum = PlayerPrefs.GetInt("CurrentPlayer");
        string fullFileName = filename != null ? filename + ".p" + playerNum : "Player" + playerNum + ".cont";
        return Path.Combine(Application.persistentDataPath, fullFileName);
    }

    // Loads data into a GameData object and returns that object (or null if the loading was not successful), if filename is null, its a continuation resume, if not it's a player save file resume
    public GameData LoadGame(string filename)
    {
        string path = CreateFilePath(filename);
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

    // Saves the game data, if filename is null, its a continuation save if not it's a player save 
    public bool SaveGame(string filename)
    {
        FileStream stream = null;
        bool saveSuccess = true;
        string path = CreateFilePath(filename);

        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            stream = new FileStream(path, FileMode.Create);

            GameData data = new GameData(gameController);

            formatter.Serialize(stream, data);
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
            saveSuccess = false;
        } 
        finally
        {
            if (stream != null)
            {
                stream.Close();
            }
        }

        return saveSuccess;
    }
}
