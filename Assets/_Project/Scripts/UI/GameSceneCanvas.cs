using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSceneCanvas : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button backButton;

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
    }

    private void OnDestroy()
    {
        // Good practice: remove listener to avoid potential leaks in some cases
        if (backButton != null)
            backButton.onClick.RemoveListener(OnBackClicked);
    }

    private void OnBackClicked()
    {
        // (Optional) burada save almak istersen ileride ekleriz
        SceneManager.LoadScene(menuSceneName);
    }
}
