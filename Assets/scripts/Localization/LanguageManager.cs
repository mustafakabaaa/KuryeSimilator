using UnityEngine;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;

    public string CurrentLanguage { get; private set; } = "tr"; // Default: Türkçe

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SetLanguage(string langCode)
    {
        CurrentLanguage = langCode;
    }
}
