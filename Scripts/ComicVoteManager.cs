using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ComicVoteManager : MonoBehaviour
{
    [Header("References")]
    public GameObject comicCardPrefab;   // Prefab with ComicCardUI component
    public Transform  comicContainer;    // ScrollView/Viewport/Content

    private string playerId = "local";
    private bool   hasVoted = false;
    private int    expectedVotes = 1;

    void Start()
    {
        VoteTracker.Reset();

        if (GameManager.Instance != null)
        {
            playerId      = GameManager.Instance.PlayerId;
            expectedVotes = GameManager.Instance.PlayerCount;
        }

        PopulateComics();
    }

    void PopulateComics()
    {
        // Clear existing cards
        for (int i = comicContainer.childCount - 1; i >= 0; i--)
            Destroy(comicContainer.GetChild(i).gameObject);

        // ✅ Use IReadOnlyList directly (no List, no LINQ)
        IReadOnlyList<ComicBank.ComicEntry> comics = ComicBank.GetAllComics();

        bool soloMode = (GameManager.Instance == null || GameManager.Instance.PlayerCount <= 1);

        foreach (var comic in comics)
        {
            // In multiplayer, skip your own comic
            if (!soloMode && comic.playerId == playerId) continue;

            SpawnCard(comic);
        }
    }

    void SpawnCard(ComicBank.ComicEntry comic)
    {
        GameObject go = Instantiate(comicCardPrefab, comicContainer);
        var card = go.GetComponent<ComicCardUI>();
        if (card == null)
        {
            Debug.LogError("[ComicVote] ComicCard prefab missing ComicCardUI.");
            return;
        }

        card.Bind(comic, () =>
        {
            if (hasVoted) return;

            VoteForComic(comic);
            hasVoted = true;

            if (card.voteButton) card.voteButton.interactable = false;
        });
    }

    void VoteForComic(ComicBank.ComicEntry comic)
    {
        Debug.Log($"[ComicVote] {playerId} voted for comic by {comic.playerId}");

        int total = ++VoteTracker.votesSubmitted;
        Debug.Log($"[ComicVote] votes: {total}/{expectedVotes}");

        if (total >= expectedVotes)
        {
            Debug.Log("[ComicVote] All players voted — advancing to Results.");
            LoadResultsOrFallback();
        }
    }

    void LoadResultsOrFallback()
    {
        if (SceneExistsInBuild("Results"))
            SceneManager.LoadScene("Results");
        else
            SceneManager.LoadScene("MainMenu");
    }

    static bool SceneExistsInBuild(string sceneName)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            var path = SceneUtility.GetScenePathByBuildIndex(i);
            var name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName) return true;
        }
        return false;
    }
}
