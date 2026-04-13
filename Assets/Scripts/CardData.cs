using UnityEngine;

[CreateAssetMenu(fileName = "NuevaCarta", menuName = "TCG/Datos de Carta")]
public class CardData : ScriptableObject
{
    public int cardId;
    public string cardName;
    public Sprite artwork; // Por si luego quieres ponerle imagen 2D
    // Aquí puedes agregar vida, daño, rareza, etc.
}