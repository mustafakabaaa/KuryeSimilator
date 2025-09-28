using UnityEngine;
using UnityEditor;

public class ConvertToURP
{
    [MenuItem("Tools/Convert Materials to URP")]
    public static void ConvertMaterials()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat.shader.name.Contains("Standard"))
            {
                mat.shader = Shader.Find("Universal Render Pipeline/Lit");
                EditorUtility.SetDirty(mat);
                Debug.Log("Converted: " + mat.name);
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("All standard materials converted to URP!");
    }
}
