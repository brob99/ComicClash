using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComicCardUI : MonoBehaviour
{
    [Header("Assign in prefab")]
    public RawImage[] cellImages = new RawImage[4];   // TL, TR, BL, BR
    public TMP_Text[] captionTexts = new TMP_Text[4]; // TL, TR, BL, BR
    public Button voteButton;

    private ComicBank.ComicEntry bound;

    public void Bind(ComicBank.ComicEntry entry, System.Action<ComicBank.ComicEntry> onVote)
    {
        bound = entry;

        for (int i = 0; i < 4; i++)
        {
            // drawings
            int idx = (entry.drawings != null && i < entry.drawings.Length) ? entry.drawings[i] : -1;
            if (i < cellImages.Length && cellImages[i] != null)
            {
                cellImages[i].texture = (idx >= 0 && ImageBank.Instance != null)
                    ? ImageBank.Instance.GetImage(idx)
                    : null;
            }

            // captions
            string cap = (entry.captions != null && i < entry.captions.Length) ? entry.captions[i] : string.Empty;
            if (i < captionTexts.Length && captionTexts[i] != null)
                captionTexts[i].text = cap ?? string.Empty;
        }

        if (voteButton != null)
        {
            voteButton.onClick.RemoveAllListeners();
            voteButton.onClick.AddListener(() => onVote?.Invoke(bound));
        }
    }
}
