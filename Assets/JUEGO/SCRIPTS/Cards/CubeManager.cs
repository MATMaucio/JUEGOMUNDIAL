using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// CubeManager
/// -----------
/// Gestiona todos los cubos del juego y su sincronización
/// entre dispositivos mediante Bluetooth.
///
/// Responsabilidades:
/// - Mantener referencia a todos los cubos disponibles.
/// - Activar cubos localmente.
/// - Desactivar y enviar cubos al otro dispositivo.
/// - Recibir activaciones desde Bluetooth.
///
/// Depende de:
/// - CubeItem (identificador único por cubo)
/// - BluetoothManager (envío de mensajes)
/// </summary>
public class CubeManager : MonoBehaviour
{
    /// <summary>
    /// Lista de todos los cubos existentes en la escena.
    /// Debe asignarse manualmente desde el Inspector.
    /// </summary>
    public List<CubeItem> allCubes;

    /// <summary>
    /// Conjunto de IDs de cubos actualmente activos.
    /// 
    /// Se usa HashSet por eficiencia:
    /// - Búsqueda O(1)
    /// - Evita duplicados
    /// </summary>
    private HashSet<string> activeCubes = new HashSet<string>();

    /// <summary>
    /// Al iniciar la escena, todos los cubos se desactivan.
    /// </summary>
    void Start()
    {
        foreach (var cube in allCubes)
        {
            cube.gameObject.SetActive(false);
        }
    }

    // =====================================================
    // ================= ACTIVACIÓN LOCAL ==================
    // =====================================================

    /// <summary>
    /// Activa un cubo por su ID.
    /// También lo marca como activo en el sistema interno.
    /// </summary>
    public void ActivateCube(string id)
    {
        foreach (var cube in allCubes)
        {
            if (cube.cubeID == id)
            {
                cube.gameObject.SetActive(true);
                activeCubes.Add(id);
            }
        }
    }

    // =====================================================
    // ================= ENVÍO REMOTO ======================
    // =====================================================

    /// <summary>
    /// Intenta enviar un cubo al otro dispositivo.
    /// 
    /// Flujo:
    /// 1. Verifica que el cubo esté activo.
    /// 2. Lo desactiva localmente.
    /// 3. Lo elimina del HashSet.
    /// 4. Envía su ID por Bluetooth.
    /// </summary>
    public void TrySendCube(string id)
    {
        // Si el cubo no está activo, no se puede enviar
        if (!activeCubes.Contains(id))
            return;

        // Apagar localmente
        foreach (var cube in allCubes)
        {
            if (cube.cubeID == id)
            {
                cube.gameObject.SetActive(false);
                activeCubes.Remove(id);
                break;
            }
        }

        // Enviar ID al otro dispositivo
        BluetoothManager.Instance.SendMessageBT(id);
    }

    // =====================================================
    // ================= RECEPCIÓN REMOTA ==================
    // =====================================================

    /// <summary>
    /// Llamado cuando se recibe un mensaje Bluetooth.
    /// Activa el cubo correspondiente.
    /// </summary>
    public void ReceiveCube(string id)
    {
        ActivateCube(id);
    }

    /// <summary>
    /// Método público para vincular a un botón UI.
    /// Permite activar cubos desde la interfaz.
    /// </summary>
    public void ActivateCubeButton(string id)
    {
        ActivateCube(id);
    }
}