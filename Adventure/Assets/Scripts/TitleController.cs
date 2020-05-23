
// Title Controller
// Loads the menu when the player clicks a key or button

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleController : MonoBehaviour
{
    // === MEMBER VARIABLES ===

    [SerializeField] private GameObject loading;
    [SerializeField] private GameObject prompt;
    private Text loadText;
    private bool menuLoaded;

    // === MONOBEHAVIOUR METHODS ===

    private void Start()
    {
        loadText = loading.GetComponent<Text>();
        menuLoaded = false;
        StartCoroutine(LoadMenu());
    }

    // === COROUTINES ===

    IEnumerator LoadMenu()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Menu");
        asyncLoad.allowSceneActivation = false;

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            string loadMsg = "Loading";

            for (int i = 0; i < asyncLoad.progress * 10; i++)
            {
                loadMsg += " .";
            }

            loadText.text = loadMsg;

            if (asyncLoad.progress >= 0.9f)
            {
                if (!menuLoaded)
                {
                    loading.SetActive(false);
                    prompt.SetActive(true);
                    menuLoaded = true;
                }
                else if (Input.anyKey)
                {
                    asyncLoad.allowSceneActivation = true;
                }
            }

            yield return null;
        }
    }
}
