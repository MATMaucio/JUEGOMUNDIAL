using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using TMPro;

public class MultiplayerManager : MonoBehaviour
{
    [Header("UI de Salas")]
    public GameObject botonSalaPrefab; 
    public Transform contenedorDeSalas; 
    
    [Header("UI de Confirmación")]
    public GameObject panelListaSalas;
    public GameObject panelConfirmacion;
    public TextMeshProUGUI textoPregunta; 

    private string codigoPendiente; 
    private string lobbyId;

    async void Start()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Jugador autenticado: " + AuthenticationService.Instance.PlayerId);
        }
    }

    // --- HOST ---
    public async void CrearPartida()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1); 
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            CreateLobbyOptions options = new CreateLobbyOptions {
                Data = new Dictionary<string, DataObject> {
                    { "RelayCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync("Sala de Pruebas", 2, options);
            lobbyId = lobby.Id;

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);

            NetworkManager.Singleton.StartHost();
            Debug.Log("Partida Creada. Código Relay: " + joinCode);
        }
        catch (System.Exception e) { Debug.LogError("Error al crear partida: " + e); }
    }

    // --- CLIENTE (BUSQUEDA Y CONEXIÓN) ---
    public async void BuscarSalas()
    {
        try
        {
            foreach (Transform hijo in contenedorDeSalas) { Destroy(hijo.gameObject); }

            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
            Debug.Log("Salas encontradas: " + queryResponse.Results.Count);

            foreach (Lobby lobby in queryResponse.Results)
            {
                string joinCode = lobby.Data["RelayCode"].Value;
                string nombre = lobby.Name;

                GameObject nuevoBoton = Instantiate(botonSalaPrefab, contenedorDeSalas);
                nuevoBoton.GetComponent<BotonSala>().Configurar(nombre, joinCode, this);
            }
        }
        catch (System.Exception e) { Debug.LogError("Error al buscar: " + e); }
    }

    public async void UnirseConCodigo(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4, (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes, joinAllocation.Key, joinAllocation.ConnectionData, joinAllocation.HostConnectionData);

            NetworkManager.Singleton.StartClient();
            Debug.Log("Unido a la partida con éxito.");
        }
        catch (System.Exception e) { Debug.LogError("Error al unirse con código: " + e); }
    }

    // --- POP-UP DE CONFIRMACIÓN DE SALA ---
    public void PrepararConfirmacion(string codigo, string nombreSala)
    {
        codigoPendiente = codigo;
        textoPregunta.text = "¿Quieres unirte a la sala: " + nombreSala + "?";
        panelConfirmacion.SetActive(true); 
    }

    public void BotonSiConfirmar()
    {
        panelConfirmacion.SetActive(false); 
        panelListaSalas.SetActive(false);   
        UnirseConCodigo(codigoPendiente);   
    }

    public void BotonNoCancelar()
    {
        codigoPendiente = ""; 
        panelConfirmacion.SetActive(false); 
    }

    // --- LIMPIEZA AL SALIR ---
    private void OnApplicationQuit() { DesconectarYLimpiar(); }
    private void OnDestroy() { DesconectarYLimpiar(); }

    private async void DesconectarYLimpiar()
    {
        try
        {
            if (!string.IsNullOrEmpty(lobbyId))
            {
                await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
                Debug.Log("Lobby fantasma eliminado de los servidores.");
                lobbyId = ""; 
            }
        }
        catch (System.Exception e) { Debug.Log("No se pudo borrar el lobby: " + e.Message); }
    }
}