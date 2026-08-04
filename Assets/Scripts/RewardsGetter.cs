using System;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardsGetter : MonoBehaviour
{
    public Image rewardImage;
    public TMP_Text rewardText;
    public void SetReward(RewardSO reward)
    {
        rewardImage.sprite = reward.sprite;
        rewardText.text = reward.amount.ToString() + "x";
    }
}
