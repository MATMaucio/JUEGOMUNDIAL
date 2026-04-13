using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class GameManager : NetworkBehaviour
{
    public GameObject panelInventarioPrincipal;
    [Header("Objetos 3D / Prefabs")]
    public GameObject cardPrefab; 
    public NetworkCard cartaSeleccionada; 

    [Header("Inventario Visual (3 Columnas)")]
    public Transform contenedorInventario; // Arrastraremos el "Content" del ScrollView aquí
    public GameObject botonCartaUIPrefab;  // El Prefab del botón de la carta 2D

    [Header("UI de Confirmación de Carta")]
    public GameObject panelConfirmarEnvio;
    public TextMeshProUGUI textoConfirmacion; 
    public Button botonCompartirPrincipal; 
    
    private BotonCartaUI botonUISeleccionado;

    // --- MECÁNICAS DE INVENTARIO ---
    
    // Estas son las dos funciones nuevas que la carta llamará al llegar o irse
 public void AgregarCartaAlInventario(NetworkCard carta)
    {
        // 1. Clonamos el botón UI y lo metemos dentro del Grid Layout (Inventario)
        GameObject nuevoBotonUI = Instantiate(botonCartaUIPrefab, contenedorInventario);
        
        // 2. Buscamos su script y lo configuramos
        BotonCartaUI scriptBoton = nuevoBotonUI.GetComponent<BotonCartaUI>();
        scriptBoton.Configurar(carta, this);
        
        Debug.Log("Carta agregada visualmente al inventario.");
    }
public void QuitarCartaDelInventario(NetworkCard carta)
    {
        Debug.Log("¡Rastreador 2: El GameManager recibió la orden de borrar a " + carta.gameObject.name + "!");

        foreach (Transform hijo in contenedorInventario)
        {
            BotonCartaUI scriptBoton = hijo.GetComponent<BotonCartaUI>();

            if (scriptBoton != null)
            {
                // Revisamos qué carta tiene guardada este botón
                string nombreCartaGuardada = scriptBoton.cartaQueRepresento != null ? scriptBoton.cartaQueRepresento.gameObject.name : "VACÍO";
                Debug.Log("Rastreador 3: Revisando un botón que representa a: " + nombreCartaGuardada);

                if (scriptBoton.cartaQueRepresento == carta)
                {
                    Destroy(hijo.gameObject);
                    Debug.Log("¡Éxito! Botón UI eliminado del inventario.");
                    break; 
                }
            }
        }
    }
    public void SeleccionarCartaDesdeUI(BotonCartaUI botonTocado)
    {
      // Si la variable de la izquierda termina vacía...
    // ¡Significa que la variable de la derecha también venía vacía!
        cartaSeleccionada = botonTocado.cartaQueRepresento;
        botonUISeleccionado = botonTocado;
        botonCompartirPrincipal.interactable = true; 
        Debug.Log("Has seleccionado: " + cartaSeleccionada.gameObject.name);
    }

    // --- CREACIÓN (PRUEBA) ---
    public void BotonCrearObjeto()
    {
        if (IsServer)
        {
            GameObject nuevaCarta = Instantiate(cardPrefab, new Vector3(0, 1, 0), Quaternion.identity);
            NetworkObject netObj = nuevaCarta.GetComponent<NetworkObject>();
            netObj.Spawn();

            nuevaCarta.GetComponent<NetworkCard>().networkCardId.Value = Random.Range(0, 3);
        }
    }

    // --- POP-UP DE COMPARTIR CARTA ---
public void BotonCompartirObjeto()
{
    Debug.Log("¡El botón físico fue presionado y la señal llegó al GameManager!");
    
    if (cartaSeleccionada != null)
    {
        string nombreCarta = cartaSeleccionada.gameObject.name.Replace("Carta_", "");
        textoConfirmacion.text = $"¿Estás seguro de enviar '{nombreCarta}' al rival?";
        panelConfirmarEnvio.SetActive(true); 
    }
    else 
    {
        // Esta es la pieza que faltaba para avisarnos del problema
        Debug.LogWarning("Intentaste compartir, pero la variable cartaSeleccionada está vacía (null).");
    }
}

public void ConfirmarEnvio()
    {
        if (cartaSeleccionada != null)
        {
            ulong idDestinatario = NetworkManager.Singleton.LocalClientId == 0 ? 1ul : 0ul;
            
            // 1. Mandamos la carta por la red
            cartaSeleccionada.TransferOwnershipServerRpc(idDestinatario);
            
            // 2. ¡EL ATAJO! Borramos nuestro propio botón visual de inmediato
            QuitarCartaDelInventario(cartaSeleccionada);
            
            // 3. Limpiamos la memoria
            botonCompartirPrincipal.interactable = false; 
            cartaSeleccionada = null; 
            panelConfirmarEnvio.SetActive(false); 
        }
    }

    public void CancelarEnvio()
    {
        panelConfirmarEnvio.SetActive(false); 
    }
    public void AlternarInventario()
{
    // Verificamos por seguridad que el enchufe no esté vacío
    if (panelInventarioPrincipal != null)
    {
        // Preguntamos: ¿Estás encendido ahora mismo? (true o false)
        bool estadoActual = panelInventarioPrincipal.activeSelf;

        // Lo cambiamos al estado exactamente opuesto con el símbolo '!'
        panelInventarioPrincipal.SetActive(!estadoActual);
    }
}
}