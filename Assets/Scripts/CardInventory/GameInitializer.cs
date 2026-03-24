using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    void Awake()
    {
        CardDatabase.Load();
        PlayerCardManager.Load();
    }
}
