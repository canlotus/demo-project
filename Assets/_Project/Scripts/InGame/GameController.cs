using System;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoardBuilder boardBuilder;

    [Header("Config")]
    [SerializeField] private DifficultyDatabaseSO difficultyDb;

    private void Start()
    {
        if (boardBuilder == null)
        {
            Debug.LogError("[GameController] boardBuilder missing!");
            return;
        }

        if (difficultyDb == null)
        {
            Debug.LogError("[GameController] difficultyDb missing! Assign DifficultyDatabase asset in inspector.");
            return;
        }

        // Determine layout + seed + difficulty based on StartMode
        DifficultyId diff;
        BoardLayoutData layout;
        int seed;

        if (GameSession.StartMode == StartMode.Continue && SaveSystem.TryLoad(out var save))
        {
            diff = save.difficulty;
            layout = new BoardLayoutData(
                save.rows,
                save.cols,
                save.emptyIndices ?? Array.Empty<int>()
            );
            seed = save.seed;
        }
        else
        {
            diff = GameSession.Difficulty;
            layout = GameSession.Layout;
            seed = GameSession.Seed;
        }

        var entry = difficultyDb.Get(diff);
        if (entry == null)
        {
            Debug.LogError($"[GameController] Difficulty entry not found for {diff}. Check DifficultyDatabase asset.");
            return;
        }

        // Build board with sprites from Scriptable
        boardBuilder.Build(layout, seed, entry.faceSprites);

        // Hook card clicks
        foreach (var card in boardBuilder.SpawnedCards)
        {
            card.Clicked += OnCardClicked;
        }

        int empties = layout.emptyIndices == null ? 0 : layout.emptyIndices.Length;
        Debug.Log($"[GameController] Board built | mode={GameSession.StartMode} diff={diff} board={layout.rows}x{layout.cols} empties={empties} seed={seed}");
    }

    private void OnDestroy()
    {
        if (boardBuilder == null) return;

        foreach (var card in boardBuilder.SpawnedCards)
        {
            if (card != null)
                card.Clicked -= OnCardClicked;
        }
    }

    private void OnCardClicked(CardView card)
    {
        // Şimdilik sadece flip
        card.Flip();
    }
}
