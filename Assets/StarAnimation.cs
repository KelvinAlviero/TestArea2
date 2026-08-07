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
    void Start()
    {
        DOTween.Init();
        starImage1.transform.DOMoveY(topMax, animSpeed)
        .SetLoops(-1, LoopType.Yoyo);
        starImage2.transform.DOMoveY(topMax, animSpeed - 0.2f)
        .SetLoops(-1, LoopType.Yoyo);
        starImage3.transform.DOMoveY(topMax, animSpeed - 0.3f)
        .SetLoops(-1, LoopType.Yoyo);

            
        // boxImage.transform.DORestart();
        // boxImage.transform.DOMoveY(bottomMax, animSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
