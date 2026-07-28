// using FirerockID.Events;
// using FirerockID.Utils;
// using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


    public class LoadingController : Singleton<LoadingController>
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI loadingText;

        private const float rotationSpeed = 100f;
        private GraphicRaycaster graphicRaycaster;
        private bool isLoading = false;
        private float currentTime = 0f;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);

            graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();

            canvas.enabled = false;
            graphicRaycaster.enabled = false;
        }

        private void OnEnable()
        {
            LoadingEvent.OnStartLoadingEvent += OnStartLoadingHandler;
            LoadingEvent.OnDoneLoadingEvent += OnDoneLoadingHandler;
            UniWebViewBridge.OnStartLoading += OnStartLoadingHandler;
            UniWebViewBridge.OnDoneLoading += OnDoneLoadingHandler;
        }

        private void OnDisable()
        {
            LoadingEvent.OnStartLoadingEvent -= OnStartLoadingHandler;
            LoadingEvent.OnDoneLoadingEvent -= OnDoneLoadingHandler;
            UniWebViewBridge.OnStartLoading -= OnStartLoadingHandler;;
            UniWebViewBridge.OnDoneLoading -= OnDoneLoadingHandler;
        }

        private void Update()
        {
            if (isLoading)
            {
                icon.transform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);

                currentTime += Time.deltaTime;
                loadingText.text = "loading";
                if (currentTime % 1f > 0.66f)
                {
                    loadingText.text += "...";
                }
                else if (currentTime % 1f > 0.33f)
                {
                    loadingText.text += "..";
                }
                else
                {
                    loadingText.text += ".";
                }
            }
        }

        private void OnStartLoadingHandler()
        {
            canvas.enabled = true;
            graphicRaycaster.enabled = true;
            isLoading = true;
        }

        private void OnDoneLoadingHandler()
        {
            canvas.enabled = false;
            graphicRaycaster.enabled = false;
            isLoading = false;
        }
    }
