using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// DeviceListUI
/// -------------
/// Se encarga de:
/// - Mostrar en pantalla los dispositivos Bluetooth emparejados.
/// - Generar botones dinámicamente para cada dispositivo.
/// - Conectar al dispositivo seleccionado al presionar un botón.
/// 
/// Requisitos:
/// - BluetoothManager debe estar inicializado.
/// - buttonPrefab debe contener:
///     • Un componente Button
///     • Un componente Text (hijo)
/// - content debe ser el contenedor (por ejemplo un VerticalLayoutGroup).
/// </summary>
public class DeviceListUI : MonoBehaviour
{
    /// <summary>
    /// Contenedor donde se instancian los botones.
    /// Generalmente es el "Content" de un ScrollView.
    /// </summary>
    public Transform content;

    /// <summary>
    /// Prefab del botón que representa un dispositivo.
    /// Debe incluir:
    /// - Button
    /// - Text en un hijo
    /// </summary>
    public GameObject buttonPrefab;

    /// <summary>
    /// Panel que contiene la lista de dispositivos.
    /// Se activa al mostrar dispositivos y se oculta al conectar.
    /// </summary>
    public GameObject panel;

    /// <summary>
    /// Muestra el panel y genera la lista de dispositivos emparejados.
    /// </summary>
    public void ShowDevices()
    {
        // Activar panel de selección
        panel.SetActive(true);

        // Obtener dispositivos emparejados desde BluetoothManager
        var devices = BluetoothManager.Instance.ScanDevices();

        // Recorrer cada dispositivo encontrado
        foreach (var device in devices)
        {
            // Obtener nombre visible del dispositivo
            string name = device.Call<string>("getName");

            // Instanciar botón dentro del contenedor
            GameObject btn = Instantiate(buttonPrefab, content);

            // Asignar el nombre al texto del botón
            btn.GetComponentInChildren<Text>().text = name;

            // Asignar acción al presionar el botón
            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log("Botón presionado");

                // Conectarse al dispositivo seleccionado
                BluetoothManager.Instance.ConnectToDevice(device);

                // Ocultar panel después de seleccionar
                panel.SetActive(false);
            });
        }
    }
}