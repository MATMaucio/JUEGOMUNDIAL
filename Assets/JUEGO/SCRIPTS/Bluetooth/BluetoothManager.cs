using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// ============================================================
/// BluetoothManager — VERSIÓN CORREGIDA
/// ============================================================
/// 
/// PROBLEMAS DEL CÓDIGO ORIGINAL:
/// -------------------------------------------------------
/// 1. Singleton sin protección: si existía otra instancia,
///    la sobreescribía sin destruir la anterior.
///
/// 2. StartServer() y ConnectToDevice() no tenían NINGÚN
///    Debug.Log para saber si la conexión funcionó o falló.
///
/// 3. ReadLoop() silenciaba TODOS los errores con un catch vacío.
///    Si el socket se cerraba o había un error de stream,
///    el hilo seguía corriendo en un loop infinito silencioso.
///
/// 4. No existía ninguna forma de saber si ya estabas conectado
///    antes de intentar enviar un mensaje.
///
/// 5. No había un concepto de ROL (host/cliente) en el manager,
///    lo que hace imposible que CubeManager sepa si debe
///    enviar o activar localmente el cubo.
///
/// CORRECCIONES APLICADAS:
/// -------------------------------------------------------
/// ✔ Singleton con destrucción del duplicado.
/// ✔ Propiedad pública IsConnected para saber el estado.
/// ✔ Propiedad pública IsHost para saber el rol.
/// ✔ Debug.Log en cada paso relevante (conexión, stream, error).
/// ✔ ReadLoop con manejo correcto del error: detiene el hilo
///   y loguea el motivo del fallo.
/// ✔ SendMessageBT verifica conexión antes de enviar.
/// ✔ Evento público OnConnected para notificar a otros scripts.
/// ============================================================
/// </summary>
public class BluetoothManager : MonoBehaviour
{
    // ================================================================
    // SINGLETON
    // ================================================================

    /// <summary>
    /// Instancia global accesible desde cualquier script.
    /// </summary>
    public static BluetoothManager Instance;

    // ================================================================
    // ESTADO PÚBLICO
    // ================================================================

    /// <summary>
    /// TRUE si ya hay un socket activo y streams configurados.
    /// Otros scripts deben verificar esto antes de enviar datos.
    /// </summary>
    public bool IsConnected { get; private set; } = false;

    /// <summary>
    /// TRUE si este dispositivo actuó como servidor (creó la partida).
    /// FALSE si actuó como cliente (se unió).
    /// CubeManager lo usa para saber si debe enviar o activar cubos.
    /// </summary>
    public bool IsHost { get; private set; } = false;

    // ================================================================
    // EVENTO DE CONEXIÓN
    // ================================================================

    /// <summary>
    /// Se dispara cuando la conexión se establece correctamente.
    /// Otros scripts pueden suscribirse para reaccionar.
    /// Ejemplo: BluetoothManager.Instance.OnConnected += MiFuncion;
    /// </summary>
    public event Action OnConnected;

    // ================================================================
    // OBJETOS ANDROID INTERNOS
    // ================================================================

    private AndroidJavaObject bluetoothAdapter;
    private AndroidJavaObject serverSocket;
    private AndroidJavaObject socket;
    private AndroidJavaObject inputStream;
    private AndroidJavaObject outputStream;

    // ================================================================
    // THREADING
    // ================================================================

    private Thread readThread;
    private bool isReading = false;

    // ================================================================
    // CONFIGURACIÓN
    // ================================================================

    /// <summary>
    /// Nombre visible del servicio Bluetooth al crear partida.
    /// </summary>
    private const string APP_NAME = "UnityBTGame";

    /// <summary>
    /// UUID estándar SPP. AMBOS dispositivos deben usar el mismo.
    /// Si este UUID no coincide, la conexión falla silenciosamente.
    /// </summary>
    private const string UUID_STRING = "00001101-0000-1000-8000-00805F9B34FB";

    // ================================================================
    // INICIALIZACIÓN
    // ================================================================

    void Awake()
    {
        // --- CORRECCIÓN: Singleton con destrucción del duplicado ---
        // En el código original, si había dos instancias,
        // una sobreescribía a la otra sin destruir el GameObject anterior.
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[BT] Ya existe una instancia de BluetoothManager. Destruyendo duplicado.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

#if UNITY_ANDROID && !UNITY_EDITOR
        // Obtener el adaptador Bluetooth nativo del dispositivo
        using (AndroidJavaClass adapterClass =
               new AndroidJavaClass("android.bluetooth.BluetoothAdapter"))
        {
            bluetoothAdapter = adapterClass
                .CallStatic<AndroidJavaObject>("getDefaultAdapter");

            if (bluetoothAdapter == null)
                Debug.LogError("[BT] El dispositivo no tiene adaptador Bluetooth.");
            else
                Debug.Log("[BT] BluetoothAdapter inicializado correctamente.");
        }
#else
        Debug.LogWarning("[BT] BluetoothManager: estás en el Editor. Las funciones Bluetooth no operan.");
#endif
    }

    // ================================================================
    // SERVIDOR — CREAR PARTIDA
    // ================================================================

    /// <summary>
    /// Inicia el dispositivo como servidor (Host).
    /// Queda escuchando hasta que un cliente se conecte.
    /// 
    /// FLUJO:
    /// 1. Crea un serverSocket con listenUsingRfcommWithServiceRecord.
    /// 2. Llama a accept() — BLOQUEANTE — espera al cliente.
    /// 3. Cuando llega un cliente, configura los streams.
    /// 4. Dispara OnConnected en el hilo principal.
    /// 
    /// NOTA: Por qué usamos Thread:
    /// accept() es bloqueante y congelaría Unity si lo llamamos
    /// directamente en el hilo principal.
    /// </summary>
    public void StartServer()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        IsHost = true;
        Debug.Log("[BT] Iniciando servidor Bluetooth... esperando cliente.");

        new Thread(() =>
        {
            try
            {
                // Convertir el UUID string al tipo java.util.UUID
                AndroidJavaObject uuid = new AndroidJavaClass("java.util.UUID")
                    .CallStatic<AndroidJavaObject>("fromString", UUID_STRING);

                // Crear el socket de escucha
                serverSocket = bluetoothAdapter.Call<AndroidJavaObject>(
                    "listenUsingRfcommWithServiceRecord", APP_NAME, uuid);

                Debug.Log("[BT] Socket servidor creado. Esperando conexión entrante...");

                // accept() bloquea hasta que alguien se conecta
                socket = serverSocket.Call<AndroidJavaObject>("accept");

                if (socket != null)
                {
                    Debug.Log("[BT] Cliente conectado. Configurando streams...");
                    SetupStreams();

                    // Cerramos el serverSocket porque ya no necesitamos
                    // aceptar más conexiones (solo es 1 cliente)
                    serverSocket.Call("close");
                }
                else
                {
                    Debug.LogError("[BT] accept() devolvió null. No se pudo establecer la conexión.");
                }
            }
            catch (Exception e)
            {
                // --- CORRECCIÓN: Error con causa específica ---
                // El original no tenía ningún debug aquí.
                Debug.LogError("[BT] Error en el servidor: " + e.Message);
            }
        }).Start();

#else
        // Simulación en Editor para pruebas
        Debug.Log("[BT EDITOR] StartServer() llamado. En el dispositivo real esperaría conexión.");
        IsHost = true;
#endif
    }

    // ================================================================
    // ESCANEO DE DISPOSITIVOS EMPAREJADOS
    // ================================================================

    /// <summary>
    /// Devuelve los dispositivos Bluetooth previamente emparejados.
    /// 
    /// IMPORTANTE: Estos NO son dispositivos "cercanos" en tiempo real.
    /// Son los que ya tenían un vínculo (bond) previo con este teléfono.
    /// Para ver dispositivos nuevos, el usuario debe emparejarlos primero
    /// desde la configuración de Android.
    /// </summary>
    public List<AndroidJavaObject> ScanDevices()
    {
        List<AndroidJavaObject> devices = new List<AndroidJavaObject>();

#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaObject bondedDevices =
            bluetoothAdapter.Call<AndroidJavaObject>("getBondedDevices");

        AndroidJavaObject iterator =
            bondedDevices.Call<AndroidJavaObject>("iterator");

        int count = 0;
        while (iterator.Call<bool>("hasNext"))
        {
            AndroidJavaObject device = iterator.Call<AndroidJavaObject>("next");
            devices.Add(device);
            count++;
        }

        Debug.Log($"[BT] Dispositivos emparejados encontrados: {count}");
#else
        Debug.Log("[BT EDITOR] ScanDevices() llamado. En el Editor no hay dispositivos reales.");
#endif

        return devices;
    }

    // ================================================================
    // CLIENTE — CONECTARSE A DISPOSITIVO
    // ================================================================

    /// <summary>
    /// Se conecta como cliente al dispositivo seleccionado.
    /// 
    /// FLUJO:
    /// 1. Crea un socket cliente con createRfcommSocketToServiceRecord.
    /// 2. Llama a connect() — BLOQUEANTE — intenta conectar al host.
    /// 3. Si tiene éxito, configura streams y dispara OnConnected.
    /// 
    /// PARÁMETRO device:
    /// Es un AndroidJavaObject de tipo BluetoothDevice,
    /// obtenido desde ScanDevices().
    /// </summary>
    public void ConnectToDevice(AndroidJavaObject device)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        IsHost = false;

        string deviceName = device.Call<string>("getName");
        Debug.Log($"[BT] Intentando conectar al dispositivo: {deviceName}");

        new Thread(() =>
        {
            try
            {
                AndroidJavaObject uuid = new AndroidJavaClass("java.util.UUID")
                    .CallStatic<AndroidJavaObject>("fromString", UUID_STRING);

                // Crear socket de cliente hacia el dispositivo
                socket = device.Call<AndroidJavaObject>(
                    "createRfcommSocketToServiceRecord", uuid);

                // connect() bloquea hasta conectar o fallar
                socket.Call("connect");

                Debug.Log($"[BT] Conexión exitosa con: {deviceName}");
                SetupStreams();
            }
            catch (Exception e)
            {
                // --- CORRECCIÓN: Debug del error de conexión ---
                // El original no avisaba si connect() fallaba.
                Debug.LogError($"[BT] Error al conectar con {deviceName}: {e.Message}");
            }
        }).Start();

#else
        Debug.Log("[BT EDITOR] ConnectToDevice() llamado. Sin dispositivo real en el Editor.");
        IsHost = false;
#endif
    }

    // ================================================================
    // CONFIGURACIÓN DE STREAMS
    // ================================================================

    /// <summary>
    /// Obtiene los streams de entrada/salida del socket conectado
    /// y arranca el hilo de lectura continua.
    /// 
    /// Se llama tanto desde StartServer() (cuando acepta cliente)
    /// como desde ConnectToDevice() (cuando conecta al host).
    /// </summary>
    void SetupStreams()
    {
        try
        {
            inputStream  = socket.Call<AndroidJavaObject>("getInputStream");
            outputStream = socket.Call<AndroidJavaObject>("getOutputStream");

            IsConnected = true;

            Debug.Log("[BT] Streams configurados. Conexión activa.");

            // Notificar al hilo principal que ya estamos conectados
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log("[BT] OnConnected disparado en hilo principal.");
                OnConnected?.Invoke();
            });

            // Iniciar lectura en segundo plano
            isReading = true;
            readThread = new Thread(ReadLoop);
            readThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError("[BT] Error configurando streams: " + e.Message);
        }
    }

    // ================================================================
    // LECTURA CONTINUA
    // ================================================================

    /// <summary>
    /// Bucle que corre en un Thread separado.
    /// Lee bytes del inputStream de forma bloqueante.
    /// 
    /// CORRECCIÓN respecto al original:
    /// - El catch vacío original silenciaba TODOS los errores.
    /// - Ahora si hay un error, se loguea y el bucle se detiene
    ///   correctamente en lugar de correr infinitamente.
    /// - Se actualiza IsConnected = false al desconectarse.
    /// </summary>
    void ReadLoop()
    {
        byte[] buffer = new byte[1024];

        while (isReading)
        {
            try
            {
                int bytes = inputStream.Call<int>("read", buffer);

                if (bytes > 0)
                {
                    string message =
                        System.Text.Encoding.UTF8.GetString(buffer, 0, bytes);

                    Debug.Log($"[BT] Mensaje recibido: '{message}'");

                    // Ejecutar en el hilo principal (Unity no permite
                    // modificar GameObjects desde otros hilos)
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        FindFirstObjectByType<CubeManager>()?.ReceiveCube(message);
                    });
                }
            }
            catch (Exception e)
            {
                // --- CORRECCIÓN: Detener el loop y reportar el error ---
                Debug.LogError("[BT] Error en ReadLoop (hilo detenido): " + e.Message);
                isReading = false;
                IsConnected = false;
            }
        }
    }

    // ================================================================
    // ENVÍO DE DATOS
    // ================================================================

    /// <summary>
    /// Envía un string al dispositivo conectado como bytes UTF8.
    /// 
    /// CORRECCIÓN:
    /// - El original no verificaba IsConnected antes de enviar.
    /// - Ahora si no hay conexión, se loguea el error en lugar
    ///   de lanzar una NullReferenceException silenciosa.
    /// </summary>
    public void SendMessageBT(string message)
    {
        if (!IsConnected || outputStream == null)
        {
            Debug.LogWarning("[BT] SendMessageBT: No hay conexión activa. Mensaje no enviado: " + message);
            return;
        }

        try
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message);
            outputStream.Call("write", bytes);
            Debug.Log($"[BT] Mensaje enviado: '{message}'");
        }
        catch (Exception e)
        {
            Debug.LogError("[BT] Error al enviar mensaje: " + e.Message);
        }
    }

    // ================================================================
    // LIMPIEZA
    // ================================================================

    /// <summary>
    /// Se ejecuta cuando el objeto es destruido.
    /// Cierra el hilo de lectura y los sockets para
    /// evitar fugas de recursos.
    /// </summary>
    void OnDestroy()
    {
        isReading = false;

        try { socket?.Call("close"); }       catch { }
        try { serverSocket?.Call("close"); } catch { }

        Debug.Log("[BT] BluetoothManager destruido. Recursos liberados.");
    }
}
