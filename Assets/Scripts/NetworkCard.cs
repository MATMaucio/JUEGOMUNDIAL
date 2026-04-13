using Unity.Netcode;
using UnityEngine;
using TMPro; // Usado para mostrar texto en el cubo/carta

public class NetworkCard : NetworkBehaviour
{
    [Header("Referencias")]
    public CardDatabase database;
    public TextMeshPro textMesh; // Para ver el nombre de la carta

    // Variable sincronizada por la red (Solo el servidor la puede cambiar por defecto)
    public NetworkVariable<int> networkCardId = new NetworkVariable<int>();

public override void OnNetworkSpawn()
    {
        // Cuando aparece en la red, actualizamos sus visuales basándonos en el ID
        networkCardId.OnValueChanged += (oldId, newId) => ActualizarVisuales(newId);
        
        // Actualizamos la primera vez que aparece
        ActualizarVisuales(networkCardId.Value);

        // --- EL ESLABÓN PERDIDO ---
        // Si la carta nace y yo soy su dueño original, la agrego a mi inventario visual
        if (IsOwner)
        {
            FindAnyObjectByType<GameManager>().AgregarCartaAlInventario(this);
        }
    }

    private void ActualizarVisuales(int id)
    {
        if (database != null && id >= 0 && id < database.cards.Count)
        {
            CardData data = database.cards[id];
            if (textMesh != null) textMesh.text = data.cardName;
            gameObject.name = "Carta_" + data.cardName;
        }
    }

    // --- LÓGICA PARA COMPARTIR/PASAR EL OBJETO ---

    // Este comando lo pide un cliente, pero lo ejecuta el Servidor
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void TransferOwnershipServerRpc(ulong newOwnerClientId)
    {
        // Cambiamos el dueño del objeto en la red
        NetworkObject netObj = GetComponent<NetworkObject>();
        netObj.ChangeOwnership(newOwnerClientId);
        
        Debug.Log($"El objeto {gameObject.name} ahora pertenece al cliente {newOwnerClientId}");
    }
    public override void OnGainedOwnership()
    {
        // Esto solo se ejecuta en la pantalla del nuevo dueño
        FindAnyObjectByType<GameManager>().AgregarCartaAlInventario(this);
    }

public override void OnLostOwnership()
    {
        Debug.Log("¡Rastreador 1: La carta detectó que perdió a su dueño!");
        FindAnyObjectByType<GameManager>().QuitarCartaDelInventario(this);
    }
}