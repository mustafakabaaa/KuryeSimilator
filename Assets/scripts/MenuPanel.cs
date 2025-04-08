using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour
{
    public GameObject menuPanel; // Inspector'dan atayacaðýnýz panel
    public Button resumeButton; // Devam et butonu
    public Button settingsButton; // Ayarlar butonu
    public Button quitButton; // Çýkýþ butonu

    private void Start()
    {
        // Butonlara týklama event'larýný ekleyin
        resumeButton.onClick.AddListener(ResumeGame);
        quitButton.onClick.AddListener(QuitGame);

        // Panel baþlangýçta kapalý olsun
        menuPanel.SetActive(false);
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
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ResumeGame()
    {
        menuPanel.SetActive(false);
        UIManager.Instance.SetMenuState(false);
        Time.timeScale = 1f;

        StartCoroutine(ForceHideCursor());
    }

    private IEnumerator ForceHideCursor()
    {
        yield return null; // 1 frame bekle

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Input sistemini resetle
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        UnityEngine.Input.ResetInputAxes();
    }

    private void QuitGame()
    {
        // Oyunu kapat (Editor'de çalýþmaz, build'de çalýþýr)
        Application.Quit();

        // Editor için
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}