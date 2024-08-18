using DG.Tweening;
using UnityEngine;

public class Platform : MonoBehaviour
{
    void Start()
    {
        transform.DOMove(transform.position + Vector3.left * 5f, 3f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
    }
}
