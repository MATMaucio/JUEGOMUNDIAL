using UnityEngine;

/// <summary>
/// TouchInputManager
/// -----------------
/// Se encarga de detectar interacción del usuario
/// mediante:
/// - Toques en pantalla (Android)
/// - Click de mouse (Editor)
///
/// Cuando el usuario toca un objeto con CubeItem,
/// se intenta enviar ese cubo al otro dispositivo
/// mediante CubeManager.
///
/// Depende de:
/// - CubeItem (para identificar el objeto tocado)
/// - CubeManager (para manejar envío del cubo)
/// </summary>
public class TouchInputManager : MonoBehaviour
{
    /// <summary>
    /// Update se ejecuta cada frame.
    /// Detecta entrada táctil o clic del mouse.
    /// </summary>
    void Update()
    {
        // ===============================
        // Entrada táctil (Android)
        // ===============================
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Solo detectamos cuando el toque inicia
            if (touch.phase == TouchPhase.Began)
            {
                CheckTouch(touch.position);
            }
        }

        // ===============================
        // Entrada con mouse (Editor)
        // ===============================
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            CheckTouch(Input.mousePosition);
        }
#endif
    }

    /// <summary>
    /// Realiza un Raycast desde la posición del toque/clic
    /// para detectar si se tocó un cubo.
    /// </summary>
    void CheckTouch(Vector2 position)
    {
        // Crear rayo desde la cámara hacia la posición tocada
        Ray ray = Camera.main.ScreenPointToRay(position);
        RaycastHit hit;

        // Si el rayo impacta un collider
        if (Physics.Raycast(ray, out hit))
        {
            // Intentar obtener el componente CubeItem
            CubeItem cube = hit.collider.GetComponent<CubeItem>();

            if (cube != null)
            {
                // Intentar enviar el cubo al otro dispositivo
                FindAnyObjectByType<CubeManager>()
                    .TrySendCube(cube.cubeID);
            }
        }
    }
}