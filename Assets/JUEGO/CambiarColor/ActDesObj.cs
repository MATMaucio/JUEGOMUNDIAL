using UnityEngine;

public class ActDesObj : MonoBehaviour
{
    public GameObject obj;

    public void Activar()
    {
        obj.SetActive(true);
    }

    public void Desactivar()
    {
        obj.SetActive(false);
    }

}
