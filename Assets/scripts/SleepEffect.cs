using UnityEngine;
using UnityEngine.UI;

public class SleepEffect : MonoBehaviour
{
    public Image blackScreen; // Canvas'taki siyah Image
    public ClockUI clockUI; // Saati kontrol eden ClockUI bileþeni
    public float fadeDuration = 2f; // Alpha deðiþim süresi
    public float waitDuration = 1f; // Tamamen siyah kalma süresi

    private void Start()
    {
        if (blackScreen == null)
        {
            Debug.LogError("Black Screen Image atanmamýþ!");
            return;
        }

        // Baþlangýçta Image'i devre dýþý býrak
        blackScreen.gameObject.SetActive(false);
    }

    public void StartSleepEffect()
    {
        blackScreen.gameObject.SetActive(true);

        // Saati gizle
        if (clockUI != null)
        {
            clockUI.enabled = false;
        }

        StartCoroutine(FadeInAndOut());
    }

    private System.Collections.IEnumerator FadeInAndOut()
    {
        // Alpha deðerini 0'dan 1'e yükselt (parabolik)
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.SmoothStep(0f, 1f, elapsedTime / fadeDuration);
            blackScreen.color = new Color(0, 0, 0, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Tamamen siyah kal
        blackScreen.color = new Color(0, 0, 0, 1);
        yield return new WaitForSeconds(waitDuration);

        // Alpha deðerini 1'den 0'a düþür (parabolik)
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.SmoothStep(1f, 0f, elapsedTime / fadeDuration);
            blackScreen.color = new Color(0, 0, 0, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Image'i devre dýþý býrak
        blackScreen.gameObject.SetActive(false);

        // Saati tekrar göster
        if (clockUI != null)
        {
            clockUI.enabled = true;
        }
    }
}