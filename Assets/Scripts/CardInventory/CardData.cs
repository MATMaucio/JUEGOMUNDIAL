using System.Collections.Generic;

[System.Serializable]
public class Card
{
    public int id;
    public string playerName;
    public string team;
    public string position;
}

[System.Serializable]
public class CardList
{
    public List<Card> cards;
}
