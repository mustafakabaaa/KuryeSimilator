using UnityEngine;
using UnityEditor;

public class ConvertSceneMaterialsToURP
{
    [MenuItem("Tools/Convert All Materials to URP")]
    public static void ConvertMaterials()
    {
        MeshRenderer[] renderers = GameObject.FindObjectsOfType<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            Material[] mats = renderer.materials; // sharedMaterials yerine materials
            for (int i = 0; i < mats.Length; i++)
            {
                Material mat = mats[i];
                if (mat != null && (mat.shader.name.Contains("Standard") || mat.shader.name.Contains("Default-Material")))
                {
                    mats[i].shader = Shader.Find("Universal Render Pipeline/Lit");
                    EditorUtility.SetDirty(mats[i]);
                    Debug.Log("Converted: " + mats[i].name);
                }
            }
            renderer.materials = mats; // deðiþiklikleri uygula
        }
        AssetDatabase.SaveAssets();
        Debug.Log("All scene materials converted to URP!");
    }
}
