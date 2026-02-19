using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DeviceListUI : MonoBehaviour
{
    public Transform content;
    public GameObject buttonPrefab;
public GameObject panel;

public void ShowDevices()
{
    panel.SetActive(true);   // 🔥 AQUÍ se activa

    var devices = BluetoothManager.Instance.ScanDevices();

    foreach (var device in devices)
    {
        string name = device.Call<string>("getName");

        GameObject btn = Instantiate(buttonPrefab, content);
        btn.GetComponentInChildren<Text>().text = name;

        btn.GetComponent<Button>().onClick.AddListener(() =>
        {
            BluetoothManager.Instance.ConnectToDevice(device);
            panel.SetActive(false); // 🔥 Se oculta al conectar
        });
    }
}

}
