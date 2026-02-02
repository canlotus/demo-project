using System;
using System.Collections.Generic;

public enum StartMode
{
    NewGame = 0,
    Continue = 1
}

[Serializable]
public struct BoardLayoutData
{
    public int rows;
    public int cols;
    public int[] emptyIndices;

    public BoardLayoutData(int rows, int cols, int[] emptyIndices)
    {
        this.rows = rows;
        this.cols = cols;
        this.emptyIndices = emptyIndices;
    }

    public int TotalCells => rows * cols;
    public int EmptyCount => emptyIndices == null ? 0 : emptyIndices.Length;
    public int PlayableCells => TotalCells - EmptyCount;
}

public static class GameSession
{
    public static StartMode StartMode { get; private set; } = StartMode.NewGame;
    public static DifficultyId Difficulty { get; private set; } = DifficultyId.Easy;
    public static BoardLayoutData Layout { get; private set; } = new BoardLayoutData(2, 2, Array.Empty<int>());
    public static int Seed { get; private set; } = 0;

    public static void SetNewGame(DifficultyId difficulty, BoardLayoutData layout, int seed)
    {
        StartMode = StartMode.NewGame;
        Difficulty = difficulty;
        Layout = layout;
        Seed = seed;
    }

    public static void SetContinue()
    {
        StartMode = StartMode.Continue;
    }
}
