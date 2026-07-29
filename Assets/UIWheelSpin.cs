using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using TMPro;
using Extras;
using I2.Loc;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;



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
    public List<GameObject> slot;
    public RewardsGetter reward;
    public RectTransform ContentRectTransform => contentRectTransform;
    public List<RewardSO> rewardDatabase;

    [SerializeField] private Button closeButton;
    [SerializeField] public Button spinningButton; // X = 6, Y = -45, SCALE = 2
    [SerializeField] public Button spinPaid;
    [SerializeField] private Button AddFlagButton; //UniWebViewBridge.Send("openMissionPage",null);
    [SerializeField] private Button MissionButton; //UniWebViewBridge.Send("openMissionPage",null);
    [SerializeField] public bool FreeSpinAvailable;



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
        UniWebViewBridge.Send("applicationReady", null); /// check
        CacheComponents();
        Init();
        ispagedisplayed = false;
        EnableCanvas();
        UIController.ShowPage<UIWheelSpin>();
        GetFlagTicket();
        SetAppLanguage();
        timer.Initializer();
        GetReward();
    }

    public void GetReward()
    {
        UniWebViewBridge.Request("getSpinWheelRewards",
            new { spinwheel_config_name = "default_testing_spinwheel" },
            onSuccess: json =>
            {
                APIController.SpinWheelRewardsResponse data = null;

                if (string.IsNullOrWhiteSpace(json) || json.Contains("\"simulated\""))
                {
#if UNITY_EDITOR
                    Debug.LogWarning("Using editor fallback rewards because the bridge returned a simulated payload.");
                    data = BuildFallbackRewards();
#else
                Debug.LogWarning("Reward response was empty.");
                return;
#endif
                }
                else
                {
                    data = JsonConvert.DeserializeObject<APIController.SpinWheelRewardsResponse>(json);
                }

                PopulateRewards(data);
                FreeSpinAvailable = data?.FreeSpinAvailable ?? false;
            },
            onError: err => Debug.LogError("getRewards error: " + err),
            timeout: 10000);
    }

    private APIController.SpinWheelRewardsResponse BuildFallbackRewards()
    {
        return new APIController.SpinWheelRewardsResponse
        {
            FreeSpinAvailable = true,
            Result = new List<APIController.SpinWheelRewardPackageResult>
        {
            new APIController.SpinWheelRewardPackageResult
            {
                Items = new List<APIController.SpinWheelDrawItem>
                {
                    new APIController.SpinWheelDrawItem { ItemId = "69fdaf4e0d3ceac0fa4715a7", Amount = 50, Name = "Gems" },
                    new APIController.SpinWheelDrawItem { ItemId = "69fdaf380d3ceac0fa4715a5", Amount = 3000, Name = "Coin" }
                }
            }
        }
        };
    }
    private void PopulateRewards(APIController.SpinWheelRewardsResponse data)
    {
        if (data?.Result == null)
        {
            Debug.LogWarning("No reward result payload.");
            return;
        }

        if (slot == null || slot.Count == 0)
        {
            Debug.LogError("No reward slots assigned in UIWheelSpin.");
            return;
        }

        if (rewardDatabase == null || rewardDatabase.Count == 0)
        {
            Debug.LogError("No reward database assigned in UIWheelSpin.");
            return;
        }

        int slotIndex = 0;
        foreach (var package in data.Result)
        {
            if (package?.Items == null) continue;

            foreach (var item in package.Items)
            {
                if (slotIndex >= slot.Count) return;

                Debug.Log($"[UIWheelSpin] Processing reward item: ItemId={item?.ItemId}, Name={item?.Name}, Amount={item?.Amount}");

                var matchingSO = rewardDatabase.Find(so => so != null && so.itemId == item.ItemId);
                if (matchingSO == null)
                {
                    Debug.LogWarning($"[UIWheelSpin] No RewardSO found for itemId={item?.ItemId}. Available IDs: {string.Join(", ", rewardDatabase.Where(so => so != null).Select(so => so.itemId).ToArray())}");
                    continue;
                }

                Debug.Log($"[UIWheelSpin] Matched RewardSO: itemId={matchingSO.itemId}, name={matchingSO.itemName}");

                var rewardUI = Instantiate(reward, Vector3.zero, Quaternion.identity, slot[slotIndex].transform);
                rewardUI.SetReward(matchingSO);

                var rect = rewardUI.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition3D = Vector3.zero;
                    rect.localPosition = Vector3.zero;
                    rect.localScale = Vector3.one;
                }

                slotIndex++;
            }
        }
    }

    public void GetFlagTicket()
    {
        var raw = UniWebViewBridge.Call("getFlagTicketBalance", null);
        if (string.IsNullOrEmpty(raw)) return;
        var data = JsonConvert.DeserializeObject<FlagTicketBalanceResponse>(raw);
        if (data == null) return;
        OnFlagTicketChange(data.balance);
    }

    public void OnFlagTicketChange(int value)
    {
        flagAmount = value;
        FlagAmount.text = "<sprite name=flag>" + flagAmount.ToString();
    }


    public void IncreaseFlagTicket()
    {
        UniWebViewBridge.Request("increaseFlagTicket", new IncreaseFlagTicketRequest { amount = 10 },
        onSuccess: json =>
        {
            var data = JsonConvert.DeserializeObject<FlagTicketBalanceResponse>(json);
            if (data != null)
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
        UniWebViewBridge.Send("openMissionPage", null);
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
        UniWebViewBridge.Send("backHomeAction", null);//send, call, request.  
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