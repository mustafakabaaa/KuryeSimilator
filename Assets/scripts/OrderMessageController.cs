using TMPro;
using UnityEngine;

public class OrderMessageController : MonoBehaviour
{
    public static OrderMessageController Instance;

    public GameObject messagePanel;
    public TextMeshProUGUI messageText;
    public float messageDuration = 3f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowWarningMessage(string message)
    {
        messageText.color = Color.red;
        ShowMessage(message);
    }

    public void ShowSuccessMessage(string message)
    {
        messageText.color = Color.green;
        ShowMessage(message);
    }

    private void ShowMessage(string message)
    {
        messagePanel.SetActive(true);
        messageText.text = message;
        Invoke("HideMessage", messageDuration);
    }

    private void HideMessage()
    {
        messagePanel.SetActive(false);
    }
}