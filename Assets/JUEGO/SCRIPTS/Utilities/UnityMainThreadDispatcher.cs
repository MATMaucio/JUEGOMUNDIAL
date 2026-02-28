using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ============================================================
/// UnityMainThreadDispatcher — SIN CAMBIOS
/// ============================================================
/// 
/// Este script NO requiere modificaciones.
/// Funciona correctamente en el proyecto original.
///
/// PROPÓSITO:
/// Unity no permite modificar GameObjects desde hilos secundarios.
/// BluetoothManager usa Threads para leer datos BT.
/// Este dispatcher permite ejecutar código en el hilo principal
/// desde esos Threads.
///
/// PATRÓN: Singleton + Cola de acciones (Queue<Action>) + Update()
/// ============================================================
/// </summary>
public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher _instance;
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();

    public static UnityMainThreadDispatcher Instance()
    {
        if (_instance == null)
        {
            GameObject obj = new GameObject("UnityMainThreadDispatcher");
            _instance = obj.AddComponent<UnityMainThreadDispatcher>();
            DontDestroyOnLoad(obj);
        }
        return _instance;
    }

    public void Enqueue(Action action)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }

    void Update()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }
}
