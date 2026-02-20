using UnityEngine;

/// <summary>
/// EnableBluetooth
/// ----------------
/// Script encargado de solicitar al usuario que active el Bluetooth
/// mediante un Intent nativo de Android.
///
/// Funcionamiento:
/// - Lanza una ventana del sistema solicitando activar el Bluetooth.
/// - NO activa el Bluetooth automáticamente (eso no está permitido).
/// - Solo funciona en Android y fuera del Editor.
///
/// Este script debe ejecutarse antes de intentar usar Bluetooth
/// si el adaptador está desactivado.
/// </summary>
public class EnableBluetooth : MonoBehaviour
{
    /// <summary>
    /// Start se ejecuta al iniciar el objeto.
    /// Aquí se crea y lanza un Intent para solicitar
    /// la activación del Bluetooth.
    /// </summary>
    void Start()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR

        // Clase Intent de Android
        using (AndroidJavaClass intentClass = 
               new AndroidJavaClass("android.content.Intent"))

        // Clase BluetoothAdapter para acceder a constantes estáticas
        using (AndroidJavaClass bluetoothAdapterClass = 
               new AndroidJavaClass("android.bluetooth.BluetoothAdapter"))

        // Obtener la actividad actual de Unity
        using (AndroidJavaObject currentActivity =
               new AndroidJavaClass("com.unity3d.player.UnityPlayer")
               .GetStatic<AndroidJavaObject>("currentActivity"))
        {
            // Obtener la constante ACTION_REQUEST_ENABLE
            // Esta acción abre la ventana del sistema que pide activar Bluetooth
            string action = bluetoothAdapterClass
                .GetStatic<string>("ACTION_REQUEST_ENABLE");

            // Crear un Intent con esa acción
            AndroidJavaObject intent =
                new AndroidJavaObject("android.content.Intent", action);

            // Lanzar el Intent
            currentActivity.Call("startActivity", intent);
        }

        #endif
    }
}