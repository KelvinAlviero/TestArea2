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
        boxImage.transform.DOMoveY(topMax, animSpeed)
            .SetLoops(-1, LoopType.Yoyo);
        // boxImage.transform.DORestart();
        // boxImage.transform.DOMoveY(bottomMax, animSpeed);
        DOTween.Play(boxImage);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
