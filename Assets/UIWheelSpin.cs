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
    [SerializeField] private RectTransform wheelBackground;
    [SerializeField] private RectTransform wheel;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Timer timer;
    // [SerializeField] private Image LightsON_Single;
    [SerializeField] private RectTransform panelRectTransform;
    [SerializeField] public TMP_Text FlagAmount;
    [SerializeField] public UIWheelBalance balanceError;

    [SerializeField] private RectTransform contentRectTransform;
    [SerializeField] public bool isSpinning = false;
    [SerializeField] public bool ispagedisplayed = false;
    [SerializeField] public int flagAmount;
    public List<GameObject> slot;
    public RewardsGetter reward;
    public RectTransform ContentRectTransform => contentRectTransform;
    public List<RewardSO> rewardDatabase;
    private readonly Dictionary<string, int> rewardSlotByItemId = new Dictionary<string, int>();

    [SerializeField] private Button closeButton;
    [SerializeField] public Button spinFree; // X = 6, Y = -45, SCALE = 2
    [SerializeField] public Button spinPaid;
    [SerializeField] private Button AddFlagButton; //UniWebViewBridge.Send("openMissionPage",null);
    [SerializeField] private Button ResetFreeSpin;
    [SerializeField] private Button MissionButton; //UniWebViewBridge.Send("openMissionPage",null);
    private bool freeSpinButton;
    public bool FreeSpinAvailable
    {
        get
        {
            return freeSpinButton;
        } 
        set
        {
            freeSpinButton = value;
        }
    }

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
        OnFlagTicketChange(0);
        GetFlagTicket();
        SetAppLanguage();
        timer.Initializer();
        GetReward();
        FreeSpinCheck();
    }

    public void CheckPaidSpinOnFree()
    {
        if (FreeSpinAvailable == true)
        {
            DisableSpinPaidButton();
        }
        else
        {
            EnableSpinPaidButton();  
        }
    }

    public void GetReward()
    {
        UniWebViewBridge.Request("getSpinWheelRewards",
            new { spinwheel_config_name = "default_testing_spinwheel" },
            onSuccess: json =>
            {
                var data = JsonConvert.DeserializeObject<APIController.SpinWheelRewardsResponse>(json);

                PopulateRewards(data);
                FreeSpinAvailable = data.FreeSpinAvailable;
                FreeSpinCheck();
                Debug.Log(FreeSpinAvailable);
                UniWebViewBridge.Send("applicationReady", null);
                // Host session is ready — refresh balance again (fixes account switch / early Call).
                GetFlagTicket();
                CheckPaidSpinOnFree();
            },
            onError: err =>
            {
                SpinningScript.ShowErrorPanel();
                Debug.LogError("getRewards error: " + err);
                UniWebViewBridge.Send("applicationReady", null);
                GetFlagTicket();
            },
            timeout: 10000);
    }

    public void FreeSpinCheck()
    {
        if (FreeSpinAvailable == false)
        {
            DisableSpinButton();
            timer.StartTimer();
            Debug.Log("FreeSpinDisabled");
        }
        else
        {
            EnableSpinButton();
        }
    }
    private void PopulateRewards(APIController.SpinWheelRewardsResponse data)
    {
        if (data?.Result == null)
        {
            Debug.LogWarning("No reward result payload.");
            return;
        }

        rewardSlotByItemId.Clear();
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

                Debug.Log($"[UIWheelSpin] Matched RewardSO: itemId={matchingSO.itemId}, name={matchingSO.itemName}, slot={slotIndex}");

                rewardSlotByItemId[item.ItemId] = slotIndex;

                var rewardUI = Instantiate(reward, Vector3.zero, Quaternion.identity, slot[slotIndex].transform);
                rewardUI.SetReward(matchingSO);

                var rect = rewardUI.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.localRotation = Quaternion.identity;
                    rect.localScale = Vector3.one;
                }
                slotIndex++;
            }
        }

        if (SpinningScript != null)
            SpinningScript.InvalidateRewardUprightCache();
    }

    public bool TryGetSlotIndex(string itemId, out int slotIndex)
    {
        return rewardSlotByItemId.TryGetValue(itemId, out slotIndex);
    }

    public void GetFlagTicket()
    {
        var raw = UniWebViewBridge.Call("getFlagTicketBalance", null);
        if (string.IsNullOrEmpty(raw))
        {
            OnFlagTicketChange(0);
            return;
        }

        var data = JsonConvert.DeserializeObject<FlagTicketBalanceResponse>(raw);
        if (data == null)
        {
            OnFlagTicketChange(0);
            return;
        }

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
        IncreaseFlagTicket();
    }

    public void OnCloseButtonClicked()
    {
        Debug.Log("Close button clicked");
        Debug.Log("UIWheelSpin closed");
        UniWebViewBridge.Send("backHomeAction", null);//send, call, request.  
        FreeSpinCheck();
                                                      // UniWebViewBridge.Send("openMissionPage",null);
                                                      // UniWebViewBridge.Send("SpinItem",new SpinItem{itemId = "69fdaf4e0d3ceac0fa4715a7"});
                                                      // var UserData = UniWebViewBridge.Call("UserData",null);
                                                      // var CurrencyData = UniWebViewBridge.Call("UserData", new Currency(CurrencyType = "data"));
                                                      // var CurrencyData = UniWebViewBridge.Request("UserData", new Currency(CurrencyType = "data"));

    }

    public void OnSpinFreeButtonClicked()
    {
        if (!FreeSpinAvailable)
        {
            Debug.Log("Free spin unavailable");
            return;
        }

        isSpinning = true;
        closeButton.interactable = false;
        spinFree.interactable = false;
        spinPaid.interactable = false;
        MissionButton.interactable = false;
        APIController.StartSpin();

        timer.StartTimer();

        // Debug.Log("UIWheelSpin: Spin button clicked");
        // Debug.Log("UIWheelSpin IsSpinning = " + isSpinning);
        // Debug.Log("UIWheelSpin Spin amount = " + APIController.spin_count);

    }

    public void OnSpinPaidButtonClicked()
    {

        if (flagAmount == 0)
        {
            balanceError.Show();
        }
        else
        {

        isSpinning = true;
        closeButton.interactable = false;
        spinFree.interactable = false;
        spinPaid.interactable = false;
        MissionButton.interactable = false;
        APIController.StartSpinPaid();
        }
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