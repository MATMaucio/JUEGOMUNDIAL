    using UnityEngine;

public class TouchInputManager : MonoBehaviour
{
void Update()
{
    if (Input.touchCount > 0)
    {
        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            CheckTouch(touch.position);
        }
    }

#if UNITY_EDITOR
    if (Input.GetMouseButtonDown(0))
    {
        CheckTouch(Input.mousePosition);
    }
#endif
}

void CheckTouch(Vector2 position)
{
    Ray ray = Camera.main.ScreenPointToRay(position);
    RaycastHit hit;

    if (Physics.Raycast(ray, out hit))
    {
        CubeItem cube = hit.collider.GetComponent<CubeItem>();

        if (cube != null)
        {
            FindAnyObjectByType<CubeManager>().TrySendCube(cube.cubeID);
        }
    }
}

}
