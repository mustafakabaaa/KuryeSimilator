using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour
{
    public GameObject menuPanel; // Inspector'dan atayacaðýnýz panel
    public Button resumeButton; // Devam et butonu
    public Button settingsButton; // Ayarlar butonu
    public Button quitButton; // Çýkýþ butonu
    public Button saveButton;

    // ESKÝ: public LoadGamePanel loadGamePanel; 
    // YENÝ: SaveGamePanel kullan
    public SaveGamePanel saveGamePanel; // Yeni eklenen save panel referansý

    private CharrController player;

    private void Start()
    {
        // Butonlara týklama event'larýný ekleyin
        resumeButton.onClick.AddListener(ResumeGame);
        quitButton.onClick.AddListener(QuitGame);

        // Panel baþlangýçta kapalý olsun
        menuPanel.SetActive(false);

        player = FindObjectOfType<CharrController>();

        // ESKÝ: saveButton.onClick.AddListener(ShowLoadPanel);
        // YENÝ: SaveGamePanel'i aç
        saveButton.onClick.AddListener(ShowSavePanel);
    }

    // ESKÝ ShowLoadPanel yerine yeni ShowSavePanel
    public void ShowSavePanel()
    {
        UIManager.Instance.CloseAllOpenPanels();
        saveGamePanel.ShowPanel(); // SaveGamePanel'i aç
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (menuPanel.activeSelf)
            {
                // Menü açýksa kapat ve oyunu devam ettir
                ResumeGame();
            }
            else
            {
                // Diðer tüm panelleri kapat
                UIManager.Instance.CloseAllOpenPanels();

                // Persistent menüyü de kapat
                if (PersistentMenuManager.Instance != null)
                {
                    PersistentMenuManager.Instance.CloseAllPanels();
                }

                // Pause menüsünü aç
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