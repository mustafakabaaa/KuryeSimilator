
using UnityEngine;
[System.Serializable]
[CreateAssetMenu(fileName ="Lighting Preset", menuName = "SC/Scriptable/Light Preset")]
public class SCLight : ScriptableObject
{
    public Gradient AmbientColor;
    public Gradient DirectionalColor;
    public Gradient FogColor;
}
