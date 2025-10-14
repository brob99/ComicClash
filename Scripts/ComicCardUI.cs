using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ComicCardUI : MonoBehaviour
{
    [Header("Panel images (2x2 order: 0,1,2,3)")]
    public RawImage[] drawingCells = new RawImage[4];

    [Header("Captions under each panel (0..3)")]
    public TMP_Text[] captionLabels = new TMP_Text[4];

    [Header("Vote button on the card")]
    public Button voteButton;

    private ComicBank.ComicEntry _bound;

    /// <summary>Bind a ComicEntry to this UI and optionally hook a vote callback.</summary>
    public void Bind(ComicBank.ComicEntry entry, Action onVote = null)
    {
        _bound = entry;

        for (int i = 0; i < 4; i++)
        {
            // drawings
            if (i < entry.drawingIndices?.Length && entry.drawingIndices[i] >= 0)
            {
                if (drawingCells != null && i < drawingCells.Length && drawingCells[i] != null)
                {
                    drawingCells[i].texture = ImageBank.Instance != null
                        ? ImageBank.Instance.GetImage(entry.drawingIndices[i])
                        : null;
                }
            }
            else
            {
                if (drawingCells != null && i < drawingCells.Length && drawingCells[i] != null)
                    drawingCells[i].texture = null;
            }

            // captions
            string cap = (i < entry.captions?.Length) ? (entry.captions[i] ?? "") : "";
            if (captionLabels != null && i < captionLabels.Length && captionLabels[i] != null)
                captionLabels[i].text = cap;
        }

        if (voteButton != null)
        {
            voteButton.onClick.RemoveAllListeners();
            if (onVote != null) voteButton.onClick.AddListener(() => onVote());
        }
    }

    // Optional convenience if you ever need to retrieve the bound entry.
    public ComicBank.ComicEntry GetBound() => _bound;
}
