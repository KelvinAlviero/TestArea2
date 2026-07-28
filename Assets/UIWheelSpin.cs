using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Text;
using TMPro;
using Extras;
using Unity.VisualScripting;
using System.Threading.Tasks;

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
        [SerializeField] private Image backgroundImage;
        // [SerializeField] private Image LightsON_Single;
        [SerializeField] private RectTransform panelRectTransform;
        
        [SerializeField] private DateTime timerStartTime = DateTime.Now;
        [SerializeField] int timerDurationInMinutes;
        [SerializeField] string saveID = "uniqueTimerSaveID";   
        [SerializeField] public TMP_Text TimeText;
        [SerializeField] public TMP_Text FlagAmount;
        private StringBuilder sb;
        private SimpleLongSave save;
        [SerializeField] private RectTransform contentRectTransform;
        [SerializeField] public bool isSpinning = false;
        [SerializeField] public bool ispagedisplayed = false;
        [SerializeField] public bool TimerDebug = false;
        
        public RectTransform ContentRectTransform => contentRectTransform;

        [SerializeField] private Button closeButton;
        [SerializeField] private Button spinningButton; // X = 6, Y = -45, SCALE = 2
        [SerializeField] private Button AddFlagButton; //UniWebViewBridge.Send("openMissionPage",null);
        [SerializeField] private Button MissionButton; //UniWebViewBridge.Send("openMissionPage",null);
        [SerializeField] public bool SpinAgain;
        

        public class FlagTicketBalanceResponse
        {
            public int balance;
        }

        public class IncreaseFlagTicketRequest
        {
            public int amount;
        }

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
            GetFlagTicket();

        }

        
        public void GetFlagTicket()
        {
            var data = UniWebViewBridge.Call("getFlagTicketBalance",null);
            var data2 = JsonUtility.FromJson<FlagTicketBalanceResponse>(data);
            if (data2 == null)
            {
                Debug.LogWarning($"[UIWheelSpin] Unable to parse flag ticket balance response: {data}");
                FlagAmount.text = "0";
                return;
            }

            OnFlagTicketChange(data2.balance);


        }

        public void OnFlagTicketChange(int value )
        {
            FlagAmount.text = "<sprite name=flag>" + value.ToString();
        }

    //     public async Task RequestIncreaseFlagBalance()
    //     {
        

    //     try
    //     {
    //         // Same pattern as your leaderboard call
    //         IncreaseFlagTicketRequest response = UniWebViewBridge.Request("spinRequest",request);
    //         Debug.Log($"[SpinWheel] OK name={response.name}, cost={response.total_cost}, currency={response.currency}");
    //     }
    //     catch (Exception e)
    //     {
    //         Debug.LogError($"[SpinWheel] Failed: {e.Message}");
    //     }
    // }

        public void IncreaseFlagTicket()
        {
            UniWebViewBridge.Request("increaseFlagTicket",new IncreaseFlagTicketRequest { amount = 10 },
            onSuccess: json =>
            {
                 var data = JsonUtility.FromJson<FlagTicketBalanceResponse>(json);
                OnFlagTicketChange(data.balance);
            },
            onError: err => Debug.Log("increase error: " + err),
            timeout: 10000);
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
            closeButton.onClick.AddListener(OnCloseButtonClicked);
            spinningButton.onClick.AddListener(OnSpinButtonClicked);
            AddFlagButton.onClick.AddListener(OnAddFlagButtonClicked);
            MissionButton.onClick.AddListener(OnMissionButtonClicked);

            panelRectTransform.gameObject.SetActive(true);
            wheelBackground.gameObject.SetActive(true);
            wheel.gameObject.SetActive(true);
            closeButton.gameObject.SetActive(true);
            spinningButton.gameObject.SetActive(true);
            MissionButton.gameObject.SetActive(true);
            
            backgroundImage.gameObject.SetActive(true);
            

        }

        private void OnDestroy()
        {
            closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            spinningButton.onClick.RemoveListener(OnSpinButtonClicked);
            MissionButton.onClick.RemoveListener(OnMissionButtonClicked);
           

            
            
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
            spinningButton.transform.localPosition = Vector2.down * 2000;
            spinningButton.transform.localScale = new Vector3(2, 2, 2);
            TimeText.gameObject.SetActive(false);

            //Position of animation
            panelRectTransform.gameObject.SetActive(true);
            wheel.gameObject.SetActive(true);
            spinningButton.gameObject.SetActive(true);

            backgroundImage.gameObject.SetActive(true);
        }

        public override void PlayHideAnimation()
        {
            panelRectTransform.gameObject.SetActive(false);
            wheelBackground.gameObject.SetActive(false);
            wheel.gameObject.SetActive(false);
            spinningButton.gameObject.SetActive(false);
            backgroundImage.gameObject.SetActive(false);
        }
        
        
        //-------- Buttons --------//
        public void OnMissionButtonClicked()
        {
            UniWebViewBridge.Send("openMissionPage",null);
        }

        public void OnAddFlagButtonClicked()
        {
            Debug.Log("Addflag tapped");
            IncreaseFlagTicket();
        }

        public void OnCloseButtonClicked()
        {
            Debug.Log("Close button clicked");
            Debug.Log("UIWheelSpin closed");
            UniWebViewBridge.Send("backHomeAction",null);//send, call, request.  
            // UniWebViewBridge.Send("openMissionPage",null);
            // UniWebViewBridge.Send("SpinItem",new SpinItem{itemId = "69fdaf4e0d3ceac0fa4715a7"});
            // var UserData = UniWebViewBridge.Call("UserData",null);
            // var CurrencyData = UniWebViewBridge.Call("UserData", new Currency(CurrencyType = "data"));
            // var CurrencyData = UniWebViewBridge.Request("UserData", new Currency(CurrencyType = "data"));

        }
        
        public void OnSpinButtonClicked()
        {
            isSpinning = true;
            closeButton.interactable = false;
            spinningButton.interactable = false;
            APIController.StartSpin();
            SpinAgain = false;
            

            // Debug.Log("UIWheelSpin: Spin button clicked");
            // Debug.Log("UIWheelSpin IsSpinning = " + isSpinning);
            // Debug.Log("UIWheelSpin Spin amount = " + APIController.spin_count);
        
        }
    
        public void EnableCloseButton()
        {            
            Debug.Log("Close button Enabled");
            
            closeButton.interactable = true;
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