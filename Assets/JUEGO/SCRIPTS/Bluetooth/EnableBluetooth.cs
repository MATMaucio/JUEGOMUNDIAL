using UnityEngine;

public class EnableBluetooth : MonoBehaviour
{
    void Start()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent"))
        using (AndroidJavaClass bluetoothAdapterClass = new AndroidJavaClass("android.bluetooth.BluetoothAdapter"))
        using (AndroidJavaObject currentActivity =
               new AndroidJavaClass("com.unity3d.player.UnityPlayer")
               .GetStatic<AndroidJavaObject>("currentActivity"))
        {
            string action = bluetoothAdapterClass.GetStatic<string>("ACTION_REQUEST_ENABLE");
            AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", action);
            currentActivity.Call("startActivity", intent);
        }
        #endif
    }
}
