using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;

public class BluetoothManager : MonoBehaviour
{
    public static BluetoothManager Instance;

    private AndroidJavaObject bluetoothAdapter;
    private AndroidJavaObject serverSocket;
    private AndroidJavaObject socket;
    private AndroidJavaObject inputStream;
    private AndroidJavaObject outputStream;

    private Thread readThread;
    private bool isReading = false;

    private const string APP_NAME = "UnityBTGame";
    private const string UUID_STRING = "00001101-0000-1000-8000-00805F9B34FB";

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        #if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass adapterClass =
               new AndroidJavaClass("android.bluetooth.BluetoothAdapter"))
        {
            bluetoothAdapter = adapterClass.CallStatic<AndroidJavaObject>("getDefaultAdapter");
        }
        #endif
    }

    // ================= SERVER =================

    public void StartServer()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        new Thread(() =>
        {
            try
            {
                AndroidJavaObject uuid = new AndroidJavaClass("java.util.UUID")
                    .CallStatic<AndroidJavaObject>("fromString", UUID_STRING);

                serverSocket = bluetoothAdapter.Call<AndroidJavaObject>(
                    "listenUsingRfcommWithServiceRecord",
                    APP_NAME,
                    uuid);

                socket = serverSocket.Call<AndroidJavaObject>("accept");

                SetupStreams();
            }
            catch (Exception e)
            {
                Debug.Log("Server error: " + e.Message);
            }
        }).Start();
        #endif
    }

    // ================= SCAN =================

    public List<AndroidJavaObject> ScanDevices()
    {
        List<AndroidJavaObject> devices = new List<AndroidJavaObject>();

        #if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaObject bondedDevices =
            bluetoothAdapter.Call<AndroidJavaObject>("getBondedDevices");

        AndroidJavaObject iterator =
            bondedDevices.Call<AndroidJavaObject>("iterator");

        while (iterator.Call<bool>("hasNext"))
        {
            AndroidJavaObject device =
                iterator.Call<AndroidJavaObject>("next");
            devices.Add(device);
        }
        #endif

        return devices;
    }

    // ================= CLIENT =================

    public void ConnectToDevice(AndroidJavaObject device)
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        new Thread(() =>
        {
            try
            {
                AndroidJavaObject uuid = new AndroidJavaClass("java.util.UUID")
                    .CallStatic<AndroidJavaObject>("fromString", UUID_STRING);

                socket = device.Call<AndroidJavaObject>(
                    "createRfcommSocketToServiceRecord", uuid);

                socket.Call("connect");

                SetupStreams();
            }
            catch (Exception e)
            {
                Debug.Log("Client error: " + e.Message);
            }
        }).Start();
        #endif
    }

    void SetupStreams()
    {
        inputStream = socket.Call<AndroidJavaObject>("getInputStream");
        outputStream = socket.Call<AndroidJavaObject>("getOutputStream");

        isReading = true;
        readThread = new Thread(ReadLoop);
        readThread.Start();
    }

    void ReadLoop()
    {
        byte[] buffer = new byte[1024];

        while (isReading)
        {
            try
            {
                int bytes = inputStream.Call<int>("read", buffer);

                if (bytes > 0)
                {
                    string message = System.Text.Encoding.UTF8.GetString(buffer, 0, bytes);

                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                      FindFirstObjectByType  <CubeManager>().ReceiveCube(message);
                    });
                }
            }
            catch { }
        }
    }

    public void SendMessageBT(string message)
    {
        if (outputStream == null) return;

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message);
        outputStream.Call("write", bytes);
    }
}
