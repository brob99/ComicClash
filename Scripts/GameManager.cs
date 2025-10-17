using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public string PlayerId { get; private set; }
    public bool   IsHost   { get; private set; } = true;   // single-player acts as host
    public int    PlayerCount { get; private set; } = 1;

    [Header("Per-draw round requirement")]
    public int RequiredDrawingsPerRound = 3;

    // Reset at the start of EVERY Draw scene (round 1 and round 2)
    private readonly Dictionary<string,int> drawingsThisRound = new Dictionary<string,int>();

    // 0 = next Caption ends to MemeCreate, 1 = next Caption ends to ComicCreate
    private int captionStage = 0;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        PlayerId = System.Guid.NewGuid().ToString();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[GM] Loaded: {scene.name}");

        // ✅ New round starts any time we enter Draw → clear counters
        if (scene.name == "Draw")
        {
            drawingsThisRound.Clear();
            Debug.Log("[GM] Reset drawing counters for new Draw round.");
        }
    }

    public void SetAsHost()              { IsHost = true; }
    public void SetPlayerCount(int n)    { PlayerCount = Mathf.Max(1, n); }

    /// Called by DrawingManager after each Submit
    public void RegisterDrawing(string playerId)
    {
        if (!drawingsThisRound.ContainsKey(playerId))
            drawingsThisRound[playerId] = 0;

        drawingsThisRound[playerId]++;
        Debug.Log($"[Draw] {playerId} {drawingsThisRound[playerId]}/{RequiredDrawingsPerRound}");

        // In local tests you're host; in real MP only host advances scenes.
        if (!IsHost && PlayerCount > 1) return;

        if (AllPlayersReachedQuota())
        {
            Debug.Log("[Draw] All players met quota → loading Caption");
            SceneManager.LoadScene("Caption");
        }
    }

    private bool AllPlayersReachedQuota()
    {
        // Wait until we’ve seen every player at least once
        if (drawingsThisRound.Count < PlayerCount) return false;

        foreach (var kv in drawingsThisRound)
            if (kv.Value < RequiredDrawingsPerRound) return false;

        return true;
    }

    /// Called by CaptionTimer when the 60s ends (or if you add a “Next” button)
    public void OnCaptionPhaseEnded()
    {
        if (!IsHost && PlayerCount > 1) return;

        if (captionStage == 0)
        {
            // After Caption #1 → MemeCreate
            captionStage = 1; // next Caption ends to ComicCreate
            Debug.Log("[GM] Caption ended (stage 0) → MemeCreate");
            SceneManager.LoadScene("MemeCreate");
        }
        else
        {
            // After Caption #2 → ComicCreate
            captionStage = 0; // reset so a new cycle starts at MemeCreate again
            Debug.Log("[GM] Caption ended (stage 1) → ComicCreate");
            SceneManager.LoadScene("ComicCreate");
        }
    }
}
