using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardBuilder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform boardContainer;
    [SerializeField] private GridLayoutGroup grid;

    [Header("Prefabs")]
    [SerializeField] private CardView cardPrefab;
    [SerializeField] private RectTransform emptyCellPrefab;

    [Header("Layout")]
    [SerializeField] private Vector2 spacing = new Vector2(12f, 12f);
    [SerializeField] private Vector2 padding = new Vector2(24f, 24f);

    public IReadOnlyList<CardView> SpawnedCards => _cards;

    private readonly List<CardView> _cards = new List<CardView>();
    private readonly Dictionary<int, CardView> _byCellIndex = new Dictionary<int, CardView>();

    public bool TryGetCardByCellIndex(int cellIndex, out CardView card)
        => _byCellIndex.TryGetValue(cellIndex, out card);

    public void Build(BoardLayoutData layout, int seed, IReadOnlyList<Sprite> faceSprites)
    {
        if (boardContainer == null || grid == null || cardPrefab == null || emptyCellPrefab == null)
        {
            Debug.LogError("[BoardBuilder] Missing references!");
            return;
        }

        if (layout.rows <= 0 || layout.cols <= 0)
        {
            Debug.LogError($"[BoardBuilder] Invalid layout: {layout.rows}x{layout.cols}");
            return;
        }

        if (layout.PlayableCells <= 0 || layout.PlayableCells % 2 != 0)
        {
            Debug.LogError($"[BoardBuilder] PlayableCells must be positive and even! playable={layout.PlayableCells}");
            return;
        }

        int pairCount = layout.PlayableCells / 2;

        if (faceSprites == null || faceSprites.Count < pairCount)
        {
            Debug.LogError($"[BoardBuilder] Not enough faceSprites. Need={pairCount}, Have={(faceSprites == null ? 0 : faceSprites.Count)}");
            return;
        }

        ClearChildren();
        _cards.Clear();
        _byCellIndex.Clear();

        // grid config
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = layout.cols;

        grid.spacing = spacing;

        grid.padding = new RectOffset(
            left: Mathf.RoundToInt(padding.x),
            right: Mathf.RoundToInt(padding.x),
            top: Mathf.RoundToInt(padding.y),
            bottom: Mathf.RoundToInt(padding.y)
        );

        ApplyAutoCellSize(layout.rows, layout.cols);

        // deck (pair ids)
        var deck = CreateShuffledDeck(layout.PlayableCells, seed);
        int deckIndex = 0;

        for (int cellIndex = 0; cellIndex < layout.TotalCells; cellIndex++)
        {
            if (IsEmpty(layout, cellIndex))
            {
                Instantiate(emptyCellPrefab, grid.transform);
            }
            else
            {
                int cardId = deck[deckIndex++];
                Sprite face = faceSprites[cardId];

                var card = Instantiate(cardPrefab, grid.transform);
                card.Init(cellIndex, cardId, face);

                _cards.Add(card);
                _byCellIndex[cellIndex] = card;
            }
        }
    }

    public void ForceAllUnmatchedFaceDownInstant()
    {
        for (int i = 0; i < _cards.Count; i++)
        {
            var c = _cards[i];
            if (c != null && !c.IsMatched)
                c.SetFaceUp(false, instant: true);
        }
    }

    private void ApplyAutoCellSize(int rows, int cols)
    {
        Vector2 size = boardContainer.rect.size;

        float totalSpacingX = grid.spacing.x * (cols - 1);
        float totalSpacingY = grid.spacing.y * (rows - 1);

        float totalPaddingX = grid.padding.left + grid.padding.right;
        float totalPaddingY = grid.padding.top + grid.padding.bottom;

        float availW = Mathf.Max(1f, size.x - totalPaddingX - totalSpacingX);
        float availH = Mathf.Max(1f, size.y - totalPaddingY - totalSpacingY);

        float cellW = availW / cols;
        float cellH = availH / rows;

        float cell = Mathf.Floor(Mathf.Min(cellW, cellH));
        cell = Mathf.Max(8f, cell);

        grid.cellSize = new Vector2(cell, cell);
    }

    private bool IsEmpty(BoardLayoutData layout, int index)
    {
        var empties = layout.emptyIndices;
        if (empties == null || empties.Length == 0) return false;

        for (int i = 0; i < empties.Length; i++)
            if (empties[i] == index) return true;

        return false;
    }

    private List<int> CreateShuffledDeck(int playableCount, int seed)
    {
        int pairCount = playableCount / 2;

        var list = new List<int>(playableCount);
        for (int i = 0; i < pairCount; i++)
        {
            list.Add(i);
            list.Add(i);
        }

        var rng = new System.Random(seed);
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }

        return list;
    }

    private void ClearChildren()
    {
        for (int i = grid.transform.childCount - 1; i >= 0; i--)
            Destroy(grid.transform.GetChild(i).gameObject);
    }
}
