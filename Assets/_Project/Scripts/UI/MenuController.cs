using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Dropdown difficultyDropdown;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Difficulty -> Board Config")]
    [SerializeField] private BoardConfig easyConfig = new BoardConfig(2, 2);
    [SerializeField] private BoardConfig mediumConfig = new BoardConfig(4, 4);
    [SerializeField] private BoardConfig hardConfig = new BoardConfig(5, 6);

    private void Awake()
    {
        // Basic null checks to avoid warnings/errors
        if (difficultyDropdown == null) Debug.LogError("[MenuController] difficultyDropdown is missing!");
        if (newGameButton == null) Debug.LogError("[MenuController] newGameButton is missing!");
        if (continueButton == null) Debug.LogError("[MenuController] continueButton is missing!");

        if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGameClicked);
        if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);

        SetupDropdownIfNeeded();
    }

    private void OnEnable()
    {
        RefreshContinueButton();
    }

    private void SetupDropdownIfNeeded()
    {
        if (difficultyDropdown == null) return;
        if (difficultyDropdown.options == null || difficultyDropdown.options.Count == 0)
        {
            difficultyDropdown.ClearOptions();
            difficultyDropdown.AddOptions(new List<string> { "Easy", "Medium", "Hard" });
            difficultyDropdown.value = 0;
            difficultyDropdown.RefreshShownValue();
        }
    }

    private void RefreshContinueButton()
    {
        if (continueButton == null) return;

        bool hasSave = SaveSystem.HasSave();
        continueButton.interactable = hasSave;
    }

    private void OnNewGameClicked()
    {
        var difficulty = GetDifficultyFromDropdown();
        var config = GetConfigForDifficulty(difficulty);
        int seed = GenerateSeed();
        GameSession.SetNewGame(difficulty, config, seed);
        var save = new SaveData
        {
            difficulty = difficulty,
            rows = config.rows,
            cols = config.cols,
            seed = seed,
            score = 0,
            matchedCardIds = Array.Empty<int>()
        };
        SaveSystem.Save(save);

        SceneManager.LoadScene(gameSceneName);
    }

    private void OnContinueClicked()
    {
        if (!SaveSystem.HasSave())
        {
            RefreshContinueButton();
            return;
        }

        GameSession.SetContinue();
        SceneManager.LoadScene(gameSceneName);
    }

    private Difficulty GetDifficultyFromDropdown()
    {
        if (difficultyDropdown == null) return Difficulty.Easy;
        int idx = difficultyDropdown.value;
        return idx switch
        {
            0 => Difficulty.Easy,
            1 => Difficulty.Medium,
            2 => Difficulty.Hard,
            _ => Difficulty.Easy
        };
    }

    private BoardConfig GetConfigForDifficulty(Difficulty difficulty)
    {
        return difficulty switch
        {
            Difficulty.Easy => easyConfig,
            Difficulty.Medium => mediumConfig,
            Difficulty.Hard => hardConfig,
            _ => easyConfig
        };
    }

    private int GenerateSeed()
    {
        long ticks = DateTime.UtcNow.Ticks;
        int seed = (int)(ticks % int.MaxValue);
        return Mathf.Abs(seed);
    }
}
