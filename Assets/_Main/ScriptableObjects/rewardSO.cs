using UnityEngine;


[CreateAssetMenu(fileName = "rewardSO", menuName = "rewardSO")]
public class RewardSO : ScriptableObject
{
    public Sprite sprite;
    public string itemId;
    public string itemName;
	public int amount;
  
}
