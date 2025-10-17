using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComicCardUI : MonoBehaviour
{
    [Header("Bindings (set in prefab)")]
    [Tooltip("Grid that holds the four Cells (each Cell contains a RawImage + TMP caption)")]
    public GridLayoutGroup panelsGrid;

    [Tooltip("Panel images in 0..3 order (top-left, top-right, bottom-left, bottom-right).")]
    public RawImage[] panelImages = new RawImage[4];

    [Tooltip("Panel captions under each image, 0..3 order matching panelImages.")]
    public TMP_Text[] panelCaptions = new TMP_Text[4];

    [Tooltip("Vote button on the card")]
    public Button voteButton;

    [Header("Appearance")]
    [Min(16)] public float captionHeight = 48f;      // fixed caption height under each image
    [Min(0)]  public float extraVerticalPadding = 24f; // small gap between image and caption

    RectTransform _cardRT;
    bool _bound;

    void Awake()
    {
        _cardRT = (RectTransform)transform;
    }

    /// <summary>
    /// Bind images + captions and wire the vote callback.
    /// </summary>
    public void Bind(ComicBank.ComicEntry entry, System.Action onVote)
    {
        _bound = true;

        for (int i = 0; i < 4; i++)
        {
            // Some panels may be empty; guard everything:
            var hasPanel = entry != null &&
                           entry.panels != null &&
                           i < entry.panels.Length &&
                           entry.panels[i] != null;

            // IMAGE
            Texture2D tex = null;
            if (hasPanel && entry.panels[i].drawingIndex >= 0 && ImageBank.Instance != null)
            {
                tex = ImageBank.Instance.GetImage(entry.panels[i].drawingIndex);
            }
            panelImages[i].texture = tex;
            panelImages[i].color = tex ? Color.white : new Color(1, 1, 1, 0.08f);

            // CAPTION
            var caption = hasPanel ? entry.panels[i].caption : "";
            panelCaptions[i].text = caption ?? "";
            panelCaptions[i].enableWordWrapping = true;
            panelCaptions[i].overflowMode = TextOverflowModes.Overflow;
        }

        voteButton.onClick.RemoveAllListeners();
        voteButton.onClick.AddListener(() => onVote?.Invoke());

        // Do an initial layout pass now and one next frame (after parent sizes settle).
        FitNow();
        StartCoroutine(FitNextFrame());
    }

    System.Collections.IEnumerator FitNextFrame()
    {
        yield return null;
        FitNow();
    }

    /// <summary>
    /// Compute a 2x2 grid that fills the card. Each cell = image area + caption area.
    /// </summary>
    public void FitNow()
    {
        if (!_bound || panelsGrid == null) return;

        var rt = (RectTransform)panelsGrid.transform;

        // Available width/height inside grid
        float w = rt.rect.width;
        float h = rt.rect.height;

        // 2x2 grid numbers
        const int cols = 2;
        const int rows = 2;

        // Account for grid padding/spacing
        float padX = panelsGrid.padding.left + panelsGrid.padding.right;
        float padY = panelsGrid.padding.top + panelsGrid.padding.bottom;
        float spacingX = panelsGrid.spacing.x * (cols - 1);
        float spacingY = panelsGrid.spacing.y * (rows - 1);

        // The cell height must include image + caption.
        // We reserve a fixed caption height per cell:
        float totalCaptionH = rows * captionHeight;

        float cellW = Mathf.Max(10f, (w - padX - spacingX) / cols);
        float cellH = Mathf.Max(10f, (h - padY - spacingY) / rows);

        panelsGrid.cellSize = new Vector2(cellW, cellH);

        // Inside each cell we have a VerticalLayoutGroup (image on top, caption below).
        // Force the caption to a fixed height and let the image take the rest.
        for (int i = 0; i < panelImages.Length && i < panelCaptions.Length; i++)
        {
            var capLE = panelCaptions[i].GetComponent<LayoutElement>();
            if (!capLE) capLE = panelCaptions[i].gameObject.AddComponent<LayoutElement>();
            capLE.minHeight = captionHeight;
            capLE.preferredHeight = captionHeight;
            capLE.flexibleHeight = 0;

            var imgLE = panelImages[i].GetComponent<LayoutElement>();
            if (!imgLE) imgLE = panelImages[i].gameObject.AddComponent<LayoutElement>();
            imgLE.minHeight = 10f;
            imgLE.preferredHeight = Mathf.Max(10f, cellH - captionHeight - extraVerticalPadding);
            imgLE.flexibleHeight = 1;

            // Preserve aspect for images
            var arf = panelImages[i].GetComponent<AspectRatioFitter>();
            if (!arf) arf = panelImages[i].gameObject.AddComponent<AspectRatioFitter>();
            arf.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            arf.aspectRatio = 1f; // any; FitInParent will keep texture aspect at runtime
        }
    }

    // If the card is resized by layout, this hook keeps cells correct.
    void OnRectTransformDimensionsChange()
    {
        if (gameObject.activeInHierarchy && _bound)
            FitNow();
    }
}
