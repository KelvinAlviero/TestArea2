using UnityEngine;
using DG.Tweening;
using UnityEngine.UIElements;

public class BoxAnimation : MonoBehaviour
{
    [SerializeField] private RectTransform boxImage;
    [SerializeField] private float topMax;
    [SerializeField] private float bottomMax;
    [SerializeField] private float animSpeed;
    [SerializeField] private float smoothSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DOTween.Init();

        float distance = topMax; // interpret this as "move up by this amount"
        var seq = DOTween.Sequence()
            .Append(boxImage.DOAnchorPosY(distance, animSpeed).SetRelative(true).SetEase(Ease.InQuad))
            .Append(boxImage.DOAnchorPosY(-distance, animSpeed).SetRelative(true).SetEase(Ease.OutQuad))
            .SetLoops(-1);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
