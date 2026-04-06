using UnityEngine;

public class InteractablePOI : MonoBehaviour
{
    // Variables para guardar los datos que nos manda el POIManager
    public string poiName;
    public string poiDescription;

    // Esta es la función que se activa al tocar el modelo 3D en el celular
    private void OnMouseDown()
    {
        // Aquí es donde mandas llamar a tu panel de UI. 
        // Suponiendo que tienes un UIManager, sería algo así:
        UIManager.Instance.OpenPOIPanel(poiName, poiDescription);    }
}