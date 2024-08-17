using Game.Scripts;
using UnityEngine;

public class LoaderCallback : MonoBehaviour
{
    bool isFirstUpdate = true;

    void Update()
    {
        // Notify SceneLoader that first frame render has done
        // So it shows the loading scene and can furher execute
        // Without UI getting stucks or immediately change
        if (isFirstUpdate)
        {
            isFirstUpdate = false;
            SceneLoader.Instance.LoaderCallback();
        }
    }
}
