using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Text;
using TMPro;
using Extras;
using Unity.VisualScripting;

public class Message
{
    public string message;
    
}
    public class UIWheelSpin : UIPage
    {
        
        [Header("Script References")]
        [SerializeField] private APIController APIController;
        [SerializeField] private SpinningScript SpinningScript;
        [SerializeField] private RectTransform wheelBackground;
        [SerializeField] private RectTransform wheel;
        [SerializeField] private RectTransform wheelPointer; 
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image LightsOFF;
        [SerializeField] private Image LightsON;
        // [SerializeField] private Image LightsON_Single;
        [SerializeField] private RectTransform panelRectTransform;
        
        [SerializeField] private DateTime timerStartTime = DateTime.Now;
        [SerializeField] int timerDurationInMinutes;
        [SerializeField] string saveID = "uniqueTimerSaveID";   
        [SerializeField] public TMP_Text TimeText;
        [SerializeField] public TMP_Text SpinAmount;
        private string PulledJWT;
        [SerializeField] public TMP_Text PulledJWTText;
        private StringBuilder sb;
        private SimpleLongSave save;
        [SerializeField] private RectTransform contentRectTransform;
        [SerializeField] public bool LightCheck;
        [SerializeField] public bool isSpinning = false;
        [SerializeField] public bool ispagedisplayed = false;
        [SerializeField] public bool TimerDebug = false;
        
        public RectTransform ContentRectTransform => contentRectTransform;

        [SerializeField] private Button closeButton;
        [SerializeField] private Button spinningButton; // X = 6, Y = -45, SCALE = 2
        [SerializeField] private Button IncreaseButton; // X = 6, Y = -45, SCALE = 2
        [SerializeField] private Button DecreaseButton;
        [SerializeField] public bool SpinAgain;
        
        private void Awake()
        {
            
            CacheComponents();
            Init();
            TimerDebug = false;
            ispagedisplayed = false;
            EnableCanvas();
            string timerData = PlayerPrefs.GetString($"TimerProduct_{saveID}", DateTime.Now.ToBinary().ToString());
            timerStartTime = DateTime.FromBinary(long.Parse(timerData));
            sb = new StringBuilder();
            UIController.ShowPage<UIWheelSpin>();
            DecreaseButton.interactable = false;
            PulledJWT = UniWebViewBridge.Call("getUserToken",null);
            PulledJWTText.text = "PulledJWT=" + PulledJWT;
            Debug.Log(PulledJWT);
        }

        
        private void Update()
        {
            // if (TimerDebug == true)
            // {
            //     ResetTimerDebug();
            //     TimerDebug = false;
            // }

            // TimeSpan timer = DateTime.Now - timerStartTime;
            // TimeSpan duration = TimeSpan.FromMinutes(timerDurationInMinutes);
            // if (timer > duration && isSpinning == false)
            // {  
            //     spinningButton.interactable = true;
            //     TimeText.gameObject.SetActive(false);
            
            // }
            // else
            // {

            //     spinningButton.interactable = false;
            //     TimeText.text = "You can spin the wheel again in: " + FormatTimer(duration - timer);
            // }
        }

        public void Init()
        {
            // Debug.Log("Init everything");
            LightsON.gameObject.SetActive(false);
            closeButton.onClick.AddListener(OnCloseButtonClicked);
            spinningButton.onClick.AddListener(OnSpinButtonClicked);
            IncreaseButton.onClick.AddListener(OnIncreaseButtonClicked); 
            DecreaseButton.onClick.AddListener(OnDecreaseButtonClicked); 

            panelRectTransform.gameObject.SetActive(true);
            wheelBackground.gameObject.SetActive(true);
            wheel.gameObject.SetActive(true);
            wheelPointer.gameObject.SetActive(true);
            closeButton.gameObject.SetActive(true);
            spinningButton.gameObject.SetActive(true);
            
            backgroundImage.gameObject.SetActive(true);
        }

        private void OnDestroy()
        {
            closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            spinningButton.onClick.RemoveListener(OnSpinButtonClicked);
            IncreaseButton.onClick.RemoveListener(OnIncreaseButtonClicked); 
            DecreaseButton.onClick.RemoveListener(OnDecreaseButtonClicked); 
            
        }
        public void InitialPosition()
        {
            wheel.gameObject.SetActive(false);
        }   


        // --- Animation --- //
        public override void PlayShowAnimation()
        {
            // Set initial positions for animation
            panelRectTransform.anchoredPosition = Vector2.down * 2000;
            wheelBackground.anchoredPosition = Vector2.down * 2000;
            wheel.anchoredPosition = Vector2.down * 2000;
            wheelPointer.anchoredPosition = Vector2.down * 2000;
            spinningButton.transform.localPosition = Vector2.down * 2000;
            spinningButton.transform.localScale = new Vector3(2, 2, 2);
            IncreaseButton.transform.localScale = new Vector3(1, 1, 1);
            IncreaseButton.transform.localPosition = Vector2.down * 2000;
            DecreaseButton.transform.localScale = new Vector3(1, 1, 1);
            DecreaseButton.transform.localPosition = Vector2.down * 2000;
            TimeText.gameObject.SetActive(false);

            //Position of animation
            panelRectTransform.gameObject.SetActive(true);
            wheel.gameObject.SetActive(true);
            spinningButton.gameObject.SetActive(true);
            LightsOFF.gameObject.SetActive(true);
            ;

            backgroundImage.gameObject.SetActive(true);
        }

        public override void PlayHideAnimation()
        {
            panelRectTransform.gameObject.SetActive(false);
            wheelBackground.gameObject.SetActive(false);
            wheel.gameObject.SetActive(false);
            wheelPointer.gameObject.SetActive(false);
            LightsOFF.gameObject.SetActive(false);
            LightsON.gameObject.SetActive(false);
            spinningButton.gameObject.SetActive(false);
            IncreaseButton.gameObject.SetActive(false);
            DecreaseButton.gameObject.SetActive(false);

            backgroundImage.gameObject.SetActive(false);
        }
        
        
        //-------- Buttons --------//
        public void OnCloseButtonClicked()
        {
            Debug.Log("Close button clicked");
            Debug.Log("UIWheelSpin closed");
            UniWebViewBridge.Send("backHomeAction",null);//send, call, request. 
            // UniWebViewBridge.Send("SpinItem",new SpinItem{itemId = "69fdaf4e0d3ceac0fa4715a7"});
            var UserData = UniWebViewBridge.Call("UserData",null);
            // var CurrencyData = UniWebViewBridge.Call("UserData", new Currency(CurrencyType = "data"));
            // var CurrencyData = UniWebViewBridge.Request("UserData", new Currency(CurrencyType = "data"));

        }
        
        public void OnSpinButtonClicked()
        {
            if (isSpinning)
                return;

            isSpinning = true;
            closeButton.interactable = false;
            spinningButton.interactable = false;

            if (APIController != null && SpinningScript != null && SpinningScript.HasPendingRewards)
            {
                SpinningScript.StartNextQueuedSpin();
                SpinAgain = SpinningScript.HasPendingRewards;
                spinningButton.interactable = false;
            }
            else
            {
                APIController.StartCoroutine(APIController.StartSpin());
                SpinAgain = false;
            }

            // Debug.Log("UIWheelSpin: Spin button clicked");
            // Debug.Log("UIWheelSpin IsSpinning = " + isSpinning);
            Debug.Log("UIWheelSpin Spin amount = " + APIController.spin_count);
        
        }
    
        public void OnIncreaseButtonClicked()
        {
            APIController.spin_count= APIController.spin_count + 1;
            Debug.Log("AmountIncreased");
            SpinAmount.text = "Spin " + APIController.spin_count;

            Debug.Log("Spin " + APIController.spin_count);
            DecreaseButton.interactable = true;

            if (APIController.spin_count >= 10)
            {
                IncreaseButton.interactable = false;
            }
            
            
        }
        public void OnDecreaseButtonClicked()
        {
            APIController.spin_count= APIController.spin_count - 1;
            Debug.Log("Amount Decreased");
            SpinAmount.text = "Spin " + APIController.spin_count;
            Debug.Log("Spin " + APIController.spin_count);
            IncreaseButton.interactable = true;

            if (APIController.spin_count <= 1)
            {
                DecreaseButton.interactable = false;
            }

        }
        public void EnableCloseButton()
        {            
            Debug.Log("Close button Enabled");
            
            closeButton.interactable = true;
            StopLightAnimation();
            LightCheck = false;
        }

        public void EnableSpinButton()
        {

            Debug.Log("Spin button Enabled");
            spinningButton.interactable = true;
        }

        public void DisableSpinButton()
        {

            Debug.Log("Spin disabled Enabled");
            spinningButton.interactable = false;
        }
        

        // --- Light animation --- //
        public void StopLightAnimation()
        {
            // Debug.Log("Stop Light Animation");
            StopCoroutine("LightAnimation");
            LightCheck = false;
            LightsON.gameObject.SetActive(false);
        }

        public IEnumerator LightAnimation() 
        {
            // Debug.Log("Light Animation Started");

            while (LightCheck == true)
            {
                
                ;
                // Debug.Log("Lights ON");
                yield return new WaitForSeconds(0.5f);
                LightsON.gameObject.SetActive(false);
                // Debug.Log("Lights OFF");
                yield return new WaitForSeconds(0.5f);
            }
            
        }

        // ------ Debug code -----//
        #if UNITY_EDITOR
        [ContextMenu("Reset Timer (Debug)")]
        public void ResetTimerDebug()
        {
            timerStartTime = DateTime.Now.AddMinutes(-timerDurationInMinutes);
            PlayerPrefs.SetString($"TimerProduct_{saveID}", timerStartTime.ToBinary().ToString());
            PlayerPrefs.Save();
            Debug.Log("Timer reset for debugging purposes.");
        }
        #endif
                
        //-------Timer Code -------//
    //     public void StartTimer(bool skipCooldown = false)
    //     {
    //         if (skipCooldown)
    //             timerStartTime = DateTime.Now.AddMinutes(-timerDurationInMinutes);
    //         else
    //             timerStartTime = DateTime.Now;
            
    //         PlayerPrefs.SetString($"TimerProduct_{saveID}", timerStartTime.ToBinary().ToString());
    //         PlayerPrefs.Save();
    //     }

    //     public void ShowTimeText()
    //     {
    //         TimeText.gameObject.SetActive(true);
    //         Debug.Log("TimeText SpinCooldown shown");
    //     }

    //     private string FormatTimer(TimeSpan timeSpan)
    //             {
    //                 sb.Clear();

    //                 if(timeSpan.Hours > 0)
    //                 {
    //                     sb.Append(timeSpan.Hours);
    //                     sb.Append(':');
    //                 }

    //                 sb.Append(timeSpan.Minutes.ToString("00"));
    //                 sb.Append(':');

    //                 sb.Append(timeSpan.Seconds.ToString("00"));

    //                 return sb.ToString();
    //             }
    //     public bool IsAvailable()
    //             {
    //                 TimeSpan timer = DateTime.Now - timerStartTime;
    //                 TimeSpan duration = TimeSpan.FromMinutes(timerDurationInMinutes);

    //                 return timer > duration;
    //             }
}