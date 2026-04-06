using UnityEngine;
using TMPro; // Recuerda que esto es necesario si usas TextMeshPro

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Elementos del Panel POI")]
    [SerializeField] private GameObject poiPanelObject; // El panel visual
    [SerializeField] private TMP_Text poiNameText;      // El texto del título
    [SerializeField] private TMP_Text poiDescText;      // El texto de la descripción

    private void Awake()
    {
        // Configuramos el Singleton para que InteractionController lo encuentre fácil
        if (Instance == null) Instance = this;
    }

    // ¡Aquí está la función con el nombre exacto que Unity te estaba pidiendo!
    public void OpenPOIPanel(string poiName, string poiDescription)
    {
        // 1. Asignamos los textos del JSON a la UI
        poiNameText.text = poiName;
        poiDescText.text = poiDescription;
        
        // 2. Encendemos el panel
        poiPanelObject.SetActive(true);
    }
    
    // Función para el botón de "Cerrar" en tu UI
    public void ClosePOIPanel()
    {
        poiPanelObject.SetActive(false);
    }
}