using Game.Scripts;
using TMPro;
using UnityEngine;

public class LoadingProgress : MonoBehaviour
{
    TMP_Text tmpText;

    void Awake()
    {
        tmpText = GetComponent<TMP_Text>();
    }

    void Update()
    {
        tmpText.text = GetLoadingText();
    }

    private string GetLoadingText()
    {
        var loadedPercentage = Mathf.Round(SceneLoader.Instance.GetLoadingProgress() * 100f);
        return $"LOADING... {loadedPercentage}%";
    }
}