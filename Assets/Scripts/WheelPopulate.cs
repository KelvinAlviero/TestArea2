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
    [SerializeField] private APIController aPIController;
    [SerializeField] private SpinningScript spinningScript;
    [SerializeField] private UISpinningScript uISpinningScript;

    [SerializeField] private FreeSpinChecker freeSpinChecker;
    public RewardsGetter rewardsGetter;
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
                var data = JsonConvert.DeserializeObject<SpinRequests.SpinWheelRewardsResponse>(json);

                PopulateRewards(data);
                freeSpinChecker.FreeSpinAvailable = data.FreeSpinAvailable;
                freeSpinChecker.FreeSpinCheck();
                Debug.Log(freeSpinChecker.FreeSpinAvailable);
                UniWebViewBridge.Send("applicationReady", null);
                // Host session is ready — refresh balance again (fixes account switch / early Call).
                flagGetter.GetFlagTicket();
                freeSpinChecker.CheckPaidSpinOnFree();
            },
            onError: err =>
            {
                spinningScript.ShowErrorPanel();
                Debug.LogError("getRewards error: " + err);
                UniWebViewBridge.Send("applicationReady", null);
                flagGetter.GetFlagTicket();
            },
            timeout: 10000);
    }



    public bool TryGetSlotIndex(string itemId, out int slotIndex)
    {
        return rewardSlotByItemId.TryGetValue(itemId, out slotIndex);
    }

    private void PopulateRewards(SpinRequests.SpinWheelRewardsResponse data)
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
                if (slotIndex >= uIWheelSpin.slot.Count) return;

                Debug.Log($"[UIWheelSpin] Processing reward item: ItemId={item?.ItemId}, Name={item?.Name}, Amount={item?.Amount}");

                var matchingSO = uIWheelSpin.rewardList.Find(so => so != null && so.itemId == item.ItemId);
                if (matchingSO == null)
                {
                    Debug.LogWarning($"[UIWheelSpin] No RewardSO found for itemId={item?.ItemId}. Available IDs: {string.Join(", ", uIWheelSpin.rewardList.Where(so => so != null).Select(so => so.itemId).ToArray())}");
                    continue;
                }

                Debug.Log($"[UIWheelSpin] Matched RewardSO: itemId={matchingSO.itemId}, name={matchingSO.itemName}, slot={slotIndex}");

                rewardSlotByItemId[item.ItemId] = slotIndex;

                var rewardUI = Instantiate(rewardsGetter, Vector3.zero, Quaternion.identity, uIWheelSpin.slot[slotIndex].transform);
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

        if (spinningScript != null)
            spinningScript.InvalidateRewardUprightCache();
    }

    
    }


