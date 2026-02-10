using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using System.Collections;

public class SettingsPanelController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("UI")]
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private Button backButton;

    private const string LanguagePrefKey = "settings.language";

    private void OnEnable()
    {
        StartCoroutine(InitializeLocalizationUI());

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(CloseSettings);
            backButton.onClick.AddListener(CloseSettings);
        }
    }

    public void OpenSettings()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            Debug.Log($"[Settings] Opened panel: {settingsPanel.name} (active={settingsPanel.activeSelf})");
        }
        else
        {
            Debug.LogWarning("[Settings] settingsPanel reference is missing.");
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }

    public void OnLanguageChanged(int index)
    {
        IList<Locale> locales = LocalizationSettings.AvailableLocales.Locales;
        if (index < 0 || index >= locales.Count)
        {
            return;
        }

        Locale selected = locales[index];
        LocalizationSettings.SelectedLocale = selected;
        PlayerPrefs.SetString(LanguagePrefKey, selected.Identifier.Code);
        PlayerPrefs.Save();
    }

    private IEnumerator InitializeLocalizationUI()
    {
        yield return LocalizationSettings.InitializationOperation;
        PopulateLanguageDropdown();
    }

    private void PopulateLanguageDropdown()
    {
        if (languageDropdown == null)
        {
            return;
        }

        IList<Locale> locales = LocalizationSettings.AvailableLocales.Locales;
        List<string> options = new List<string>();
        foreach (Locale locale in locales)
        {
            options.Add(locale.LocaleName);
        }

        languageDropdown.ClearOptions();
        languageDropdown.AddOptions(options);

        int selectedIndex = GetSavedLanguageIndex(locales);
        languageDropdown.SetValueWithoutNotify(selectedIndex);
        LocalizationSettings.SelectedLocale = locales[selectedIndex];

        languageDropdown.onValueChanged.RemoveListener(OnLanguageChanged);
        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    private int GetSavedLanguageIndex(IList<Locale> locales)
    {
        string savedCode = PlayerPrefs.GetString(LanguagePrefKey, string.Empty);
        if (string.IsNullOrEmpty(savedCode))
        {
            return LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);
        }

        for (int i = 0; i < locales.Count; i++)
        {
            if (locales[i].Identifier.Code == savedCode)
            {
                return i;
            }
        }

        return 0;
    }
}
