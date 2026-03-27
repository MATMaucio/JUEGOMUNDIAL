using UnityEngine;

public class InteractionController : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 15f; 
    [SerializeField] private PlayerMovement playerMovementScript;
    [SerializeField] private UIManager uiManagerScript;
    
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        
        if (playerMovementScript == null)
        {
            Debug.LogError("PlayerMovement script not assigned to InteractionController!");
            enabled = false; 
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleTouchInteraction();
        }
    }

    private void HandleTouchInteraction()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        // Disparar el rayo y guardar la información del choque
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Forma óptima: Buscamos el script en lugar de comparar Tags
            if (hit.collider.TryGetComponent<InteractablePOI>(out InteractablePOI tappedPOI))
            {
                float distanceToPOI = Vector3.Distance(playerMovementScript.transform.position, hit.collider.transform.position);

                if (distanceToPOI <= interactionDistance)
                {
                    OnPOITapped(tappedPOI);
                }
                else
                {
                    Debug.Log("Poképarada demasiado lejos para interactuar. ¡Acércate!");
                }
            }
        }
    }

    private void OnPOITapped(InteractablePOI poi)
    {
        // Llamar directamente al UIManager para abrir la ventana con los datos
        if (uiManagerScript != null)
        {
            UIManager.Instance.OpenPOIPanel(poi.poiName, poi.poiDescription);
        }
    }
}