using System;
using System.Collections;
using System.Collections.Generic;
using Forgehub.SpookyBubbles;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UISpinningScript : MonoBehaviour
{
    [Header("Script References")]
    public UIWheelReward uIWheelReward;
    public UIWheelSpin uIWheelSpin;
    public APIController APIController;
    public WheelPopulate wheelPopulate;
    public SpinningScript spinningScript;
    public Timer timer;
    private int Reward1, Reward2, Reward3, Reward4, Reward5, Reward6, Reward7, Reward8;
    [SerializeField] private float activeTargetAngle;
    [SerializeField] private int activeRewardResult;
    public Rigidbody2D rbody;
    



    [Header("WheelSpin config")]
    [SerializeField] private float landingTuner = 1f; // , <1 Increase power (Lower Resistance),  >1 Decrease power (Higher Resitance)
    [SerializeField] private float minLandingStopPower = 2600; // minimum landing deceleration
    [SerializeField] private float maxLandingStopPower = 2600; // maximum landing deceleration
    [SerializeField] private float preSpinDuration = 2f; // seconds to spin very fast before applying forced landing
    [SerializeField] private float preSpinSpeed = 3000f; // deg/s during pre-spin
    [SerializeField] private float preSpinDamping = 0f; // small damping during pre-spin so it stays fast
    
    [SerializeField] private float switchAngularVelocity = 1500f; // angular velocity applied when switching to landing phase
    [SerializeField] private float desiredLandingMinDecel = 2500f; // desired min computed deceleration
    [SerializeField] private float desiredLandingMaxDecel = 2600f; // desired max computed deceleration
    [SerializeField] private float landingAdjustmentWait = 0.1f; // seconds to wait before retrying decel check
    [SerializeField] private int landingAdjustmentAttempts = 1000; // max retries for adjusting landing
    [SerializeField] private float rewardWaitTimeout = 10f; // keep spinning until reward or this timeout
    [SerializeField] public float stopPower;
    [SerializeField] private float DelayedWinTime = 3f; //Delays popup
    [SerializeField] private float DelayedSpinTime = 0.5f;
    [SerializeField] public float smoothRotationDuration = 0.5f;
    private Coroutine preSpinCoroutine;

    public void ChangeTargetAngle(float value)
    {
        activeTargetAngle = value;
    }

    public Rigidbody2D GetRbody()
    {
        return rbody;
    }

    public float GetTargetAngle()
    {
        return activeTargetAngle;
    }

    public void Rotate() //used by Wheel button, don't delete pls
    {
        Rotate(spinningScript.rewardType);
    }

    public Coroutine GetSpinCoroutine() //used by Wheel button, don't delete pls
    {
        return preSpinCoroutine;
    }

    public void SetSpinCoroutine(Coroutine value) //used by Wheel button, don't delete pls
    {
        preSpinCoroutine = value;
    }

    public void SetStopPower(float value) //used by Wheel button, don't delete pls
    {
        stopPower = value;
    }


    private void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (rbody.angularVelocity > 0f) 
        {
            float currentStopPower = stopPower;

            rbody.angularVelocity -= currentStopPower * Time.deltaTime;
            rbody.angularVelocity = Mathf.Clamp(rbody.angularVelocity, 0f, 1440f);
        }

        if (rbody.angularVelocity <= 0f && uIWheelSpin.GetIsSpinning())
        {
            rbody.angularVelocity = 0f;
            timer.SetSpinEndTimer(Time.deltaTime) ;
            if (timer.GetSpinEndTimer() >= DelayedSpinTime)
            {
                FinalizeSpinResults();
                uIWheelSpin.SetIsSpinning(false);
                timer.SetSpinEndTimer(0f);
            }
        }
        spinningScript.BackendReward = APIController.ObtainedReward;
    }

    public void Rotate(SpinningScript.RewardType rewardToUse)
    {
        if (!uIWheelSpin.GetIsSpinning())
        {
            spinningScript.ReceivedBackend = false;
            spinningScript.rewardType = rewardToUse;

            // Start pre-spin; landing target is set when API reward arrives via UnserializedReward
            if (preSpinCoroutine != null)
                StopCoroutine(preSpinCoroutine);

            preSpinCoroutine = StartCoroutine(PreSpinThenSwitch());
            uIWheelSpin.SetIsSpinning(true);
        }
    }


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
        while (!spinningScript.ReceivedBackend && waited < rewardWaitTimeout)
        {
            if (rbody.angularVelocity < switchAngularVelocity)
                rbody.angularVelocity = switchAngularVelocity;

            waited += Time.deltaTime;
            yield return null;
        }

        if (!spinningScript.ReceivedBackend)
        {
            spinningScript.HandleSpinFailed();
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

        Debug.Log($"Landing switch: cur={transform.eulerAngles.z:F1} target={GetTargetAngle():F1} remaining={CalculateAngularDistanceToTarget():F1}");
        rbody.angularVelocity = switchAngularVelocity;

        int attempt = 0;
        float angularDistance;
        float computedDecel;
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

    private void WheelCenterizer() //Center reward back on spin end, depends on ApplyReward
    {
        float rot = transform.eulerAngles.z;
        float normalizedRot = (rot + 360f) % 360f;
        float targetAngle;

        if (normalizedRot >= 0f && normalizedRot < 45f)
        {
            targetAngle = Reward1;
            spinningScript.ApplyReward(1, targetAngle, "UltimateBooster");
        }
        else if (normalizedRot < 90f)
        {
            targetAngle = Reward2;
            spinningScript.ApplyReward(2, targetAngle, "Gems");
        }
        else if (normalizedRot < 135f)
        {
            targetAngle = Reward3;
            spinningScript.ApplyReward(3, targetAngle, "DashImmune");
        }
        else if (normalizedRot < 180f)
        {
            targetAngle = Reward4;
            spinningScript.ApplyReward(4, targetAngle, "Magnet");
        }
        else if (normalizedRot < 225f)
        {
            targetAngle = Reward5;
            spinningScript.ApplyReward(5, targetAngle, "Shield");
        }
        else if (normalizedRot < 270f)
        {
            targetAngle = Reward6;
            spinningScript.ApplyReward(6, targetAngle, "MagnetImmune");
        }
        else if (normalizedRot < 315f)
        {
            targetAngle = Reward7;
            spinningScript.ApplyReward(7, targetAngle, "Coins");
        }
        else
        {
            targetAngle = 340f;
            spinningScript.ApplyReward(8, targetAngle, "Speed");
        }
    }


    // ----- Afterspin stuffs ----- //
    public IEnumerator DelayedWin()
    {
        // Debug.Log("DelayedWin called");
        yield return new WaitForSeconds(DelayedWinTime);
        uIWheelReward.PlayShowAnimation();
    }

    public IEnumerator SmoothRotateTo(float targetAngle)
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

    public IEnumerator SmoothRotateToThenDelayedWin(float targetAngle) //rotate then win 
    {

        yield return SmoothRotateTo(targetAngle);
        StartCoroutine(DelayedWin());
    }

    public float CalculateAngularDistanceToTarget()
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

    public void FinalizeSpinResults()
    {
        // If we have a forced/backend-resolved reward, smoothly center to that target.
        // Otherwise determine the sector and center to that.
        if (spinningScript.rewardType != SpinningScript.RewardType.Normal || spinningScript.ReceivedBackend)
        {
            spinningScript.rewardResult = activeRewardResult;
            StartCoroutine(SmoothRotateToThenDelayedWin(activeTargetAngle));
        }
        else
        {
            WheelCenterizer();
        }
        return;
    }

    // Put item in slots
    public void ConfigureForcedRewardBySlot(int slotIndex)
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
}