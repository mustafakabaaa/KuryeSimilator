using UnityEngine;
using UnityEngine.UI;

public class ChangeImageColor : MonoBehaviour
{
    public Image blackScreen; // Canvas'taki Image bileþeni

    private void Start()
    {
        if (blackScreen != null)
        {
            // Image'in rengini ve alpha deðerini ayarla
            blackScreen.color = new Color(0, 0, 0, 0f); // Siyah, %50 þeffaf
        }
    }
}