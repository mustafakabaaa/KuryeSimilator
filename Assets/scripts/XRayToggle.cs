using UnityEngine;
using UnityEngine.Rendering.Universal;

public class XRayManager : MonoBehaviour
{
    public static XRayManager Instance;

    public UniversalRendererData rendererData; // Inspector’dan baðla
    public string featureName = "RenderObjects"; // X-Ray feature adý
    public float defaultDuration = 3f;

    private ScriptableRendererFeature xrayFeature;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Sahne deðiþince silinmez
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Render Feature’ý bul
        foreach (var feature in rendererData.rendererFeatures)
        {
            if (feature.name == featureName)
            {
                xrayFeature = feature;
                xrayFeature.SetActive(false); // Baþlangýçta kapalý
                break;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            ShowXRay(defaultDuration);
        }
    }

    public void ShowXRay(float duration)
    {
        if (xrayFeature != null)
            StartCoroutine(XRayRoutine(duration));
    }

    private System.Collections.IEnumerator XRayRoutine(float duration)
    {
        xrayFeature.SetActive(true);
        yield return new WaitForSeconds(duration);
        xrayFeature.SetActive(false);
    }
}
