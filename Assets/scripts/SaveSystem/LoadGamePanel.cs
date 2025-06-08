using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;
using Unity.VisualScripting;

public class LoadGamePanel : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel;
    public Transform saveItemContainer;
    public GameObject saveItemPrefab;
    public Button closeButton;
    public TMP_InputField saveNameInput;
    public Button saveButton;
    public TextMeshProUGUI modeText;

    private bool isSaveMode;

    public void ShowPanel(bool show)
    {
        gameObject.SetActive(show);
        UIManager.Instance.SetLoadGamePanelState(show);
    }



    public void ClosePanel()
    {
        gameObject.SetActive(false);
        UIManager.Instance.SetLoadGamePanelState(false);
    }


    private void RefreshSaveList()
    {
        foreach (Transform child in saveItemContainer)
        {
            Destroy(child.gameObject); // Parantezler kaldýrýldý
        }

        var saves = Directory.GetFiles(Application.persistentDataPath, "*.json")
                          .OrderByDescending(f => File.GetLastWriteTime(f));

        foreach (var savePath in saves)
        {
            var item = Instantiate(saveItemPrefab, saveItemContainer);
            var saveName = Path.GetFileNameWithoutExtension(savePath);

            // Text'leri doldur
            var texts = item.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length > 0) texts[0].text = saveName;
            if (texts.Length > 1) texts[1].text = File.GetLastWriteTime(savePath).ToString("dd.MM.yyyy HH:mm");

            // Buton ayarlarý
            var buttons = item.GetComponentsInChildren<Button>();
            if (buttons.Length > 0)
                buttons[0].onClick.AddListener(() => LoadSave(saveName));
        }
    }

    public void OnSaveButtonClick()
    {
        string saveName = string.IsNullOrWhiteSpace(saveNameInput.text)
            ? $"save_{DateTime.Now:yyyyMMdd_HHmmss}"
            : saveNameInput.text;

        SaveManager.Instance.SaveGame(saveName);
        RefreshSaveList();
    }

    private void LoadSave(string saveName)
    {
        SaveManager.Instance.LoadSpecificSave(saveName);
        ClosePanel();
    }
}