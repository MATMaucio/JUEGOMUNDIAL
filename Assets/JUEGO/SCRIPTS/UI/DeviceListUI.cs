using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// ============================================================
/// DeviceListUI — VERSIÓN CORREGIDA
/// ============================================================
/// 
/// PROBLEMAS DEL CÓDIGO ORIGINAL:
/// -------------------------------------------------------
/// 1. Si ScanDevices() devolvía lista vacía, el panel abría
///    pero no mostraba nada ni avisaba al usuario (ni Debug).
///
/// 2. No había ningún Debug al presionar el botón de conexión
///    (ni éxito ni fallo). Solo un Debug.Log("Botón presionado").
///
/// 3. Los botones generados no se destruían si se llamaba a
///    ShowDevices() más de una vez, acumulando botones duplicados.
///
/// 4. No había un texto de "sin dispositivos" cuando la lista estaba vacía.
///
/// CORRECCIONES APLICADAS:
/// -------------------------------------------------------
/// ✔ Se limpian los botones anteriores antes de generar nuevos.
/// ✔ Debug cuando no hay dispositivos encontrados.
/// ✔ Debug al presionar un botón (iniciando conexión).
/// ✔ Suscripción al evento OnConnected para confirmar si la conexión
///   tuvo éxito después de presionar el botón.
/// ✔ Texto de "sin dispositivos" si la lista está vacía.
/// ✔ Referencia al nombre del dispositivo guardada en closure
///   para el log post-conexión.
///
/// FLUJO COMPLETO:
/// -------------------------------------------------------
/// ShowDevices()
///   → limpia botones previos
///   → llama BluetoothManager.ScanDevices()
///   → si hay dispositivos: genera un botón por cada uno
///   → si no hay: muestra texto vacío y Debug
/// Usuario presiona botón
///   → Debug: "Intentando conectar a: [nombre]"
///   → BluetoothManager.ConnectToDevice(device)
///   → panel se cierra
/// BluetoothManager establece conexión
///   → evento OnConnected
///   → Debug: "Conexión establecida con: [nombre]"
///   O si falla (error en el Thread):
///   → Debug.LogError desde BluetoothManager
/// ============================================================
/// </summary>
public class DeviceListUI : MonoBehaviour
{
    // ================================================================
    // REFERENCIAS (asignar desde el Inspector)
    // ================================================================

    /// <summary>
    /// Contenedor donde se instancian los botones.
    /// Usualmente es el "Content" de un ScrollView con VerticalLayoutGroup.
    /// </summary>
    public Transform content;

    /// <summary>
    /// Prefab del botón de dispositivo.
    /// REQUISITOS del prefab:
    ///   - Componente Button
    ///   - Un hijo con componente Text (para el nombre del dispositivo)
    /// </summary>
    public GameObject buttonPrefab;

    /// <summary>
    /// Panel completo de selección de dispositivos.
    /// Se activa al abrir y desactiva al seleccionar.
    /// </summary>
    public GameObject panel;

    /// <summary>
    /// (Opcional) Texto que aparece si no hay dispositivos emparejados.
    /// Si no se asigna, simplemente no se muestra nada.
    /// </summary>
    public Text emptyText;

    // ================================================================
    // ESTADO INTERNO
    // ================================================================

    /// <summary>
    /// Nombre del último dispositivo al que se intentó conectar.
    /// Se usa en el callback de conexión para el Debug.
    /// </summary>
    private string lastSelectedDeviceName = "";

    // ================================================================
    // CICLO DE VIDA
    // ================================================================

    void Start()
    {
        // Suscribirse al evento de conexión para dar feedback al usuario
        if (BluetoothManager.Instance != null)
            BluetoothManager.Instance.OnConnected += OnConnectionSuccessful;
    }

    void OnDestroy()
    {
        if (BluetoothManager.Instance != null)
            BluetoothManager.Instance.OnConnected -= OnConnectionSuccessful;
    }

    // ================================================================
    // MOSTRAR DISPOSITIVOS
    // ================================================================

    /// <summary>
    /// Abre el panel y genera los botones de dispositivos emparejados.
    /// 
    /// LLAMADO DESDE: RoleSelectorUI.JoinGame()
    /// </summary>
    public void ShowDevices()
    {
        if (panel == null)
        {
            Debug.LogError("[DeviceListUI] 'panel' no está asignado en el Inspector.");
            return;
        }
        if (content == null)
        {
            Debug.LogError("[DeviceListUI] 'content' no está asignado en el Inspector.");
            return;
        }
        if (buttonPrefab == null)
        {
            Debug.LogError("[DeviceListUI] 'buttonPrefab' no está asignado en el Inspector.");
            return;
        }

        // --- CORRECCIÓN: Limpiar botones previos ---
        // Si ShowDevices() se llama más de una vez, los botones
        // anteriores se acumulan. Esto los elimina antes de regenerar.
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // Activar panel
        panel.SetActive(true);

        // Obtener dispositivos emparejados
        List<AndroidJavaObject> devices = BluetoothManager.Instance.ScanDevices();

        // --- CORRECCIÓN: Manejar lista vacía ---
        if (devices.Count == 0)
        {
            Debug.LogWarning("[DeviceListUI] No se encontraron dispositivos emparejados. " +
                             "Asegúrate de emparejar el dispositivo desde la configuración de Android.");

            // Mostrar texto de aviso si está asignado
            if (emptyText != null)
            {
                emptyText.gameObject.SetActive(true);
                emptyText.text = "No hay dispositivos emparejados.\nVe a Configuración > Bluetooth.";
            }
            return;
        }

        // Ocultar texto vacío si había uno
        if (emptyText != null)
            emptyText.gameObject.SetActive(false);

        Debug.Log($"[DeviceListUI] Mostrando {devices.Count} dispositivo(s) en el panel.");

        // Generar un botón por cada dispositivo
        foreach (AndroidJavaObject device in devices)
        {
            // Obtener nombre del dispositivo
            string name = device.Call<string>("getName");

            // Instanciar botón en el contenedor
            GameObject btn = Instantiate(buttonPrefab, content);

            // Asignar el nombre al texto del botón
            Text btnText = btn.GetComponentInChildren<Text>();
            if (btnText != null)
                btnText.text = name;
            else
                Debug.LogWarning($"[DeviceListUI] El prefab del botón no tiene componente Text en sus hijos.");

            // --- CORRECCIÓN: Capturar variables en closure correctamente ---
            // En C# los foreach pueden tener problemas con closures.
            // Creamos copias locales para asegurar que cada botón
            // capture su propio device y name.
            AndroidJavaObject capturedDevice = device;
            string capturedName = name;

            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnDeviceButtonPressed(capturedDevice, capturedName);
            });

            Debug.Log($"[DeviceListUI] Botón creado para dispositivo: {name}");
        }
    }

    // ================================================================
    // ACCIÓN AL PRESIONAR BOTÓN DE DISPOSITIVO
    // ================================================================

    /// <summary>
    /// Se ejecuta cuando el usuario presiona el botón de un dispositivo.
    /// 
    /// IMPORTANTE:
    /// La conexión real es asíncrona (ocurre en un Thread en BluetoothManager).
    /// Aquí solo iniciamos el proceso y cerramos el panel.
    /// El resultado (éxito o fallo) llegará via el evento OnConnected
    /// o por un Debug.LogError en BluetoothManager.
    /// </summary>
    void OnDeviceButtonPressed(AndroidJavaObject device, string deviceName)
    {
        // --- CORRECCIÓN: Debug al intentar conectar ---
        Debug.Log($"[DeviceListUI] Botón presionado. Intentando conectar con: '{deviceName}'...");

        // Guardar nombre para el callback de conexión
        lastSelectedDeviceName = deviceName;

        // Iniciar conexión en BluetoothManager
        BluetoothManager.Instance.ConnectToDevice(device);

        // Cerrar panel inmediatamente (la conexión ocurre en segundo plano)
        panel.SetActive(false);
    }

    // ================================================================
    // CALLBACK DE CONEXIÓN EXITOSA
    // ================================================================

    /// <summary>
    /// Se dispara cuando BluetoothManager confirma que la conexión
    /// se estableció correctamente.
    /// 
    /// Corre en el hilo principal gracias a UnityMainThreadDispatcher.
    /// </summary>
    void OnConnectionSuccessful()
    {
        Debug.Log($"[DeviceListUI] ✓ Conexión Bluetooth exitosa con: '{lastSelectedDeviceName}'");
    }
}
