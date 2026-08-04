using System;
using System.Collections;
using System.Collections.Generic;
using Forgehub.SpookyBubbles;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UISpinningScript : MonoBehaviour
{
    [Header("Script References")]
    public UIWheelReward uIWheelReward;
    public UIWheelSpin uIWheelSpin;
    public APIController APIController;
    public WheelPopulate wheelPopulate;
    public SpinningScript spinningScript;
    private int Reward1, Reward2, Reward3, Reward4 ,Reward5 ,Reward6 ,Reward7, Reward8;


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

     public void ShowErrorPanel()
    {
         if (errorPanel != null)
            errorPanel.Show("webview/error");
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

}