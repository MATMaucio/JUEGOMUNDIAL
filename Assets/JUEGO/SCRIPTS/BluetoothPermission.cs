using UnityEngine;
using UnityEngine.Android;

public class BluetoothPermission : MonoBehaviour
{
    void Start()
    {
        if (!Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_CONNECT"))
        {
            Permission.RequestUserPermission("android.permission.BLUETOOTH_CONNECT");
            Permission.RequestUserPermission("android.permission.ACCESS_FINE_LOCATION");

        }

        if (!Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_SCAN"))
        {
            Permission.RequestUserPermission("android.permission.BLUETOOTH_SCAN");
            Permission.RequestUserPermission("android.permission.ACCESS_FINE_LOCATION");

        }
    }
}
