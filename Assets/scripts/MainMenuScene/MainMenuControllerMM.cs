using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuControllerMM : MonoBehaviour
{
    public SaveGamePanel saveGamePanel; // Inspector'dan referans ver

    public GameObject mainMenuPanel;
    public GameObject loadGamePanel;

    public void OnContinuePressed()
    {
        SaveManager.Instance.LoadGame();
        Destroy(SaveManager.Instance.gameObject);

        SceneLoader.Load(SceneList.GameScene);
    }

    public void OnNewGamePressed()
    {
        SaveManager.Instance.ResetGameData();
        Destroy(SaveManager.Instance.gameObject);

        SceneLoader.Load(SceneList.GameScene);
    }

    public void OnLoadGamePressed()
    {
        mainMenuPanel.SetActive(false);

        loadGamePanel.SetActive(true);
        saveGamePanel.ShowPanel();
    }

    public void OnBackPressed()
    {
        loadGamePanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void OnQuitPressed()
    {
        Application.Quit();
    }
}
