using UnityEngine;

using UnityEngine.UI;

public class PCUIController : MonoBehaviour
{
    [SerializeField] private GameObject _pcUI; // PC UI paneli (Canvas veya baþka bir GameObject)
    [SerializeField] private Button _closeButton; // Inspector'dan atayýn

    private void Start()
    {
        _pcUI.SetActive(false);
        _closeButton.onClick.AddListener(ClosePCUI);
    }


    public void TogglePCUI() // Laptop'dan çaðrýlacak
    {
        if (_pcUI.activeSelf) ClosePCUI();
        else OpenPCUI();
    }
    private void OnEnable()
    {
        UIManager.OnClosePC += ClosePCUI;

    }
    private void OnDisable()
    {
        UIManager.OnClosePC -= ClosePCUI;
    }


    public void OpenPCUI()
    {
        // Diðer tüm UI'larý kapat
        UIManager.Instance.CloseAllOpenPanels();

        // PC UI'ý aç
        _pcUI.SetActive(true);

        // UIManager'a durumu bildir
        UIManager.Instance.SetPCState(true);

        // Ýmleci göster ve serbest býrak
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Oyun zamanýný duraklat (isteðe baðlý)
        Time.timeScale = 0f;
    }

    public void ClosePCUI()
    {
        // PC UI'ý kapat
        _pcUI.SetActive(false);

        // UIManager'a durumu bildir
        UIManager.Instance.SetPCState(false);

        // Ýmleci gizle ve kilitle (FPS tarzý oyunlar için)
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Oyun zamanýný devam ettir (isteðe baðlý)
        Time.timeScale = 1f;
    }

    // ESC tuþu ile kapatma desteði
    private void Update()
    {
        if (_pcUI.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePCUI();
        }
    }
}