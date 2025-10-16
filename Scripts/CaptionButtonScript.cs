using UnityEngine;
using TMPro;

public class CaptionButtonScript : MonoBehaviour
{
    private TMP_Text textElement;

    void Start()
    {
        textElement = GetComponentInChildren<TMP_Text>();
        if (textElement != null)
        {
            textElement.enableAutoSizing = true;
            textElement.fontSizeMin = 14;
            textElement.fontSizeMax = 36;
            textElement.textWrappingMode = TextWrappingModes.PreserveWhitespace;  // ✅ Updated!
        }
        else
        {
            Debug.LogWarning("CaptionButtonScript: TMP_Text not found on " + gameObject.name);
        }
    }
}
