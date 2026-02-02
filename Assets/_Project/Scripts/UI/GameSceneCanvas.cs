using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSceneCanvas : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button backButton;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button gameOverMenuButton;

    [Header("Scene")]
    [SerializeField] private string menuSceneName = "MenuScene";

    private void Awake()
    {
        if (backButton == null)
        {
            Debug.LogError("[GameSceneCanvas] Back button is missing!");
            return;
        }

        backButton.onClick.AddListener(OnBackClicked);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (gameOverMenuButton != null)
            gameOverMenuButton.onClick.AddListener(OnBackClicked);
    }

    private void OnDestroy()
    {
        if (backButton != null)
            backButton.onClick.RemoveListener(OnBackClicked);

        if (gameOverMenuButton != null)
            gameOverMenuButton.onClick.RemoveListener(OnBackClicked);
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Oyun bittiğinde geri butonunu da disable edebilirsin (opsiyonel)
        // if (backButton != null) backButton.interactable = false;
    }

    private void OnBackClicked()
    {
        SceneManager.LoadScene(menuSceneName);
    }
}
