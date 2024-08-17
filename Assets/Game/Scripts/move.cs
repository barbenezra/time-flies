using DG.Tweening;
using UnityEngine;

public class move : MonoBehaviour
{
    void Start()
    {
        var target = new Vector3(transform.position.x + 10f, transform.position.y,
            transform.position.z);

        transform.DOMove(target, 3f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
    }
}
