using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToMainMenu : MonoBehaviour
{
    public void _GoToMainMenu()
    {
        // 일시정지 패널 등에서 호출될 수 있어, 멈춰있던 timeScale을 복구하고 씬을 전환한다.
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}