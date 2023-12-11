using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Chance Card", menuName = "Monopoly/Cards/Chance")]
public class SCR_ChanceCard : ScriptableObject
{
    public string textOnCard; // 카드에 표시될 텍스트 또는 설명
    public int rewardMoney; // 카드를 통해 획득할 수 있는 돈의 양
    public int penalityMoney; // 카드를 통해 지불해야 하는 돈의 양
    public int moveToBoardIndex = -1; // 플레이어가 이동해야 하는 보드의 인덱스 (-1은 아무 작용 없음을 의미)
    public bool payToPlayer;
    [Header("MoveToLocations")]
    public bool nextRailroad;
    public bool nextUtility;
    public int moveStepsBackwards;
    [Header("Jail Content")]
    public bool goToJail; // 플레이어가 감옥에 가야하는지의 여부
    public bool jailFreeCard; // 이 카드가 감옥에서 벗어날 수 있는 '무료 석방 카드'인지의 여부
    [Header("Street Repairs")]
    public bool streetRepairs; // 거리 수리와 관련된 비용을 지불해야 하는지의 여부
    public int streetRepairsHousePrice = 40;
    public int streetRepairsHotelPrice = 115;
}
