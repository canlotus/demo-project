using UnityEngine;

public class GameSceneDebugController : MonoBehaviour
{
    private void Start()
    {
        if (GameSession.StartMode == StartMode.Continue)
        {
            if (SaveSystem.TryLoad(out var save))
            {
                Debug.Log($"[GameScene] CONTINUE | diff={save.difficulty} board={save.rows}x{save.cols} seed={save.seed} score={save.score}");
            }
            else
            {
                Debug.LogWarning("[GameScene] CONTINUE requested but no save found. Falling back to NEW.");
                Debug.Log($"[GameScene] NEW (fallback) | diff={GameSession.Difficulty} board={GameSession.BoardConfig.rows}x{GameSession.BoardConfig.cols} seed={GameSession.Seed}");
            }
        }
        else
        {
            Debug.Log($"[GameScene] NEW | diff={GameSession.Difficulty} board={GameSession.BoardConfig.rows}x{GameSession.BoardConfig.cols} seed={GameSession.Seed}");
        }
    }
}
