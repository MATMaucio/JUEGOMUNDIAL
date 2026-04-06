using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking; // ¡NUEVO! Necesario para leer archivos en Android

public class POIManager : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovementScript;
    [SerializeField] private GameObject poiPrefab;
    [SerializeField] private float spawnRadius = 400f; 

    private const double earthRadiusMeters = 6378137.0;
    
    private List<POIData> allPOIs = new List<POIData>();
    private Dictionary<POIData, GameObject> activePOIs = new Dictionary<POIData, GameObject>();
    private Queue<GameObject> poiPool = new Queue<GameObject>();

    private bool dataLoaded = false;
    private bool isTryingToLoad = false; // Candado para no abrir el archivo mil veces

    private void Update()
    {
        // Solo intentamos cargar si ya tenemos GPS y no lo hemos intentado antes
        if (playerMovementScript.IsOriginSet)
        {
            if (!dataLoaded && !isTryingToLoad)
            {
                isTryingToLoad = true;
                StartCoroutine(LoadPOIDataAndroidSafe());
            }
            
            // Solo empezamos a acomodar postes si el JSON ya terminó de cargar
            if (dataLoaded)
            {
                ManagePOIs();
            }
        }
    }

    // --- LA NUEVA FUNCIÓN A PRUEBA DE ANDROID ---
    private IEnumerator LoadPOIDataAndroidSafe()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "MexiParadas.json");
        string jsonContent = "";

        // Si la ruta contiene "://", significa que estamos adentro de un APK de Android
        if (filePath.Contains("://") || filePath.Contains(":///"))
        {
            using (UnityWebRequest www = UnityWebRequest.Get(filePath))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    jsonContent = www.downloadHandler.text;
                }
                else
                {
                    Debug.LogError("Error al leer JSON en Android: " + www.error);
                }
            }
        }
        else 
        {
            // Si estamos en la computadora, lo leemos de la forma clásica
            if (File.Exists(filePath))
            {
                jsonContent = File.ReadAllText(filePath);
            }
        }

        // Si logramos sacar el texto, lo convertimos a datos
        if (!string.IsNullOrEmpty(jsonContent))
        {
            POIList lista = JsonUtility.FromJson<POIList>(jsonContent);
            allPOIs = lista.paradas;
            dataLoaded = true; // ¡Damos luz verde para que el Update empiece a colocarlos!
            Debug.Log($"¡JSON cargado con éxito en el celular! {allPOIs.Count} paradas listas.");
        }
    }

    private void ManagePOIs()
    {
        List<POIData> poisToRemove = new List<POIData>(activePOIs.Keys);

        foreach (POIData poi in allPOIs)
        {
            float posX = CalculateDistanceX(playerMovementScript.OriginLongitude, poi.lon);
            float posZ = CalculateDistanceZ(playerMovementScript.OriginLatitude, poi.lat);
            Vector3 poiPosition = new Vector3(posX, 0.5f, posZ);

            float distanceToPlayer = Vector3.Distance(playerMovementScript.transform.position, poiPosition);

            if (distanceToPlayer <= spawnRadius)
            {
                if (!activePOIs.ContainsKey(poi))
                {
                    GameObject poiObject = GetPOIFromPool(poiPosition, poi);
                    activePOIs.Add(poi, poiObject);
                }
                poisToRemove.Remove(poi);
            }
        }

        foreach (POIData oldPoi in poisToRemove)
        {
            ReturnPOIToPool(activePOIs[oldPoi]);
            activePOIs.Remove(oldPoi);
        }
    }

    private GameObject GetPOIFromPool(Vector3 position, POIData data)
    {
        GameObject obj;
        if (poiPool.Count > 0)
        {
            obj = poiPool.Dequeue();
            obj.transform.position = position;
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(poiPrefab, position, Quaternion.identity, transform);
        }

        if (obj.TryGetComponent<InteractablePOI>(out InteractablePOI script))
        {
            script.poiName = data.nombre;
            script.poiDescription = data.descripcion; 
        }
        return obj;
    }

    private void ReturnPOIToPool(GameObject obj)
    {
        obj.SetActive(false);
        poiPool.Enqueue(obj);
    }

    private float CalculateDistanceX(double originLon, double targetLon)
    {
        double deltaLon = (targetLon - originLon) * (System.Math.PI / 180.0);
        double distance = deltaLon * earthRadiusMeters * System.Math.Cos(playerMovementScript.OriginLatitude * (System.Math.PI / 180.0));
        return (float)distance;
    }

    private float CalculateDistanceZ(double originLat, double targetLat)
    {
        double deltaLat = (targetLat - originLat) * (System.Math.PI / 180.0);
        double distance = deltaLat * earthRadiusMeters;
        return (float)distance;
    }
}