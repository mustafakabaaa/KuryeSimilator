using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotButton : MonoBehaviour
{
    public TextMeshProUGUI saveNameText;
    private string saveFileName;

    public void Setup(string fileName)
    {
        saveFileName = fileName;
        saveNameText.text = fileName;

        // Dinamik olarak onClick listener'ý ekle
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        Debug.Log("[MainMenu] Selected save to load: " + saveFileName);
        SaveManager.Instance.SetPendingLoad(saveFileName);

        // Game sahnesini yükle
        SceneLoader.Load(SceneList.GameScene);
    }
}
