using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToMainMenu : MonoBehaviour
{
    public void _GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}