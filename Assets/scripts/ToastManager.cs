using UnityEngine;
using System.Collections.Generic;

public class ToastManager : MonoBehaviour
{
    public static ToastManager Instance;

    public GameObject toastTextPrefab; // Sadece TextMeshPro bir prefab
    public GameObject panel;           // WarningPanel gibi ana panel
    public Transform contentParent;    // Panelin içinde Vertical Layout olan boþ bir parent
    [SerializeField] private int maxToastCount = 5;

    private List<GameObject> activeToasts = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ShowToast(string message, float duration = 3f)
    {
        if (!panel.activeSelf)
            panel.SetActive(true);

        // Eðer maksimum sayýya ulaþýldýysa en eskiyi sil
        if (activeToasts.Count >= maxToastCount)
        {
            Destroy(activeToasts[0]);
            activeToasts.RemoveAt(0);
        }

        GameObject toastGO = Instantiate(toastTextPrefab, contentParent);
        ToastMessageItem item = toastGO.GetComponent<ToastMessageItem>();
        item.Setup(message, duration);

        activeToasts.Add(toastGO);

        // Her ToastMessageItem kendi süresinin sonunda kendini yok edince listeden çýkaracak
        StartCoroutine(CheckPanelVisibility());
    }

    private System.Collections.IEnumerator CheckPanelVisibility()
    {
        yield return null; // 1 frame bekle ki Instantiate bitsin

        while (true)
        {
            yield return null;

            // Aktif Toast listesinden null olanlarý temizle
            activeToasts.RemoveAll(toast => toast == null);

            if (activeToasts.Count == 0)
            {
                panel.SetActive(false);
                yield break; // Coroutine burada bitiriyoruz
            }
        }
    }
}
