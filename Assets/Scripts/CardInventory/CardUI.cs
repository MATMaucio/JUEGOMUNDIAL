using UnityEngine;
using TMPro;

public class CardUI : MonoBehaviour
{
    public TMP_Text nameText;
    public GameObject lockedOverlay;

    public void Setup(Card card, bool owned)
    {
        if (owned)
        {
            nameText.text = card.playerName;
            lockedOverlay.SetActive(false);
        }
        else
        {
            nameText.text = "???";
            lockedOverlay.SetActive(true);
        }
    }
}