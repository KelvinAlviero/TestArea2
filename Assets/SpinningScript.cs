using System.Collections;
using System.Collections.Generic;
using Forgehub.SpookyBubbles;
using JetBrains.Annotations;
using Microsoft.Unity.VisualStudio.Editor;
using Unity.VisualScripting;
using UnityEngine;


public class SpinningScript : MonoBehaviour
{

    
    [Header("Script References")]
    public UIWheelReward UIWheelReward;
    public UIWheelSpin UIWheelSpin;
    [SerializeField] public int RewardShift = 0; //wheel adjustment
    [SerializeField] private int Angle1, Angle2, Angle3, Angle4 ,Angle5 ,Angle6 ,Angle7;
    [SerializeField] public float rotatePower ;
    [SerializeField] public float stopPower;
    [SerializeField] public Image DebugWheelPoint;
    [SerializeField] public float scalingFactor = 50f; // Physics tuning: higher = less deceleration
    [SerializeField] public float landingTuner = 0.55f; // >1 undershoots, <1 overshoots
    [SerializeField] public int rewardResult;
    [SerializeField] private bool activeForceReward;
    [SerializeField] private float activeTargetAngle;
    [SerializeField] private int activeRewardResult;
    [SerializeField] private RewardType activeRewardType;

    [SerializeField] private Rigidbody2D rbody;
    [SerializeField] private List<float> rewardAngles;
    [SerializeField] public float preSpinDuration = 3f; // seconds to spin very fast before applying forced landing
    [SerializeField] public float preSpinSpeed = 3000f; // deg/s during pre-spin
    [SerializeField] public float preSpinDamping = 5f; // small damping during pre-spin so it stays fast
    [SerializeField] public float switchAngularVelocity = 2000f; // angular velocity applied when switching to landing phase
    private Coroutine preSpinCoroutine;
    [SerializeField] public float smoothRotationDuration = 0.5f; // Duration for smooth rotation to center
    [SerializeField] private Coroutine lightAnimationCoroutine;
    [SerializeField] private int DelayedWinTime = 1000;
    [SerializeField] private float ChangeDelay = 3f;
    

    int inRotate;
    
    private void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
        ConfigureForcedReward(rewardType);
        // Initialize reward angles
        rewardAngles = new List<float>();
    }

    float t;

    public enum RewardType //List for rewards
    {
        Random, //Pick this for random reward
        Coin10,
        Placeholder1,
        Placeholder2,
        Coin20,
        Placeholder3,
        Prebooster
    }

    public RewardType rewardType;

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
                ResolveReward();
                inRotate = 0;
                t = 0f;
            }
        }
    }

    
    public void Rotete()
    {
        if (inRotate == 0)
        {
            activeRewardType = rewardType;
            ConfigureForcedReward(activeRewardType);

            // Start pre-spin coroutine which will spin fast, then switch to landing phase
            if (preSpinCoroutine != null)
                StopCoroutine(preSpinCoroutine);

            preSpinCoroutine = StartCoroutine(PreSpinThenSwitch());
            inRotate = 1;
        }
    }

    private IEnumerator PreSpinThenSwitch()
    {
        float previousStopPower = stopPower;
        stopPower = preSpinDamping;
        rbody.angularVelocity = preSpinSpeed;

        float elapsed = 0f;
        while (elapsed < preSpinDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }


        if (activeForceReward)
        {
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
        else
        {
            // Random mode: apply a random starting velocity and damping
            rbody.angularVelocity = Random.Range(6000f, 10000f);
            stopPower = Random.Range(500f, 900f);
        }

        // cleanup coroutine handle
        preSpinCoroutine = null;
        yield break;
    }

    private float CalculateAngularDistanceToTarget()
    {
        float currentAngle = transform.eulerAngles.z;
        float distance = (activeTargetAngle - currentAngle + 360f) % 360f;
        return distance;
    }


    private void ConfigureForcedReward(RewardType selectedReward) // Forced reward section
    {
        activeForceReward = selectedReward != RewardType.Random;
        if (!activeForceReward)
        {
            activeTargetAngle = 0f;
            activeRewardResult = 0;
            return;
        }

        switch (selectedReward) //This is connected to RewardType
        {
            case RewardType.Coin10:
                activeTargetAngle = 30f + RewardShift;
                activeRewardResult = 6;
                break;
            case RewardType.Placeholder1:
                activeTargetAngle = 90f + RewardShift;
                activeRewardResult = 1;
                break;
            case RewardType.Placeholder2:
                activeTargetAngle = 150f + RewardShift;
                activeRewardResult = 2;
                break;
            case RewardType.Coin20:
                activeTargetAngle = 210f + RewardShift;
                activeRewardResult = 3;
                break;
            case RewardType.Placeholder3:
                activeTargetAngle = 270f + RewardShift;
                activeRewardResult = 4;
                break;
            case RewardType.Prebooster:
                activeTargetAngle = 330f + RewardShift;
                activeRewardResult = 5;
                break;
            default:
                activeForceReward = false;
                activeTargetAngle = 0f;
                activeRewardResult = 0;
                break;
        }
    }

    public void SetDebugReward(RewardType selectedReward)
    {
        rewardType = selectedReward;
        if (inRotate == 0)
        {
            ConfigureForcedReward(rewardType);
        }
    }

    private void ResolveReward()
    {
        if (activeForceReward)
        {
            // In forced mode, wheel naturally stopped at target due to calculated stopPower
            // Now smoothly center it on the reward before showing the delayed-win animation
            Debug.Log(GetRewardName(activeRewardResult));
            rewardResult = activeRewardResult;
            StartCoroutine(SmoothRotateToThenDelayedWin(activeTargetAngle));
            activeForceReward = false;
            return;
        }

        ResolveRandomReward();
    }

    private void ResolveRandomReward()
    {
        float rot = transform.eulerAngles.z;
        float normalizedRot = (rot - RewardShift + 360f) % 360f;
        float targetAngle;

        if (normalizedRot >= 0f && normalizedRot < 60f)
        {
            targetAngle = 30f + RewardShift;
            ApplyReward(6, targetAngle, "10 Coins");
        }
        else if (normalizedRot < 120f)
        {
            targetAngle = 90f + RewardShift;
            ApplyReward(1, targetAngle, "Placeholder");
        }
        else if (normalizedRot < 180f)
        {
            targetAngle = 150f + RewardShift;
            ApplyReward(2, targetAngle, "Placeholder");
        }
        else if (normalizedRot < 240f)
        {
            targetAngle = 210f + RewardShift;
            ApplyReward(3, targetAngle, "20 coins");
        }
        else if (normalizedRot < 300f)
        {
            targetAngle = 270f + RewardShift;
            ApplyReward(4, targetAngle, "Placeholder");
        }
        else
        {
            targetAngle = 330f + RewardShift;
            ApplyReward(5, targetAngle, "Prebooster");
        }
    }

    private void ApplyReward(int rewardId, float targetAngle, string debugText)
    {
        Debug.Log(debugText);
        rewardResult = rewardId;
        StartCoroutine(SmoothRotateToThenDelayedWin(targetAngle));
    }

    private IEnumerator SmoothRotateToThenDelayedWin(float targetAngle)
    {
        yield return SmoothRotateTo(targetAngle);
        DelayedWin();
    }

    private string GetRewardName(int rewardId)
    {
        switch (rewardId)
        {
            case 6: return "10 Coins";
            case 1: return "Placeholder";
            case 2: return "Placeholder";  
            case 3: return "20 coins";
            case 4: return "Placeholder";
            case 5: return "PreBooster";
            default: return "Unknown Reward";
        }
    }

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


    public void Win(int Score)
    {
        print(Score);
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

// ----- Debug draw for reward angles in the editor 
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