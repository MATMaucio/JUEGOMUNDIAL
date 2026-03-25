using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IntListWrapper
{
    public List<int> ids;
}

public static class PlayerCardManager
{
    private static List<int> playerCards = new List<int>();

    // Cargar datos al iniciar el juego
    public static void Load()
    {
        if (!PlayerPrefs.HasKey("PLAYER_CARDS"))
        {
            playerCards = new List<int>();
            return;
        }

        string json = PlayerPrefs.GetString("PLAYER_CARDS");
        playerCards = JsonUtility.FromJson<IntListWrapper>(json).ids;
        Debug.Log("Player cards loaded: " + playerCards.Count + " cards owned.");
    }

    private static void Save()
    {
        IntListWrapper wrapper = new IntListWrapper();
        wrapper.ids = playerCards;

        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString("PLAYER_CARDS", json);
        PlayerPrefs.Save();
    }

    public static bool VerifyCard(int cardID)
    {
        if (playerCards.Contains(cardID))
        {
            Debug.Log("Carta repetida");
            return false;
        }

        playerCards.Add(cardID);
        Save();

        Debug.Log("Carta nueva desbloqueada: " + cardID);
        return true;
    }

    public static List<int> GetPlayerCards()
    {
        return playerCards;
    }

    public static bool HasCard(int cardID)
    {
        return playerCards.Contains(cardID);
    }

    public static int GetOwnedCount()
    {
        return playerCards.Count;
    }
}
