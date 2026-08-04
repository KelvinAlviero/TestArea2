using System;
using System.Collections;
using System.Collections.Generic;
using Forgehub.SpookyBubbles;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;



public class SpinningScript : MonoBehaviour
{ //oml man why did i make this script so bloated
//I gotta ask the lads how to cut this down cuz this ain't company standard coding    
    [Header("Script References")]
    public UIWheelReward uIWheelReward;
    public UIWheelSpin uIWheelSpin;
    public UISpinningScript uISpinningScript;
    public APIController APIController;
    public WheelPopulate wheelPopulate;
    public SpinningScript spinningScript;
    public Timer timer;
    [Space(10)]

    [Header("WheelSpin")] 
    
    [SerializeField] private RectTransform[] slots;
    
    [SerializeField] private Dictionary<string, RewardType> rewardMap;
    
    [Space(10)]
    [SerializeField] public RewardType rewardType;
    
    [Space(10)]

    [Header("Results Debug")]
    private List<float> RewardAngleBoundaries; 
    private List<float> RewardAngles;
    public List<string> rewardAmounts; // Keeping this line intact
    public int rewardResult; // Keeping this line intact
    [SerializeField] private UIWheelError errorPanel;
    
    
    [Space(10)]

    [Header("Others")]
    public bool ReceivedBackend;
    public string BackendReward;
    
    [Space(10)]
    
    

    private float lastWheelZForUpright = float.NaN;
    private Transform[] cachedRewardTransforms;
    private int cachedRewardCount = -1;
    
    public Dictionary<string, RewardType> GetRewardList => rewardMap;

    
    private void Start()
    {
        
        // Initialize reward angles
        RewardAngleBoundaries = new List<float>();
        RewardAngles = new List<float>();
        rewardAmounts = new List<string>();
        // EnsureDebugAngles();
        rewardMap = new Dictionary<string, RewardType>
        {
            //
            { "69fdaf4e0d3ceac0fa4715a7", RewardType.Gems10 },
            { "69fdaf380d3ceac0fa4715a5", RewardType.Currency },
            { "6a47cb262754bd1e11ffd778", RewardType.UltimateBooster },
            { "69fdaeed0d3ceac0fa47159f", RewardType.Magnet },
            { "69fdaf260d3ceac0fa4715a3", RewardType.Shield },
            { "69fdaf030d3ceac0fa4715a1", RewardType.Speed },
            { "6a47cb262754bd1e11ffd776", RewardType.MagnetImmune},
            { "6a47cb262754bd1e11ffd777", RewardType.DashImmune}
        };
    }

    public enum RewardType //List for rewards
    {
        Normal, //Pick this for random reward
        Gems10, //69fdaf4e0d3ceac0fa4715a7
        Currency, //69fdaf380d3ceac0fa4715a5
        UltimateBooster,//6a47cb262754bd1e11ffd778
        Magnet, //69fdaeed0d3ceac0fa47159f
        Shield, //69fdaf260d3ceac0fa4715a3
        Speed, //69fdaf030d3ceac0fa4715a1
        MagnetImmune, //6a47cb262754bd1e11ffd776
        DashImmune //6a4b79d32754bd1e11ffdbbe
    }

    public RewardType GetRewardType()
    {
        return rewardType;
    }

    // ----- Update function ----- //
    

    private void LateUpdate()
    {
        float wheelZ = transform.localEulerAngles.z;
        // Skip when the wheel isn't moving — avoids per-frame UI dirtying while idle.
        if (!float.IsNaN(lastWheelZForUpright) &&
            Mathf.Abs(Mathf.DeltaAngle(lastWheelZForUpright, wheelZ)) < 0.01f)
            return;

        lastWheelZForUpright = wheelZ;
        KeepSlotsUpright(wheelZ);
    }

    private void KeepSlotsUpright(float wheelZ)
    {
        if (slots == null || slots.Length == 0)
            return;

        RefreshRewardCacheIfNeeded();

        float counterZ = -wheelZ;
        for (int i = 0; i < cachedRewardCount; i++)
        {
            Transform rewardTransform = cachedRewardTransforms[i];
            if (rewardTransform == null)
                continue;

            rewardTransform.localEulerAngles = new Vector3(0f, 0f, counterZ);
        }
    }

    private void RefreshRewardCacheIfNeeded()
    {
        int childCount = 0;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
                childCount += slots[i].childCount;
        }

        if (cachedRewardTransforms != null && cachedRewardCount == childCount)
            return;

        cachedRewardTransforms = new Transform[childCount];
        cachedRewardCount = childCount;

        int index = 0;
        for (int i = 0; i < slots.Length; i++)
        {
            RectTransform slotTransform = slots[i];
            if (slotTransform == null)
                continue;

            // Ensure slots stay aligned with the wheel (no leftover counter-rotation).
            slotTransform.localEulerAngles = Vector3.zero;

            for (int c = 0; c < slotTransform.childCount; c++)
                cachedRewardTransforms[index++] = slotTransform.GetChild(c);
        }
    }

    /// <summary>
    /// Call after rewards are instantiated into slots so the upright cache refreshes.
    /// </summary>
    public void InvalidateRewardUprightCache()
    {
        cachedRewardCount = -1;
    }
    // ----- Spinning function, uses button to start ----- //
    public void HandleSpinFailed()
    {
        if (uISpinningScript.GetSpinCoroutine() != null)
        {
            StopCoroutine(uISpinningScript.GetSpinCoroutine());
            uISpinningScript.SetSpinCoroutine(null);
        }

        uISpinningScript.GetRbody().angularVelocity = 0f;
        uISpinningScript.SetStopPower(0f);
        uIWheelSpin.SetIsSpinning(false);
        ReceivedBackend = false;
        timer.SetSpinEndTimer(0f);
        ShowErrorPanel();

        if (uIWheelSpin != null)
        {
            uIWheelSpin.EnableSpinButton();
            uIWheelSpin.EnableCloseButton();
            uIWheelSpin.GetIsSpinning();
        }
    }


    // ----- Processing while spinning ----- //
    public void ApplyReward(int rewardId, float targetAngle, string debugText)
    {
        Debug.Log("Frontend: " +debugText);
        rewardResult = rewardId;
        StartCoroutine(uISpinningScript.SmoothRotateToThenDelayedWin(targetAngle));
    }

    public void ShowErrorPanel()
    {
         if (errorPanel != null)
            errorPanel.Show("webview/error");
    }
}