using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MainMenuLoadGameUIController : MonoBehaviour
{
    public GameObject saveButtonPrefab;
    public Transform contentParent;
    public GameObject loadGamePanel;
    public TextMeshProUGUI selectedSaveText;

    void OnEnable()
    {
        PopulateSaveList();
        UpdateSelectedSaveLabel(SaveManager.LastLoadedSaveName);
    }

    private void PopulateSaveList()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        List<string> saveFiles = SaveManager.Instance.GetAllSaveFiles();

        foreach (string saveFile in saveFiles)
        {
            GameObject buttonObj = Instantiate(saveButtonPrefab, contentParent);
            SetButtonLabel(buttonObj, saveFile);
            string saveFileCopy = saveFile;

            buttonObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                UpdateSelectedSaveLabel(saveFileCopy);
                LoadSaveAndStartGame(saveFileCopy);
            });
        }
    }

    private void LoadSaveAndStartGame(string saveName)
    {
        SaveManager.Instance.SetPendingLoad(saveName);
        Debug.Log($"[MainMenu] Selected save to load: {saveName}");
        SceneLoader.Load(SceneList.GameScene); 
    }

    public void CloseLoadPanel()
    {
        loadGamePanel.SetActive(false);
    }

    private void UpdateSelectedSaveLabel(string saveName)
    {
        if (selectedSaveText == null)
        {
            return;
        }

        selectedSaveText.text = string.IsNullOrEmpty(saveName) ? "-" : saveName;
    }

    private void SetButtonLabel(GameObject buttonObj, string saveName)
    {
        TextMeshProUGUI tmp = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = saveName;
            return;
        }

        Text legacyText = buttonObj.GetComponentInChildren<Text>();
        if (legacyText != null)
        {
            legacyText.text = saveName;
        }
    }
}
