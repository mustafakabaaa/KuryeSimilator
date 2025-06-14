using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class MainMenuLoadGameUIController : MonoBehaviour
{
    public GameObject saveButtonPrefab;
    public Transform contentParent;
    public GameObject loadGamePanel;

    void OnEnable()
    {
        PopulateSaveList();
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
            buttonObj.GetComponentInChildren<Text>().text = saveFile;

            buttonObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                LoadSaveAndStartGame(saveFile);
            });
        }
    }

    private void LoadSaveAndStartGame(string saveName)
    {
        SaveManager.Instance.LoadSpecificSave(saveName);
        SceneLoader.Load(SceneList.GameScene); 
    }

    public void CloseLoadPanel()
    {
        loadGamePanel.SetActive(false);
    }
}
