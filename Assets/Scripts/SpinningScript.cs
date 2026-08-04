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
    [Space(10)]

    [Header("WheelSpin")] 
    [SerializeField] private Rigidbody2D rbody;
    [SerializeField] private RectTransform[] slots;
    
    [SerializeField] private Dictionary<string, RewardType> rewardMap;
    [SerializeField] public float stopPower;
    [Space(10)]

    [Header("WheelSpin config")]
    [SerializeField] public float landingTuner = 0.55f; // , <1 Increase power (Lower Resistance),  >1 Decrease power (Higher Resitance)
    [SerializeField] public float minLandingStopPower = 10f; // minimum landing deceleration
    [SerializeField] public float maxLandingStopPower = 8000f; // maximum landing deceleration
    [SerializeField] public float preSpinDuration = 3f; // seconds to spin very fast before applying forced landing
    [SerializeField] public float preSpinSpeed = 3000f; // deg/s during pre-spin
    [SerializeField] public float preSpinDamping = 5f; // small damping during pre-spin so it stays fast
    [SerializeField] public float switchAngularVelocity = 2000f; // angular velocity applied when switching to landing phase
    [SerializeField] public float desiredLandingMinDecel = 400f; // desired min computed deceleration
    [SerializeField] public float desiredLandingMaxDecel = 500f; // desired max computed deceleration
    [SerializeField] public float landingAdjustmentWait = 0.5f; // seconds to wait before retrying decel check
    [SerializeField] public int landingAdjustmentAttempts; // max retries for adjusting landing
    [SerializeField] public float smoothRotationDuration = 0.5f; // Duration for smooth rotation to center
    [SerializeField] private float DelayedWinTime = 3f; //Delays popup
    [SerializeField] private float DelayedSpinTime = 1f;
    [SerializeField] private float rewardWaitTimeout = 10f; // keep spinning until reward or this timeout
    [SerializeField] private float activeTargetAngle;
    [SerializeField] public bool MoreSpins;
    // [SerializeField] public float AngleFix = 22f;

    [Space(10)]

    [Header("Results Debug")]
    [SerializeField] private RewardType activeRewardType;
    [SerializeField] private List<float> RewardAngleBoundaries; 
    [SerializeField] private List<float> RewardAngles;
    [SerializeField] public List<string> rewardAmounts; // Keeping this line intact
     [SerializeField] public int rewardResult; // Keeping this line intact
    [SerializeField] private int activeRewardResult;
    [SerializeField] private float SpinEndTimer;
    [SerializeField] private TMP_Text Debug_RewardList;
    [Space(10)]

    [Header("Others")]
    [SerializeField] public bool ReceivedBackend;
    [SerializeField] public string BackendReward;
    [SerializeField] public RewardType rewardType;
    [SerializeField] bool inRotate;
    [SerializeField] private UIWheelError errorPanel;
    [Space(10)]
    [SerializeField] private Coroutine preSpinCoroutine;
    [SerializeField] private Coroutine lightAnimationCoroutine;

    private float lastWheelZForUpright = float.NaN;
    private Transform[] cachedRewardTransforms;
    private int cachedRewardCount = -1;
    

    private void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
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

    // ----- Update function ----- //
    private void Update()
    {
        if (rbody.angularVelocity > 0f) 
        {
            float currentStopPower = stopPower;

            rbody.angularVelocity -= currentStopPower * Time.deltaTime;
            rbody.angularVelocity = Mathf.Clamp(rbody.angularVelocity, 0f, 1440f);
        }

        if (rbody.angularVelocity <= 0f && inRotate)
        {
            rbody.angularVelocity = 0f;
            SpinEndTimer += Time.deltaTime;
            if (SpinEndTimer >= DelayedSpinTime)
            {
                uISpinningScript.FinalizeSpinResults();
                inRotate = false;
                SpinEndTimer = 0f;
            }
        }
        activeRewardType = rewardType;
        BackendReward = APIController.ObtainedReward;
    }

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

    // ----- ID to case translator ----- //
    public bool TryResolveReward(string incomingItemId, out RewardType resolvedReward)
    {

        if (rewardMap.TryGetValue(incomingItemId, out resolvedReward))
            return true;

        return false;
    }


    // ----- Input rewards Quue ----- //
    // public void QueueRewards(IEnumerable<string> incomingItemIds)
    // {
    //     // Makes new list if null
    //     if (rewardAmounts == null)
    //         rewardAmounts = new List<string>();

    //     rewardAmounts.Clear();
    //     rewardQueueIndex = 0;

    //     if (incomingItemIds == null)
    //         return;

    //     foreach (string incomingItemId in incomingItemIds)
    //     {
    //         if (string.IsNullOrEmpty(incomingItemId))
    //             continue;

    //         rewardAmounts.Add(incomingItemId);
    //     }
    //     Debug.Log("Queued reward IDs: " + string.Join(",", rewardAmounts));
        
    // }

    // public bool HasPendingRewards => rewardAmounts != null && rewardQueueIndex < rewardAmounts.Count;

    // ----- Start extra spins -- //
    // public void StartNextQueuedSpin()
    // {
    //     if (!HasPendingRewards)
    //     {
    //         MoreSpins = false;
    //         Debug.Log("No queued rewards left.");
    //         return;
    //     }

    //     string incomingItemId = rewardAmounts[rewardQueueIndex];
    //     rewardQueueIndex++;

    //     if (TryResolveReward(incomingItemId, out RewardType resolvedReward))
    //     {
    //         rewardType = resolvedReward;
    //         activeRewardType = resolvedReward;
    //         ConfigureForcedReward(resolvedReward);
    //         ReceivedBackend = true;
    //         Rotate(resolvedReward);
    //         // Debug.Log("Starting queued reward: " + incomingItemId + " -> " + resolvedReward);
    //         Debug_RewardList.text = rewardType.ToString();
    //         MoreSpins = true;
    //     }
    //     else
    //     {
    //         ReceivedBackend = false;
    //         Debug.LogWarning("Unknown queued reward ID: " + incomingItemId);
    //     }
    // }

    public void UnserializedReward(string incomingItemId) //Translates ID into cases
    {
        if (!inRotate)
            return;

        if (uIWheelSpin == null || !wheelPopulate.TryGetSlotIndex(incomingItemId, out int slotIndex))
        {
            ReceivedBackend = false;
            Debug.LogWarning("No populated slot for reward ID: " + incomingItemId);
            HandleSpinFailed();
            return;
        }

        if (TryResolveReward(incomingItemId, out RewardType resolvedReward))
        {
            rewardType = resolvedReward;
            activeRewardType = resolvedReward;
        }

        wheelPopulate.ConfigureForcedRewardBySlot(slotIndex);
        ReceivedBackend = true;
        Debug.Log($"Landing slot={slotIndex} angle={activeTargetAngle} itemId={incomingItemId}");
    }


    // ----- Spinning function, uses button to start ----- //
    public void HandleSpinFailed()
    {
        if (preSpinCoroutine != null)
        {
            StopCoroutine(preSpinCoroutine);
            preSpinCoroutine = null;
        }

        rbody.angularVelocity = 0f;
        stopPower = 0f;
        inRotate = false;
        ReceivedBackend = false;
        SpinEndTimer = 0f;
        uISpinningScript.ShowErrorPanel();

        if (uIWheelSpin != null)
        {
            uIWheelSpin.EnableSpinButton();
            uIWheelSpin.EnableCloseButton();
            uIWheelSpin.GetIsSpinning();
        }
    }


    // -----  During spin functions ----- //
    private IEnumerator PreSpinThenSwitch() //Changes to new angle for forced rewards (Refactor needed because wtf is this)
    {
        stopPower = preSpinDamping;
        rbody.angularVelocity = preSpinSpeed;

        // 1) Always spin for at least preSpinDuration
        float elapsed = 0f;
        while (elapsed < preSpinDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 2) If reward not ready yet, keep spinning until it arrives or timeout
        float waited = 0f;
        while (!ReceivedBackend && waited < rewardWaitTimeout)
        {
            if (rbody.angularVelocity < switchAngularVelocity)
                rbody.angularVelocity = switchAngularVelocity;

            waited += Time.deltaTime;
            yield return null;
        }

        if (!ReceivedBackend)
        {
            HandleSpinFailed();
            yield break;
        }

        // 3) Reward ready — land on that slot
        float approachThreshold = 120f;
        float approachWaitTimeout = 5f;
        float approached = 0f;

        rbody.angularVelocity = Mathf.Min(rbody.angularVelocity, switchAngularVelocity);

        while (uISpinningScript.CalculateAngularDistanceToTarget() > approachThreshold && approached < approachWaitTimeout)
        {
            approached += Time.deltaTime;
            yield return null;
        }

        Debug.Log($"Landing switch: cur={transform.eulerAngles.z:F1} target={activeTargetAngle:F1} remaining={uISpinningScript.CalculateAngularDistanceToTarget():F1}");
        rbody.angularVelocity = switchAngularVelocity;

        int attempt = 0;
        float angularDistance = 0f;
        float computedDecel = 0f;

        while (true)
        {
            angularDistance = uISpinningScript.CalculateAngularDistanceToTarget();
            while (angularDistance < 60f) angularDistance += 360f;

            float v = Mathf.Abs(rbody.angularVelocity);
            computedDecel = v * v / (2f * angularDistance);

            if (computedDecel >= desiredLandingMinDecel && computedDecel <= desiredLandingMaxDecel)
                break;

            attempt++;
            if (attempt >= landingAdjustmentAttempts)
            {
                Debug.Log($"Computed decel out of range after {attempt} attempts ({computedDecel:F1}), proceeding with clamped value.");
                break;
            }

            float waitTime = 0f;
            while (waitTime < landingAdjustmentWait)
            {
                waitTime += Time.deltaTime;
                yield return null;
            }
        }

        Debug.Log($"Landing decel final: {computedDecel:F1}");
        stopPower = Mathf.Clamp(computedDecel * landingTuner, minLandingStopPower, maxLandingStopPower);

        preSpinCoroutine = null;
        yield break;
    }


    // ----- Processing while spinning ----- //
    

    public void ApplyReward(int rewardId, float targetAngle, string debugText)
    {
        Debug.Log("Frontend: " +debugText);
        rewardResult = rewardId;
        StartCoroutine(uISpinningScript.SmoothRotateToThenDelayedWin(targetAngle));
    }



}