
using Game.Scripts.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayAudio : MonoBehaviour
{

    [Button]
    public void PlaySound(string audioName)
    {
        SoundManager.Instance.Play(audioName);
    }
}
