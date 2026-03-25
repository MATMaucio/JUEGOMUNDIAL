using System.Collections.Generic;
using UnityEngine;

public static class CardDatabase
{
    public static List<Card> allCards;

    public static int TOTAL_CARDS => allCards.Count;

    public static void Load()
    {
        TextAsset json = Resources.Load<TextAsset>("cards");
        CardList data = JsonUtility.FromJson<CardList>(json.text);
        allCards = data.cards;
        Debug.Log("Card Database Loaded: " + allCards.Count + " cards available.");
    }

    public static Card GetCardByID(int id)
    {
        return allCards.Find(c => c.id == id);
    }
}
