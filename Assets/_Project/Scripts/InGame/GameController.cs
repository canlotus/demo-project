using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private BoardBuilder boardBuilder;

    private void Start()
    {
        if (boardBuilder == null)
        {
            Debug.LogError("[GameController] boardBuilder missing!");
            return;
        }

        // Continue ise save'den layout al, değilse session'dan
        BoardLayoutData layout;
        int seed;

        if (GameSession.StartMode == StartMode.Continue && SaveSystem.TryLoad(out var save))
        {
            layout = new BoardLayoutData(save.rows, save.cols, save.emptyIndices ?? System.Array.Empty<int>());
            seed = save.seed;
        }
        else
        {
            layout = GameSession.Layout;
            seed = GameSession.Seed;
        }

        boardBuilder.Build(layout, seed);

        // Hook card clicks
        foreach (var card in boardBuilder.SpawnedCards)
        {
            card.Clicked += OnCardClicked;
        }

        Debug.Log($"[GameController] Board built: {layout.rows}x{layout.cols}, empties={layout.emptyIndices?.Length ?? 0}, seed={seed}");
    }

    private void OnDestroy()
    {
        if (boardBuilder == null) return;
        foreach (var card in boardBuilder.SpawnedCards)
        {
            if (card != null) card.Clicked -= OnCardClicked;
        }
    }

    private void OnCardClicked(CardView card)
    {
        // Şimdilik sadece flip
        card.Flip();
    }
}
