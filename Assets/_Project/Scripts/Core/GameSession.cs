using System;
using UnityEngine;

public enum Difficulty
{
    Easy = 0,
    Medium = 1,
    Hard = 2
}

public enum StartMode
{
    NewGame = 0,
    Continue = 1
}

[Serializable]
public struct BoardConfig
{
    public int rows;
    public int cols;

    public BoardConfig(int rows, int cols)
    {
        this.rows = rows;
        this.cols = cols;
    }
}

public static class GameSession
{
    public static StartMode StartMode { get; private set; } = StartMode.NewGame;
    public static Difficulty Difficulty { get; private set; } = Difficulty.Easy;
    public static BoardConfig BoardConfig { get; private set; } = new BoardConfig(2, 2);
    public static int Seed { get; private set; } = 0;

    public static void SetNewGame(Difficulty difficulty, BoardConfig config, int seed)
    {
        StartMode = StartMode.NewGame;
        Difficulty = difficulty;
        BoardConfig = config;
        Seed = seed;
    }

    public static void SetContinue()
    {
        StartMode = StartMode.Continue;
    }
}
