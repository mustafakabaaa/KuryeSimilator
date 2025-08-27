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

        // Ba�lang��ta kapal�
        contextMenuPanel.SetActive(false);
    }

    public void ShowContextMenu(string saveFileName, Vector3 position, SaveGamePanel parent)
    {
        currentSaveFileName = saveFileName;
        parentPanel = parent;

        // Ba�l�k g�ncelle
        titleText.text = $"Save: {saveFileName}";

        // RectTransform ile canvas'�n ortas�na konumland�r
        RectTransform menuRect = contextMenuPanel.GetComponent<RectTransform>();
        // Anchor'� center'a ayarla ve pozisyonu s�f�rla
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

        // Sa� kenara ta�ma kontrol�
        if (menuCorners[2].x > canvasCorners[2].x)
        {
            float overflow = menuCorners[2].x - canvasCorners[2].x;
            menuRect.position -= new Vector3(overflow + 10, 0, 0);
        }

        // Alt kenara ta�ma kontrol�
        if (menuCorners[0].y < canvasCorners[0].y)
        {
            float overflow = canvasCorners[0].y - canvasCorners[0].y;
            menuRect.position += new Vector3(0, overflow + 10, 0);
        }
    }

    private void LoadSave()
    {
        Debug.Log($"Loading save: {currentSaveFileName}");

        // Save dosyas�ndan chapter title'� al (�imdilik sadece log i�in)
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
        // Silme onay� iste
        ShowDeleteConfirmation();
    }

    private void ShowDeleteConfirmation()
    {
        // Unity-friendly onay sistemi
        Debug.Log($"Delete confirmation for: {currentSaveFileName}");
        // �imdilik direkt sil (isterseniz �zel onay popup'� ekleyebiliriz)
        SaveManager.Instance.DeleteSave(currentSaveFileName);
        parentPanel.RefreshSaveList1();
        CloseMenu();
        // TODO: �zel onay popup'� eklenebilir
    }

    // Save dosyas�ndan chapter title'� alan metod
    private string GetChapterTitleFromSave(string saveFileName)
    {
        try
        {
            string savePath = System.IO.Path.Combine(Application.persistentDataPath, $"{saveFileName}.json");
            if (System.IO.File.Exists(savePath))
            {
                string json = System.IO.File.ReadAllText(savePath);
                // Bu k�sm� oyununuzun save format�na g�re ayarlay�n
                // �rnek: JSON'dan chapter bilgisini �ekin
                return "Chapter 02: The Journey Begins"; // Bu sat�r� save dosyan�zdan ger�ek veri alacak �ekilde de�i�tirin
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Save dosyas� okunamad�: {e.Message}");
        }

        return "Unknown Chapter";
    }

    public void CloseMenu()
    {
        contextMenuPanel.SetActive(false);
        currentSaveFileName = "";
    }

    // Menü dışına tıklanınca kapat
    private void Update()
    {
        if (contextMenuPanel.activeSelf && Input.GetMouseButtonDown(0))
        {
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null && parentCanvas.worldCamera != null)
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(
                    contextMenuPanel.GetComponent<RectTransform>(),
                    Input.mousePosition,
                    parentCanvas.worldCamera))
                {
                    CloseMenu();
                }
            }
            else
            {
                // Canvas veya camera bulunamazsa basit kontrol yap
                if (!RectTransformUtility.RectangleContainsScreenPoint(
                    contextMenuPanel.GetComponent<RectTransform>(),
                    Input.mousePosition))
                {
                    CloseMenu();
                }
            }
        }
    }
}