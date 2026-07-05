using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public InfoPanelUI infoPanel;

    public void OnStartClicked()
    {
        SceneManager.LoadScene("Intro");
    }

    public void OnInfoClicked()
    {
        infoPanel.Show();
    }
}
