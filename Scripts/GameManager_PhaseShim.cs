using UnityEngine.SceneManagement;

public static class GameManager_PhaseShim
{
    // Fallback: no-arg version
    public static void AdvancePhase(this GameManager gm)
    {
        // ComicVote → Results is the only place still calling AdvancePhase in your latest errors.
        // If you want something else, change the scene name here.
        SceneManager.LoadScene("Results");
    }

    // Keyed version – we just forward to the no-arg for now so both signatures compile.
    public static void AdvancePhase(this GameManager gm, string key)
    {
        // If you later want keyed routing, switch on 'key' here.
        // For now, do the same as above so your build compiles.
        gm.AdvancePhase();
    }
}
