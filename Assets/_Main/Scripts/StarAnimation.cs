using UnityEngine;
using DG.Tweening;
using UnityEngine.UIElements;

public class StarAnimation : MonoBehaviour
{
    [SerializeField] private RectTransform starImage1;
    [SerializeField] private RectTransform starImage2;
    [SerializeField] private RectTransform starImage3;
    [SerializeField] private int topMax;
    [SerializeField] private float animSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        star1();
        star2();
        star3();
    }

    private void star1()
    {
        float distance = topMax; // interpret this as "move up by this amount"
        var seq = DOTween.Sequence()
            .Append(starImage1.DOAnchorPosY(distance, animSpeed).SetRelative(true).SetEase(Ease.InQuad))
            .Append(starImage1.DOAnchorPosY(-distance, animSpeed).SetRelative(true).SetEase(Ease.OutQuad))
            .SetLoops(-1);
    }
    private void star2()
    {
        float distance = topMax; // interpret this as "move up by this amount"
        var seq = DOTween.Sequence()
            .Append(starImage2.DOAnchorPosY(distance, animSpeed - 0.2f).SetRelative(true).SetEase(Ease.InQuad))
            .Append(starImage2.DOAnchorPosY(-distance, animSpeed - 0.2f).SetRelative(true).SetEase(Ease.OutQuad))
            .SetLoops(-1);
    }
    private void star3()
    {
        float distance = topMax; // interpret this as "move up by this amount"
        var seq = DOTween.Sequence()
            .Append(starImage3.DOAnchorPosY(distance, animSpeed - 0.3f).SetRelative(true).SetEase(Ease.InQuad))
            .Append(starImage3.DOAnchorPosY(-distance, animSpeed - 0.3f).SetRelative(true).SetEase(Ease.OutQuad))
            .SetLoops(-1);
    }



}
