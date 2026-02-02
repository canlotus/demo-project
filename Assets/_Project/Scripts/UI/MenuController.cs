using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private DifficultyDatabaseSO difficultyDb;

    [Header("UI")]
    [SerializeField] private TMP_Dropdown difficultyDropdown;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "GameScene";

    private void Awake()
    {
        if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGameClicked);
        if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);
        BuildDropdownFromConfig();
    }

    private void OnEnable()
    {
        RefreshContinueButton();
    }

    private void BuildDropdownFromConfig()
    {
        if (difficultyDropdown == null || difficultyDb == null) return;

        difficultyDropdown.ClearOptions();

        var opts = new List<string>();
        foreach (var d in difficultyDb.difficulties)
            opts.Add(d.id.ToString()); // "Easy/Medium/Hard"

        if (opts.Count == 0)
        {
            opts.Add("Easy");
            Debug.LogWarning("[MenuController] DifficultyDatabase is empty. Add entries in ScriptableObject.");
        }

        difficultyDropdown.AddOptions(opts);
        difficultyDropdown.value = 0;
        difficultyDropdown.RefreshShownValue();
    }

    private void RefreshContinueButton()
    {
        if (continueButton == null) return;
        continueButton.interactable = SaveSystem.HasSave();
    }

    private void OnNewGameClicked()
    {
        if (difficultyDb == null) return;
        var entry = difficultyDb.GetByIndex(difficultyDropdown != null ? difficultyDropdown.value : 0);
        if (entry == null) return;
        if (entry.PlayableCells % 2 != 0) return;
        int seed = GenerateSeed();
        var emptyArr = entry.emptyIndices == null ? Array.Empty<int>() : entry.emptyIndices.ToArray();
        var layout = new BoardLayoutData(entry.rows, entry.cols, emptyArr);
        GameSession.SetNewGame(entry.id, layout, seed);
        var save = new SaveData
        {
            difficulty = entry.id,
            rows = entry.rows,
            cols = entry.cols,
            emptyIndices = emptyArr,
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

    private int GenerateSeed()
    {
        long ticks = DateTime.UtcNow.Ticks;
        int seed = (int)(ticks % int.MaxValue);
        return Mathf.Abs(seed);
    }
}
