using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform playerTransform;
    
    [Header("Configuración de Vista")]
    [SerializeField] private float distance = 15f; // Distancia fija al jugador
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Configuración de Rotación Táctil")]
    [SerializeField] private float rotationSpeedX = 5f; // Velocidad izquierda/derecha
    [SerializeField] private float rotationSpeedY = 3f; // Velocidad arriba/abajo
    
    // LÍMITES VERTICALES: Evitan que la cámara rompa el suelo o se voltee
    [SerializeField] private float minVerticalAngle = 10f; // Lo más bajo que puede mirar (casi a ras de suelo)
    [SerializeField] private float maxVerticalAngle = 80f; // Lo más alto que puede mirar (vista de pájaro)

    private float currentRotationAngleX = 0f;
    private float currentRotationAngleY = 45f; // Ángulo inicial inclinado
    private Vector2 lastMousePosition;

    private void LateUpdate()
    {
        if (playerTransform == null) return;

        // 1. GESTIÓN DEL INPUT (Dedo/Mouse)
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            float deltaX = Input.mousePosition.x - lastMousePosition.x;
            float deltaY = Input.mousePosition.y - lastMousePosition.y;

            // Rotación Horizontal (Izquierda/Derecha)
            currentRotationAngleX += deltaX * rotationSpeedX * Time.deltaTime;
            
            // Rotación Vertical (Arriba/Abajo) - Invertimos la resta para que se sienta natural al arrastrar
            currentRotationAngleY -= deltaY * rotationSpeedY * Time.deltaTime;

            // EL CANDADO: Forzamos a que el ángulo Y no pase de los límites
            currentRotationAngleY = Mathf.Clamp(currentRotationAngleY, minVerticalAngle, maxVerticalAngle);

            lastMousePosition = Input.mousePosition;
        }

        // 2. CÁLCULO DE POSICIÓN ORBITAL
        // Aplicamos ambos ángulos (Y para la inclinación, X para el giro alrededor)
        Quaternion rotation = Quaternion.Euler(currentRotationAngleY, currentRotationAngleX, 0);
        
        // Empujamos la cámara hacia atrás según la 'distance'
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 targetPosition = rotation * negDistance + playerTransform.position;

        // 3. APLICAR MOVIMIENTO Y MIRAR
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        transform.LookAt(playerTransform.position + Vector3.up * 1.5f);
    }
}