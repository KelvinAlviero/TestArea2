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
    public APIController APIController;
    public WheelPopulate wheelPopulate;
    public SpinningScript spinningScript;
    [Space(10)]

    [Header("WheelSpin")] 
    [SerializeField] private Rigidbody2D rbody;
    [SerializeField] private RectTransform[] slots;
    [SerializeField] private int Reward1, Reward2, Reward3, Reward4 ,Reward5 ,Reward6 ,Reward7, Reward8;
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
        EnsureDebugAngles();
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
                FinalizeSpinResults();
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

        ConfigureForcedRewardBySlot(slotIndex);
        ReceivedBackend = true;
        Debug.Log($"Landing slot={slotIndex} angle={activeTargetAngle} itemId={incomingItemId}");
    }


    // ----- Spinning function, uses button to start ----- //
    public void Rotate() //used by Wheel button, don't delete pls
    {
        Rotate(rewardType);
    }

    public void Rotate(RewardType rewardToUse) 
    {
        if (inRotate == false)
        {
            ReceivedBackend = false;
            activeRewardType = rewardToUse;

            // Start pre-spin; landing target is set when API reward arrives via UnserializedReward
            if (preSpinCoroutine != null)
                StopCoroutine(preSpinCoroutine);

            preSpinCoroutine = StartCoroutine(PreSpinThenSwitch());
            inRotate = true;
        }
    }

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
        ShowErrorPanel();

        if (uIWheelSpin != null)
        {
            uIWheelSpin.EnableSpinButton();
            uIWheelSpin.EnableCloseButton();
            uIWheelSpin.GetIsSpinning();
        }
    }

    public void ShowErrorPanel()
    {
         if (errorPanel != null)
            errorPanel.Show("webview/error");
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

        while (CalculateAngularDistanceToTarget() > approachThreshold && approached < approachWaitTimeout)
        {
            approached += Time.deltaTime;
            yield return null;
        }

        Debug.Log($"Landing switch: cur={transform.eulerAngles.z:F1} target={activeTargetAngle:F1} remaining={CalculateAngularDistanceToTarget():F1}");
        rbody.angularVelocity = switchAngularVelocity;

        int attempt = 0;
        float angularDistance = 0f;
        float computedDecel = 0f;

        while (true)
        {
            angularDistance = CalculateAngularDistanceToTarget();
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
    private void ConfigureForcedRewardBySlot(int slotIndex)
    {
        // Slot order from PopulateRewards matches Reward1..Reward8 angles (Slot1..Slot8).
        switch (slotIndex)
        {
            case 0:
                activeTargetAngle = Reward1;
                activeRewardResult = 1;
                break;
            case 1:
                activeTargetAngle = Reward2;
                activeRewardResult = 2;
                break;
            case 2:
                activeTargetAngle = Reward3;
                activeRewardResult = 3;
                break;
            case 3:
                activeTargetAngle = Reward4;
                activeRewardResult = 4;
                break;
            case 4:
                activeTargetAngle = Reward5;
                activeRewardResult = 5;
                break;
            case 5:
                activeTargetAngle = Reward6;
                activeRewardResult = 6;
                break;
            case 6:
                activeTargetAngle = Reward7;
                activeRewardResult = 7;
                break;
            case 7:
                activeTargetAngle = Reward8;
                activeRewardResult = 8;
                break;
            default:
                activeTargetAngle = Reward1;
                activeRewardResult = 1;
                Debug.LogWarning("Invalid slot index for landing: " + slotIndex);
                break;
        }
    }

    private void ConfigureForcedReward(RewardType rewardType) // Forced reward section, calls from UnserialiedReward
    {
        switch (rewardType) //This is connected to RewardType
        {
            
            case RewardType.UltimateBooster:
                activeTargetAngle = Reward1;
                activeRewardResult = 1;
                break;
            case RewardType.Gems10:
                activeTargetAngle = Reward2;//
                activeRewardResult = 2;
                break;
            case RewardType.DashImmune:
                activeTargetAngle = Reward3;//210f
                activeRewardResult = 3;
                break;
            case RewardType.Magnet:
                activeTargetAngle = Reward4;//270f
                activeRewardResult = 4;
                break;
            case RewardType.Shield:
                activeTargetAngle = Reward5;//330f
                activeRewardResult = 5; 
                break;
            case RewardType.MagnetImmune:
                activeTargetAngle = Reward6;//30f 
                activeRewardResult = 6;   
                break;
            case RewardType.Currency:
                activeTargetAngle = Reward7;//30f 
                activeRewardResult = 7;// 7
                break;
            case RewardType.Speed:
                activeTargetAngle = Reward8;//30f 
                activeRewardResult = 8; //8
                break;
            default:
                activeTargetAngle = 0f;
                activeRewardResult = 8;
                Debug.Log("No angle targetted" + activeTargetAngle);
                Debug.Log("RewardType forced" + rewardType);
                break;
        }
    }

    private void ApplyReward(int rewardId, float targetAngle, string debugText)
    {
        Debug.Log("Frontend: " +debugText);
        rewardResult = rewardId;
        StartCoroutine(SmoothRotateToThenDelayedWin(targetAngle));
    }

    private void WheelCenterizer() //Center reward back on spin end, depends on ApplyReward
    {
        float rot = transform.eulerAngles.z;
        float normalizedRot = (rot + 360f) % 360f;
        float targetAngle;

        if (normalizedRot >= 0f  && normalizedRot < 45f )
        {
            targetAngle = Reward1;
            ApplyReward(1, targetAngle, "UltimateBooster"); 
        }
        else if (normalizedRot < 90f )
        {
            targetAngle = Reward2;
            ApplyReward(2, targetAngle, "Gems");
        }
        else if (normalizedRot < 135f )
        {
            targetAngle = Reward3;
            ApplyReward(3, targetAngle, "DashImmune");
        }
        else if (normalizedRot < 180f )
        {
            targetAngle = Reward4;
            ApplyReward(4, targetAngle, "Magnet");
        }
        else if (normalizedRot < 225f )
        {
            targetAngle = Reward5;
            ApplyReward(5, targetAngle, "Shield");
        }
        else if (normalizedRot < 270f )
        {
            targetAngle = Reward6;
            ApplyReward(6, targetAngle, "MagnetImmune");
        }
        else if (normalizedRot < 315f )
        {
            targetAngle = Reward7;
            ApplyReward(7, targetAngle, "Coins");
        }
        else
        {
            targetAngle = 340f;
            ApplyReward(8, targetAngle, "Speed");
        }
    }
        
    // ----- Afterspin stuffs ----- //
    public IEnumerator DelayedWin() 
    {
        // Debug.Log("DelayedWin called");
        yield return new WaitForSeconds(DelayedWinTime);
        uIWheelReward.PlayShowAnimation();
        
        if (lightAnimationCoroutine != null)
        {
            StopCoroutine(lightAnimationCoroutine);
        }
    }
    private IEnumerator SmoothRotateTo(float targetAngle)
    {
        float startAngle = transform.eulerAngles.z;
        float elapsedTime = 0f;

        while (elapsedTime < smoothRotationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / smoothRotationDuration;
            float newAngle = Mathf.LerpAngle(startAngle, targetAngle, t);
            transform.eulerAngles = new Vector3(0, 0, newAngle);
            yield return null;
        }

        transform.eulerAngles = new Vector3(0, 0, targetAngle);
    }
    private IEnumerator SmoothRotateToThenDelayedWin(float targetAngle) //rotate then win 
    {

        yield return SmoothRotateTo(targetAngle);
        StartCoroutine(DelayedWin());
    }
    private float CalculateAngularDistanceToTarget()
{
        // Normalize current and target to [0,360) and compute positive forward delta
        float currentAngle = transform.eulerAngles.z % 360f;
        if (currentAngle < 0f) currentAngle += 360f;
        float target = activeTargetAngle % 360f;
        if (target < 0f) target += 360f;
        currentAngle = Mathf.Repeat(currentAngle, 360f);

        float delta = (target - currentAngle + 360f) % 360f; //spin extra time
        
        // Protect against near-zero distances
        if (delta < 0.001f) delta = 0f;
        return delta;
}
    private void FinalizeSpinResults()
    {
        // If we have a forced/backend-resolved reward, smoothly center to that target.
        // Otherwise determine the sector and center to that.
        if (rewardType != RewardType.Normal || ReceivedBackend)
        {
            rewardResult = activeRewardResult;
            StartCoroutine(SmoothRotateToThenDelayedWin(activeTargetAngle));
        }
        else
        {
            WheelCenterizer();
        }
        return;
    }


    

// ----- Debug draw for reward angles in the editor ----- //
private void EnsureDebugAngles()
    //I'll fix later
    {
        if (RewardAngleBoundaries == null)
            RewardAngleBoundaries = new List<float>();

        if (RewardAngles == null)
            RewardAngles = new List<float>();

        if (RewardAngleBoundaries.Count == 0)
        {
            RewardAngleBoundaries.Add(0f);
            RewardAngleBoundaries.Add(45f);
            RewardAngleBoundaries.Add(90f);
            RewardAngleBoundaries.Add(135f);
            RewardAngleBoundaries.Add(180f);
            RewardAngleBoundaries.Add(225f);
            RewardAngleBoundaries.Add(270f);
            RewardAngleBoundaries.Add(315f);
        }

        if (RewardAngles.Count == 0)
        {
            if (Reward1 != 0f)
                RewardAngles.Add(Reward1);
            if (Reward2 != 0f)
                RewardAngles.Add(Reward2);
            if (Reward3 != 0f)
                RewardAngles.Add(Reward3);
            if (Reward4 != 0f)
                RewardAngles.Add(Reward4);
            if (Reward5 != 0f)
                RewardAngles.Add(Reward5);
            if (Reward6 != 0f)
                RewardAngles.Add(Reward6);
            if (Reward7 != 0f)
                RewardAngles.Add(Reward7);
            if (Reward8 != 0f)
                RewardAngles.Add(Reward8);
        }
    }
    private void OnDrawGizmos()
    {
        EnsureDebugAngles();

        //Debug for lines
        Gizmos.matrix = transform.localToWorldMatrix;
        foreach (float angle in RewardAngleBoundaries)
        {
            float rad = angle * Mathf.Deg2Rad;
            Vector3 position = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * 2000;
            
            Gizmos.color = Color.green;
            Gizmos.DrawLine(Vector3.zero, position);
        }

        //Debug boxes for each reward
        foreach (float angle in RewardAngles)
        {
            float rad = angle * Mathf.Deg2Rad;
            Vector3 position = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * 2000;
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(Vector3.zero, position);
        }
    }

}