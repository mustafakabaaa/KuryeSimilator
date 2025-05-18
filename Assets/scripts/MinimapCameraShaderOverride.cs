using UnityEngine;

public class MinimapCameraShaderOverride : MonoBehaviour
{
    public Shader unlitShader;

    void Start()
    {
        if (unlitShader == null)
        {
            unlitShader = Shader.Find("Custom/UnlitMinimap");
        }

        Camera cam = GetComponent<Camera>();
        if (cam != null && unlitShader != null)
        {
            cam.SetReplacementShader(unlitShader, "");
        }
    }
}
