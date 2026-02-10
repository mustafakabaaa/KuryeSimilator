using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Localization.Settings;

public class MainMenuControllerMM : MonoBehaviour
{
    private const string UiTable = "UI";

    public SaveGamePanel saveGamePanel; // Inspector'dan referans ver

    public GameObject mainMenuPanel;
    public GameObject loadGamePanel;
    public SettingsPanelController settingsPanelController;

    [Header("Main Menu Texts")]
    [SerializeField] private TextMeshProUGUI newGameText;
    [SerializeField] private TextMeshProUGUI continueText;
    [SerializeField] private TextMeshProUGUI loadGameText;
    [SerializeField] private TextMeshProUGUI settingsText;
    [SerializeField] private TextMeshProUGUI quitText;
    [SerializeField] private TextMeshProUGUI backText;

    public void OnContinuePressed()
    {
        string lastSave = SaveManager.Instance.GetLastSaveFileNameSafe();
        if (!string.IsNullOrEmpty(lastSave))
        {
            SaveManager.Instance.SetPendingLoad(lastSave);
            Debug.Log($"[MainMenu] Continue -> loading save: {lastSave}");
        }
        SceneLoader.Load(SceneList.GameScene);
    }

    public void OnNewGamePressed()
    {
        SaveManager.Instance.SetPendingNewGame();
        SceneLoader.Load(SceneList.GameScene);
    }

    public void OnLoadGamePressed()
    {
        mainMenuPanel.SetActive(false);

        loadGamePanel.SetActive(true);
        saveGamePanel.ShowPanel();
    }

    public void OnSettingsPressed()
    {
        if (settingsPanelController != null)
        {
            settingsPanelController.OpenSettings();
        }
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

    private void OnEnable()
    {
        ApplyLocalization();
        LocalizationSettings.SelectedLocaleChanged += HandleLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChanged;
    }

    private void HandleLocaleChanged(UnityEngine.Localization.Locale _)
    {
        ApplyLocalization();
    }

    private void ApplyLocalization()
    {
        SetText(newGameText, "ui.main_new_game");
        SetText(continueText, "ui.main_continue");
        SetText(loadGameText, "ui.main_load_game");
        SetText(settingsText, "ui.main_settings");
        SetText(quitText, "ui.main_quit");
        SetText(backText, "ui.main_back");
    }

    private void SetText(TextMeshProUGUI text, string key)
    {
        if (text == null)
        {
            return;
        }

        text.text = LocalizationHelper.Localize(UiTable, key);
    }
}
