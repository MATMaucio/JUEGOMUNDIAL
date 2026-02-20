using UnityEngine;

/// <summary>
/// Clase que verifica si el Bluetooth del dispositivo Android está activado.
/// 
/// IMPORTANTE:
/// - Solo funciona en Android.
/// - No se ejecuta dentro del Editor de Unity.
/// - Usa AndroidJavaClass y AndroidJavaObject para comunicarse con la API nativa de Android.
/// </summary>
public class BluetoothCheck : MonoBehaviour
{
    /// <summary>
    /// Método Start de Unity.
    /// Se ejecuta automáticamente cuando el objeto que contiene este script se inicializa.
    /// </summary>
    void Start()
    {
        // Compilación condicional:
        // Este bloque SOLO se ejecuta si:
        // - La plataforma es Android
        // - NO estamos dentro del Editor de Unity
        #if UNITY_ANDROID && !UNITY_EDITOR

        // AndroidJavaClass permite acceder a una clase nativa de Android.
        // En este caso estamos accediendo a:
        // android.bluetooth.BluetoothAdapter
        using (AndroidJavaClass bluetoothAdapterClass = 
               new AndroidJavaClass("android.bluetooth.BluetoothAdapter"))
        {
            // Llamamos al método estático getDefaultAdapter()
            // que devuelve el adaptador Bluetooth del dispositivo.
            AndroidJavaObject adapter = bluetoothAdapterClass
                                         .CallStatic<AndroidJavaObject>("getDefaultAdapter");

            // Llamamos al método isEnabled() del adaptador
            // para saber si el Bluetooth está activado.
            bool isEnabled = adapter.Call<bool>("isEnabled");

            // Mostramos en la consola de Unity el estado del Bluetooth.
            Debug.Log("Bluetooth activo: " + isEnabled);
        }

        #endif
    }
}