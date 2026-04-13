using UnityEngine;
using TMPro; // Para modificar el texto del botón
using Unity.Services.Lobbies;

public class BotonSala : MonoBehaviour
{
    public TextMeshProUGUI textoNombreSala; // Arrastraremos el texto del botón aquí

    private string codigoDeSala; // El secreto que guarda este botón
    private MultiplayerManager manager; // Referencia al manager principal

    // El Manager llamará a esta función justo después de clonar el botón
    public void Configurar(string nombre, string codigo, MultiplayerManager refManager)
    {
        textoNombreSala.text = nombre;
        codigoDeSala = codigo;
        manager = refManager;
    }

    // Esta función se la asignaremos al evento "On Click" del botón en Unity
    public void ConectarASala()
    {
        Debug.Log("Intentando conectar a la sala con código: " + codigoDeSala);
        manager.PrepararConfirmacion(codigoDeSala, textoNombreSala.text);
    }
}