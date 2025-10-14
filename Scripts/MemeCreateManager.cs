using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MemeCreateManager : MonoBehaviour
{
    [Header("Prefabs & References")]
    public GameObject captionButtonPrefab;    // prefab for each caption tile
    public Transform captionGridParent;       // GridLayoutGroup in the CaptionContainer
    public Button    submitButton;

    // runtime
    private readonly List<GameObject> captionButtons = new();
    private int    selectedCaptionIndex = -1;
    private string selectedCaption      = null;

    // tuning
    private const int captionsToShow = 20;   // try to give this many

    void Start()
    {
        if (submitButton != null) submitButton.onClick.AddListener(SubmitMeme);
        if (submitButton != null) submitButton.interactable = false;

        PopulateCaptionPool();
    }

    // ---------------- captions ----------------
    void PopulateCaptionPool()
    {
        captionButtons.Clear();

        var pool = CaptionBank.GetCaptions()
                              .OrderBy(_ => Random.value)
                              .Take(captionsToShow)
                              .ToList();

        // Grid: 2 rows, variable columns with autosized width
        var grid = captionGridParent.GetComponent<GridLayoutGroup>();
        grid.constraint      = GridLayoutGroup.Constraint.FixedRowCount;
        grid.constraintCount = 2;

        var rt       = (RectTransform)captionGridParent;
        float space  = grid.spacing.x;
        int cols     = Mathf.Max(1, Mathf.CeilToInt(pool.Count / 2f));
        float cellW  = (rt.rect.width - space * (cols - 1)) / cols;
        grid.cellSize = new Vector2(cellW, grid.cellSize.y);

        // clear old
        foreach (Transform c in captionGridParent) Destroy(c.gameObject);

        for (int i = 0; i < pool.Count; i++)
        {
            string cap = pool[i];
            GameObject btnGO = Instantiate(captionButtonPrefab, captionGridParent);
            captionButtons.Add(btnGO);

            var t = btnGO.GetComponentInChildren<TMP_Text>(true);
            if (t != null)
            {
                t.text             = cap;
                t.enableAutoSizing = true;
            }

            int captured = i;
            btnGO.GetComponent<Button>().onClick.AddListener(() =>
            {
                selectedCaption      = cap;
                selectedCaptionIndex = captured;
                HighlightCaptions();
                CheckReady();
            });
        }
    }

    void HighlightCaptions()
    {
        for (int i = 0; i < captionButtons.Count; i++)
        {
            bool active = i == selectedCaptionIndex;
            var o = captionButtons[i].GetComponent<Outline>() ??
                    captionButtons[i].AddComponent<Outline>();
            o.effectColor    = Color.blue;
            o.effectDistance = new Vector2(5, 5);
            o.enabled        = active;
        }
    }

    // ---------------- submit ----------------
    void CheckReady()
    {
        bool haveDrawing = MemeCreateState.SelectedDrawing != -1;
        bool haveCaption = !string.IsNullOrEmpty(selectedCaption);
        if (submitButton != null) submitButton.interactable = haveDrawing && haveCaption;
    }

    void SubmitMeme()
    {
        if (submitButton != null && !submitButton.interactable) return; // safety

        string me = GameManager.Instance != null ? GameManager.Instance.PlayerId : "Unknown";
        MemeBank.AddMeme(MemeCreateState.SelectedDrawing, selectedCaption, me);

        if (submitButton != null) submitButton.interactable = false;

        int expected = GameManager.Instance != null ? GameManager.Instance.PlayerCount : 1;
        if (MemeBank.Count >= expected)
        {
            Debug.Log("[MemeCreate] all players submitted – loading MemeVote");
            SceneManager.LoadScene("MemeVote");
        }
        else
        {
            Debug.Log("[MemeCreate] submission recorded – waiting for others");
        }
    }
}
