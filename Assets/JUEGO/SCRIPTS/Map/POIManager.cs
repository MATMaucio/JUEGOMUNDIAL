using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class POIManager : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovementScript;
    [SerializeField] private GameObject poiPrefab;
    [SerializeField] private float spawnRadius = 400f; 

    private const float earthRadiusMeters = 6378137.0f;
    
    // La base de datos en texto
    private List<POIData> allPOIs = new List<POIData>();
    
    // Paradas encendidas actualmente (Datos -> GameObject)
    private Dictionary<POIData, GameObject> activePOIs = new Dictionary<POIData, GameObject>();
    
    // EL POOL: La "piscina" de objetos apagados listos para reciclarse
    private Queue<GameObject> poiPool = new Queue<GameObject>();

    private bool dataLoaded = false;

    private void Update()
    {
        if (playerMovementScript.IsOriginSet)
        {
            if (!dataLoaded)
            {
                LoadPOIData();
                dataLoaded = true;
            }
            ManagePOIs();
        }
    }

    private void LoadPOIData()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "MexiParadas.json");
        if (File.Exists(filePath))
        {
            string jsonContent = File.ReadAllText(filePath);
            POIList lista = JsonUtility.FromJson<POIList>(jsonContent);
            allPOIs = lista.paradas;
            Debug.Log($"Base de datos cargada: {allPOIs.Count} paradas. Object Pool activado.");
        }
    }

    private void ManagePOIs()
    {
        // Usamos esta lista para saber cuáles ya quedaron lejos y hay que apagar
        List<POIData> poisToRemove = new List<POIData>(activePOIs.Keys);

        foreach (POIData poi in allPOIs)
        {
            float posX = CalculateDistanceX(playerMovementScript.OriginLongitude, poi.lon);
            float posZ = CalculateDistanceZ(playerMovementScript.OriginLatitude, poi.lat);
            Vector3 poiPosition = new Vector3(posX, 0.5f, posZ);

            float distanceToPlayer = Vector3.Distance(playerMovementScript.transform.position, poiPosition);

            // Si está dentro del radio...
            if (distanceToPlayer <= spawnRadius)
            {
                // ...y no está prendida, la sacamos del Pool
                if (!activePOIs.ContainsKey(poi))
                {
                    GameObject poiObject = GetPOIFromPool(poiPosition, poi);
                    activePOIs.Add(poi, poiObject);
                }
                
                // Como está cerca, la quitamos de la lista de "basura"
                poisToRemove.Remove(poi);
            }
        }

        // APAGAR Y RECICLAR: Los objetos que quedaron lejos regresan al Pool
        foreach (POIData oldPoi in poisToRemove)
        {
            ReturnPOIToPool(activePOIs[oldPoi]);
            activePOIs.Remove(oldPoi);
        }
    }

    // --- LÓGICA DEL OBJECT POOL ---

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

        // --- LA CONEXIÓN DE DATOS ---
        if (obj.TryGetComponent<InteractablePOI>(out InteractablePOI script))
        {
            script.poiName = data.nombre;
            script.poiDescription = data.descripcion; // ¡Ahora también guardamos la descripción!
        }
        
        return obj;
    }

    private void ReturnPOIToPool(GameObject obj)
    {
        // Lo apagamos y lo metemos a la reserva para el futuro
        obj.SetActive(false);
        poiPool.Enqueue(obj);
    }

    // --- MATEMÁTICA DE POSICIONAMIENTO ---

    private float CalculateDistanceX(float originLon, float targetLon)
    {
        float deltaLon = (targetLon - originLon) * Mathf.Deg2Rad;
        return deltaLon * earthRadiusMeters * Mathf.Cos(playerMovementScript.OriginLatitude * Mathf.Deg2Rad);
    }

    private float CalculateDistanceZ(float originLat, float targetLat)
    {
        float deltaLat = (targetLat - originLat) * Mathf.Deg2Rad;
        return deltaLat * earthRadiusMeters;
    }
}