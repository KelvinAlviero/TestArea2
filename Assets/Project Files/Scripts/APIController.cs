using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using Forgehub.SpookyBubbles;

public class APIController : MonoBehaviour
{
    [SerializeField] public SpinningScript SpinningScript;
    [SerializeField] public UISpinningScript uISpinningScript;
    [SerializeField] public UIWheelSpin uIWheelSpin;
    [SerializeField] public UIWheelReward uiWheelReward;
    [SerializeField] private WheelPopulate wheelPopulate;
    [SerializeField] private FreeSpinChecker freeSpinChecker;
    [SerializeField] private FlagGetter flagGetter;
    [SerializeField] private IDTranslator iDTranslator;
    public string ObtainedReward;
    public RewardSO SelectedReward;

    public void RewardObtainer()
    {
        var data = UniWebViewBridge.Call("getFlagTicketBalance", null);
        var data2 = JsonUtility.FromJson<SpinRequests.SpinWheelRewardsResponse>(data);
    }

    private void ApplySpinResponseState(SpinRequests.SpinWheelSpinResponse data)
    {
        if (uIWheelSpin == null)
            return;

        if (data?.FreeSpinUsed == true)
        {
            freeSpinChecker.FreeSpinAvailable = false;
            Debug.Log("FreeSpinDisabled");
        }
        else
        {
            freeSpinChecker.FreeSpinAvailable = true;
            Debug.Log("FreeSpinAvailableAfterSpin");
        }
    }

    public void StartSpin()
    {
        uISpinningScript.Rotate();
        UniWebViewBridge.Request(
            "spinRequest",
            new SpinRequests.SpinWheelSpinRequest
            {
                SpinWheelConfigName = "default_testing_spinwheel",
                SpinCount = 1
            },
            onSuccess: json =>
            {
                var data = JsonConvert.DeserializeObject<SpinRequests.SpinWheelSpinResponse>(json);
                Debug.Log("spin ok: " + JsonConvert.SerializeObject(data, Formatting.Indented));

                ApplySpinResponseState(data);
                Debug.Log("FreeSpinning");
                ShowWonReward(data);

            },
            onError: err =>
            {
                Debug.LogError("spin error: " + err);
                SpinningScript.HandleSpinFailed();
                
            },
            timeout: 10000);
    }

    public void StartSpinPaid()
    {
        if (flagGetter.flagAmount > 0)
        {
        uISpinningScript.Rotate();
        UniWebViewBridge.Request(
            "spinRequest",
            new SpinRequests.SpinWheelSpinRequest
            {
                SpinWheelConfigName = "default_testing_spinwheel",
                SpinCount = 1
            },
            onSuccess: json =>
            {
                var data = JsonConvert.DeserializeObject<SpinRequests.SpinWheelSpinResponse>(json);
                Debug.Log("spin ok: " + JsonConvert.SerializeObject(data, Formatting.Indented));
                Debug.Log("[APIController] Paid spin response received.");

                var cost = Mathf.Max(0, data?.TotalCost ?? 0);
                if (cost > 0)
                {
                    var flag = Mathf.Max(0, flagGetter.flagAmount - cost);
                    flagGetter.OnFlagTicketChange(flag);
                    Debug.Log("[APIController] Using Paid spin");
                    Debug.Log("Freespinstatus" + freeSpinChecker.FreeSpinAvailable);
                }
                else
                {
                    Debug.Log("[APIController] Paid spin had zero cost.");
                    Debug.Log("Freespinstatus" + freeSpinChecker.FreeSpinAvailable);
                }

                ShowWonReward(data);
            },
            onError: err =>
            {
                Debug.LogError("spin error: " + err);
                SpinningScript.HandleSpinFailed();
                Debug.Log("Freespinstatus" + freeSpinChecker.FreeSpinAvailable);
            },
            timeout: 10000);
        }
        else
        {
            uIWheelSpin.balanceError.Show();
            Debug.Log("Freespinstatus" + freeSpinChecker.FreeSpinAvailable);
        }
    }

    private void ShowWonReward(SpinRequests.SpinWheelSpinResponse data)
    {
        Debug.Log("Reward list avalible= " + uIWheelSpin.rewardList);
        var item = GetFirstItem(data);
        if (item == null)
        {
            Debug.LogWarning("Spin returned no items");
            SpinningScript.HandleSpinFailed();
            return;
        }
        var database = uIWheelSpin.rewardList ?? uIWheelSpin.rewardList;
        var matchingSO = database.Find(so => so.itemId == item.ItemId);

        if (uIWheelSpin.rewardList == null)
                {
                    
                    return;
                }

        if (matchingSO == null)
        {
            Debug.LogWarning($"No RewardSO for itemId={item.ItemId}");
            SpinningScript.HandleSpinFailed();
            return;
        }
        SelectedReward = matchingSO;
        ObtainedReward = item.ItemId;
        uiWheelReward.SetReward(matchingSO);
        iDTranslator.UnserializedReward(item.ItemId);
    }

    private SpinRequests.SpinWheelDrawItem GetFirstItem(SpinRequests.SpinWheelSpinResponse data)
    {
        if (data?.Draws == null) return null;
        foreach (var draw in data.Draws)
        {
            if (draw?.Items == null || draw.Items.Count == 0) continue;
            return draw.Items[0];
        }
        return null;
    }


    public void DebugSpin()
    {
        uISpinningScript.Rotate();
    }

    
}
