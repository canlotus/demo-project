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

    [Header("Continuous Flip Settings")]
    [SerializeField] private int maxFaceUpUnresolvedCards = 4; 

    private DifficultyEntry _entry;
    private SaveData _save;

    private bool _previewRunning;

    private readonly HashSet<int> _matchedCellSet = new HashSet<int>();

    private readonly List<CardView> _open = new List<CardView>(2);

    private readonly Queue<Pair> _pendingPairs = new Queue<Pair>();
    private bool _resolverRunning;

    private struct Pair
    {
        public CardView a;
        public CardView b;
        public Pair(CardView a, CardView b) { this.a = a; this.b = b; }
    }

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
                matchedCellIndices = Array.Empty<int>(),
                isCompleted = false
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

        if (_save.isCompleted)
        {
            boardBuilder.ForceAllUnmatchedFaceDownInstant();
            if (gameSceneCanvas != null) gameSceneCanvas.ShowGameOver();
            return;
        }

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
                card.MakeEmptyInstant();
            }
        }
    }

    private IEnumerator PreviewRoutine(float seconds)
    {
        _previewRunning = true;

        yield return new WaitForSecondsRealtime(seconds);

        foreach (var card in boardBuilder.SpawnedCards)
        {
            if (card == null) continue;
            if (card.IsMatched) continue;
            yield return StartCoroutine(card.FlipTo(false));
        }

        _save.previewDone = true;
        SaveSystem.Save(_save);

        _previewRunning = false;
    }

    private void OnCardClicked(CardView card)
    {
        if (card == null) return;
        if (_save == null) return;

        if (_save.isCompleted) return;
        if (_previewRunning) return;

        if (card.IsMatched) return;
        if (card.IsBusy) return;
        if (card.IsFaceUp) return; 
        if (CountFaceUpUnresolved() >= maxFaceUpUnresolvedCards)
        {
            return;
        }

        StartCoroutine(FlipAndSelectRoutine(card));
    }

    private IEnumerator FlipAndSelectRoutine(CardView card)
    {
        yield return StartCoroutine(card.FlipTo(true));
        if (card == null) yield break;
        if (card.IsMatched) yield break;

        _open.Add(card);

        if (_open.Count >= 2)
        {
            var a = _open[0];
            var b = _open[1];

            _open.Clear();
            if (a == null || b == null || a == b)
                yield break;

            _pendingPairs.Enqueue(new Pair(a, b));
            _save.attempts++;
            RefreshUI();
            SaveSystem.Save(_save);
            if (!_resolverRunning)
                StartCoroutine(ResolvePendingPairsRoutine());
        }
    }

    private int CountFaceUpUnresolved()
    {
        int count = 0;
        foreach (var c in boardBuilder.SpawnedCards)
        {
            if (c == null) continue;
            if (c.IsMatched) continue;
            if (c.IsFaceUp) count++;
        }

        return count;
    }

    private IEnumerator ResolvePendingPairsRoutine()
    {
        _resolverRunning = true;

        while (_pendingPairs.Count > 0)
        {
            if (_save == null || _save.isCompleted) break;

            var pair = _pendingPairs.Dequeue();
            var a = pair.a;
            var b = pair.b;
            if (a == null || b == null) continue;
            if (a.IsMatched || b.IsMatched) continue;
            if (!a.IsFaceUp) yield return StartCoroutine(a.FlipTo(true));
            if (!b.IsFaceUp) yield return StartCoroutine(b.FlipTo(true));

            bool isMatch = a.CardId == b.CardId;

            if (isMatch)
            {
                if (AudioManager.I != null) AudioManager.I.PlayMatch();

                _save.matches++;

                _matchedCellSet.Add(a.CellIndex);
                _matchedCellSet.Add(b.CellIndex);
                _save.matchedCellIndices = ToArray(_matchedCellSet);

                RefreshUI();
                SaveSystem.Save(_save);

                float vanishDur = Mathf.Max(0f, _entry.matchVanishSeconds);

                yield return StartCoroutine(a.VanishToEmpty(vanishDur));
                yield return StartCoroutine(b.VanishToEmpty(vanishDur));
            }
            else
            {
                if (AudioManager.I != null) AudioManager.I.PlayMismatch();

                float flash = Mathf.Max(0f, _entry.mismatchFlashSeconds);

                yield return StartCoroutine(FlashBoth(a, b, flash));

                yield return StartCoroutine(a.FlipTo(false));
                yield return StartCoroutine(b.FlipTo(false));
            }

            if (IsGameOver())
            {
                Debug.Log("[GameController] GAME OVER (all matched)");
                if (AudioManager.I != null) AudioManager.I.PlayWin();
                _save.isCompleted = true;
                SaveSystem.Save(_save);
                _open.Clear();
                _pendingPairs.Clear();

                if (gameSceneCanvas != null)
                    gameSceneCanvas.ShowGameOver();

                break;
            }
        }

        _resolverRunning = false;
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
