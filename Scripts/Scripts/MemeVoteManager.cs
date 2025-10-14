using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MemeVoteManager : MonoBehaviour
{
    [Header("References")]
    public GameObject memeDisplayPrefab;   // children: Drawing (RawImage), Caption (TMP_Text), VoteButton (Button)
    public Transform  memeContainer;       // Content of your horizontal ScrollView

    private string playerId;
    private bool   hasVoted = false;
    private int    expectedVotes = 1;

    void Start()
    {
        VoteTracker.Reset();

        playerId      = GameManager.Instance != null ? GameManager.Instance.PlayerId    : "Unknown";
        expectedVotes = GameManager.Instance != null ? GameManager.Instance.PlayerCount : 1;

        PopulateMemes();
    }

    void PopulateMemes()
    {
        List<MemeBank.MemeEntry> memes = MemeBank.GetAllMemes();

        bool soloMode = (GameManager.Instance == null || GameManager.Instance.PlayerCount <= 1);

        foreach (var meme in memes)
        {
            // In multiplayer, don't show your own meme
            if (!soloMode && meme.playerId == playerId) continue;

            SpawnCard(meme);
        }
    }

    void SpawnCard(MemeBank.MemeEntry meme)
    {
        GameObject card = Instantiate(memeDisplayPrefab, memeContainer);

        RawImage img = card.transform.Find("Drawing").GetComponent<RawImage>();
        TMP_Text cap = card.transform.Find("Caption").GetComponent<TMP_Text>();
        Button   btn = card.transform.Find("VoteButton").GetComponent<Button>();

        img.texture = ImageBank.Instance.GetImage(meme.drawingIndex);
        cap.text    = meme.captionText;

        btn.onClick.AddListener(() =>
        {
            if (hasVoted) return;

            VoteForMeme(meme);
            hasVoted = true;
            btn.interactable = false;
        });
    }

    void VoteForMeme(MemeBank.MemeEntry meme)
    {
        Debug.Log($"[MemeVote] {playerId} voted for meme by {meme.playerId}");

        int total = ++VoteTracker.votesSubmitted;
        Debug.Log($"[MemeVote] votes: {total}/{expectedVotes}");

        if (total >= expectedVotes)
        {
            Debug.Log("[MemeVote] All players voted — loading Draw (round 2)...");
            // Just load Draw; your GameManager's Draw scene logic should handle the next step.
            SceneManager.LoadScene("Draw");
        }
    }
}
