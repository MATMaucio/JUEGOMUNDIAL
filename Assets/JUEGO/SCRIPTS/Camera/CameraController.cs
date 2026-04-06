using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform playerTransform;
    
    [Header("Configuración de Vista")]
    [SerializeField] private float distance = 15f; // Distancia actual al jugador
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Configuración de Zoom ")]
    [SerializeField] private float minDistance = 5f;   // Lo más cerca que puede hacer zoom
    [SerializeField] private float maxDistance = 50f;  // Lo más lejos que puede alejar el mapa
    [SerializeField] private float zoomSpeedTouch = 0.05f; // Sensibilidad para los dedos
    [SerializeField] private float zoomSpeedMouse = 10f;   // Sensibilidad para la compu

    [Header("Configuración de Rotación Táctil")]
    [SerializeField] private float rotationSpeedX = 5f; // Velocidad izquierda/derecha
    [SerializeField] private float rotationSpeedY = 3f; // Velocidad arriba/abajo
    
    // LÍMITES VERTICALES: Evitan que la cámara rompa el suelo o se voltee
    [SerializeField] private float minVerticalAngle = 10f; 
    [SerializeField] private float maxVerticalAngle = 80f; 

    private float currentRotationAngleX = 0f;
    private float currentRotationAngleY = 45f; // Ángulo inicial inclinado
    private Vector2 lastMousePosition;

    private void LateUpdate()
    {
        if (playerTransform == null) return;

        // --- 1. GESTIÓN DEL ZOOM (La Magia Nueva) ---
        
        // A. En el celular: Detectamos si hay 2 dedos en la pantalla (Pinch to Zoom)
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

            float prevMagnitude = (touch0PrevPos - touch1PrevPos).magnitude;
            float currentMagnitude = (touch0.position - touch1.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;
            distance -= difference * zoomSpeedTouch;
        }
        
        // B. En la computadora: Usamos mouseScrollDelta (Más directo y no falla en el Editor)
        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0)
        {
            // El scroll devuelve 1 o -1. Lo multiplicamos por tu velocidad.
            distance -= scroll * zoomSpeedMouse;
        }

        // CANDADO DEL ZOOM: Evitamos que hagan zoom infinito o atraviesen al jugador
        distance = Mathf.Clamp(distance, minDistance, maxDistance);


        // --- 2. GESTIÓN DEL INPUT DE ROTACIÓN (Tu código original modificado) ---
        
        // El truco de optimización: Solo rotamos si hay un dedo (o el mouse) 
        // y NO estamos haciendo zoom (touchCount < 2) para que la cámara no se vuelva loca
        if (Input.GetMouseButtonDown(0) && Input.touchCount < 2)
        {
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0) && Input.touchCount < 2)
        {
            float deltaX = Input.mousePosition.x - lastMousePosition.x;
            float deltaY = Input.mousePosition.y - lastMousePosition.y;

            // Rotación Horizontal (Izquierda/Derecha)
            currentRotationAngleX += deltaX * rotationSpeedX * Time.deltaTime;
            
            // Rotación Vertical (Arriba/Abajo)
            currentRotationAngleY -= deltaY * rotationSpeedY * Time.deltaTime;

            // EL CANDADO VERTICAL
            currentRotationAngleY = Mathf.Clamp(currentRotationAngleY, minVerticalAngle, maxVerticalAngle);

            lastMousePosition = Input.mousePosition;
        }


        // --- 3. CÁLCULO DE POSICIÓN ORBITAL (Tu código original) ---
        
        // Aplicamos ambos ángulos (Y para la inclinación, X para el giro alrededor)
        Quaternion rotation = Quaternion.Euler(currentRotationAngleY, currentRotationAngleX, 0);
        
        // Empujamos la cámara hacia atrás usando nuestra nueva variable dinámica 'distance'
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 targetPosition = rotation * negDistance + playerTransform.position;

        // --- 4. APLICAR MOVIMIENTO Y MIRAR ---
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        transform.LookAt(playerTransform.position + Vector3.up * 1.5f);
    }
}