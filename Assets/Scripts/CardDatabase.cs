using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BaseDeDatos", menuName = "TCG/Base de Datos")]
public class CardDatabase : ScriptableObject
{
    public List<CardData> cards = new List<CardData>();
}