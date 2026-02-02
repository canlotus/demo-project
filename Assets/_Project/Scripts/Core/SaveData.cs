using System;
using System.IO;
using UnityEngine;

[Serializable]
public class SaveData
{
    public int version = 1;

    public DifficultyId difficulty;
    public int rows;
    public int cols;
    public int[] emptyIndices;

    public int seed;
    public int score;

    // ileride:
    public int[] matchedCardIds;
}


public static class SaveSystem
{
    private const string FileName = "save.json";

    private static string FilePath =>
        Path.Combine(Application.persistentDataPath, FileName);

    public static bool HasSave()
    {
        return File.Exists(FilePath);
    }

    public static bool TryLoad(out SaveData data)
    {
        data = null;

        try
        {
            if (!File.Exists(FilePath))
                return false;

            string json = File.ReadAllText(FilePath);
            if (string.IsNullOrEmpty(json))
                return false;

            data = JsonUtility.FromJson<SaveData>(json);
            return data != null;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SaveSystem] Load failed: {e.Message}");
            return false;
        }
    }

    public static void Save(SaveData data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(FilePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Save failed: {e.Message}");
        }
    }

    public static void DeleteSave()
    {
        try
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SaveSystem] Delete failed: {e.Message}");
        }
    }
}
