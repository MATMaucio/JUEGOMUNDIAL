using UnityEngine;
using System.Collections;

public class GPSController : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovementScript;
    [SerializeField] private float updateDistanceInMeters = 1f;
    [SerializeField] private float updateTimeInSeconds = 1f;

#if UNITY_EDITOR
    [Header("Simulador de GPS (Solo Editor)")]
    [Tooltip("Modifica estos valores en Modo Play para simular que caminas")]
    [SerializeField] private float editorLatitude = 19.4352f; // Palacio de Bellas Artes, CDMX
    [SerializeField] private float editorLongitude = -99.1412f;
#endif

    private void Start()
    {
        StartCoroutine(InitializeGPS());
    }

    private IEnumerator InitializeGPS()
    {
        // --- NUEVO: PEDIR PERMISO EXPLÍCITO EN ANDROID ---
#if UNITY_ANDROID
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.FineLocation);
            
            // Esperamos un par de segundos para que el usuario presione "Aceptar"
            yield return new WaitForSeconds(2f); 
        }
#endif

#if UNITY_EDITOR
        // --- CÓDIGO SOLO PARA LA COMPUTADORA ---
        Debug.Log($"Iniciando GPS simulado en el Editor... (Distancia configurada: {updateDistanceInMeters}m)");        yield return new WaitForSeconds(1f); // Simular un pequeño tiempo de carga
        InvokeRepeating(nameof(UpdatePlayerLocation), 0f, updateTimeInSeconds);
        
#else
        // --- CÓDIGO REAL PARA EL CELULAR ---
        Input.location.Start(updateDistanceInMeters, updateTimeInSeconds);

        int maxWaitTimer = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWaitTimer > 0)
        {
            yield return new WaitForSeconds(1);
            maxWaitTimer--;
        }

        if (maxWaitTimer < 1 || Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("Unable to determine device location.");
            yield break;
        }

        Debug.Log("GPS successfully initialized.");
        InvokeRepeating(nameof(UpdatePlayerLocation), 0f, updateTimeInSeconds);
#endif
    }

    private void UpdatePlayerLocation()
    {
#if UNITY_EDITOR
        // En el editor, enviamos las coordenadas falsas del Inspector
        if (playerMovementScript != null)
        {
            playerMovementScript.UpdatePositionFromGPS(editorLatitude, editorLongitude);
        }
#else
        // En el celular, leemos la antena real
        if (Input.location.status == LocationServiceStatus.Running)
        {
            float currentLatitude = Input.location.lastData.latitude;
            float currentLongitude = Input.location.lastData.longitude;

            if (playerMovementScript != null)
            {
                playerMovementScript.UpdatePositionFromGPS(currentLatitude, currentLongitude);
            }
        }
#endif
    }

    private void OnDisable()
    {
        Input.location.Stop();
        CancelInvoke(nameof(UpdatePlayerLocation));
    }   
}