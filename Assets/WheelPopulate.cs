using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using TMPro;
using Extras;
using I2.Loc;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


public class WheelPopulate : MonoBehaviour
{
    [Header("Script References")]
    [SerializeField] private APIController APIController;
    [SerializeField] private SpinningScript SpinningScript;
    [SerializeField] private WheelPopulate wheelPopulate;
    [SerializeField] private FreeSpinChecker freeSpinChecker;
    [SerializeField] public UIWheelSpin uIWheelSpin;
    [SerializeField] private Timer timer;
    [SerializeField] private FlagGetter flagGetter;

    private readonly Dictionary<string, int> rewardSlotByItemId = new Dictionary<string, int>();


    public void GetReward()
    {
        UniWebViewBridge.Request("getSpinWheelRewards",
            new { spinwheel_config_name = "default_testing_spinwheel" },
            onSuccess: json =>
            {
                var data = JsonConvert.DeserializeObject<APIController.SpinWheelRewardsResponse>(json);

                PopulateRewards(data);
                freeSpinChecker.FreeSpinAvailable = data.FreeSpinAvailable;
                freeSpinChecker.FreeSpinCheck();
                Debug.Log(freeSpinChecker.FreeSpinAvailable);
                UniWebViewBridge.Send("applicationReady", null);
                // Host session is ready — refresh balance again (fixes account switch / early Call).
                FlagGetter.GetFlagTicket();
                CheckPaidSpinOnFree();
            },
            onError: err =>
            {
                SpinningScript.ShowErrorPanel();
                Debug.LogError("getRewards error: " + err);
                UniWebViewBridge.Send("applicationReady", null);
                GetFlagTicket();
            },
            timeout: 10000);
    }

    public bool TryGetSlotIndex(string itemId, out int slotIndex)
    {
        return rewardSlotByItemId.TryGetValue(itemId, out slotIndex);
    }

    private void PopulateRewards(APIController.SpinWheelRewardsResponse data)
    {
        if (data?.Result == null)
        {
            Debug.LogWarning("No reward result payload.");
            return;
        }

        rewardSlotByItemId.Clear();
        int slotIndex = 0;
        foreach (var package in data.Result)
        {
            if (package?.Items == null) continue;

            foreach (var item in package.Items)
            {
                if (slotIndex >= slot.Count) return;

                Debug.Log($"[UIWheelSpin] Processing reward item: ItemId={item?.ItemId}, Name={item?.Name}, Amount={item?.Amount}");

                var matchingSO = rewardDatabase.Find(so => so != null && so.itemId == item.ItemId);
                if (matchingSO == null)
                {
                    Debug.LogWarning($"[UIWheelSpin] No RewardSO found for itemId={item?.ItemId}. Available IDs: {string.Join(", ", rewardDatabase.Where(so => so != null).Select(so => so.itemId).ToArray())}");
                    continue;
                }

                Debug.Log($"[UIWheelSpin] Matched RewardSO: itemId={matchingSO.itemId}, name={matchingSO.itemName}, slot={slotIndex}");

                rewardSlotByItemId[item.ItemId] = slotIndex;

                var rewardUI = Instantiate(reward, Vector3.zero, Quaternion.identity, slot[slotIndex].transform);
                rewardUI.SetReward(matchingSO);

                var rect = rewardUI.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.localRotation = Quaternion.identity;
                    rect.localScale = Vector3.one;
                }
                slotIndex++;
            }
        }

        if (SpinningScript != null)
            SpinningScript.InvalidateRewardUprightCache();
    }
}

