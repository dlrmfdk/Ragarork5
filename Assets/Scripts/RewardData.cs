using UnityEngine;

public enum RewardType
{
    Gold,       // 고정 골드 보상
    CardReward  // 카드 보상: 후보 카드 중 1장을 선택
}

[CreateAssetMenu(fileName = "RewardData", menuName = "Scriptable Objects/RewardData")]
public class RewardData : ScriptableObject
{
    public string rewardName;
    public string description;
    public Sprite icon;
    public RewardType rewardType;
    public int value; // 골드 보상의 경우 n개의 골드, 카드 보상은 value를 카드 보상 옵션 수 등에 활용할 수 있음
}
