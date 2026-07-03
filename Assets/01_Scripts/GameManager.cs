using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager I;
    public GameObject gameOverPanel;
    public GameObject victoryPanel;

    void Awake() { I = this; }

    public bool IsPlaying => !gameOverPanel.activeSelf && !victoryPanel.activeSelf;

    public void GameOver()
    {
        if (!IsPlaying) return;
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Victory()
    {
        if (!IsPlaying) return;
        victoryPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
