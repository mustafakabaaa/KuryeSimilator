[System.Serializable]
public class DialogueOption
{
    public string text; // Oyuncu seçeneði metni
    public int nextNodeIndex; // Sonraki node indeksi, -1 ise diyalog bitiþi
    public int tipEffect; // Bahþiþ etkisi (kullanabilirsin)
}