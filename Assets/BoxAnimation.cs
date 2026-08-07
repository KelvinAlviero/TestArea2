using UnityEngine;
using DG.Tweening;

public class BoxAnimation : MonoBehaviour
{
    private RectTransform boxImage;
    private float topMax;
    private float bottomMax;
    private float animSpeed;
    private float smoothSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DOTween.Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
