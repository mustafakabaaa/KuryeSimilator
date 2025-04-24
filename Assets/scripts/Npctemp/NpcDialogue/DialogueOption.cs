// DialogueOption.cs
[System.Serializable]
public class DialogueOption
{
    public string text; // "Kahve lütfen"
    public int nextNodeIndex; // Hangi node'a geçilecek?
    public int tipEffect; // +3, -1 gibi bahþiþ etkisi
}