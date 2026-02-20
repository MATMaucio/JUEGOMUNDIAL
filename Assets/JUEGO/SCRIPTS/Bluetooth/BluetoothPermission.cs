using UnityEngine;
using UnityEngine.Android;

/// <summary>
/// BluetoothPermission
/// --------------------
/// Script encargado de solicitar permisos de Bluetooth en tiempo de ejecución
/// en dispositivos Android.
///
/// IMPORTANTE:
/// - Desde Android 12 (API 31) los permisos de Bluetooth se separaron.
/// - Ahora se requieren permisos específicos como:
///     • BLUETOOTH_CONNECT
///     • BLUETOOTH_SCAN
/// - También se solicita ACCESS_FINE_LOCATION porque algunos dispositivos
///   aún lo requieren para el escaneo Bluetooth.
///
/// Este script debe ejecutarse antes de usar cualquier funcionalidad
/// relacionada con Bluetooth.
/// </summary>
public class BluetoothPermission : MonoBehaviour
{
    /// <summary>
    /// Start se ejecuta cuando el objeto se inicializa.
    /// Aquí se verifican y solicitan permisos si aún no fueron concedidos.
    /// </summary>
    void Start()
    {
        // =============================
        // Permiso para conectarse a dispositivos Bluetooth
        // =============================

        if (!Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_CONNECT"))
        {
            // Solicita permiso para conectarse a dispositivos emparejados
            Permission.RequestUserPermission("android.permission.BLUETOOTH_CONNECT");

            // Algunos dispositivos aún requieren ubicación para Bluetooth
            Permission.RequestUserPermission("android.permission.ACCESS_FINE_LOCATION");
        }

        // =============================
        // Permiso para escanear dispositivos Bluetooth
        // =============================

        if (!Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_SCAN"))
        {
            // Solicita permiso para escanear dispositivos cercanos
            Permission.RequestUserPermission("android.permission.BLUETOOTH_SCAN");

            // Se vuelve a solicitar ubicación por compatibilidad
            Permission.RequestUserPermission("android.permission.ACCESS_FINE_LOCATION");
        }
    }
}