using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class EndingUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Button restartButton;
    public Button quitButton;
    public GameObject endingPanel;

    void Start()
    {
        restartButton.onClick.AddListener(RestartGame);
        quitButton.onClick.AddListener(QuitGame);
        endingPanel.SetActive(false);
    }

    public void ShowEnding(string title, string description)
    {
        titleText.text = title;
        descriptionText.text = description;
        endingPanel.SetActive(true);
        Time.timeScale = 0;
    }

    void RestartGame()
    {
        Time.timeScale = 1;
        PlayerPrefs.DeleteKey("SaveData");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
        Application.ExternalEval("window.location.reload();");
#else
        Application.Quit();
#endif
    }
}
