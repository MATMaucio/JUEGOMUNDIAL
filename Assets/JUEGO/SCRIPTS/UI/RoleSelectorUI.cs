using UnityEngine;

/// <summary>
/// RoleSelectorUI
/// ----------------
/// Se encarga de permitir al jugador elegir su rol en la conexión Bluetooth:
/// 
/// - CreateGame()  → Actúa como servidor (Host).
/// - JoinGame()    → Actúa como cliente (Busca dispositivos y se conecta).
///
/// Este script normalmente está vinculado a botones en la UI:
///     • Botón "Crear Partida"
///     • Botón "Unirse a Partida"
///
/// Depende de:
/// - BluetoothManager (gestión de conexión)
/// - DeviceListUI (mostrar lista de dispositivos disponibles)
/// </summary>
public class RoleSelectorUI : MonoBehaviour
{
    /// <summary>
    /// Referencia al sistema que muestra la lista de dispositivos
    /// cuando el jugador quiere unirse a una partida.
    /// </summary>
    public DeviceListUI deviceListUI;

    /// <summary>
    /// Método llamado cuando el jugador selecciona "Crear partida".
    /// 
    /// Inicia el dispositivo como servidor Bluetooth,
    /// esperando que otro jugador se conecte.
    /// </summary>
    public void CreateGame()
    {
        BluetoothManager.Instance.StartServer();
    }

    /// <summary>
    /// Método llamado cuando el jugador selecciona "Unirse a partida".
    /// 
    /// Muestra la lista de dispositivos emparejados
    /// para que el usuario elija a cuál conectarse.
    /// </summary>
    public void JoinGame()
    {
        deviceListUI.ShowDevices();
    }
}