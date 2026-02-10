using UnityEngine.Localization.Settings;

public static class LocalizationHelper
{
    public static string Localize(string table, string keyOrText)
    {
        if (string.IsNullOrEmpty(keyOrText))
        {
            return string.Empty;
        }

        if (string.IsNullOrEmpty(table))
        {
            return keyOrText;
        }

        if (!LooksLikeKey(keyOrText))
        {
            return keyOrText;
        }

        string localized = LocalizationSettings.StringDatabase.GetLocalizedString(table, keyOrText);
        return string.IsNullOrEmpty(localized) ? keyOrText : localized;
    }

    private static bool LooksLikeKey(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        for (int i = 0; i < value.Length; i++)
        {
            if (char.IsWhiteSpace(value[i]))
            {
                return false;
            }
        }

        return value.StartsWith("dlg.") ||
               value.StartsWith("ui.") ||
               value.StartsWith("order.") ||
               value.StartsWith("npc.") ||
               value.StartsWith("ord.");
    }
}
