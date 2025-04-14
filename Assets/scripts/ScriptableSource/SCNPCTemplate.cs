// SCNPCTemplate.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewNPCTemplate", menuName = "SC/NPC/NPC Template")]
public class SCNPCTemplate : ScriptableObject
{
    [Header("Görsel Ayarlar")]
    public Sprite npcSprite; // 2D karakterler için
    public Material npcMaterial; // 3D karakterler için material
    public RuntimeAnimatorController animatorController; // Animasyonlar
    public Avatar npcAvatar; // Humanoid rig için avatar (opsiyonel)

    [Header("Ekstra Ayarlar")]
    public float npcScale = 1.0f; // NPC boyutunu ayarlamak için
    public Color npcColor = Color.white; // Renk efekti (2D/3D)
}