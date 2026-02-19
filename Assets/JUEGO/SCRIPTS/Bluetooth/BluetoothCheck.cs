using UnityEngine;

public class BluetoothCheck : MonoBehaviour
{
    void Start()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass bluetoothAdapterClass = 
               new AndroidJavaClass("android.bluetooth.BluetoothAdapter"))
        {
            AndroidJavaObject adapter = bluetoothAdapterClass
                                         .CallStatic<AndroidJavaObject>("getDefaultAdapter");

            bool isEnabled = adapter.Call<bool>("isEnabled");
            Debug.Log("Bluetooth activo: " + isEnabled);
        }
        #endif
    }
}
