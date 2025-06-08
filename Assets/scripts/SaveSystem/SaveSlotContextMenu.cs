using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotContextMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject contextMenuPanel;
    public Button loadButton;
    public Button saveOverButton;
    public Button deleteButton;
    public Button cancelButton;
    public TextMeshProUGUI titleText;

    private string currentSaveFileName;
    private SaveGamePanel parentPanel;

    private void Start()
    {
        // Button event'lerini ayarla
        loadButton.onClick.AddListener(LoadSave);
        saveOverButton.onClick.AddListener(SaveOver);
        deleteButton.onClick.AddListener(DeleteSave);
        cancelButton.onClick.AddListener(CloseMenu);

        // Baþlangýçta kapalý
        contextMenuPanel.SetActive(false);
    }

    public void ShowContextMenu(string saveFileName, Vector3 position, SaveGamePanel parent)
    {
        currentSaveFileName = saveFileName;
        parentPanel = parent;

        // Baþlýk güncelle
        titleText.text = $"Save: {saveFileName}";

        // RectTransform ile canvas'ýn ortasýna konumlandýr
        RectTransform menuRect = contextMenuPanel.GetComponent<RectTransform>();
        // Anchor'ý center'a ayarla ve pozisyonu sýfýrla
        menuRect.anchoredPosition = Vector2.zero;

        contextMenuPanel.SetActive(true);
    }

    private void AdjustMenuPosition()
    {
        RectTransform menuRect = contextMenuPanel.GetComponent<RectTransform>();
        RectTransform canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        Vector3[] menuCorners = new Vector3[4];
        menuRect.GetWorldCorners(menuCorners);

        Vector3[] canvasCorners = new Vector3[4];
        canvasRect.GetWorldCorners(canvasCorners);

        // Sað kenara taþma kontrolü
        if (menuCorners[2].x > canvasCorners[2].x)
        {
            float overflow = menuCorners[2].x - canvasCorners[2].x;
            menuRect.position -= new Vector3(overflow + 10, 0, 0);
        }

        // Alt kenara taþma kontrolü
        if (menuCorners[0].y < canvasCorners[0].y)
        {
            float overflow = canvasCorners[0].y - canvasCorners[0].y;
            menuRect.position += new Vector3(0, overflow + 10, 0);
        }
    }

    private void LoadSave()
    {
        Debug.Log($"Loading save: {currentSaveFileName}");

        // Save dosyasýndan chapter title'ý al (þimdilik sadece log için)
        string chapterTitle = GetChapterTitleFromSave(currentSaveFileName);
        Debug.Log($"Chapter title: {chapterTitle}");

        // Removed the problematic line: parentPanel.SetCurrentChapterTitle(chapterTitle);

        SaveManager.Instance.LoadSpecificSave(currentSaveFileName);
        parentPanel.ClosePanel(); // Ana paneli kapat
        CloseMenu();
    }

    private void SaveOver()
    {
        Debug.Log($"Saving over: {currentSaveFileName}");
        SaveManager.Instance.SaveGame(currentSaveFileName);
        parentPanel.RefreshSaveList1(); // Listeyi yenile
        CloseMenu();
    }

    private void DeleteSave()
    {
        // Silme onayý iste
        ShowDeleteConfirmation();
    }

    private void ShowDeleteConfirmation()
    {
        // Unity-friendly onay sistemi
        Debug.Log($"Delete confirmation for: {currentSaveFileName}");
        // Þimdilik direkt sil (isterseniz özel onay popup'ý ekleyebiliriz)
        SaveManager.Instance.DeleteSave(currentSaveFileName);
        parentPanel.RefreshSaveList1();
        CloseMenu();
        // TODO: Özel onay popup'ý eklenebilir
    }

    // Save dosyasýndan chapter title'ý alan metod
    private string GetChapterTitleFromSave(string saveFileName)
    {
        try
        {
            string savePath = System.IO.Path.Combine(Application.persistentDataPath, $"{saveFileName}.json");
            if (System.IO.File.Exists(savePath))
            {
                string json = System.IO.File.ReadAllText(savePath);
                // Bu kýsmý oyununuzun save formatýna göre ayarlayýn
                // Örnek: JSON'dan chapter bilgisini çekin
                return "Chapter 02: The Journey Begins"; // Bu satýrý save dosyanýzdan gerçek veri alacak þekilde deðiþtirin
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Save dosyasý okunamadý: {e.Message}");
        }

        return "Unknown Chapter";
    }

    public void CloseMenu()
    {
        contextMenuPanel.SetActive(false);
        currentSaveFileName = "";
    }

    // Menü dýþýna týklanýnca kapat
    private void Update()
    {
        if (contextMenuPanel.activeSelf && Input.GetMouseButtonDown(0))
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                contextMenuPanel.GetComponent<RectTransform>(),
                Input.mousePosition,
                GetComponentInParent<Canvas>().worldCamera))
            {
                CloseMenu();
            }
        }
    }
}