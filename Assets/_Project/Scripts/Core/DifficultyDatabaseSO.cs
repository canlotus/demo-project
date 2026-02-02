using System;
using System.Collections.Generic;
using UnityEngine;

public enum DifficultyId
{
    Easy = 0,
    Medium = 1,
    Hard = 2
}

[Serializable]
public class DifficultyEntry
{
    public DifficultyId id = DifficultyId.Easy;

    [Header("Board")]
    public int rows = 2;
    public int cols = 2;
    public List<int> emptyIndices = new List<int>();
    public int TotalCells => rows * cols;
    public int PlayableCells => TotalCells - (emptyIndices?.Count ?? 0);

    public bool IsEmpty(int index)
    {
        if (emptyIndices == null) return false;
        return emptyIndices.Contains(index);
    }
}

[CreateAssetMenu(menuName = "CardMatch/Difficulty Database", fileName = "DifficultyDatabase")]
public class DifficultyDatabaseSO : ScriptableObject
{
    public List<DifficultyEntry> difficulties = new List<DifficultyEntry>();

    public DifficultyEntry Get(DifficultyId id)
    {
        for (int i = 0; i < difficulties.Count; i++)
        {
            if (difficulties[i].id == id)
                return difficulties[i];
        }
        return null;
    }

    public DifficultyEntry GetByIndex(int dropdownIndex)
    {
        if (difficulties == null || difficulties.Count == 0) return null;
        dropdownIndex = Mathf.Clamp(dropdownIndex, 0, difficulties.Count - 1);
        return difficulties[dropdownIndex];
    }

    public int GetIndex(DifficultyId id)
    {
        for (int i = 0; i < difficulties.Count; i++)
            if (difficulties[i].id == id) return i;
        return 0;
    }
}
