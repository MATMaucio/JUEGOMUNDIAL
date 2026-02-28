using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ============================================================
/// CubeManager — VERSIÓN CORREGIDA
/// ============================================================
/// 
/// PROBLEMAS DEL CÓDIGO ORIGINAL:
/// -------------------------------------------------------
/// 1. TrySendCube() siempre enviaba el cubo por Bluetooth,
///    sin importar si el usuario era HOST o CLIENTE.
///    En un juego donde solo el HOST puede enviar cubos,
///    esto es incorrecto.
///
/// 2. No había Debug al activar, desactivar o enviar cubos.
///
/// 3. ReceiveCube() no confirmaba con Debug si el cubo
///    recibido existía en la lista local o no.
///
/// CORRECCIONES APLICADAS:
/// -------------------------------------------------------
/// ✔ TrySendCube() verifica BluetoothManager.IsHost antes de enviar.
///   - Si es HOST: desactiva el cubo local y lo envía por BT.
///   - Si es CLIENTE: no puede enviar cubos (solo recibirlos).
/// ✔ Debug en cada paso de activación, desactivación y envío.
/// ✔ ReceiveCube() loguea si el cubo fue encontrado o no.
/// ✔ Verificación de conexión antes de enviar.
///
/// FLUJO COMPLETO:
/// -------------------------------------------------------
/// [HOST presiona cubo]
///   TouchInputManager → TrySendCube(id)
///   → Verifica que el cubo esté activo
///   → Desactiva el cubo localmente
///   → Debug: "Cubo [id] desactivado localmente y enviado al cliente"
///   → BluetoothManager.SendMessageBT(id)
///
/// [CLIENTE recibe el mensaje]
///   BluetoothManager ReadLoop → ReceiveCube(id)
///   → ActivateCube(id)
///   → Debug: "Cubo [id] activado por recepción Bluetooth"
/// ============================================================
/// </summary>
public class CubeManager : MonoBehaviour
{
    // ================================================================
    // REFERENCIAS (asignar desde el Inspector)
    // ================================================================

    /// <summary>
    /// Lista de todos los CubeItem de la escena.
    /// Asignar manualmente desde el Inspector de Unity.
    /// </summary>
    public List<CubeItem> allCubes;

    // ================================================================
    // ESTADO INTERNO
    // ================================================================

    /// <summary>
    /// IDs de cubos actualmente activos (visibles en escena).
    /// HashSet para búsqueda O(1) y sin duplicados.
    /// </summary>
    private HashSet<string> activeCubes = new HashSet<string>();

    // ================================================================
    // INICIALIZACIÓN
    // ================================================================

    void Start()
    {
        // Desactivar todos los cubos al iniciar la escena
        foreach (var cube in allCubes)
        {
            cube.gameObject.SetActive(false);
        }

        Debug.Log($"[CubeManager] {allCubes.Count} cubo(s) registrados. Todos desactivados al inicio.");
    }

    // ================================================================
    // ACTIVACIÓN LOCAL
    // ================================================================

    /// <summary>
    /// Activa un cubo por su ID (lo hace visible en escena).
    /// Usado cuando:
    /// - Se recibe un mensaje Bluetooth con el ID del cubo.
    /// - Se activa desde un botón UI (ActivateCubeButton).
    /// </summary>
    public void ActivateCube(string id)
    {
        bool found = false;

        foreach (var cube in allCubes)
        {
            if (cube.cubeID == id)
            {
                cube.gameObject.SetActive(true);
                activeCubes.Add(id);
                found = true;
                Debug.Log($"[CubeManager] Cubo activado localmente: '{id}'");
                break;
            }
        }

        if (!found)
        {
            Debug.LogWarning($"[CubeManager] ActivateCube: No se encontró ningún cubo con ID '{id}'.");
        }
    }

    // ================================================================
    // ENVÍO REMOTO (solo HOST)
    // ================================================================

    /// <summary>
    /// Intenta desactivar un cubo local y enviarlo al dispositivo conectado.
    /// 
    /// REGLA DE ROL:
    /// - Solo el HOST puede enviar cubos al cliente.
    /// - Si este dispositivo es CLIENTE, no puede enviar.
    ///   (El diseño del juego define que los cubos viajan Host → Cliente)
    /// 
    /// LLAMADO DESDE: TouchInputManager cuando el usuario toca un cubo.
    /// </summary>
    public void TrySendCube(string id)
    {
        // --- CORRECCIÓN: Verificar rol ---
        // El código original no revisaba IsHost.
        // Si un cliente tocaba un cubo, igual intentaba enviarlo.
        if (BluetoothManager.Instance == null)
        {
            Debug.LogWarning("[CubeManager] TrySendCube: BluetoothManager no disponible.");
            return;
        }

        if (!BluetoothManager.Instance.IsHost)
        {
            Debug.Log($"[CubeManager] TrySendCube cancelado: este dispositivo es CLIENTE, no puede enviar cubos.");
            return;
        }

        // Verificar que el cubo esté activo
        if (!activeCubes.Contains(id))
        {
            Debug.LogWarning($"[CubeManager] TrySendCube: El cubo '{id}' no está activo. No se puede enviar.");
            return;
        }

        // Verificar que haya conexión activa
        if (!BluetoothManager.Instance.IsConnected)
        {
            Debug.LogWarning($"[CubeManager] TrySendCube: No hay conexión Bluetooth activa. Cubo '{id}' no enviado.");
            return;
        }

        // Desactivar localmente
        foreach (var cube in allCubes)
        {
            if (cube.cubeID == id)
            {
                cube.gameObject.SetActive(false);
                activeCubes.Remove(id);
                Debug.Log($"[CubeManager] Cubo '{id}' desactivado localmente (HOST).");
                break;
            }
        }

        // Enviar ID al cliente por Bluetooth
        BluetoothManager.Instance.SendMessageBT(id);
        Debug.Log($"[CubeManager] Cubo '{id}' enviado al cliente por Bluetooth.");
    }

    // ================================================================
    // RECEPCIÓN REMOTA (solo CLIENTE)
    // ================================================================

    /// <summary>
    /// Llamado cuando se recibe un mensaje Bluetooth con un ID de cubo.
    /// Activa el cubo correspondiente en este dispositivo.
    /// 
    /// LLAMADO DESDE: BluetoothManager.ReadLoop (via UnityMainThreadDispatcher)
    /// </summary>
    public void ReceiveCube(string id)
    {
        Debug.Log($"[CubeManager] Recibido por Bluetooth: cubo '{id}'. Activando...");
        ActivateCube(id);
    }

    // ================================================================
    // ACTIVACIÓN DESDE UI
    // ================================================================

    /// <summary>
    /// Permite activar un cubo desde un botón de la interfaz.
    /// Útil para testing o para mecánicas adicionales.
    /// </summary>
    public void ActivateCubeButton(string id)
    {
        Debug.Log($"[CubeManager] ActivateCubeButton llamado para: '{id}'");
        ActivateCube(id);
    }
}
