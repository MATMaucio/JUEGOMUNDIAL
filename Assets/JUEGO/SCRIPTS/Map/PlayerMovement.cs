using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float CurrentLatitude { get; private set; }
    public float CurrentLongitude { get; private set; }
    // Referencia directa al transform del muñeco para optimizar el rendimiento
    [SerializeField] private Transform avatarTransform;
    [SerializeField] private MapRenderer mapRendererScript;
    
    // Coordenadas de origen para establecer el punto 0,0,0 en Unity
    public float OriginLatitude { get; private set; } = 0f;
    public float OriginLongitude { get; private set; } = 0f;
    public bool IsOriginSet { get; private set; } = false;

    // Radio de la Tierra en metros para calcular la distancia real
    private const float earthRadiusMeters = 6378137.0f;

    public void UpdatePositionFromGPS(float currentLatitude, float currentLongitude)
    {
        CurrentLatitude = currentLatitude;
        CurrentLongitude = currentLongitude;
        // El primer dato GPS recibido se convierte en el centro del mapa
        if (!IsOriginSet)
        {
            OriginLatitude = currentLatitude;
            OriginLongitude = currentLongitude;
            IsOriginSet = true;
            Debug.Log("Origin coordinates set.");
            return; 
        }

        // Calcular la distancia en metros desde el origen hasta la posición actual
        float positionX = CalculateDistanceX(OriginLongitude, currentLongitude);
        float positionZ = CalculateDistanceZ(OriginLatitude, currentLatitude);

        // Actualizar la posición del avatar (manteniendo su altura Y actual)
        Vector3 targetPosition = new Vector3(positionX, avatarTransform.position.y, positionZ);
        
        // Mover el avatar suavemente hacia la nueva posición
        avatarTransform.position = Vector3.Lerp(avatarTransform.position, targetPosition, Time.deltaTime * 5f);
    }

    private float CalculateDistanceX(float lon1, float lon2)
    {
        // Fórmula para calcular la distancia horizontal (Eje X)
        float deltaLon = (lon2 - lon1) * Mathf.Deg2Rad;
        return deltaLon * earthRadiusMeters * Mathf.Cos(OriginLatitude * Mathf.Deg2Rad);
    }

    private float CalculateDistanceZ(float lat1, float lat2)
    {
        // Fórmula para calcular la distancia vertical (Eje Z)
        float deltaLat = (lat2 - lat1) * Mathf.Deg2Rad;
        return deltaLat * earthRadiusMeters;
    }
}