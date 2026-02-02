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

    public void Build(BoardLayoutData layout, int seed)
    {
        if (boardContainer == null || grid == null || cardPrefab == null || emptyCellPrefab == null)
        {
            Debug.LogError("[BoardBuilder] Missing references!");
            return;
        }

        ClearChildren();
        _cards.Clear();

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

        // playable cell count must be even for pairs
        if (layout.PlayableCells % 2 != 0)
        {
            Debug.LogError($"[BoardBuilder] PlayableCells must be even! playable={layout.PlayableCells}");
            return;
        }

        // compute cell size (fit)
        ApplyAutoCellSize(layout.rows, layout.cols);

        // create deck (pair ids)
        var deck = CreateShuffledDeck(layout.PlayableCells, seed);

        int deckIndex = 0;

        for (int i = 0; i < layout.TotalCells; i++)
        {
            if (IsEmpty(layout, i))
            {
                Instantiate(emptyCellPrefab, grid.transform);
            }
            else
            {
                var card = Instantiate(cardPrefab, grid.transform);
                int cardId = deck[deckIndex++];
                card.Init(cardId);
                _cards.Add(card);
            }
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

        float cell = Mathf.Floor(Mathf.Min(cellW, cellH)); // kare kart
        grid.cellSize = new Vector2(cell, cell);
    }

    private bool IsEmpty(BoardLayoutData layout, int index)
    {
        var empties = layout.emptyIndices;
        if (empties == null) return false;
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
        {
            Destroy(grid.transform.GetChild(i).gameObject);
        }
    }
}
