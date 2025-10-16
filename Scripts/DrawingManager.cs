using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DrawingManager : MonoBehaviour
{
    public Color currentColor = Color.black;
    public float brushSize = 5f;
    private bool isErasing = false;

    public Texture2D drawingTexture;

    [Header("UI Buttons")]
    public List<Button> colorButtons = new List<Button>();
    public List<Button> sizeButtons = new List<Button>();
    public Button eraseButton;
    public Button submitButton;

    private Button activeColorButton;
    private Button activeSizeButton;
    private Button activeEraseButton;

    void Start()
    {
        InitializeTexture();
        InitializeButtons();
        UpdateButtonSelection();
    }

    void Update()
    {
        if (Input.GetMouseButton(0)) // Left click
        {
            DrawOnCanvas();
        }
    }

    void InitializeTexture()
    {
        drawingTexture = new Texture2D(512, 512);
        GetComponent<RawImage>().texture = drawingTexture;
        ClearCanvas();
    }

    void DrawOnCanvas()
    {
        Vector2 localMousePosition;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                GetComponent<RectTransform>(), Input.mousePosition, null, out localMousePosition))
            return;

        int x = Mathf.RoundToInt(
            (localMousePosition.x + GetComponent<RectTransform>().rect.width / 2) /
            GetComponent<RectTransform>().rect.width * drawingTexture.width);

        int y = Mathf.RoundToInt(
            (localMousePosition.y + GetComponent<RectTransform>().rect.height / 2) /
            GetComponent<RectTransform>().rect.height * drawingTexture.height);

        if (x < 0 || x >= drawingTexture.width || y < 0 || y >= drawingTexture.height) return;

        Color drawColor = isErasing ? Color.white : currentColor;
        List<Vector2Int> pixelsToColor = new List<Vector2Int>();

        for (int i = -Mathf.CeilToInt(brushSize / 2); i <= Mathf.FloorToInt(brushSize / 2); i++)
        {
            for (int j = -Mathf.CeilToInt(brushSize / 2); j <= Mathf.FloorToInt(brushSize / 2); j++)
            {
                int newX = Mathf.Clamp(x + i, 0, drawingTexture.width - 1);
                int newY = Mathf.Clamp(y + j, 0, drawingTexture.height - 1);
                pixelsToColor.Add(new Vector2Int(newX, newY));
            }
        }

        foreach (Vector2Int pixel in pixelsToColor)
        {
            drawingTexture.SetPixel(pixel.x, pixel.y, drawColor);
        }

        drawingTexture.Apply();
    }

    void InitializeButtons()
    {
        foreach (Button btn in colorButtons)
            btn.onClick.AddListener(() => SetColor(btn));

        foreach (Button btn in sizeButtons)
            btn.onClick.AddListener(() => SetBrushSize(btn));

        if (eraseButton)
            eraseButton.onClick.AddListener(ToggleEraser);

        if (submitButton)
            submitButton.onClick.AddListener(SubmitDrawing);
    }

    public void SetColor(Button button)
    {
        currentColor = button.GetComponent<Image>().color;
        isErasing = false;

        activeColorButton = button;
        activeEraseButton = null;
        UpdateButtonSelection();
    }

    public void SetBrushSize(Button button)
    {
        float size = 5f;
        switch (button.name)
        {
            case "Small": size = 2.5f; break;
            case "Medium": size = 5f; break;
            case "Big": size = 10f; break;
            case "Huge": size = 20f; break;
        }

        brushSize = size;
        activeSizeButton = button;
        UpdateButtonSelection();
    }

    public void ToggleEraser()
    {
        isErasing = true;
        currentColor = Color.white;

        activeEraseButton = eraseButton;
        activeColorButton = null;
        UpdateButtonSelection();
    }

    public void SubmitDrawing()
    {
        byte[] imageData = drawingTexture.EncodeToPNG();

        if (ImageBank.Instance == null || GameManager.Instance == null)
        {
            Debug.LogError("ImageBank or GameManager not initialized.");
            return;
        }

        ImageBank.Instance.AddImage(imageData, GameManager.Instance.PlayerId);

        // Count this toward the per-scene quota and potentially advance.
        GameManager.Instance.RegisterDrawing(GameManager.Instance.PlayerId);

        ClearCanvas();
    }

    void ClearCanvas()
    {
        Color32[] whitePixels = new Color32[drawingTexture.width * drawingTexture.height];
        for (int i = 0; i < whitePixels.Length; i++)
            whitePixels[i] = Color.white;

        drawingTexture.SetPixels32(whitePixels);
        drawingTexture.Apply();
    }

    private void UpdateButtonSelection()
    {
        foreach (Button btn in colorButtons)
            SetButtonBorder(btn, btn == activeColorButton);

        foreach (Button btn in sizeButtons)
            SetButtonBorder(btn, btn == activeSizeButton);

        SetButtonBorder(eraseButton, eraseButton == activeEraseButton);
    }

    private void SetButtonBorder(Button button, bool isActive)
    {
        if (!button) return;

        Outline outline = button.GetComponent<Outline>();
        if (outline == null)
        {
            outline = button.gameObject.AddComponent<Outline>();
            outline.effectColor = Color.blue;
            outline.effectDistance = new Vector2(5, 5);
        }
        outline.enabled = isActive;
    }
}
