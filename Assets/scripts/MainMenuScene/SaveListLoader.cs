using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;

public class SaveListLoader : MonoBehaviour
{
    public GameObject saveSlotButtonPrefab;
    public Transform container;

    private void Start()
    {
        LoadSaveSlots();
    }

    private void LoadSaveSlots()
    {
        string[] files = Directory.GetFiles(Application.persistentDataPath, "*.json");

        foreach (string file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            GameObject buttonGO = Instantiate(saveSlotButtonPrefab, container);

            var buttonScript = buttonGO.GetComponent<SaveSlotButton>();
            buttonScript.Setup(fileName);
        }
    }
}
