using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("PlayScene"); // Replace "GameScene" with the actual scene name.
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
