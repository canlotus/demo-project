using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardBuilder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform boardContainer;   // BoardArea
    [SerializeField] private GridLayoutGroup grid;           // BoardGrid

    [Header("Prefabs")]
    [SerializeField] private CardView cardPrefab;
    [SerializeField] private RectTransform emptyCellPrefab;

    [Header("Layout")]
    [SerializeField] private Vector2 spacing = new Vector2(12f, 12f);
    [SerializeField] private Vector2 padding = new Vector2(24f, 24f);

    public IReadOnlyList<CardView> SpawnedCards => _cards;
    private readonly List<CardView> _cards = new List<CardView>();

    /// <summary>
    /// Builds the board with masking (empty cells) and assigns face sprites by cardId.
    /// cardId range: 0..(pairCount-1). Two cards share same id => same sprite.
    /// </summary>
    public void Build(BoardLayoutData layout, int seed, IReadOnlyList<Sprite> faceSprites)
    {
        if (boardContainer == null || grid == null || cardPrefab == null || emptyCellPrefab == null)
        {
            Debug.LogError("[BoardBuilder] Missing references! Check boardContainer, grid, cardPrefab, emptyCellPrefab.");
            return;
        }

        if (layout.rows <= 0 || layout.cols <= 0)
        {
            Debug.LogError($"[BoardBuilder] Invalid layout rows/cols: {layout.rows}x{layout.cols}");
            return;
        }

        // playable cell count must be even for pairs
        if (layout.PlayableCells <= 0 || layout.PlayableCells % 2 != 0)
        {
            Debug.LogError($"[BoardBuilder] PlayableCells must be positive and even! playable={layout.PlayableCells}");
            return;
        }

        int pairCount = layout.PlayableCells / 2;

        if (faceSprites == null)
        {
            Debug.LogError("[BoardBuilder] faceSprites is null. Assign sprites in Difficulty Scriptable.");
            return;
        }

        if (faceSprites.Count < pairCount)
        {
            Debug.LogError($"[BoardBuilder] Not enough faceSprites! Need {pairCount} unique sprites but have {faceSprites.Count}. " +
                           $"(Board playable={layout.PlayableCells} => pairs={pairCount})");
            return;
        }

        ClearChildren();
        _cards.Clear();

        // Grid config
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = layout.cols;
        grid.spacing = spacing;

        grid.padding = new RectOffset(
            left: Mathf.RoundToInt(padding.x),
            right: Mathf.RoundToInt(padding.x),
            top: Mathf.RoundToInt(padding.y),
            bottom: Mathf.RoundToInt(padding.y)
        );

        // Cell size (fit boardContainer)
        ApplyAutoCellSize(layout.rows, layout.cols);

        // Create deck (pair ids), shuffled by seed
        var deck = CreateShuffledDeck(layout.PlayableCells, seed);

        int deckIndex = 0;

        // Build cells in row-major order
        for (int i = 0; i < layout.TotalCells; i++)
        {
            if (IsEmpty(layout, i))
            {
                Instantiate(emptyCellPrefab, grid.transform);
            }
            else
            {
                int cardId = deck[deckIndex++];
                Sprite face = faceSprites[cardId];

                var card = Instantiate(cardPrefab, grid.transform);
                card.Init(cardId, face);
                _cards.Add(card);
            }
        }

        if (deckIndex != layout.PlayableCells)
        {
            Debug.LogWarning($"[BoardBuilder] DeckIndex mismatch: used={deckIndex}, playable={layout.PlayableCells}. Check empty indices.");
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

        float cell = Mathf.Floor(Mathf.Min(cellW, cellH)); // square
        cell = Mathf.Max(8f, cell); // safety

        grid.cellSize = new Vector2(cell, cell);
    }

    private bool IsEmpty(BoardLayoutData layout, int index)
    {
        var empties = layout.emptyIndices;
        if (empties == null || empties.Length == 0) return false;

        for (int i = 0; i < empties.Length; i++)
        {
            if (empties[i] == index) return true;
        }
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
