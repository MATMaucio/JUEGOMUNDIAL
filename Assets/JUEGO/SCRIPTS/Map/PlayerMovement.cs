using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float CurrentLatitude { get; private set; }
    public float CurrentLongitude { get; private set; }
    
    [Header("Referencias")]
    [SerializeField] private Transform avatarTransform;
    [SerializeField] private MapRenderer mapRendererScript;
    
    [Header("Configuración de Movimiento")]
    [SerializeField] private float walkSpeed = 5f; // Velocidad del Lerp
    // Si la nueva posición está a más de 30 metros, saltamos de golpe
    [SerializeField] private float teleportThreshold = 30f; 
    
    // Aquí guardamos la meta hacia donde debe caminar el avatar
    private Vector3 targetPosition;
    private bool hasTarget = false;

    public float OriginLatitude { get; private set; } = 0f;
    public float OriginLongitude { get; private set; } = 0f;
    public bool IsOriginSet { get; private set; } = false;

    private const float earthRadiusMeters = 6378137.0f;

    private void Update()
    {
        // El movimiento suave DEBE ir aquí para que corra en cada frame (60 FPS)
        if (IsOriginSet && hasTarget)
        {
            avatarTransform.position = Vector3.Lerp(avatarTransform.position, targetPosition, Time.deltaTime * walkSpeed);
        }
    }

    public void UpdatePositionFromGPS(float currentLatitude, float currentLongitude)
    {
        CurrentLatitude = currentLatitude;
        CurrentLongitude = currentLongitude;
        
        if (!IsOriginSet)
        {
            OriginLatitude = currentLatitude;
            OriginLongitude = currentLongitude;
            IsOriginSet = true;
            
            // Establecemos su posición inicial
            targetPosition = avatarTransform.position;
            hasTarget = true;
            
            Debug.Log("Origin coordinates set.");
            return; 
        }

        // 1. Calculamos la meta en base a las nuevas coordenadas
        float positionX = CalculateDistanceX(OriginLongitude, currentLongitude);
        float positionZ = CalculateDistanceZ(OriginLatitude, currentLatitude);

        targetPosition = new Vector3(positionX, avatarTransform.position.y, positionZ);
        hasTarget = true;

        // --- 2. EL CANDADO DE TELETRANSPORTE ---
        float distanceToTarget = Vector3.Distance(avatarTransform.position, targetPosition);

        if (distanceToTarget > teleportThreshold)
        {
            // Si la distancia es gigante (abriste la app lejos), cortamos el Lerp y lo teletransportamos
            avatarTransform.position = targetPosition;
            Debug.Log($"Salto largo detectado ({distanceToTarget}m). Teletransportando avatar.");
        }
    }

    private float CalculateDistanceX(float lon1, float lon2)
    {
        float deltaLon = (lon2 - lon1) * Mathf.Deg2Rad;
        return deltaLon * earthRadiusMeters * Mathf.Cos(OriginLatitude * Mathf.Deg2Rad);
    }

    private float CalculateDistanceZ(float lat1, float lat2)
    {
        float deltaLat = (lat2 - lat1) * Mathf.Deg2Rad;
        return deltaLat * earthRadiusMeters;
    }
}