using UnityEngine;
using UnityEngine.UI;

public class CaptionManager : MonoBehaviour
{
    [Header("UI")]
    public InputField captionInputField;   // Unity UI (legacy) InputField
    public Button     submitButton;

    [Header("Limits")]
    public int characterLimit = 150;

    /* ------------------------------------------------------------ */

    void Start()
    {
        // Safety checks
        if (captionInputField == null || submitButton == null)
        {
            Debug.LogError("[CaptionManager] Missing UI reference(s)");
            enabled = false;
            return;
        }

        // Set limits + single-line mode so Enter submits instead of newline
        captionInputField.characterLimit = characterLimit;
        captionInputField.lineType       = InputField.LineType.SingleLine;

        submitButton.onClick.AddListener(SubmitCaption);

        // Also fire when the user presses Enter inside the field
        captionInputField.onEndEdit.AddListener(OnFieldEndEdit);
    }

    void Update()
    {
        // Extra guard: if the InputField is focused and the user hits Return
        // (Unity sometimes misses this in WebGL or older InputField versions)
        if (captionInputField.isFocused &&
            Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SubmitCaption();
        }
    }

    /* ------------------------------------------------------------ */

    void OnFieldEndEdit(string _)
    {
        // Unity calls this on Enter or when focus leaves the field
        // We only submit if Enter was pressed (Input still registers)
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            SubmitCaption();
    }

    public void SubmitCaption()
    {
        string text = captionInputField.text.Trim();
        if (string.IsNullOrEmpty(text))   // nothing to do
            return;

        CaptionBank.AddCaption(text);
        Debug.Log($"[Caption] added: {text}");

        captionInputField.text = string.Empty;
        captionInputField.ActivateInputField();   // ready for next entry
    }
}
