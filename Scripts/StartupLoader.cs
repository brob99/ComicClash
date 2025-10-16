using UnityEngine;
using UnityEngine.SceneManagement;

public class StartupLoader : MonoBehaviour
{
    void Start()
    {
        // Load the main menu or first gameplay scene after this initializes
        SceneManager.LoadScene("MainMenu");
    }
}
