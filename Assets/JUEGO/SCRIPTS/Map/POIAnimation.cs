using UnityEngine;

public class POIAnimation : MonoBehaviour
{
    [Header("Configuración de Animación")]
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float floatAmplitude = 0.2f;
    [SerializeField] private float floatFrequency = 1f;

    private Vector3 startPosition;

    private void Start()
    {
        // Guardamos la posición inicial para que flote respecto a ella
        startPosition = transform.position;
    }

    private void Update()
    {
        // 1. Rotación constante sobre el eje Y
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // 2. Movimiento de flotación usando una onda Senoidal (Sin)
        float newY = startPosition.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
}