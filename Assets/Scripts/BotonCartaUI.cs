using UnityEngine;
using TMPro;

public class BotonCartaUI : MonoBehaviour
{
    public NetworkCard cartaQueRepresento; // El objeto de red que esta UI controla
    private GameManager manager;

    // Esta función la llamaremos cuando la carta nazca en la pantalla
    public void Configurar(NetworkCard cartaReal, GameManager gm)
    {
        cartaQueRepresento = cartaReal;
        manager = gm;
        
        // Aquí podrías cambiar el texto para que diga el nombre de la carta
        GetComponentInChildren<TextMeshProUGUI>().text = cartaReal.gameObject.name;
    }

    // Se asigna al evento OnClick() del Botón en Unity
    public void TocarCarta()
    {
        
        manager.SeleccionarCartaDesdeUI(this);
    }
}