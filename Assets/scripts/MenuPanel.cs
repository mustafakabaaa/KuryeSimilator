using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuPanel : MonoBehaviour
{
    private const string UiTable = "UI";
    public GameObject menuPanel; // Inspector'dan atayaca��n�z panel
    public Button resumeButton; // Devam et butonu
    public Button settingsButton; // Ayarlar butonu
    public Button quitButton; // ��k�� butonu
    public Button saveButton;
    public Button backMeninMenu;
    // ESK�: public LoadGamePanel loadGamePanel; 
    // YEN�: SaveGamePanel kullan
    public SaveGamePanel saveGamePanel; // Yeni eklenen save panel referans�

    private CharrController player;

    private void Start()
    {
        // Butonlara t�klama event'lar�n� ekleyin
        resumeButton.onClick.AddListener(ResumeGame);
        quitButton.onClick.AddListener(QuitGame);
        backMeninMenu.onClick.AddListener(BackMeinMenu);
        // Panel ba�lang��ta kapal� olsun
        menuPanel.SetActive(false);

        player = FindObjectOfType<CharrController>();

        // ESK�: saveButton.onClick.AddListener(ShowLoadPanel);
        // YEN�: SaveGamePanel'i a�
        saveButton.onClick.AddListener(ShowSavePanel);
        ApplyLocalization();
    }

    private void OnEnable()
    {
        ApplyLocalization();
    }

    private void ApplyLocalization()
    {
        SetButtonText(resumeButton, "ui.menu_resume");
        SetButtonText(settingsButton, "ui.menu_settings");
        SetButtonText(quitButton, "ui.menu_quit");
        SetButtonText(saveButton, "ui.menu_save");
        SetButtonText(backMeninMenu, "ui.menu_main_menu");
    }

    private void SetButtonText(Button button, string key)
    {
        if (button == null)
        {
            return;
        }

        TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = LocalizationHelper.Localize(UiTable, key);
        }
    }

    // ESK� ShowLoadPanel yerine yeni ShowSavePanel
    public void ShowSavePanel()
    {
        UIManager.Instance.CloseAllOpenPanels();
        saveGamePanel.ShowPanel(); // SaveGamePanel'i a�
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (menuPanel.activeSelf)
            {
                // Men� a��ksa kapat ve oyunu devam ettir
                ResumeGame();
            }
            else
            {
                // Di�er t�m panelleri kapat
                UIManager.Instance.CloseAllOpenPanels();

                // Persistent men�y� de kapat
                if (PersistentMenuManager.Instance != null)
                {
                    PersistentMenuManager.Instance.CloseAllPanels();
                }

                // Pause men�s�n� a�
                PauseGame();
            }
        }
    }

    private void PauseGame()
    {
        menuPanel.SetActive(true);
        UIManager.Instance.SetMenuState(true);
        Time.timeScale = 0f; // Oyunu duraklat
        AudioListener.pause = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void BackMeinMenu()
    {
        SceneManager.LoadScene(0);
    }
    private void ResumeGame()
    {
        menuPanel.SetActive(false);
        UIManager.Instance.SetMenuState(false);
        AudioListener.pause = false;
        Time.timeScale = 1f;
        StartCoroutine(ForceHideCursor());
    }

    private IEnumerator ForceHideCursor()
    {
        yield return null; // 1 frame bekle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        UnityEngine.Input.ResetInputAxes();
    }

    private void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}