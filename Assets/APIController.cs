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
    [SerializeField] public UIWheelSpin uIWheelSpin;
    [SerializeField] public UIWheelReward uiWheelReward;
    [SerializeField] private WheelPopulate wheelPopulate;
    [SerializeField] private FreeSpinChecker freeSpinChecker;
    public string email = "aaa@gmail.com";
    public string password = "qwerty123";
    public string returnSecureToken = "true";
    public string JWTToken;
    public string datadebug;
    public TMP_Text MailSender;
    public TMP_Text DebugText_SpinAmount;
    public TMP_Text DebugText_ItemList;
    public TMP_Text DebugText_Status;
    public TMP_Text JSON_Body;
    public TMP_Text JSON_Raw;
    private string PulledJWT;
    public TMP_Text PulledJWTText;
    public TMP_Text Debug1;
    public TMP_Text Debug2;

    public string ObtainedReward;
    public string UnserializeditemId;
    public int spin_count = 1;
    [SerializeField] private TMP_Text JWT_Text;
    [SerializeField] private TMP_Text JWT_Translated;
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
            uIWheelSpin.FreeSpinAvailable = false;
            Debug.Log("FreeSpinDisabled");
        }
        else
        {
            uIWheelSpin.FreeSpinAvailable = true;
            Debug.Log("FreeSpinAvailableAfterSpin");
        }
    }

    public void StartSpin()
    {
        SpinningScript.Rotate();
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
        if (uIWheelSpin.flagAmount > 0)
        {
        SpinningScript.Rotate();
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
                    var flag = Mathf.Max(0, uIWheelSpin.flagAmount - cost);
                    uIWheelSpin.OnFlagTicketChange(flag);
                    Debug.Log("[APIController] Using Paid spin");
                    Debug.Log("Freespinstatus" + uIWheelSpin.FreeSpinAvailable);
                }
                else
                {
                    Debug.Log("[APIController] Paid spin had zero cost.");
                    Debug.Log("Freespinstatus" + uIWheelSpin.FreeSpinAvailable);
                }

                ShowWonReward(data);
            },
            onError: err =>
            {
                Debug.LogError("spin error: " + err);
                SpinningScript.HandleSpinFailed();
                Debug.Log("Freespinstatus" + uIWheelSpin.FreeSpinAvailable);
            },
            timeout: 10000);
        }
        else
        {
            uIWheelSpin.balanceError.Show();
            Debug.Log("Freespinstatus" + uIWheelSpin.FreeSpinAvailable);
        }
    }

    private void ShowWonReward(SpinWheelSpinResponse data)
    {
        var item = GetFirstItem(data);
        if (item == null)
        {
            Debug.LogWarning("Spin returned no items");
            SpinningScript.HandleSpinFailed();
            return;
        }
        var database = uIWheelSpin.rewardDatabase ?? uIWheelSpin.rewardDatabase;
        var matchingSO = database.Find(so => so.itemId == item.ItemId);
        if (matchingSO == null)
        {
            Debug.LogWarning($"No RewardSO for itemId={item.ItemId}");
            SpinningScript.HandleSpinFailed();
            return;
        }
        SelectedReward = matchingSO;
        ObtainedReward = item.ItemId;
        uiWheelReward.SetReward(matchingSO);
        SpinningScript.UnserializedReward(item.ItemId);
    }

    private static SpinWheelDrawItem GetFirstItem(SpinWheelSpinResponse data)
    {
        if (data?.Draws == null) return null;
        foreach (var draw in data.Draws)
        {
            if (draw?.Items == null || draw.Items.Count == 0) continue;
            return draw.Items[0];
        }
        return null;
    }
}
