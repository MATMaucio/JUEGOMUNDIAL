using UnityEngine;

/// <summary>
/// ============================================================
/// RoleSelectorUI — VERSIÓN CORREGIDA
/// ============================================================
/// 
/// PROBLEMAS DEL CÓDIGO ORIGINAL:
/// -------------------------------------------------------
/// 1. CreateGame() no tenía ningún Debug para saber si
///    StartServer() se llamó correctamente o no.
///
/// 2. No había feedback de que el proceso de espera había iniciado.
///
/// 3. JoinGame() no verificaba si BluetoothManager estaba disponible
///    antes de mostrar dispositivos.
///
/// CORRECCIONES APLICADAS:
/// -------------------------------------------------------
/// ✔ Debug.Log al crear partida (paso 1 del proceso).
/// ✔ Debug.Log al unirse a partida.
/// ✔ Verificación de nulos antes de llamar a subsistemas.
/// ✔ Suscripción al evento OnConnected para confirmar conexión exitosa.
/// 
/// CÓMO FUNCIONA AHORA:
/// -------------------------------------------------------
/// Botón "Crear Partida" → CreateGame()
///   → BluetoothManager.StartServer() [host espera cliente]
///   → Debug: "Servidor iniciado, esperando cliente..."
///   → Cuando cliente conecta → OnConnected → Debug: "Conexión establecida como HOST"
///
/// Botón "Unirse a Partida" → JoinGame()
///   → DeviceListUI.ShowDevices() [abre panel con botones]
///   → Usuario presiona botón de dispositivo
///   → BluetoothManager.ConnectToDevice()
///   → Debug según resultado de la conexión
/// ============================================================
/// </summary>
public class RoleSelectorUI : MonoBehaviour
{
    /// <summary>
    /// Referencia al panel que muestra la lista de dispositivos.
    /// Se asigna desde el Inspector de Unity.
    /// </summary>
    public DeviceListUI deviceListUI;

    // ================================================================
    // INICIALIZACIÓN
    // ================================================================

    void Start()
    {
        // Suscribirse al evento de conexión exitosa del BluetoothManager.
        // Cuando la conexión se establezca (independientemente del rol),
        // se ejecutará OnBluetoothConnected().
        //
        // NOTA: Nos suscribimos en Start (y no en Awake) para asegurar que
        // BluetoothManager.Instance ya esté inicializado.
        if (BluetoothManager.Instance != null)
        {
            BluetoothManager.Instance.OnConnected += OnBluetoothConnected;
        }
        else
        {
            Debug.LogError("[RoleSelectorUI] BluetoothManager.Instance es null. " +
                           "Asegúrate de que el GameObject con BluetoothManager " +
                           "esté en la escena y tenga Awake antes que este script.");
        }
    }

    void OnDestroy()
    {
        // Limpiar la suscripción al destruir este objeto
        // para evitar referencias a objetos destruidos.
        if (BluetoothManager.Instance != null)
            BluetoothManager.Instance.OnConnected -= OnBluetoothConnected;
    }

    // ================================================================
    // CREAR PARTIDA (HOST)
    // ================================================================

    /// <summary>
    /// Llamado cuando el jugador presiona "Crear Partida".
    /// 
    /// Este dispositivo actuará como SERVIDOR.
    /// Quedará en espera hasta que otro jugador se conecte.
    /// La conexión real ocurre en un Thread interno de BluetoothManager.
    /// </summary>
    public void CreateGame()
    {
        if (BluetoothManager.Instance == null)
        {
            Debug.LogError("[RoleSelectorUI] No se puede crear partida: BluetoothManager no existe.");
            return;
        }

        // Llamar al manager para iniciar el servidor
        BluetoothManager.Instance.StartServer();

        // Debug inmediato: la llamada se realizó
        // (la conexión real ocurre cuando alguien se conecte)
        Debug.Log("[RoleSelectorUI] Servidor iniciado. Este dispositivo es HOST. " +
                  "Esperando que un cliente se conecte...");
    }

    // ================================================================
    // UNIRSE A PARTIDA (CLIENTE)
    // ================================================================

    /// <summary>
    /// Llamado cuando el jugador presiona "Unirse a Partida".
    /// 
    /// Abre el panel de dispositivos para que el usuario
    /// seleccione a cuál conectarse.
    /// </summary>
    public void JoinGame()
    {
        if (deviceListUI == null)
        {
            Debug.LogError("[RoleSelectorUI] deviceListUI no está asignado en el Inspector.");
            return;
        }

        Debug.Log("[RoleSelectorUI] Abriendo lista de dispositivos emparejados...");
        deviceListUI.ShowDevices();
    }

    // ================================================================
    // CALLBACK DE CONEXIÓN EXITOSA
    // ================================================================

    /// <summary>
    /// Se ejecuta cuando BluetoothManager establece la conexión.
    /// Funciona para ambos roles (Host y Cliente).
    /// 
    /// Este método corre en el HILO PRINCIPAL gracias a
    /// UnityMainThreadDispatcher (llamado internamente por BluetoothManager).
    /// </summary>
    void OnBluetoothConnected()
    {
        string rol = BluetoothManager.Instance.IsHost ? "HOST" : "CLIENTE";
        Debug.Log($"[RoleSelectorUI] ¡Conexión Bluetooth establecida! Rol: {rol}");
    }
}
