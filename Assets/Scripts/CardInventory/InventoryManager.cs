using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public Transform content;
    public GameObject cardPrefab;

    public void GenerateInventory()
    {
        // Limpiar anterior
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        foreach (Card card in CardDatabase.allCards)
        {
            GameObject obj = Instantiate(cardPrefab, content);
            CardUI ui = obj.GetComponent<CardUI>();

            bool owned = PlayerCardManager.HasCard(card.id);

            ui.Setup(card, owned);
        }

        Debug.Log(
            PlayerCardManager.GetOwnedCount() + "/" + CardDatabase.TOTAL_CARDS
        );
    }
}
