using System;
using System.Collections;
using System.Collections.Generic;
using Forgehub.SpookyBubbles;
using JetBrains.Annotations;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;


public class SpinningScript : MonoBehaviour
{

    
    [Header("Script References")]
    public UIWheelReward UIWheelReward;
    public UIWheelSpin UIWheelSpin;
    public APIController APIController;
    // [SerializeField] public int RewardShift = 0; //wheel adjustment
    [Space(10)]

    [Header("WheelSpin")]
    [SerializeField] private Rigidbody2D rbody;
    [SerializeField] private int Reward1, Reward2, Reward3, Reward4 ,Reward5 ,Reward6 ,Reward7;
    [SerializeField] private Dictionary<string, RewardType> rewardMap;
    [SerializeField] public float rotatePower ;
    [SerializeField] public float stopPower;
    [SerializeField] public Image DebugWheelPoint;
    [Space(10)]

    [Header("WheelSpin config")]
    [SerializeField] public float scalingFactor = 50f; // Physics tuning: higher = less deceleration
    [SerializeField] public float landingTuner = 0.55f; // >1 undershoots, <1 overshoots
    [SerializeField] public float preSpinDuration = 3f; // seconds to spin very fast before applying forced landing
    [SerializeField] public float preSpinSpeed = 3000f; // deg/s during pre-spin
    [SerializeField] public float preSpinDamping = 5f; // small damping during pre-spin so it stays fast
    [SerializeField] public float switchAngularVelocity = 2000f; // angular velocity applied when switching to landing phase
    [SerializeField] public float smoothRotationDuration = 0.5f; // Duration for smooth rotation to center
    [SerializeField] private int DelayedWinTime = 1000;
    [SerializeField] private float ChangeDelay = 3f;
    [SerializeField] private float activeTargetAngle;
    [Space(10)]

    [Header("Results Debug")]
    [SerializeField] private RewardType activeRewardType;
    [SerializeField] private List<float> rewardAngles;   
    [SerializeField] public List<string> rewardAmounts;
    [SerializeField] public int rewardResult;
    [SerializeField] private int activeRewardResult;
    [SerializeField] private int rewardQueueIndex;
    [SerializeField] private TMP_Text Debug_RewardList;
    [Space(10)]

    [Header("Others")]
    [SerializeField] public bool ReceivedBackend;
    [SerializeField] public string BackendReward;
    [SerializeField] public RewardType rewardType;
    [SerializeField] int inRotate;
    [Space(10)]
    [SerializeField] private Coroutine preSpinCoroutine;
    [SerializeField] private Coroutine lightAnimationCoroutine;

    
    
    
    private void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
        // Initialize reward angles
        rewardAngles = new List<float>();
        rewardAmounts = new List<string>();
        rewardQueueIndex = 0;
        rewardMap = new Dictionary<string, RewardType>
    {
        { "69fdaf4e0d3ceac0fa4715a7", RewardType.Gems10 },
        { "69fdaf380d3ceac0fa4715a5", RewardType.Coin20 },
        { "6a2f6ece2754bd1e11ffcc90", RewardType.Booster },
        { "69fdaeed0d3ceac0fa47159f", RewardType.Magnet },
        { "69fdaf260d3ceac0fa4715a3", RewardType.Shield },
        { "69fdaf030d3ceac0fa4715a1", RewardType.Speed }
    };
    }

    public enum RewardType //List for rewards
    {
        Normal, //Pick this for random reward
        Gems10, //69fdaf4e0d3ceac0fa4715a7
        Coin20, //69fdaf380d3ceac0fa4715a5
        Booster,//6a2f6ece2754bd1e11ffcc90
        Magnet, //69fdaeed0d3ceac0fa47159f
        Shield, //69fdaf260d3ceac0fa4715a3
        Speed //69fdaf030d3ceac0fa4715a1
    }

    float t;

    // ----- ID to case translator ----- //
    public bool TryResolveReward(string incomingItemId, out RewardType resolvedReward)
    {

        if (rewardMap.TryGetValue(incomingItemId, out resolvedReward))
            return true;

        return false;
    }

    public void QueueRewards(IEnumerable<string> incomingItemIds)
    {
        if (rewardAmounts == null)
            rewardAmounts = new List<string>();

        rewardAmounts.Clear();
        rewardQueueIndex = 0;

        if (incomingItemIds == null)
            return;

        foreach (string incomingItemId in incomingItemIds)
        {
            if (string.IsNullOrEmpty(incomingItemId))
                continue;

            rewardAmounts.Add(incomingItemId);
        }
        Debug.Log("Queued reward IDs: " + string.Join(",", rewardAmounts));
        
    }

    public bool HasPendingRewards => rewardAmounts != null && rewardQueueIndex < rewardAmounts.Count;

    public void StartNextQueuedSpin()
    {
        if (!HasPendingRewards)
        {
            Debug.Log("No queued rewards left.");
            return;
        }

        string incomingItemId = rewardAmounts[rewardQueueIndex];
        rewardQueueIndex++;

        if (TryResolveReward(incomingItemId, out RewardType resolvedReward))
        {
            rewardType = resolvedReward;
            activeRewardType = resolvedReward;
            ConfigureForcedReward(resolvedReward);
            ReceivedBackend = true;
            Rotate(resolvedReward);
            Debug.Log("Starting queued reward: " + incomingItemId + " -> " + resolvedReward);
            
            //Debug reward list
            // string formattedText = "";
            // foreach (string item in rewardAmounts)
            // {
            //     formattedText += "• <indent=5%>" + item + "</indent>\n";
            // }

            Debug_RewardList.text = rewardType.ToString();
        }
        else
        {
            ReceivedBackend = false;
            Debug.LogWarning("Unknown queued reward ID: " + incomingItemId);
        }
    }

    public void UnserializedReward(string incomingItemId) //Translates ID into cases
    {
        if (TryResolveReward(incomingItemId, out RewardType resolvedReward))
        {
            rewardType = resolvedReward;
            activeRewardType = resolvedReward;

            ConfigureForcedReward(resolvedReward);
            ReceivedBackend = true;
            string combined = String.Join(",", rewardAmounts ?? new List<string>());
            Debug.Log("Reward Amounts " + combined);
        }
        else
        {
            ReceivedBackend = false;
            Debug.LogWarning("Unknown reward ID: " + incomingItemId);
        }
    }

    // ----- Update function ----- //
    private void Update()
    {
        if (rbody.angularVelocity > 0f) 
        {
            rbody.angularVelocity -= stopPower * Time.deltaTime;
            rbody.angularVelocity = Mathf.Clamp(rbody.angularVelocity, 0f, 1440f);
        }

        if (rbody.angularVelocity <= 0f && inRotate == 1)
        {
            rbody.angularVelocity = 0f;
            t += Time.deltaTime;
            if (t >= 0.5f)
            {
                FinalizeSpinResults();
                inRotate = 0;
                t = 0f;
            }
        }
        activeRewardType = rewardType;
        BackendReward = APIController.ObtainedReward;
        Debug.Log("Backend Reward: "+ APIController.ObtainedReward);
        Debug.Log("activeRewardType" + activeRewardType);
    }

    // ----- Spinning function, uses button to start ----- //
    public void Rotate()
    {
        Rotate(rewardType);
    }

    public void Rotate(RewardType rewardToUse)
    {
        if (inRotate == 0)
        {
            activeRewardType = rewardToUse;

            ConfigureForcedReward(activeRewardType);

            // Start pre-spin coroutine which will spin fast, then switch to landing phase
            if (preSpinCoroutine != null)
                StopCoroutine(preSpinCoroutine);

            preSpinCoroutine = StartCoroutine(PreSpinThenSwitch());
            inRotate = 1;
        }
    }


    // -----  During spin functions ----- //
    private IEnumerator PreSpinThenSwitch() //Changes to new angle for forced rewards
    {
        // float previousStopPower = stopPower;
        stopPower = preSpinDamping;
        rbody.angularVelocity = preSpinSpeed;

        float elapsed = 0f;
        while (elapsed < preSpinDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (ReceivedBackend == true) //Calls the change only once backend is received. 
        {
            Debug.Log("changing reward");
            // Set consistent angular velocity for the landing phase
            rbody.angularVelocity = switchAngularVelocity;

            // Compute angular distance to active target and ensure at least one sector rotation
            float angularDistance = CalculateAngularDistanceToTarget();
            while (angularDistance < 60f)
                angularDistance += 360f;

            // Using rotational kinematics: distance = v^2 / (2 * a) -> a = v^2 / (2 * distance)
            float v = Mathf.Abs(rbody.angularVelocity);
            float computedDecel = (v * v) / (2f * angularDistance);

            // Apply tuner and guard against tiny/huge values
            stopPower = Mathf.Clamp(computedDecel * landingTuner, 10f, 20000f);
        }
        // cleanup coroutine handle
        preSpinCoroutine = null;
        yield break;
    }


    // ----- Processing while spinning ----- //
    private void ConfigureForcedReward(RewardType rewardType) // Forced reward section, calls from UnserialiedReward
    {
        switch (rewardType) //This is connected to RewardType
        {
            case RewardType.Shield:
                activeTargetAngle = Reward1;//90f
                activeRewardResult = 1;
                Debug.Log("TargetAngle" + activeTargetAngle);
                Debug.Log("RewardType forced" + rewardType);
                break;
            case RewardType.Magnet:
                activeTargetAngle = Reward2;//150f
                activeRewardResult = 2;
                Debug.Log("TargetAngle" + activeTargetAngle);
                Debug.Log("RewardType forced" + rewardType);
                break;
            case RewardType.Coin20:
                activeTargetAngle = Reward3;//210f
                activeRewardResult = 3;
                Debug.Log("TargetAngle" + activeTargetAngle);
                Debug.Log("RewardType forced" + rewardType);
                break;
            case RewardType.Speed:
                activeTargetAngle = Reward4;//270f
                activeRewardResult = 4;
                Debug.Log("TargetAngle" + activeTargetAngle);
                Debug.Log("RewardType forced" + rewardType);
                break;
            case RewardType.Booster:
                activeTargetAngle = Reward5;//330f
                activeRewardResult = 5;
                Debug.Log("TargetAngle" + activeTargetAngle);
                Debug.Log("RewardType forced" + rewardType);
                break;
            case RewardType.Gems10:
                activeTargetAngle = Reward6;//30f 
                activeRewardResult = 6;
                Debug.Log("TargetAngle" + activeTargetAngle);
                Debug.Log("RewardType forced" + rewardType);
                break;
            default:
                activeTargetAngle = 0f;
                activeRewardResult = 5;
                Debug.Log("No angle targetted" + activeTargetAngle);
                Debug.Log("RewardType forced" + rewardType);
                break;
        }
    }

    private void ApplyReward(int rewardId, float targetAngle, string debugText)
    {
        Debug.Log(debugText);
        rewardResult = rewardId;
        StartCoroutine(SmoothRotateToThenDelayedWin(targetAngle));
    }

    private void WheelCenterizer() //Center reward back on spin end, depends on ApplyReward
    {
        float rot = transform.eulerAngles.z;
        float normalizedRot = (rot + 360f) % 360f;
        float targetAngle;

        if (normalizedRot >= 0f && normalizedRot < 60f)
        {
            targetAngle = Reward6;
            ApplyReward(6, targetAngle, "10 Coins"); 
        }
        else if (normalizedRot < 120f)
        {
            targetAngle = Reward1;
            ApplyReward(1, targetAngle, "Placeholder");
        }
        else if (normalizedRot < 180f)
        {
            targetAngle = Reward2;
            ApplyReward(2, targetAngle, "Placeholder");
        }
        else if (normalizedRot < 240f)
        {
            targetAngle = Reward3;
            ApplyReward(3, targetAngle, "20 coins");
        }
        else if (normalizedRot < 300f)
        {
            targetAngle = Reward4;
            ApplyReward(4, targetAngle, "Placeholder");
        }
        else
        {
            targetAngle = Reward5;
            ApplyReward(5, targetAngle, "Prebooster");
        }
    }
        
    // ----- Afterspin stuffs ----- //
    public async void DelayedWin() 
    {
        UIWheelSpin.LightCheck = true;
        StartCoroutine(UIWheelSpin.LightAnimation());
        Debug.Log("DelayedWin called");
        await System.Threading.Tasks.Task.Delay(DelayedWinTime);
        UIWheelReward.PlayShowAnimation();
        
        if (lightAnimationCoroutine != null)
        {
            StopCoroutine(lightAnimationCoroutine);
        }
    }
    private IEnumerator SmoothRotateTo(float targetAngle)
    {
        RectTransform rect = GetComponent<RectTransform>();
        float startAngle = rect.eulerAngles.z;
        float elapsedTime = 0f;

        while (elapsedTime < smoothRotationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / smoothRotationDuration;
            float newAngle = Mathf.LerpAngle(startAngle, targetAngle, t);
            rect.eulerAngles = new Vector3(0, 0, newAngle);
            yield return null;
        }

        rect.eulerAngles = new Vector3(0, 0, targetAngle);
    }
    private IEnumerator SmoothRotateToThenDelayedWin(float targetAngle) //rotate then win 
    {
        yield return SmoothRotateTo(targetAngle);
        DelayedWin();
    }
    private float CalculateAngularDistanceToTarget()
    {
        float currentAngle = transform.eulerAngles.z;
        float distance = (activeTargetAngle - currentAngle + 360f) % 360f;
        return distance;
    }
    private void FinalizeSpinResults()
    {
            //wheel naturally stopped at target due to calculated stopPower
            // Now smoothly center it on the reward before showing the delayed-win animation
            rewardResult = activeRewardResult;
            StartCoroutine(SmoothRotateToThenDelayedWin(activeTargetAngle));
            
            WheelCenterizer();
            return;
    }


// ----- Debug draw for reward angles in the editor ----- //
    private void OnDrawGizmos()
    {
        //Debug for lines
        Gizmos.matrix = transform.localToWorldMatrix;
    
        
        foreach (float angle in rewardAngles)
        {
            float rad = angle * Mathf.Deg2Rad;
            Vector3 position = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * 100;
            
            Gizmos.color = Color.green;
            Gizmos.DrawLine(Vector3.zero, position);
        }
    }

}