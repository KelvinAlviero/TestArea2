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
    public SpinningScript SpinningScript;
    public UIWheelSpin uIWheelSpin;
    public UIWheelReward uiWheelReward;
    public string URL_GetUser = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=AIzaSyB-VcA8mR2rOlVxlxObaZYIY27yIYFdb70";
    public string URL_StartSpin = "https://mh-dev.dreamforgecreation.com/api/v1/spinwheel/spin";
    // public string URL_GetGems = ""
    // public string URL_RecieveReward = "https://mh-dev.dreamforgecreation.com/api/v1/spinwheel/rewards?spinwheel_config_name=default_testing_spinwheel";
    public string URL_SendRewards = "https://mh-dev.dreamforgecreation.com/api/v1/mailbox/test";
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

    [Serializable]
    public class SpinWheelSpinRequest
    {
        [JsonProperty("spinwheel_config_name")]
        public string SpinWheelConfigName;

        [JsonProperty("spin_count")]
        public int SpinCount;
    }

    [Serializable]
    public class SpinWheelSpinResponse
    {
        public string Currency;

        public List<SpinWheelDraw> Draws;

        [JsonProperty("free_spin_used")]
        public bool FreeSpinUsed;

        public string Name;

        [JsonProperty("spin_count")]
        public int SpinCount;

        [JsonProperty("total_cost")]
        public int TotalCost;
    }

    [Serializable]
    public class SpinWheelDraw
    {
        [JsonProperty("package_id")]
        public string PackageId;

        public List<SpinWheelDrawItem> Items;
    }

    [Serializable]
    public class SpinWheelDrawItem
    {
        public int Amount;

        public string ItemId;

        public string ItemType;

        public string LogId;

        public string Name;
    }

    [Serializable]
    public class SpinWheelRewardsResponse
    {
        public string Currency;

        [JsonProperty("free_spin_available")]
        public bool FreeSpinAvailable;

        public string Name;

        public int Price;

        public List<SpinWheelRewardPackageResult> Result;

        public string Type;
    }

    [Serializable]
    public class SpinWheelRewardPackageResult
    {
        [JsonProperty("package_id")]
        public string PackageId;

        public List<SpinWheelRewardsItemData> Items;
    }

    [Serializable]
    public class SpinWheelRewardsItemData
    {
        public int Amount;

        [JsonProperty("item_id")]
        public string ItemId;

        [JsonProperty("item_type")]
        public string ItemType;

        public string Name;
    }

    public IEnumerator SendReward()
    {
        if (!string.IsNullOrEmpty(PulledJWT))
        {
            JWTToken = PulledJWT;
        }

        using UnityWebRequest request = UnityWebRequest.Get(URL_SendRewards);
        request.SetRequestHeader("Content-Type", "application/json");


        if (!string.IsNullOrEmpty(JWTToken))
        {
            request.SetRequestHeader("Authorization", "Bearer " + JWTToken);
        }

        yield return request.SendWebRequest();
        string rawJson = request.downloadHandler?.text ?? "";

        if (JSON_Raw != null)
            JSON_Raw.text = string.IsNullOrEmpty(rawJson) ? "<empty response>" : rawJson;

        if (DebugText_Status != null)
        {
            DebugText_Status.text = "GET Result: " + request.result + "\n" +
                                    "Code: " + request.responseCode + "\n" +
                                    "Error: " + request.error + "\n";
        }

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("GET request success");
            Debug.Log(rawJson);
            MailSender.text = "Success" + rawJson;
        }
        else
        {
            Debug.LogError("GET request failed: " + request.responseCode);
            Debug.LogError(rawJson);
            MailSender.text = "Failed" + rawJson;
        }
    }

    public void RewardObtainer()
    {
        var data = UniWebViewBridge.Call("getFlagTicketBalance", null);
        var data2 = JsonUtility.FromJson<SpinWheelRewardsResponse>(data);
    }

    private void ApplySpinResponseState(SpinWheelSpinResponse data)
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
            new SpinWheelSpinRequest
            {
                SpinWheelConfigName = "default_testing_spinwheel",
                SpinCount = 1
            },
            onSuccess: json =>
            {
                var data = JsonConvert.DeserializeObject<SpinWheelSpinResponse>(json);
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
            new SpinWheelSpinRequest
            {
                SpinWheelConfigName = "default_testing_spinwheel",
                SpinCount = 1
            },
            onSuccess: json =>
            {
                var data = JsonConvert.DeserializeObject<SpinWheelSpinResponse>(json);
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
