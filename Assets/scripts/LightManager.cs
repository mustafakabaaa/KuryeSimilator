using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class LightManager : MonoBehaviour
{
    [SerializeField] private Light DirectionalLight;
    [SerializeField] private SCLight Preset;

    [SerializeField, Range(0f, 24)] private float TimeOfDay;
    [SerializeField] private float timeMultiplier = 1f; // Gün döngüsü hýzýný kontrol eder

    private void FixedUpdate()
    {
        if (Preset == null)
        {
            return;
        }
        if (Application.isPlaying)
        {
            TimeOfDay += Time.deltaTime * timeMultiplier; // Gün döngüsü hýzýný ayarla
            TimeOfDay %= 24;
            UpdateLighting(TimeOfDay / 24);
        }
        else
        {
            UpdateLighting(TimeOfDay / 24);
        }
    }

    private void UpdateLighting(float timePreset)
    {
        RenderSettings.ambientLight = Preset.AmbientColor.Evaluate(timePreset);
        RenderSettings.fogColor = Preset.FogColor.Evaluate(timePreset);

        if (DirectionalLight != null)
        {
            DirectionalLight.color = Preset.DirectionalColor.Evaluate(timePreset);
            DirectionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePreset * 360) - 90f, 170f, 0));
        }
    }

    private void OnValidate()
    {
        if (DirectionalLight != null)
            return;
        if (RenderSettings.sun != null)
        {
            DirectionalLight = RenderSettings.sun;
        }
        else
        {
            Light[] lights = GameObject.FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    DirectionalLight = light;
                    return;
                }
            }
        }
    }
}