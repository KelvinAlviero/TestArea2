using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Text;
using TMPro;
using Extras;
using I2.Loc;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Forgehub.SpookyBubbles;



public class Message
{
    public string message;

}
public class UIWheelSpin : UIPage
{
    [Header("Script References")]
    [SerializeField] private APIController APIController;
    [SerializeField] private SpinningScript SpinningScript;
    [SerializeField] private WheelPopulate wheelPopulate;
    [SerializeField] private FreeSpinChecker freeSpinChecker;
    [SerializeField] private Timer timer;
    [SerializeField] private FlagGetter flagGetter;



    [SerializeField] private RectTransform wheelBackground;
    [SerializeField] private RectTransform wheel;
    [SerializeField] private Image backgroundImage;
    
    // [SerializeField] private Image LightsON_Single;
    [SerializeField] private RectTransform panelRectTransform;
    
    [SerializeField] public UIWheelBalance balanceError;

    [SerializeField] private RectTransform contentRectTransform;
    private bool isSpinning = false;
    [SerializeField] public bool ispagedisplayed = false;
    
    public List<GameObject> slot;
    public RectTransform ContentRectTransform => contentRectTransform;
    [FormerlySerializedAs("rewardDatabase")]
    public List<RewardSO> rewardList;

    [SerializeField] private Button closeButton;
    [SerializeField] public Button spinFree; // X = 6, Y = -45, SCALE = 2
    [SerializeField] public Button spinPaid;
    [SerializeField] private Button addFlagButton; //UniWebViewBridge.Send("openMissionPage",null);
    [SerializeField] private Button ResetFreeSpin;
    [SerializeField] private Button MissionButton; //UniWebViewBridge.Send("openMissionPage",null);
    public bool freeSpinButton;

    public bool IsSpinning => isSpinning;
    public Button AddFlagButton => addFlagButton;

    public bool GetIsSpinning()
    {
        return isSpinning;
    }

    public void SetIsSpinning(bool value)
    {
        isSpinning = value;
    }



    public class LanguageResponse
    {
        public string language;
    }

    [Serializable]
    private class OpenEventCenterRequest
    {
        public string sectionId;
    }

    private void Awake()
    {
        CacheComponents();
        Init();
        ispagedisplayed = false;
        EnableCanvas();        
    }

    private void OnEnable()
    {
        // Clear stale balance immediately so a previous account can't linger.
        flagGetter.OnFlagTicketChange(0);
        flagGetter.GetFlagTicket();
        SetAppLanguage();
        timer.Initializer();
        wheelPopulate.GetReward();
        freeSpinChecker.FreeSpinCheck();
    }

    private void Update()
    {
        timer.TimerConstant();
    }

    public void Init()
    {
        // Debug.Log("Init everything");
        closeButton.onClick.AddListener(OnCloseButtonClicked);
        spinFree.onClick.AddListener(OnSpinFreeButtonClicked);
        spinPaid.onClick.AddListener(OnSpinPaidButtonClicked);
        AddFlagButton.onClick.AddListener(OnAddFlagButtonClicked);
        MissionButton.onClick.AddListener(OnMissionButtonClicked);
        panelRectTransform.gameObject.SetActive(true);
        wheelBackground.gameObject.SetActive(true);
        wheel.gameObject.SetActive(true);
        closeButton.gameObject.SetActive(true);
        spinFree.gameObject.SetActive(true);
        spinPaid.gameObject.SetActive(true);
        MissionButton.gameObject.SetActive(true);
        backgroundImage.gameObject.SetActive(true);
        ResetFreeSpin.onClick.AddListener(OnResetFreeSpinButtonClicked);
    }

    private void OnDestroy()
    {
        closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        spinFree.onClick.RemoveListener(OnSpinFreeButtonClicked);
        MissionButton.onClick.RemoveListener(OnMissionButtonClicked);
        spinPaid.onClick.RemoveListener(OnSpinPaidButtonClicked);
        ResetFreeSpin.onClick.RemoveListener(OnResetFreeSpinButtonClicked);
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
        spinFree.transform.localPosition = Vector2.down * 2000;
        spinFree.transform.localScale = new Vector3(2, 2, 2);
        timer.TimeText.gameObject.SetActive(false);

        //Position of animation
        panelRectTransform.gameObject.SetActive(true);
        wheel.gameObject.SetActive(true);
        spinFree.gameObject.SetActive(true);

        backgroundImage.gameObject.SetActive(true);
    }

    public override void PlayHideAnimation()
    {
        panelRectTransform.gameObject.SetActive(false);
        wheelBackground.gameObject.SetActive(false);
        wheel.gameObject.SetActive(false);
        spinFree.gameObject.SetActive(false);
        backgroundImage.gameObject.SetActive(false);
    }


    //-------- Buttons --------//
    public void OnMissionButtonClicked()
    {
        UniWebViewBridge.Send("openEventCenterPage", new OpenEventCenterRequest { sectionId = "liveops_spinwheel" });
    }

    public void OnAddFlagButtonClicked()
    {
        Debug.Log("Addflag tapped");
        flagGetter.IncreaseFlagTicket();
    }

    public void OnCloseButtonClicked()
    {
        Debug.Log("Close button clicked");
        Debug.Log("UIWheelSpin closed");
        UniWebViewBridge.Send("backHomeAction", null);//send, call, request.  
        freeSpinChecker.FreeSpinCheck();
                                                      // UniWebViewBridge.Send("openMissionPage",null);
                                                      // UniWebViewBridge.Send("SpinItem",new SpinItem{itemId = "69fdaf4e0d3ceac0fa4715a7"});
                                                      // var UserData = UniWebViewBridge.Call("UserData",null);
                                                      // var CurrencyData = UniWebViewBridge.Call("UserData", new Currency(CurrencyType = "data"));
                                                      // var CurrencyData = UniWebViewBridge.Request("UserData", new Currency(CurrencyType = "data"));

    }

    public void OnSpinFreeButtonClicked()
    {
        if (!freeSpinChecker.FreeSpinAvailable)
        {
            Debug.Log("Free spin unavailable");
            return;
        }

        closeButton.interactable = false;
        spinFree.interactable = false;
        spinPaid.interactable = false;
        MissionButton.interactable = false;
        APIController.StartSpin();
        timer.StartTimer();
    }

    public void OnSpinPaidButtonClicked()
    {
        APIController.DebugSpin();

        // if (flagGetter.flagAmount == 0)
        // {
        //     balanceError.Show();
        // }
        // else
        // {


        // closeButton.interactable = false;
        // spinFree.interactable = false;
        // spinPaid.interactable = false;
        // MissionButton.interactable = false;
        // APIController.StartSpinPaid();
        // }
    }

    public void OnResetFreeSpinButtonClicked()
    {
        timer.TimerDebug = true;

    }

    public void EnableCloseButton()
    {
        Debug.Log("Close button Enabled");

        closeButton.interactable = true;
    }

    public void EnableSpinButton()
    {

        Debug.Log("Spin button Enabled");
        spinFree.interactable = true;
    }

    public void DisableSpinButton()
    {

        Debug.Log("Spin disabled Enabled");
        spinFree.interactable = false;
    }

    public void EnableSpinPaidButton()
    {
        Debug.Log("Spin button Enabled");
        spinPaid.interactable = true;
    }

    public void DisableSpinPaidButton()
    {
        Debug.Log("Spin button disabled");
        spinPaid.interactable = false;
    }

    public void DisableMissionButton()
    {
        Debug.Log("Mission button Enabled");
        MissionButton.interactable = false;
    }

    public void EnableMissionButton()
    {
        Debug.Log("Mission button Enabled");
        MissionButton.interactable = true;
    }

    public void SetAppLanguage()
    {
        var lang = UniWebViewBridge.Call("getAppLanguage", null);
        if (lang == null) return;
        var data = JsonConvert.DeserializeObject<LanguageResponse>(lang);
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

