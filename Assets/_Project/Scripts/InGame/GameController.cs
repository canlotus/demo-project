using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private BoardBuilder boardBuilder;

    [Header("Config")]
    [SerializeField] private DifficultyDatabaseSO difficultyDb;

    [Header("UI")]

    [SerializeField] private GameSceneCanvas gameSceneCanvas;
    [SerializeField] private TMP_Text matchCountText;
    [SerializeField] private TMP_Text attemptCountText;

    private DifficultyEntry _entry;
    private SaveData _save;

    private bool _inputLocked;
    private bool _previewRunning;

    private CardView _first;
    private CardView _second;

    private readonly HashSet<int> _matchedCellSet = new HashSet<int>();

    private void Start()
    {
        if (boardBuilder == null)
        {
            Debug.LogError("[GameController] boardBuilder missing!");
            return;
        }
        if (difficultyDb == null)
        {
            Debug.LogError("[GameController] difficultyDb missing!");
            return;
        }
        if (matchCountText == null || attemptCountText == null)
        {
            Debug.LogError("[GameController] TMP refs missing (matchCountText / attemptCountText)!");
            return;
        }

        DifficultyId diff;
        BoardLayoutData layout;
        int seed;

        SaveData loaded = null;
        bool hasSave = (GameSession.StartMode == StartMode.Continue) && SaveSystem.TryLoad(out loaded);

        if (hasSave && loaded != null)
        {
            _save = loaded;
            diff = _save.difficulty;
            layout = new BoardLayoutData(_save.rows, _save.cols, _save.emptyIndices ?? Array.Empty<int>());
            seed = _save.seed;

            _matchedCellSet.Clear();
            if (_save.matchedCellIndices != null)
                for (int i = 0; i < _save.matchedCellIndices.Length; i++)
                    _matchedCellSet.Add(_save.matchedCellIndices[i]);
        }
        else
        {
            diff = GameSession.Difficulty;
            layout = GameSession.Layout;
            seed = GameSession.Seed;

            _save = new SaveData
            {
                difficulty = diff,
                rows = layout.rows,
                cols = layout.cols,
                emptyIndices = layout.emptyIndices ?? Array.Empty<int>(),
                seed = seed,
                attempts = 0,
                matches = 0,
                previewDone = false,
                matchedCellIndices = Array.Empty<int>()
            };

            _matchedCellSet.Clear();
            SaveSystem.Save(_save);
        }

        _entry = difficultyDb.Get(diff);
        if (_entry == null)
        {
            Debug.LogError($"[GameController] Difficulty entry not found: {diff}");
            return;
        }

        boardBuilder.Build(layout, seed, _entry.faceSprites);

        ApplyMatchedFromSave();

        foreach (var card in boardBuilder.SpawnedCards)
            card.Clicked += OnCardClicked;

        RefreshUI();

        if (!_save.previewDone && _entry.previewSeconds > 0f)
            StartCoroutine(PreviewRoutine(_entry.previewSeconds));
        else
            boardBuilder.ForceAllUnmatchedFaceDownInstant();
    }

    private void OnDestroy()
    {
        if (boardBuilder == null) return;
        foreach (var card in boardBuilder.SpawnedCards)
            if (card != null) card.Clicked -= OnCardClicked;
    }

    private void ApplyMatchedFromSave()
    {
        if (_matchedCellSet.Count == 0) return;

        foreach (var cellIndex in _matchedCellSet)
        {
            if (boardBuilder.TryGetCardByCellIndex(cellIndex, out var card) && card != null)
            {
                // ✅ layout bozulmasın diye SetActive(false) yok:
                card.MakeEmptyInstant();
            }
        }
    }

    private IEnumerator PreviewRoutine(float seconds)
    {
        _previewRunning = true;
        _inputLocked = true;

        yield return new WaitForSecondsRealtime(seconds);

        // Preview bitti -> unmatched kartları kapat
        foreach (var card in boardBuilder.SpawnedCards)
        {
            if (card == null) continue;
            if (card.IsMatched) continue;

            // matched değilse kapat
            yield return StartCoroutine(card.FlipTo(false));
        }

        _save.previewDone = true;
        SaveSystem.Save(_save);

        _inputLocked = false;
        _previewRunning = false;
    }

    private void OnCardClicked(CardView card)
    {
        if (card == null) return;
        if (_previewRunning) return;
        if (_inputLocked) return;
        if (card.IsMatched) return;

        if (_first == card) return;

        StartCoroutine(HandleClickRoutine(card));
    }

    private IEnumerator HandleClickRoutine(CardView card)
    {
        if (card.IsFaceUp) yield break;

        if (_first == null)
        {
            yield return StartCoroutine(card.FlipTo(true));
            _first = card;
            yield break;
        }

        if (_second == null)
        {
            _inputLocked = true;

            yield return StartCoroutine(card.FlipTo(true));
            _second = card;

            _save.attempts++;
            RefreshUI();
            SaveSystem.Save(_save);

            yield return StartCoroutine(ResolvePairRoutine());

            _inputLocked = false;
        }
    }

    private IEnumerator ResolvePairRoutine()
    {
        if (_first == null || _second == null) yield break;

        bool isMatch = _first.CardId == _second.CardId;

        if (isMatch)
        {
            _save.matches++;

            _matchedCellSet.Add(_first.CellIndex);
            _matchedCellSet.Add(_second.CellIndex);
            _save.matchedCellIndices = ToArray(_matchedCellSet);

            RefreshUI();
            SaveSystem.Save(_save);

            float vanishDur = Mathf.Max(0f, _entry.matchVanishSeconds);

            // ✅ SetActive(false) yerine empty'ye dönüş
            yield return StartCoroutine(_first.VanishToEmpty(vanishDur));
            yield return StartCoroutine(_second.VanishToEmpty(vanishDur));
        }
        else
        {
            float flash = Mathf.Max(0f, _entry.mismatchFlashSeconds);

            yield return StartCoroutine(FlashBoth(_first, _second, flash));

            yield return StartCoroutine(_first.FlipTo(false));
            yield return StartCoroutine(_second.FlipTo(false));
        }

        _first = null;
        _second = null;

        if (IsGameOver())
        {
            Debug.Log("[GameController] GAME OVER (all matched)");

            _save.isCompleted = true;     // ✅ completed işaretle
            SaveSystem.Save(_save);       // ✅ kaydet

            _inputLocked = true;          // ✅ input kapat (oyun bitti)

            if (gameSceneCanvas != null)  // ✅ panel aç
                gameSceneCanvas.ShowGameOver();
        }
    }

    private IEnumerator FlashBoth(CardView a, CardView b, float duration)
    {
        Coroutine ca = StartCoroutine(a.FlashMismatch(duration));
        Coroutine cb = StartCoroutine(b.FlashMismatch(duration));
        yield return ca;
        yield return cb;
    }

    private bool IsGameOver()
    {
        // Artık objeler aktif kaldığı için IsMatched ile kontrol ediyoruz
        foreach (var c in boardBuilder.SpawnedCards)
        {
            if (c == null) continue;
            if (!c.IsMatched) return false;
        }
        return true;
    }

    private void RefreshUI()
    {
        matchCountText.text = $"{_save.matches}";
        attemptCountText.text = $"{_save.attempts}";
    }

    private static int[] ToArray(HashSet<int> set)
    {
        var arr = new int[set.Count];
        int i = 0;
        foreach (var v in set) arr[i++] = v;
        return arr;
    }
}
