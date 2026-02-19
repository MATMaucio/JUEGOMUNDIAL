using UnityEngine;

public class RoleSelectorUI : MonoBehaviour
{
    public DeviceListUI deviceListUI;

    public void CreateGame()
    {
        BluetoothManager.Instance.StartServer();
    }

    public void JoinGame()
    {
        deviceListUI.ShowDevices();
    }
}
