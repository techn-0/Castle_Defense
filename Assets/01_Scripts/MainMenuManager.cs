using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void OnStartClicked()
    {
        SceneManager.LoadScene("Intro");
    }
}
