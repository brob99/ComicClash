using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ComicVoteManager : MonoBehaviour
{
    [Header("References")]
    public GameObject comicCardPrefab;     // Prefab with ComicCardUI
    public Transform comicContainer;        // ScrollView/Viewport/Content

    private string _playerId = "local";
    private bool _hasVoted;
    private int _expectedVotes = 1;

    void Start()
    {
        VoteTracker.Reset();

        if (GameManager.Instance != null)
        {
            _playerId = GameManager.Instance.PlayerId;
            _expectedVotes = GameManager.Instance.PlayerCount;
        }

        PopulateComics();
    }

    private void PopulateComics()
    {
        // Clear any existing
        if (comicContainer != null)
        {
            for (int i = comicContainer.childCount - 1; i >= 0; i--)
                Destroy(comicContainer.GetChild(i).gameObject);
        }

        IReadOnlyList<ComicBank.ComicEntry> comics = ComicBank.GetAllComics();
        int total = comics?.Count ?? 0;
        Debug.Log($"[ComicVote] Comics available: {total}");

        if (total == 0)
        {
            Debug.LogWarning("[ComicVote] No comics to vote on.");
            return;
        }

        for (int i = 0; i < total; i++)
        {
            var comic = comics[i];

            // (Optional) Skip own comic
            if (comic.authorId == _playerId)
                continue;

            var go = Instantiate(comicCardPrefab, comicContainer);
            var ui = go.GetComponent<ComicCardUI>();
            if (ui == null)
            {
                Debug.LogError("[ComicVote] ComicCard prefab missing ComicCardUI.");
                continue;
            }

            ui.Bind(comic, () => VoteFor(comic));
        }
    }

    private void VoteFor(ComicBank.ComicEntry comic)
    {
        if (_hasVoted) return;

        _hasVoted = true;
        VoteTracker.votesSubmitted++;
        Debug.Log($"[ComicVote] {_playerId} voted for comic {comic.id}. Votes: {VoteTracker.votesSubmitted}/{_expectedVotes}");

        if (VoteTracker.votesSubmitted >= _expectedVotes)
        {
            Debug.Log("[ComicVote] All votes in. Advancing…");
            VoteTracker.Reset();

            if (GameManager.Instance != null)
                GameManager.Instance.AdvancePhase("ComicVote"); // moves to Results (or next phase)
            else
                SceneManager.LoadScene("Results");
        }
    }
}
