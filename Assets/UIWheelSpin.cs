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
using I2.Loc;


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
        [SerializeField] private Timer timer;
        // [SerializeField] private Image LightsON_Single;
        [SerializeField] private RectTransform panelRectTransform;
        [SerializeField] public TMP_Text FlagAmount;
        
        [SerializeField] private RectTransform contentRectTransform;
        [SerializeField] public bool isSpinning = false;
        [SerializeField] public bool ispagedisplayed = false;
        [SerializeField] public int flagAmount;
        
        
        public RectTransform ContentRectTransform => contentRectTransform;

        [SerializeField] private Button closeButton;
        [SerializeField] public Button spinningButton; // X = 6, Y = -45, SCALE = 2
        [SerializeField] public Button spinPaid;
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

        public class LanguageResponse
        {
            public string language;
        }

        private void Awake()
        {
            UniWebViewBridge.Send("applicationReady",null); /// check
            CacheComponents();
            Init();
            ispagedisplayed = false;
            EnableCanvas();
            UIController.ShowPage<UIWheelSpin>();
            GetFlagTicket();
            SetAppLanguage();
            timer.Initializer();
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
            flagAmount = value;
            FlagAmount.text = "<sprite name=flag>" + flagAmount.ToString();
        }


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
            timer.TimerConstant();
        }

        public void Init()
        {
            // Debug.Log("Init everything");
            closeButton.onClick.AddListener(OnCloseButtonClicked);
            spinningButton.onClick.AddListener(OnSpinButtonClicked);
            spinPaid.onClick.AddListener(OnSpinButtonClicked);
            AddFlagButton.onClick.AddListener(OnAddFlagButtonClicked);
            MissionButton.onClick.AddListener(OnMissionButtonClicked);
            panelRectTransform.gameObject.SetActive(true);
            wheelBackground.gameObject.SetActive(true);
            wheel.gameObject.SetActive(true);
            closeButton.gameObject.SetActive(true);
            spinningButton.gameObject.SetActive(true);
            spinPaid.gameObject.SetActive(true);
            MissionButton.gameObject.SetActive(true);
            backgroundImage.gameObject.SetActive(true);
            

        }

        private void OnDestroy()
        {
            closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            spinningButton.onClick.RemoveListener(OnSpinButtonClicked);
            MissionButton.onClick.RemoveListener(OnMissionButtonClicked);
            spinPaid.onClick.RemoveListener(OnSpinButtonClicked);
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
            timer.TimeText.gameObject.SetActive(false);

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
            spinPaid.interactable = false;
            APIController.StartSpin();
            SpinAgain = false;
            timer.StartTimer();

            // Debug.Log("UIWheelSpin: Spin button clicked");
            // Debug.Log("UIWheelSpin IsSpinning = " + isSpinning);
            // Debug.Log("UIWheelSpin Spin amount = " + APIController.spin_count);
        
        }

        public void OnSpinPaidButtonClicked()
        {
            //Spin Paid code
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

        public void SetAppLanguage()
        {
            var lang = UniWebViewBridge.Call("getAppLanguage",null);
            var data = JsonUtility.FromJson<LanguageResponse>(lang);
            if (lang != null)
            {
                if (LocalizationManager.HasLanguage(data.language))
                {
                    LocalizationManager.CurrentLanguage = data.language;
                    Debug.Log($"[LanguageController] Change to {data.language} language");
                }
                else
                {
                    Debug.LogError($"[LanguageController] Language {data.language} Not found");
                }
            }
        }
        
}