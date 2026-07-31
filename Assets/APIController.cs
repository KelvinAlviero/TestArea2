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
        public string SpinWheelConfigName { get; set; }

        [JsonProperty("spin_count")]
        public int SpinCount { get; set; }
    }

    [Serializable]
    public class SpinWheelSpinResponse
    {
        public string Currency { get; set; }

        public List<SpinWheelDraw> Draws { get; set; }

        [JsonProperty("free_spin_used")]
        public bool FreeSpinUsed { get; set; }

        public string Name { get; set; }

        [JsonProperty("spin_count")]
        public int SpinCount { get; set; }

        [JsonProperty("total_cost")]
        public int TotalCost { get; set; }
    }

    [Serializable]
    public class SpinWheelDraw
    {
        [JsonProperty("package_id")]
        public string PackageId { get; set; }

        public List<SpinWheelDrawItem> Items { get; set; }
    }

    [Serializable]
    public class SpinWheelDrawItem
    {
        public int Amount { get; set; }

        public string ItemId { get; set; }

        public string ItemType { get; set; }

        public string LogId { get; set; }

        public string Name { get; set; }
    }

    [Serializable]
    public class SpinWheelRewardsResponse
    {
        public string Currency { get; set; }

        [JsonProperty("free_spin_available")]
        public bool FreeSpinAvailable { get; set; }

        public string Name { get; set; }

        public int Price { get; set; }

        public List<SpinWheelRewardPackageResult> Result { get; set; }

        public string Type { get; set; }
    }

    [Serializable]
    public class SpinWheelRewardPackageResult
    {
        [JsonProperty("package_id")]
        public string PackageId { get; set; }
        public List<SpinWheelRewardsItemData> Items { get; set; }
    }
    [Serializable]
    public class SpinWheelRewardsItemData
    {
        public int Amount { get; set; }
        [JsonProperty("item_id")]
        public string ItemId { get; set; }
        [JsonProperty("item_type")]
        public string ItemType { get; set; }
        public string Name { get; set; }
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

    //     public IEnumerator StartSpin()
    //     {
    //         var data = new SpinRequest
    //         {
    //             SpinWheelConfigName = "default_testing_spinwheel",
    //             SpinCount = spin_count
    //         };

    //         string json = JsonUtility.ToJson(data);

    //         if (JSON_Body != null)
    //             JSON_Body.text = json;

    //         using UnityWebRequest request = new UnityWebRequest(URL_StartSpin, "POST");
    //         byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
    //         request.uploadHandler = new UploadHandlerRaw(bodyRaw);
    //         request.downloadHandler = new DownloadHandlerBuffer();
    //         request.SetRequestHeader("Content-Type", "application/json");

    //         if (!string.IsNullOrEmpty(JWTToken))
    //         {
    //             request.SetRequestHeader("Authorization", "Bearer " + JWTToken);
    //         }
    //         else
    //         {
    //             yield return null;
    //         }

    //         string rawJson = request.downloadHandler?.text ?? "";

    //         if (JSON_Raw != null)
    //             JSON_Raw.text = string.IsNullOrEmpty(rawJson) ? "<empty response>" : rawJson;

    //         if (DebugText_Status != null)
    //         {
    //             DebugText_Status.text = "Result: " + request.result + "\n" +
    //                                     "Code: " + request.responseCode + "\n" +
    //                                     "Error: " + request.error + "\n";
    //         }

    //             if (request.result == UnityWebRequest.Result.Success)
    //         {
    //             Debug.Log("Request success");
    //             Debug.Log(rawJson);
    //         }
    //             else
    //         {
    //             Debug.LogError("Request failed: " + request.responseCode);
    //             Debug.LogError(rawJson);
    //             JSON_Raw.text = rawJson;
    //         }
    //     }
    // }    

    public void RewardObtainer()
    {
        var data = UniWebViewBridge.Call("getFlagTicketBalance", null);
        var data2 = JsonUtility.FromJson<SpinWheelRewardsResponse>(data);
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

                if (data?.FreeSpinUsed == true)
                {
                    uIWheelSpin.FreeSpinAvailable = false;
                }
                else
                {
                    var cost = Mathf.Max(0, data?.TotalCost ?? 0);
                    if (cost > 0)
                    {
                        var flag = Mathf.Max(0, uIWheelSpin.flagAmount - cost);
                        uIWheelSpin.OnFlagTicketChange(flag);
                        Debug.Log("Using Paid spin");
                    }
                }

                if (uIWheelSpin != null)
                {
                    uIWheelSpin.FreeSpinCheck();
                }

                ShowWonReward(data);
            },
            onError: err =>
            {
                Debug.LogError("spin error: " + err);
                SpinningScript.HandleSpinFailed();
            },
            timeout: 10000);
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

//PUT OTHER CODE BLOCK UNDER ME PLEAAAASEEEE


// More spins code, don't use please 


//         SpinResponse response = JsonUtility.FromJson<SpinResponse>(rawJson);

//         if (response != null &&
//         response.draws != null &&
//         response.draws.Count > 0)
//         {
//             List<string> queuedItemIds = new List<string>();
//             SpinItem firstItem = null;

//             foreach (var draw in response.draws)
//             {
//                 if (draw == null || draw.items == null)
//                     continue;

//                 foreach (var item in draw.items)
//                 {
//                     if (item == null || string.IsNullOrEmpty(item.itemId))
//                         continue;

//                     queuedItemIds.Add(item.itemId);

//                     if (firstItem == null)
//                         firstItem = item;
//                 }
//             }

//             if (queuedItemIds.Count > 0 && firstItem != null)
//             {
//                 if (DebugText_SpinAmount != null)
//                     DebugText_SpinAmount.text = "Reward spun " + queuedItemIds.Count + " times";

//                 UnserializeditemId = firstItem.itemId;
//                 UnserializedItems();

//                 if (SpinningScript != null)
//                 {
//                     SpinningScript.ReceivedBackend = true;
//                     SpinningScript.QueueRewards(queuedItemIds);
//                     SpinningScript.StartNextQueuedSpin();
//                 }
//             }
//             else
//             {
//                 if (DebugText_SpinAmount != null)
//                     DebugText_SpinAmount.text = "No rewards";
//             }
//         }
//         else
//         {
//             if (DebugText_SpinAmount != null)
//                 DebugText_SpinAmount.text = "No rewards";
//         }
//     }


// }
// public void UnserializedItems()
// {
//     ObtainedReward = UnserializeditemId;
//     // Debug.Log("ObtainedReward" + ObtainedReward);
// }
//      public IEnumerator SignIn()
// {
//     var data = new
//     {
//         email = email,
//         password = password,
//         returnSecureToken = returnSecureToken
//     };
//     string json = JsonUtility.ToJson(data);

//     using UnityWebRequest request = new UnityWebRequest(URL_GetUser, "POST");
//     byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
//     request.uploadHandler = new UploadHandlerRaw(bodyRaw);
//     request.downloadHandler = new DownloadHandlerBuffer();
//     request.SetRequestHeader("Content-Type", "application/json");

//     yield return request.SendWebRequest();

//     if (request.result == UnityWebRequest.Result.ConnectionError ||
//         request.result == UnityWebRequest.Result.ProtocolError)
//     {
//         Debug.LogError(request.error);
//         Debug.LogError(request.downloadHandler.text);
//     }
//     else
//     {
//         Debug.Log(request.downloadHandler.text);
//     }
// }    

// public IEnumerator RecieveSpin()
// {
//     using (UnityWebRequest request = UnityWebRequest.Get(URL_RecieveReward))
//     {
//         yield return request.SendWebRequest();

//         if (request.result == UnityWebRequest.Result.ConnectionError)
//         Debug.Log(request.error);

//         else
//         {
//             Debug.Log("ReceiveSpin: Request success");

//         }
//     }
// }

// ----- Unused code ----- //
