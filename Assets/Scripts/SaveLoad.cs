using UnityEngine;

public static class SaveLoad
{
    const string BestKey = "CityRunner_BestScore";
    const string CoinsKey = "CityRunner_TotalCoins";

    public static int LoadBest() { return PlayerPrefs.GetInt(BestKey, 0); }

    public static void SaveBest(int value)
    {
        PlayerPrefs.SetInt(BestKey, value);
        PlayerPrefs.Save();
    }

    public static int LoadTotalCoins() { return PlayerPrefs.GetInt(CoinsKey, 0); }

    public static void AddCoins(int amount)
    {
        PlayerPrefs.SetInt(CoinsKey, LoadTotalCoins() + amount);
        PlayerPrefs.Save();
    }
}
