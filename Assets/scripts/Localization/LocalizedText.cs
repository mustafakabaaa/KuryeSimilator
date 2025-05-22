[System.Serializable]
public class LocalizedText
{
    public string tr;
    public string en;

    public string GetText(string languageCode)
    {
        return languageCode switch
        {
            "tr" => tr,
            "en" => en,
            _ => en
        };
    }
}

