using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFlowManager : MonoBehaviour
{
    public void GoToDrawScene()
    {
        SceneManager.LoadScene("Draw");
    }

    public void GoToCaptionScene()
    {
        SceneManager.LoadScene("Caption");
    }

    public void GoToMemeCreateScene()
    {
        SceneManager.LoadScene("MemeCreate");
    }

    public void GoToMemeVoteScene()
    {
        SceneManager.LoadScene("MemeVote");
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void GoToPlayScene()
    {
        SceneManager.LoadScene("PlayScene");
    }

    public void GoToResults()
    {
        SceneManager.LoadScene("Results");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
