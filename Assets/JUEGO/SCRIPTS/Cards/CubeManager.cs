using UnityEngine;
using System.Collections.Generic;

public class CubeManager : MonoBehaviour
{
    public List<CubeItem> allCubes;

    private HashSet<string> activeCubes = new HashSet<string>();

    void Start()
    {
        foreach (var cube in allCubes)
        {
            cube.gameObject.SetActive(false);
        }
    }



    public void ActivateCube(string id)
    {
        foreach (var cube in allCubes)
        {
            if (cube.cubeID == id)
            {
                cube.gameObject.SetActive(true);
                activeCubes.Add(id);
            }
        }
    }

    public void TrySendCube(string id)
    {
        if (!activeCubes.Contains(id))
            return;

        // Apagar localmente
        foreach (var cube in allCubes)
        {
            if (cube.cubeID == id)
            {
                cube.gameObject.SetActive(false);
                activeCubes.Remove(id);
                break;
            }
        }

        BluetoothManager.Instance.SendMessageBT(id);
    }

    public void ReceiveCube(string id)
    {
        ActivateCube(id);
    }
    public void ActivateCubeButton(string id)
{
    ActivateCube(id);
}

}
