using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComicVoteManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject comicCardPrefab;    // Prefab that has ComicCardUI
    [SerializeField] private RectTransform comicContainer;  // ScrollView/Viewport/Content

    private string playerId;
    private bool hasVoted;
    private int expectedVotes = 1;

    void Start()
    {
        VoteTracker.Reset();

        if (GameManager.Instance != null)
        {
            playerId       = GameManager.Instance.PlayerId;
            expectedVotes  = GameManager.Instance.PlayerCount;
        }

        PopulateComics();
    }

    private void PopulateComics()
    {
        // Get comics from the bank
        IReadOnlyList<ComicBank.ComicEntry> comics = ComicBank.GetAllComics();
        Debug.Log($"[ComicVote] Comics available (raw): {comics.Count}");

        // Clear previous children
        for (int i = comicContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(comicContainer.GetChild(i).gameObject);
        }

        // Instantiate & bind
        foreach (var entry in comics)
        {
            var go = Instantiate(comicCardPrefab, comicContainer);
            var ui = go.GetComponentInChildren<ComicCardUI>(true);
            if (ui == null)
            {
                Debug.LogError("[ComicVote] ComicCard prefab missing ComicCardUI.");
                continue;
            }

            SizeCardToViewport(go.GetComponent<RectTransform>());

            ui.Bind(entry, () =>
            {
                if (hasVoted) return;
                hasVoted = true;

                // Your existing tracker / GM hooks
                VoteTracker.RegisterComicVote(playerId, entry.id);
                TryAdvanceAfterVote();
            });
        }

        Debug.Log($"[ComicVote] Rendered cards: {comicContainer.childCount}");
    }

    private void TryAdvanceAfterVote()
    {
        // If you already have a central “advance after all voted” flow,
        // keep using it; otherwise this simple check is fine for solo tests.
        if (VoteTracker.ComicVotesCount >= expectedVotes)
        {
            GameManager.Instance.AdvancePhase("ComicVote");
        }
    }

    // Make each card fill the viewport vertically (keeps a 3:4 width:height ratio)
    private void SizeCardToViewport(RectTransform card)
    {
        if (card == null) return;

        var viewport = comicContainer.GetComponentInParent<RectTransform>(); // ScrollView/Viewport
        if (viewport == null) return;

        var le = card.GetComponent<LayoutElement>();
        if (le == null) le = card.gameObject.AddComponent<LayoutElement>();

        float targetH = viewport.rect.height - 30f;  // padding
        le.preferredHeight = targetH;

        // keep a portrait card; width = height * 0.75 (420x560 default => 3:4)
        var arf = card.GetComponent<AspectRatioFitter>();
        if (arf == null) arf = card.gameObject.AddComponent<AspectRatioFitter>();
        arf.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
        arf.aspectRatio = 0.75f;

        // Ensure RawImages preserve aspect (in case your art isn’t square)
        foreach (var raw in card.GetComponentsInChildren<RawImage>(true))
            raw.gameObject.GetComponent<RawImage>().preserveAspect = true;
    }
}
