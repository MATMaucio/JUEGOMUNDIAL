using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UnityMainThreadDispatcher
/// --------------------------
/// Permite ejecutar acciones en el hilo principal de Unity
/// desde otros hilos (Threads).
///
/// ¿Por qué es necesario?
/// -----------------------
/// Unity NO permite modificar objetos de la escena
/// desde hilos secundarios.
///
/// En tu proyecto:
/// - BluetoothManager recibe datos en un Thread.
/// - Ese Thread NO puede activar/desactivar GameObjects.
/// - Entonces usa este Dispatcher para ejecutar el código
///   en el hilo principal (seguro para Unity).
///
/// Patrón utilizado:
/// - Singleton
/// - Cola de ejecución (Queue<Action>)
/// - Procesamiento en Update()
/// </summary>
public class UnityMainThreadDispatcher : MonoBehaviour
{
    /// <summary>
    /// Instancia única del dispatcher.
    /// </summary>
    private static UnityMainThreadDispatcher _instance;

    /// <summary>
    /// Cola de acciones pendientes de ejecutar.
    /// Se usa Queue porque:
    /// - Mantiene orden FIFO
    /// - Permite procesar tareas en secuencia
    /// </summary>
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();

    /// <summary>
    /// Obtiene la instancia del dispatcher.
    /// 
    /// Si no existe, la crea automáticamente
    /// y la marca como persistente entre escenas.
    /// </summary>
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

    /// <summary>
    /// Agrega una acción a la cola para ejecutarse
    /// en el siguiente frame dentro del hilo principal.
    /// </summary>
    public void Enqueue(Action action)
    {
        lock (_executionQueue) // Protección contra acceso simultáneo
        {
            _executionQueue.Enqueue(action);
        }
    }

    /// <summary>
    /// Se ejecuta cada frame.
    /// Procesa todas las acciones pendientes.
    /// </summary>
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