using System;
using System.Collections;
using System.Collections.Generic;
using Forgehub.SpookyBubbles;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class IDTranslator: MonoBehaviour
{
    public SpinningScript spinningScript;
    public UIWheelSpin uIWheelSpin;
    public WheelPopulate wheelPopulate;
    public UISpinningScript uIspinningScript;
    // ----- ID to case translator ----- //
    public bool TryResolveReward(string incomingItemId, out SpinningScript.RewardType resolvedReward)
    {

        if (spinningScript.GetRewardList.TryGetValue(incomingItemId, out resolvedReward))
            return true;

        return false;  
    }

    public void UnserializedReward(string incomingItemId) //Translates ID into cases
    {
        if (!uIWheelSpin.GetIsSpinning())
            return;

        if (uIWheelSpin == null || !wheelPopulate.TryGetSlotIndex(incomingItemId, out int slotIndex))
        {
            spinningScript.ReceivedBackend = false;
            Debug.LogWarning("No populated slot for reward ID: " + incomingItemId);
            spinningScript.HandleSpinFailed();
            return;
        }

        if (TryResolveReward(incomingItemId, out SpinningScript.RewardType resolvedReward))
        {
            spinningScript.rewardType = resolvedReward;
        }

        uIspinningScript.ConfigureForcedRewardBySlot(slotIndex);
        spinningScript.ReceivedBackend = true;
        Debug.Log($"Landing slot={slotIndex} angle={uIspinningScript.GetTargetAngle()} itemId={incomingItemId}");
    }
}