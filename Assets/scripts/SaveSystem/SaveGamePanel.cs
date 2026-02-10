using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;

public class SaveGamePanel : MonoBehaviour
{
    private const string UiTable = "UI";
    [Header("UI Elements")]
    public GameObject panel;
    public Transform saveItemContainer;
    public GameObject saveItemPrefab; // Normal save slot prefab
    public GameObject emptySaveSlotPrefab; // Empty save slot prefab
    public Button closeButton;

    [Header("Save Settings")]
    public int maxSaveSlots = 4; // Maksimum save slot sayýsý

    [Header("Context Menu")]
    public SaveSlotContextMenu contextMenu; // Context menu referansý

    private void Start()
    {
        closeButton.onClick.AddListener(ClosePanel);
    }

    public void ShowPanel()
    {
        panel.SetActive(true);
        UIManager.Instance.SetMenuState(true);
        RefreshSaveList();
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        UIManager.Instance.SetMenuState(false);
    }

    private void RefreshSaveList()
    {
        // Mevcut slot'larý temizle
        foreach (Transform child in saveItemContainer)
        {
            Destroy(child.gameObject);
        }

        // Mevcut save dosyalarýný al
        List<SaveFileInfo> existingSaves = GetExistingSaves();

        // Mevcut save'leri göster
        foreach (var saveInfo in existingSaves)
        {
            CreateSaveSlotItem(saveInfo);
        }

        // Boþ slot'larý ekle (sadece mevcut save sayýsý max'tan azsa)
        int emptySlotsNeeded = maxSaveSlots - existingSaves.Count;
        for (int i = 0; i < emptySlotsNeeded; i++)
        {
            CreateEmptySaveSlot();
        }
    }

    // Public metod - context menu'den çaðýrýlabilir
    public void RefreshSaveList1()
    {
        RefreshSaveList();
    }

    private List<SaveFileInfo> GetExistingSaves()
    {
        List<SaveFileInfo> saves = new List<SaveFileInfo>();

        // Tüm .json dosyalarýný bul
        string[] files = Directory.GetFiles(Application.persistentDataPath, "*.json");

        foreach (string filePath in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            DateTime lastWriteTime = File.GetLastWriteTime(filePath);

            // Save dosyasýndan chapter bilgisini al
            string chapterInfo = GetChapterInfoFromSave(filePath);

            saves.Add(new SaveFileInfo
            {
                fileName = fileName,
                filePath = filePath,
                lastSaveTime = lastWriteTime,
                chapterInfo = chapterInfo
            });
        }

        // Tarihe göre sýrala (en yeni en üstte)
        return saves.OrderByDescending(s => s.lastSaveTime).ToList();
    }

    private string GetChapterInfoFromSave(string filePath)
    {
        try
        {
            string json = File.ReadAllText(filePath);
            // Basit chapter bilgisi döndür (bu kýsmý oyununuza göre özelleþtirebilirsiniz)
            return "Chapter 01: New Arrivals";
        }
        catch
        {
            return "Unknown Chapter";
        }
    }

    private void CreateSaveSlotItem(SaveFileInfo saveInfo)
    {
        var item = Instantiate(saveItemPrefab, saveItemContainer);

        Debug.Log($"Creating save slot for: {saveInfo.fileName}");

        // Prefab içindeki TextMeshPro componentlerini isimlerine göre bul
        var chapterTitleComponent = FindTextComponentByName(item.transform, "ChapterTitle");
        var dateTimeComponent = FindTextComponentByName(item.transform, "DateTime");
        var gameTimeComponent = FindTextComponentByName(item.transform, "GameTime");

        // ChapterTitle'a save dosyasýnýn adýný yaz
        if (chapterTitleComponent != null)
        {
            chapterTitleComponent.text = saveInfo.fileName;
            Debug.Log($"ChapterTitle set to: {saveInfo.fileName} for item: {item.name}");
        }
        else
        {
            Debug.LogError($"ChapterTitle component bulunamadý! Item: {item.name}");
        }

        // Diðer text'leri de ayarla
        if (dateTimeComponent != null)
        {
            // Save dosyasýndan kayýtlý tarihi çek
            string savedDateTime = GetSavedDateTimeFromFile(saveInfo.filePath);
            dateTimeComponent.text = savedDateTime;
            Debug.Log($"DateTime set to: {savedDateTime} for item: {item.name}");
        }
        else
        {
            //Debug.LogError($"DateTime component bulunamadý! Item: {item.name}");
        }

        if (gameTimeComponent != null)
        {
            // Save dosyasýndan gerçek dünya saatini çek - HER DOSYA ÝÇÝN AYRI AYRI OKU
            string realWorldTime = GetRealWorldTimeFromSave(saveInfo.filePath);
            gameTimeComponent.text = realWorldTime;
            Debug.Log($"GameTime set to: {realWorldTime} for item: {item.name}");
        }
        else
        {
            Debug.LogError($"GameTime component bulunamadý! Item: {item.name}");
        }

        // Button event'ini ayarla - Context Menu'yu aç
        var button = item.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => {
                Vector3 menuPosition = button.transform.position + new Vector3(200, 0, 0); // Button'un saðýnda aç
                contextMenu.ShowContextMenu(saveInfo.fileName, menuPosition, this);
            });
        }
    }

    // GameObject adýna göre TextMeshPro component'ini bulan yardýmcý metod - SADECE DIREKT CHILD'LARI KONTROL ET
    private TextMeshProUGUI FindTextComponentByName(Transform parent, string objectName)
    {
        // SADECE direkt child'larý kontrol et - recursive arama yapma!
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == objectName)
            {
                var textComponent = child.GetComponent<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    Debug.Log($"Component bulundu: {objectName} - Parent: {parent.name} - Child: {child.name}");
                    return textComponent;
                }
            }

            // Eðer direkt child'da yoksa, bir seviye daha derine in (ama tüm hierarchy'yi tarama)
            for (int j = 0; j < child.childCount; j++)
            {
                Transform grandChild = child.GetChild(j);
                if (grandChild.name == objectName)
                {
                    var textComponent = grandChild.GetComponent<TextMeshProUGUI>();
                    if (textComponent != null)
                    {
                        Debug.Log($"Component bulundu (grandchild): {objectName} - Parent: {parent.name} - GrandChild: {grandChild.name}");
                        return textComponent;
                    }
                }
            }
        }

        Debug.LogWarning($"Component bulunamadý: {objectName} - Parent: {parent.name}");
        return null;
    }

    private void CreateEmptySaveSlot()
    {
        var item = Instantiate(emptySaveSlotPrefab, saveItemContainer);

        // Empty slot text'ini ayarla
        var text = item.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = LocalizationHelper.Localize(UiTable, "ui.save_empty_slot");
        }

        // Button event'ini ayarla - Yeni save oluþtur
        var button = item.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(CreateNewSave);
        }
    }

    private void CreateNewSave()
    {
        // Yeni save dosyasý adý oluþtur
        string newSaveName = GenerateNewSaveName();

        // Oyunu kaydet
        SaveManager.Instance.SaveGame(newSaveName);

        Debug.Log($"Yeni save oluþturuldu: {newSaveName}");

        // Save listesini yenile
        RefreshSaveList();
    }

    private string GenerateNewSaveName()
    {
        // Mevcut save'leri kontrol et ve yeni isim oluþtur
        List<SaveFileInfo> existingSaves = GetExistingSaves();

        for (int i = 1; i <= maxSaveSlots; i++)
        {
            string potentialName = $"MDL_{i:D2}";

            if (!existingSaves.Any(s => s.fileName == potentialName))
            {
                return potentialName;
            }
        }

        // Eðer tüm slot'lar doluysa timestamp kullan
        return $"save_{DateTime.Now:yyyyMMdd_HHmmss}";
    }

    private string GetSavedDateTimeFromFile(string filePath)
    {
        try
        {
            string json = File.ReadAllText(filePath);

            // JSON içinden saveDateTime'ý çek
            if (json.Contains("\"saveDateTime\":"))
            {
                // Manuel parsing
                int startIndex = json.IndexOf("\"saveDateTime\":\"") + 16;
                int endIndex = json.IndexOf("\"", startIndex);
                if (startIndex > 15 && endIndex > startIndex)
                {
                    string dateTimeString = json.Substring(startIndex, endIndex - startIndex);
                    if (DateTime.TryParse(dateTimeString, out DateTime parsedDate))
                    {
                        return parsedDate.ToString("dd.MM.yyyy - HH:mm:ss");
                    }
                }
            }

            // Eðer JSON'da saveDateTime bulunamazsa dosya tarihini kullan
            return File.GetLastWriteTime(filePath).ToString("dd.MM.yyyy - HH:mm:ss");
        }
        catch (Exception e)
        {
            Debug.LogError($"GetSavedDateTimeFromFile hatasý: {e.Message} - Dosya: {filePath}");
            // Hata durumunda dosya tarihini kullan
            return File.GetLastWriteTime(filePath).ToString("dd.MM.yyyy - HH:mm:ss");
        }
    }

    private string GetRealWorldTimeFromSave(string filePath)
    {
        try
        {
            // Dosya mevcut mu kontrol et
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"Dosya bulunamadý: {filePath}");
                return DateTime.Now.ToString("HH:mm:ss");
            }

            string json = File.ReadAllText(filePath);

            // Debug için JSON içeriðini kontrol et
            Debug.Log($"JSON içeriði ({Path.GetFileName(filePath)}): {json.Substring(0, Math.Min(200, json.Length))}...");

            // Manuel parsing ile realWorldTime'ý bul
            if (json.Contains("\"realWorldTime\":"))
            {
                int startIndex = json.IndexOf("\"realWorldTime\":\"") + 17;
                int endIndex = json.IndexOf("\"", startIndex);

                if (startIndex > 16 && endIndex > startIndex)
                {
                    string realWorldTime = json.Substring(startIndex, endIndex - startIndex);
                    Debug.Log($"Bulunan realWorldTime ({Path.GetFileName(filePath)}): {realWorldTime}");
                    return realWorldTime;
                }
                else
                {
                    Debug.LogWarning($"realWorldTime parsing hatasý - startIndex: {startIndex}, endIndex: {endIndex}");
                }
            }
            else
            {
                Debug.LogWarning($"realWorldTime field bulunamadý dosyada: {Path.GetFileName(filePath)}");
            }

            // Eðer realWorldTime bulunamazsa dosya oluþturma zamanýný kullan
            DateTime fileTime = File.GetCreationTime(filePath);
            return fileTime.ToString("HH:mm:ss");
        }
        catch (Exception e)
        {
            Debug.LogError($"GetRealWorldTimeFromSave hatasý: {e.Message} - Dosya: {filePath}");
            return DateTime.Now.ToString("HH:mm:ss");
        }
    }

    private void LoadSave(string saveName)
    {
        Debug.Log($"Save yükleniyor: {saveName}");
        SaveManager.Instance.LoadSpecificSave(saveName);
        ClosePanel();
    }
}

[System.Serializable]
public class SaveFileInfo
{
    public string fileName;
    public string filePath;
    public DateTime lastSaveTime;
    public string chapterInfo;
}