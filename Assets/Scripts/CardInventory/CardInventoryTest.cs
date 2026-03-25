using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CardInventoryTest : MonoBehaviour
{
    public Canvas inventoryCanvas;
    public InventoryManager inventoryManager;

    private PlayerInputActions inputActions;
    private bool isInventoryOpen = false;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Enable();

        inputActions.Gameplay.ToggleInventory.performed += OnToggleInventory;
        inputActions.Gameplay.GenerateCard.performed += OnGenerateCard;
    }

    private void OnDisable()
    {
        inputActions.Gameplay.ToggleInventory.performed -= OnToggleInventory;
        inputActions.Gameplay.GenerateCard.performed -= OnGenerateCard;

        inputActions.Disable();
    }

    private void OnToggleInventory(InputAction.CallbackContext context)
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryCanvas.enabled = isInventoryOpen;

        if (isInventoryOpen)
        {
            inventoryManager.GenerateInventory();
        }
    }

    private void OnGenerateCard(InputAction.CallbackContext context)
    {
        if (isInventoryOpen) return;

        int randomID = Random.Range(1, 11);

        bool isNew = PlayerCardManager.VerifyCard(randomID);

        if (isNew)
            Debug.Log("Nueva carta obtenida: " + randomID);
        else
            Debug.Log("Carta repetida: " + randomID);
    }
}