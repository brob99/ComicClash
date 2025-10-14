using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ComicVoteManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject comicCardPrefab;   // Prefab with ComicCardUI
    [SerializeField] private Transform comicContainer;     // ScrollView/Viewport/Content

    private string playerId = "local";
    private int expectedVotes = 1;
    private bool hasVoted = false;

    void Start()
    {
        VoteTracker.Reset();

        if (GameManager.Instance != null)
        {
            playerId = GameManager.Instance.PlayerId;
            expectedVotes = Mathf.Max(1, GameManager.Instance.PlayerCount);
        }

        PopulateComics();
    }

    private void PopulateComics()
    {
        // Clear
        for (int i = comicContainer.childCount - 1; i >= 0; i--)
            Destroy(comicContainer.GetChild(i).gameObject);

        var all = ComicBank.GetAllComics();
        Debug.Log($"[ComicVote] Bank had {all.Count} comics.");

        bool excludeOwn = GameManager.Instance == null ? false : (GameManager.Instance.PlayerCount > 1);

        // De-dup by a stable key (id or drawings+captions)
        IEnumerable<ComicBank.ComicEntry> stream = all;
        var list = stream
            .Where(c => !excludeOwn || c.playerId != playerId)
            .GroupBy(c => string.IsNullOrEmpty(c.id)
                ? $"{string.Join(",", c.drawings ?? new int[4])}|{string.Join("|", c.captions ?? new string[4])}"
                : c.id)
            .Select(g => g.First())
            .ToList();

        Debug.Log($"[ComicVote] Showing {list.Count} comics after filtering (excludeOwn={excludeOwn}).");

        foreach (var comic in list)
        {
            var go = Instantiate(comicCardPrefab, comicContainer);
            var ui = go.GetComponent<ComicCardUI>();
            if (ui == null)
            {
                Debug.LogError("[ComicVote] ComicCard prefab missing ComicCardUI.");
                Destroy(go);
                continue;
            }
            ui.Bind(comic, OnVote);
        }

        if (list.Count == 0)
            Debug.Log("[ComicVote] No other comics to vote on (solo or none submitted).");
    }

    private void OnVote(ComicBank.ComicEntry comic)
    {
        if (hasVoted) return;
        hasVoted = true;

        VoteTracker.votesSubmitted++;
        Debug.Log($"[ComicVote] {playerId} voted for comic by {comic.playerId}. votes={VoteTracker.votesSubmitted}/{expectedVotes}");

        if (VoteTracker.votesSubmitted >= expectedVotes)
        {
            Debug.Log("[ComicVote] All players voted — advancing phase.");
            if (GameManager.Instance != null)
                GameManager.Instance.AdvancePhase("ComicVote");
            else
                SceneManager.LoadScene("Results"); // fallback
        }
    }
}
