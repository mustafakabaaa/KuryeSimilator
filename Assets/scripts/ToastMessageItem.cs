using TMPro;
using UnityEngine;

public class ToastMessageItem : MonoBehaviour
{
    private float lifetime;
    private float timer;
    private TextMeshProUGUI textMesh;

    public void Setup(string message, float duration)
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        textMesh.text = message;
        lifetime = duration;
        timer = 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
