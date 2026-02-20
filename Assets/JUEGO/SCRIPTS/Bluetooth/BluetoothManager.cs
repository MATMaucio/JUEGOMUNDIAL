using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// BluetoothManager
/// -----------------
/// Gestor central de conexión Bluetooth para Android usando SPP (Serial Port Profile).
///
/// Funcionalidades:
/// - Actuar como servidor (espera conexiones)
/// - Escanear dispositivos emparejados
/// - Conectarse como cliente
/// - Enviar y recibir mensajes
///
/// Arquitectura:
/// - Singleton (Instance)
/// - Uso de AndroidJavaObject para interactuar con la API nativa de Android
/// - Uso de Threads para evitar bloquear el hilo principal de Unity
/// - Recepción de datos en segundo plano
/// - Envío de datos en formato UTF8
///
/// IMPORTANTE:
/// - Solo funciona en Android (no en Editor).
/// - Requiere permisos Bluetooth en AndroidManifest.
/// </summary>
public class BluetoothManager : MonoBehaviour
{
    // ================= SINGLETON =================

    /// <summary>
    /// Instancia global del BluetoothManager.
    /// Permite acceder desde cualquier script mediante:
    /// BluetoothManager.Instance
    /// </summary>
    public static BluetoothManager Instance;

    // ================= ANDROID OBJECTS =================

    /// <summary>
    /// Adaptador Bluetooth principal del dispositivo.
    /// Representa android.bluetooth.BluetoothAdapter
    /// </summary>
    private AndroidJavaObject bluetoothAdapter;

    /// <summary>
    /// Socket del servidor (cuando actuamos como host).
    /// </summary>
    private AndroidJavaObject serverSocket;

    /// <summary>
    /// Socket activo de conexión (cliente o servidor).
    /// </summary>
    private AndroidJavaObject socket;

    /// <summary>
    /// Stream de entrada (lectura de datos).
    /// </summary>
    private AndroidJavaObject inputStream;

    /// <summary>
    /// Stream de salida (envío de datos).
    /// </summary>
    private AndroidJavaObject outputStream;

    // ================= THREADING =================

    /// <summary>
    /// Hilo dedicado a la lectura continua de datos.
    /// </summary>
    private Thread readThread;

    /// <summary>
    /// Controla el ciclo de lectura.
    /// </summary>
    private bool isReading = false;

    // ================= CONFIGURACIÓN =================

    /// <summary>
    /// Nombre del servicio Bluetooth.
    /// Visible cuando otro dispositivo busca la conexión.
    /// </summary>
    private const string APP_NAME = "UnityBTGame";

    /// <summary>
    /// UUID estándar para SPP (Serial Port Profile).
    /// Ambos dispositivos deben usar el mismo UUID.
    /// </summary>
    private const string UUID_STRING = "00001101-0000-1000-8000-00805F9B34FB";

    // ================= INICIALIZACIÓN =================

    void Awake()
    {
        // Implementación básica de Singleton
        Instance = this;

        // Evita que el objeto se destruya al cambiar de escena
        DontDestroyOnLoad(gameObject);

        #if UNITY_ANDROID && !UNITY_EDITOR

        // Obtener el adaptador Bluetooth del dispositivo
        using (AndroidJavaClass adapterClass =
               new AndroidJavaClass("android.bluetooth.BluetoothAdapter"))
        {
            bluetoothAdapter = adapterClass
                .CallStatic<AndroidJavaObject>("getDefaultAdapter");
        }

        #endif
    }

    // ============================================================
    // ======================= SERVER ==============================
    // ============================================================

    /// <summary>
    /// Inicia el dispositivo como servidor Bluetooth.
    /// Espera que otro dispositivo se conecte.
    /// </summary>
    public void StartServer()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR

        // Ejecutamos en un hilo separado para no congelar Unity
        new Thread(() =>
        {
            try
            {
                // Crear UUID
                AndroidJavaObject uuid = new AndroidJavaClass("java.util.UUID")
                    .CallStatic<AndroidJavaObject>("fromString", UUID_STRING);

                // Crear socket servidor
                serverSocket = bluetoothAdapter.Call<AndroidJavaObject>(
                    "listenUsingRfcommWithServiceRecord",
                    APP_NAME,
                    uuid);

                // Espera bloqueante hasta que un cliente se conecte
                socket = serverSocket.Call<AndroidJavaObject>("accept");

                // Configurar streams una vez conectado
                SetupStreams();
            }
            catch (Exception e)
            {
                Debug.Log("Server error: " + e.Message);
            }
        }).Start();

        #endif
    }

    // ============================================================
    // ======================= SCAN ================================
    // ============================================================

    /// <summary>
    /// Devuelve la lista de dispositivos previamente emparejados (bonded).
    /// </summary>
    public List<AndroidJavaObject> ScanDevices()
    {
        List<AndroidJavaObject> devices = new List<AndroidJavaObject>();

        #if UNITY_ANDROID && !UNITY_EDITOR

        AndroidJavaObject bondedDevices =
            bluetoothAdapter.Call<AndroidJavaObject>("getBondedDevices");

        AndroidJavaObject iterator =
            bondedDevices.Call<AndroidJavaObject>("iterator");

        while (iterator.Call<bool>("hasNext"))
        {
            AndroidJavaObject device =
                iterator.Call<AndroidJavaObject>("next");

            devices.Add(device);
        }

        #endif

        return devices;
    }

    // ============================================================
    // ======================= CLIENT ==============================
    // ============================================================

    /// <summary>
    /// Se conecta como cliente a un dispositivo Bluetooth seleccionado.
    /// </summary>
    public void ConnectToDevice(AndroidJavaObject device)
    {
        #if UNITY_ANDROID && !UNITY_EDITOR

        new Thread(() =>
        {
            try
            {
                AndroidJavaObject uuid = new AndroidJavaClass("java.util.UUID")
                    .CallStatic<AndroidJavaObject>("fromString", UUID_STRING);

                // Crear socket cliente
                socket = device.Call<AndroidJavaObject>(
                    "createRfcommSocketToServiceRecord", uuid);

                // Intentar conexión (bloqueante)
                socket.Call("connect");

                SetupStreams();
            }
            catch (Exception e)
            {
                Debug.Log("Client error: " + e.Message);
            }
        }).Start();

        #endif
    }

    // ============================================================
    // ================== STREAM CONFIGURATION =====================
    // ============================================================

    /// <summary>
    /// Configura los streams de entrada y salida
    /// e inicia el hilo de lectura.
    /// </summary>
    void SetupStreams()
    {
        inputStream = socket.Call<AndroidJavaObject>("getInputStream");
        outputStream = socket.Call<AndroidJavaObject>("getOutputStream");

        isReading = true;
        readThread = new Thread(ReadLoop);
        readThread.Start();
    }

    // ============================================================
    // ===================== RECEIVE LOOP ==========================
    // ============================================================

    /// <summary>
    /// Bucle continuo de lectura de datos Bluetooth.
    /// Corre en segundo plano.
    /// </summary>
    void ReadLoop()
    {
        byte[] buffer = new byte[1024];

        while (isReading)
        {
            try
            {
                // Método bloqueante: espera datos
                int bytes = inputStream.Call<int>("read", buffer);

                if (bytes > 0)
                {
                    string message =
                        System.Text.Encoding.UTF8.GetString(buffer, 0, bytes);

                    // Unity NO permite modificar objetos desde otros hilos.
                    // Por eso usamos un dispatcher para ejecutar en el hilo principal.
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        FindFirstObjectByType<CubeManager>()
                            .ReceiveCube(message);
                    });
                }
            }
            catch
            {
                // Se ignoran errores silenciosamente
                // (idealmente aquí debería manejarse mejor)
            }
        }
    }

    // ============================================================
    // ========================= SEND ==============================
    // ============================================================

    /// <summary>
    /// Envía un mensaje al dispositivo conectado.
    /// </summary>
    public void SendMessageBT(string message)
    {
        if (outputStream == null) return;

        byte[] bytes =
            System.Text.Encoding.UTF8.GetBytes(message);

        outputStream.Call("write", bytes);
    }
}