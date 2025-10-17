using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComicVoteManager : MonoBehaviour
{
    [Header("References")]
    public GameObject comicCardPrefab;   // Prefab with ComicCardUI
    public RectTransform comicContainer; // ScrollRect Content (or any parent the cards go under)

    [Header("Layout")]
    [Tooltip("Margin to subtract from viewport width for the card width.")]
    public float horizontalMargin = 60f;

    private int _votes = 0;
    private int _expectedVotes = 1;
    private string _playerId = "local";

    void Start()
    {
        if (GameManager.Instance != null)
        {
            _playerId = GameManager.Instance.PlayerId;
            _expectedVotes = Mathf.Max(1, GameManager.Instance.PlayerCount);
        }

        Populate();
    }

    void Populate()
    {
        // Get comics
        var comics = ComicBank.Instance != null ? ComicBank.Instance.All() : new List<ComicBank.ComicEntry>();
        Debug.Log($"[ComicVote] Comics available: {comics.Count}");

        // Find available viewport width (parent of content is usually the Viewport)
        float availableW = ((RectTransform)comicContainer.parent).rect.width;
        float cardW = Mathf.Max(200f, availableW - horizontalMargin);

        foreach (var entry in comics)
        {
            var go = Instantiate(comicCardPrefab, comicContainer);
            var ui = go.GetComponent<ComicCardUI>();

            // Size the card to (almost) viewport width
            var le = go.GetComponent<LayoutElement>();
            if (!le) le = go.AddComponent<LayoutElement>();
            le.preferredWidth = cardW;
            le.flexibleWidth = 0;
            le.flexibleHeight = 0;

            ui.Bind(entry, () => OnVote(entry, ui));
            ui.FitNow();
        }
    }

    void OnVote(ComicBank.ComicEntry entry, ComicCardUI ui)
    {
        // Disable all vote buttons after this player has voted
        foreach (var other in comicContainer.GetComponentsInChildren<ComicCardUI>(true))
        {
            if (other.voteButton != null)
                other.voteButton.interactable = false;
        }

        _votes++;
        Debug.Log($"[ComicVote] {_playerId} voted for comic {entry?.id}. Votes now: {_votes}/{_expectedVotes}");

        // Everyone voted?
        if (_votes >= _expectedVotes)
        {
            Debug.Log("[ComicVote] All players voted — advancing.");
            // Advance to results (or whatever your next scene is)
            SceneFlowManager.Instance?.LoadResults(); // <— replace with your existing scene flow call
        }
        else
        {
            Debug.Log("[ComicVote] Waiting for other players...");
        }
    }
}
