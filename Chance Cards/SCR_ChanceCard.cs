using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Chance Card", menuName = "Monopoly/Cards/Chance")]
public class SCR_ChanceCard : ScriptableObject
{
    public string textOnCard; // ī�忡 ǥ�õ� �ؽ�Ʈ �Ǵ� ����
    public int rewardMoney; // ī�带 ���� ȹ���� �� �ִ� ���� ��
    public int penalityMoney; // ī�带 ���� �����ؾ� �ϴ� ���� ��
    public int moveToBoardIndex = -1; // �÷��̾ �̵��ؾ� �ϴ� ������ �ε��� (-1�� �ƹ� �ۿ� ������ �ǹ�)
    public bool payToPlayer;
    [Header("MoveToLocations")]
    public bool nextRailroad;
    public bool nextUtility;
    public int moveStepsBackwards;
    [Header("Jail Content")]
    public bool goToJail; // �÷��̾ ������ �����ϴ����� ����
    public bool jailFreeCard; // �� ī�尡 �������� ��� �� �ִ� '���� ���� ī��'������ ����
    [Header("Street Repairs")]
    public bool streetRepairs; // �Ÿ� ������ ���õ� ����� �����ؾ� �ϴ����� ����
    public int streetRepairsHousePrice = 40;
    public int streetRepairsHotelPrice = 115;
}
