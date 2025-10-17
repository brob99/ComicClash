using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ComicCreateManager : MonoBehaviour
{
    [Header("Preview (right side)")]
    public Button[]   panelButtons    = new Button[4];   // panel 0..3
    public RawImage[] previewImages   = new RawImage[4];
    public TMP_Text[] previewCaptions = new TMP_Text[4];

    [Header("Pools (left side)")]
    public Transform  drawingContent;        // ScrollView → Content (drawings)
    public GameObject drawingButtonPrefab;   // must contain Button + RawImage

    public Transform  captionContent;        // ScrollView → Content (captions)
    public GameObject captionButtonPrefab;   // must contain Button + TMP_Text

    [Header("UI")]
    public Button     submitButton;
    public GameObject confirmDialog;         // Yes → FinalSubmit, No → CancelConfirm

    struct Panel { public int drawing; public string caption; }
    readonly Panel[] panels = new Panel[4] {
        new Panel{drawing=-1,caption=null},
        new Panel{drawing=-1,caption=null},
        new Panel{drawing=-1,caption=null},
        new Panel{drawing=-1,caption=null}
    };

    int    activePanel = 0;
    string playerId;
    System.Random rng;

    void Start()
    {
        playerId = GameManager.Instance != null ? GameManager.Instance.PlayerId : "Unknown";
        rng      = new System.Random();

        // panel selectors
        for (int i = 0; i < panelButtons.Length; i++)
        {
            int idx = i;
            if (panelButtons[i] != null)
                panelButtons[i].onClick.AddListener(() => SelectPanel(idx));
        }
        HighlightPanels();

        PopulateDrawings();
        PopulateCaptions();

        if (submitButton != null) submitButton.onClick.AddListener(OnSubmitClicked);
        if (submitButton != null) submitButton.interactable = false;
        if (confirmDialog != null) confirmDialog.SetActive(false);
    }

    // ========================== POOLS ==========================
    void PopulateDrawings()
    {
        var all = ImageBank.Instance.GetImages();
        var indices = Enumerable.Range(0, all.Count()).ToList();

        // exclude own in multi-player
        if (GameManager.Instance != null && GameManager.Instance.PlayerCount > 1)
            indices = indices.Where(i => all[i].playerId != playerId).ToList();

        indices = indices.OrderBy(_ => rng.Next()).Take(10).ToList();

        foreach (int idx in indices)
        {
            var g = Instantiate(drawingButtonPrefab, drawingContent);
            g.name = idx.ToString(); // for highlight comparison
            var img = g.GetComponentInChildren<RawImage>();
            if (img != null) img.texture = ImageBank.Instance.GetImage(idx);

            int local = idx;
            g.GetComponent<Button>().onClick.AddListener(() =>
            {
                panels[activePanel].drawing = local;
                UpdatePreview(activePanel);
                HighlightDrawButtons(local);
                CheckSubmitState();
            });
        }
    }

    void PopulateCaptions()
    {
        var caps = CaptionBank.GetCaptions().ToList();
        if (caps.Count == 0) return;

        // blacklist random 1/3 for variety, then take up to 20
        caps = caps.OrderBy(_ => rng.Next()).Skip(caps.Count / 3).Take(20).ToList();

        foreach (string cap in caps)
        {
            var g = Instantiate(captionButtonPrefab, captionContent);
            var t = g.GetComponentInChildren<TMP_Text>();
            if (t != null) t.text = cap;

            g.GetComponent<Button>().onClick.AddListener(() =>
            {
                panels[activePanel].caption = cap;
                UpdatePreview(activePanel);
                HighlightCaptionButtons(cap);
                CheckSubmitState();
            });
        }
    }

    // ========================= SELECTION =======================
    void SelectPanel(int idx)
    {
        activePanel = idx;
        HighlightPanels();
        HighlightDrawButtons(panels[idx].drawing);
        HighlightCaptionButtons(panels[idx].caption);
    }

    void HighlightPanels()
    {
        for (int i = 0; i < panelButtons.Length; i++)
        {
            var o = panelButtons[i].GetComponent<Outline>() ??
                    panelButtons[i].gameObject.AddComponent<Outline>();
            o.effectColor    = Color.blue;
            o.effectDistance = new Vector2(5, 5);
            o.enabled        = (i == activePanel);
        }
    }

    void HighlightDrawButtons(int drawingIdx)
    {
        foreach (Transform child in drawingContent)
        {
            var o = child.GetComponent<Outline>() ?? child.gameObject.AddComponent<Outline>();
            o.effectColor    = Color.blue;
            o.effectDistance = new Vector2(5, 5);
            o.enabled        = (child.name == drawingIdx.ToString());
        }
    }

    void HighlightCaptionButtons(string txt)
    {
        foreach (Transform child in captionContent)
        {
            var o = child.GetComponent<Outline>() ?? child.gameObject.AddComponent<Outline>();
            o.effectColor    = Color.blue;
            o.effectDistance = new Vector2(5, 5);

            var t = child.GetComponentInChildren<TMP_Text>();
            o.enabled = (t != null && t.text == txt);
        }
    }

    void UpdatePreview(int panelIdx)
    {
        if (panelIdx < 0 || panelIdx >= panels.Length) return;

        if (panels[panelIdx].drawing >= 0)
        {
            var tex = ImageBank.Instance.GetImage(panels[panelIdx].drawing);
            if (previewImages[panelIdx] != null) previewImages[panelIdx].texture = tex;
        }

        if (previewCaptions[panelIdx] != null)
            previewCaptions[panelIdx].text = panels[panelIdx].caption ?? "";
    }

    // =========================== SUBMIT ========================
    void CheckSubmitState()
    {
        bool ready = panels.Any(p => p.drawing >= 0); // at least one panel chosen
        if (submitButton != null) submitButton.interactable = ready;
    }

    void OnSubmitClicked()
    {
        bool allDrawingsChosen = panels.All(p => p.drawing >= 0);

        if (!allDrawingsChosen && confirmDialog != null)
        {
            confirmDialog.SetActive(true); // Yes → FinalSubmit(), No → CancelConfirm()
            return;
        }

        FinalSubmit();
    }

    // wired to the Yes button in the confirm dialog
    public void FinalSubmit()
    {
        // Convert local panels → ComicBank.Panel[]
        var payload = panels.Select(p => new ComicBank.Panel
        {
            drawingIndex = p.drawing,
            captionText  = p.caption
        }).ToArray();

        // 👇 Match your ComicBank signature: AddComic(string authorId, ComicBank.Panel[] panels)
        ComicBank.AddComic(playerId, payload);

        // 👇 Match your ComicBank.Count as a PROPERTY
        int expected = GameManager.Instance != null ? GameManager.Instance.PlayerCount : 1;
        if (ComicBank.Count >= expected)
        {
            Debug.Log("[ComicCreate] all players submitted – loading ComicVote");
            SceneManager.LoadScene("ComicVote");
        }
        else
        {
            Debug.Log("[ComicCreate] submission recorded – waiting for others");
        }
    }

    // wired to the No button
    public void CancelConfirm()
    {
        if (confirmDialog != null) confirmDialog.SetActive(false);
    }
}
