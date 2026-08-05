using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using Forgehub.SpookyBubbles;

public static class SpinRequests
{
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

    
}