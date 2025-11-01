using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;

public class XRayManager : MonoBehaviour
{
    public static XRayManager Instance;

    public UniversalRendererData rendererData; // Inspectordan bala
    public string featureName = "RenderObjects"; // X-Ray feature ad
    public float defaultDuration = 3f;

    private ScriptableRendererFeature xrayFeature;
    private Keyboard keyboard;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Sahne deiince silinmez
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Render Feature bul
        foreach (var feature in rendererData.rendererFeatures)
        {
            if (feature.name == featureName)
            {
                xrayFeature = feature;
                xrayFeature.SetActive(false); // Balangta kapal
                break;
            }
        }

        // Input System Keyboard referansı
        keyboard = Keyboard.current;
    }

    void Update()
    {
        if (keyboard != null && keyboard.xKey.wasPressedThisFrame)
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

