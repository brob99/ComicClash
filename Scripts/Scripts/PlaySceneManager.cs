using UnityEngine;
using UnityEngine.SceneManagement;

public class PlaySceneManager : MonoBehaviour
{
    public void HostGame()
    {
        Debug.Log("HostGame() clicked");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetAsHost();
            GameManager.Instance.SetPlayerCount(1);  // Simulate 1 players
        }
        else
        {
            Debug.LogError("GameManager.Instance is null!");
        }

        SceneManager.LoadScene("Draw");  // Or use SceneFlowManager if you prefer
    }

    public void JoinGame()
    {
        Debug.Log("JoinGame() clicked");
        // Multiplayer not yet implemented
    }
}
