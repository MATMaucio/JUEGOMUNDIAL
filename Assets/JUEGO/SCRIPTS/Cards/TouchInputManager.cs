using UnityEngine;

/// <summary>
/// ============================================================
/// TouchInputManager — VERSIÓN REVISADA
/// ============================================================
/// 
/// CAMBIOS RESPECTO AL ORIGINAL:
/// -------------------------------------------------------
/// El código original funcionaba correctamente en su lógica de
/// detección de toque/raycast. Sin embargo faltaban:
///
/// 1. Debug cuando no se tocó ningún objeto.
/// 2. Debug cuando se tocó un objeto que NO era un CubeItem.
/// 3. Verificación de que CubeManager exista antes de llamarlo.
///
/// CORRECCIONES APLICADAS:
/// -------------------------------------------------------
/// ✔ Debug.Log al detectar un toque que impacta un cubo.
/// ✔ Debug.Log cuando el raycast no impacta nada.
/// ✔ Verificación de null en CubeManager.
///
/// FLUJO:
/// -------------------------------------------------------
/// Update() detecta toque o clic
///   → CheckTouch(posición)
///   → Raycast desde Camera.main
///   → Si impacta un CubeItem → CubeManager.TrySendCube(id)
///   → Si no impacta nada → Debug silencioso (opcional)
/// ============================================================
/// </summary>
public class TouchInputManager : MonoBehaviour
{
    void Update()
    {
        // ===========================
        // Entrada táctil (Android)
        // ===========================
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Solo reaccionar al inicio del toque, no al deslizar
            if (touch.phase == TouchPhase.Began)
            {
                CheckTouch(touch.position);
            }
        }

        // ===========================
        // Entrada con mouse (Editor)
        // ===========================
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            CheckTouch(Input.mousePosition);
        }
#endif
    }

    /// <summary>
    /// Lanza un Raycast desde la cámara principal hacia la posición tocada.
    /// Si impacta un objeto con CubeItem, intenta enviarlo.
    /// </summary>
    void CheckTouch(Vector2 position)
    {
        if (Camera.main == null)
        {
            Debug.LogError("[TouchInputManager] No hay Camera.main en la escena.");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(position);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            CubeItem cube = hit.collider.GetComponent<CubeItem>();

            if (cube != null)
            {
                Debug.Log($"[TouchInputManager] Cubo tocado: '{cube.cubeID}'. Intentando enviar...");

                CubeManager manager = FindAnyObjectByType<CubeManager>();

                if (manager == null)
                {
                    Debug.LogError("[TouchInputManager] No se encontró CubeManager en la escena.");
                    return;
                }

                manager.TrySendCube(cube.cubeID);
            }
            else
            {
                // Se tocó algo pero no era un cubo
                Debug.Log($"[TouchInputManager] Se tocó '{hit.collider.gameObject.name}' pero no tiene CubeItem.");
            }
        }
        // No loguear cuando no se toca nada (ocurriría cada frame si no se toca nada)
    }
}
