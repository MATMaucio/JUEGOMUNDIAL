using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class MapRenderer : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Configuración del Mapa")]
    [SerializeField] private int zoomLevel = 17;
    [SerializeField] private int renderDistance = 2; 

    private Dictionary<Vector2Int, GameObject> activeTiles = new Dictionary<Vector2Int, GameObject>();
    private Vector2Int lastPlayerTile = new Vector2Int(-1, -1);
    
    // EL POOL: La piscina de Planos 3D reciclables
    private Queue<GameObject> tilePool = new Queue<GameObject>();

    private const float EarthCircumference = 40075016.686f; 

    private void Update()
    {
        if (playerMovement.IsOriginSet)
        {
            Vector2Int currentTile = GetTileCoords(playerMovement.CurrentLatitude, playerMovement.CurrentLongitude);

            if (currentTile != lastPlayerTile)
            {
                UpdateVisibleTiles(currentTile);
                lastPlayerTile = currentTile;
            }
        }
    }

    private void UpdateVisibleTiles(Vector2Int centerTile)
    {
        List<Vector2Int> tilesToRemove = new List<Vector2Int>(activeTiles.Keys);

        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int y = -renderDistance; y <= renderDistance; y++)
            {
                Vector2Int targetCoords = new Vector2Int(centerTile.x + x, centerTile.y + y);
                
                // Si la necesitamos, la salvamos de la eliminación
                tilesToRemove.Remove(targetCoords);

                if (!activeTiles.ContainsKey(targetCoords))
                {
                    StartCoroutine(DownloadAndPlaceTile(targetCoords));
                }
            }
        }

        // APAGAR Y RECICLAR: Los cuadros que quedaron muy lejos
        foreach (Vector2Int oldTileCoords in tilesToRemove)
        {
            ReturnTileToPool(oldTileCoords);
        }
    }

    // --- LÓGICA DEL OBJECT POOL PARA EL MAPA ---

    private GameObject GetTileFromPool()
    {
        // Si hay planos en la reserva, sacamos uno pero LO DEJAMOS APAGADO
        if (tilePool.Count > 0)
        {
            return tilePool.Dequeue();
        }
        
        // Si no hay, creamos uno nuevo, pero también LO APAGAMOS de inmediato
        GameObject newTile = Instantiate(tilePrefab, transform);
        newTile.SetActive(false);
        return newTile;
    }

    private void ReturnTileToPool(Vector2Int coords)
    {
        if (activeTiles.TryGetValue(coords, out GameObject tileObject))
        {
            tileObject.SetActive(false); // Apagamos el plano
            
            // ¡CRÍTICO PARA LA VRAM! Destruimos la imagen vieja para no saturar la memoria
            Material tileMat = tileObject.GetComponent<MeshRenderer>().material;
            if (tileMat.mainTexture != null)
            {
                Destroy(tileMat.mainTexture);
                tileMat.mainTexture = null;
            }

            // Lo metemos a la reserva y lo quitamos de los activos
            tilePool.Enqueue(tileObject);
            activeTiles.Remove(coords);
        }
    }

    // --- DESCARGA Y POSICIONAMIENTO ---

    private IEnumerator DownloadAndPlaceTile(Vector2Int coords)
    {
        // 1. SACAR DEL POOL (Viene apagado)
        GameObject newTile = GetTileFromPool();
        newTile.name = $"Tile_{coords.x}_{coords.y}";
        activeTiles.Add(coords, newTile);

        // 2. ESCALA Y POSICIÓN
        float latRad = playerMovement.OriginLatitude * Mathf.Deg2Rad;
        float metersPerTile = (EarthCircumference * Mathf.Cos(latRad)) / Mathf.Pow(2, zoomLevel);

        float scaleFactor = metersPerTile / 10f;
        newTile.transform.localScale = new Vector3(scaleFactor, 1f, scaleFactor);

        Vector2Int originCoords = GetTileCoords(playerMovement.OriginLatitude, playerMovement.OriginLongitude);
        float posX = (coords.x - originCoords.x) * metersPerTile;        float posZ = (originCoords.y - coords.y) * metersPerTile;

        newTile.transform.position = new Vector3(posX, 0f, posZ);

        // 3. DESCARGAR NUEVA TEXTURA
        string url = $"https://a.tile.openstreetmap.org/{zoomLevel}/{coords.x}/{coords.y}.png";
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            request.SetRequestHeader("User-Agent", "Unity_GPS_Game_v1"); 
            yield return request.SendWebRequest();

            // 4. VERIFICACIÓN DE SEGURIDAD Y ENCENDIDO
            if (request.result == UnityWebRequest.Result.Success)
            {
                // ¿El jugador sigue cerca o ya se fue corriendo mientras descargábamos?
                if (activeTiles.ContainsKey(coords))
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(request);
                    texture.wrapMode = TextureWrapMode.Clamp;
                    
                    Material tileMaterial = newTile.GetComponent<MeshRenderer>().material;
                    tileMaterial.mainTexture = texture;
                    tileMaterial.mainTextureScale = new Vector2(-1f, -1f);
                    tileMaterial.mainTextureOffset = new Vector2(1f, 1f);
                    
                    // ¡LA MAGIA! Encendemos el cuadro SOLO cuando ya está perfectamente pintado
                    newTile.SetActive(true);
                }
                else
                {
                    // Si el jugador se alejó antes de que terminara la descarga, 
                    // destruimos la imagen para no generar basura en la RAM.
                    Destroy(DownloadHandlerTexture.GetContent(request));
                }
            }
            else
            {
                // Si no hay internet, devolvemos el cuadro al Pool
                ReturnTileToPool(coords);
            }
        }
    }

    public Vector2Int GetTileCoords(float lat, float lon)
    {
        int x = (int)((lon + 180.0f) / 360.0f * (1 << zoomLevel));
        float latRad = lat * Mathf.PI / 180.0f;
        int y = (int)((1.0f - Mathf.Log(Mathf.Tan(latRad) + 1.0f / Mathf.Cos(latRad)) / Mathf.PI) / 2.0f * (1 << zoomLevel));
        return new Vector2Int(x, y);
    }
}