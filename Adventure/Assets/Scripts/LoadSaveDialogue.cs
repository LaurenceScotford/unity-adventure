// Load Save Dialogue
// Manages the dialogue to load and save games

using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadSaveDialogue : MonoBehaviour
{
    // === MEMBER VARIABLES ===

    [SerializeField] private Text heading;
    [SerializeField] private Text loadSaveButton;
    [SerializeField] private Text placeholder;
    [SerializeField] private InputField input;
    [SerializeField] private Text prompt;
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private GameObject contentArea;

    private int player;
    private string[] fileList;
    private string loadSaveMode;
    private Animation anim;

    // === MONOBEHAVIOUR METHODS ===

    // Start is called before the first frame update
    private void Start()
    {
        anim = prompt.GetComponentInChildren<Animation>();
        prompt.text = "";
        player = PlayerPrefs.GetInt("CurrentPlayer");
        loadSaveMode = (PlayerPrefs.GetString("LoadSaveMode"));

        heading.text = "Player " + player + " Saved Games";

        if (loadSaveMode == "load")
        {
            loadSaveButton.text = "LOAD";
            placeholder.text = "Enter the name of a file to load or select one from the list ...";
        }
        else
        {
            loadSaveButton.text = "SAVE";
            placeholder.text = "Enter a filename (letters, numbers, _ and - are allowed and filenames must start with a letter) ...";
        }

        string loadPath = "*.p" + player;
        fileList = Directory.GetFiles(Application.persistentDataPath, loadPath);

        for (int i = 0; i < fileList.Length; i++)
        {
            Button newButton = Instantiate(buttonPrefab) as Button;
            newButton.transform.SetParent(contentArea.transform);
            Text buttonText = newButton.GetComponentInChildren(typeof(Text)) as Text;
            string fname = Path.GetFileName(fileList[i]).Split('.')[0];
            buttonText.text = fname;
            int fileIndex = i;
            newButton.onClick.AddListener(delegate { FileSelected(fname); });
        }
    }

    // === PUBLIC METHODS ===

    // Cancel operation and return to originating scene
    public void Cancel()
    {
        string originatingScene = PlayerPrefs.GetString("OriginatingScene");
        SetUpPrefs(false);
        SceneManager.LoadScene(originatingScene);
    }

    // Save / load action
    public void ConfirmSaveLoad()
    {
        if (CheckFileName() && (loadSaveMode == "save" || CheckFileExists()))
        {
            SetUpPrefs(true);
            SceneManager.LoadScene("Game");
        }
    }

    // Puts the selected filename into the input field
    public void FileSelected(string filename)
    {
        input.text = filename;
    }

    // === PRIVATE METHODS ===

    private bool CheckFileExists()
        
    {
        if (File.Exists(Path.Combine(Application.persistentDataPath, input.text + ".p" + PlayerPrefs.GetInt("CurrentPlayer"))))
        {
            return true;
        }

        SetPrompt("That save file doesn't exist! Please check the filename and try again ...");
        return false;
    }

    private bool CheckFileName()
    {
        Regex re = new Regex(@"^[a-z][a-z|0-9|\-|_]*$", RegexOptions.IgnoreCase);
        
        if (re.IsMatch(input.text))
        {
            return true;
        }

        SetPrompt("The filename must start with a letter and contain only letters, numbers, - or _");
        return false;
    }

    private void SetPrompt(string message)
    {
        prompt.text = message;
        anim.Play();
    }

    // Sey up player prefs before changing scene
    private void SetUpPrefs(bool success)
    {
        if (!success && PlayerPrefs.GetString("OriginatingScene") == "Menu")
        {
            PlayerPrefs.DeleteKey("CurrentPlayer");
        }
        else if (success)
        {
            PlayerPrefs.SetString("CurrentFile", input.text);
            PlayerPrefs.SetString("CurrentMode", loadSaveMode);
        }
        
        PlayerPrefs.DeleteKey("OriginatingScene");
        PlayerPrefs.DeleteKey("LoadSaveMode");
    }
}


