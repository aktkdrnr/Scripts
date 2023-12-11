using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Community Card", menuName = "Monopoly/Cards/Community")] 
//�����͸� �����ϴµ� ���Ǹ�, �� �����ʹ� ���� ������ �پ��� �����̳� ���ǿ� ���� ������ �� �ֽ��ϴ�.
//SCR_CommunityCard Ŭ������ ��Ȯ�� Ŀ�´�Ƽ ī�忡 ���õ� �����͸� �����ϴ� �뵵�� ������� ������ ���Դϴ�.
public class SCR_CommunityCards : ScriptableObject
{
    public string textOnCard; // ī�忡 ǥ�õ� �ؽ�Ʈ �Ǵ� ����
    public int rewardMoney; // ī�带 ���� ȹ���� �� �ִ� ���� ��
    public int penalityMoney; // ī�带 ���� �����ؾ� �ϴ� ���� ��
    public int moveToBoardIndex = -1; // �÷��̾ �̵��ؾ� �ϴ� ������ �ε��� (-1�� �ƹ� �ۿ� ������ �ǹ�)
    public bool collectFromPlayer; // �ٸ� �÷��̾�κ��� ���� �����ؾ� �ϴ����� ����
    [Header("Jail Content")]
    public bool goToJail; // �÷��̾ ������ �����ϴ����� ����
    public bool jailFreeCard; // �� ī�尡 �������� ��� �� �ִ� '���� ���� ī��'������ ����
    [Header("Street Repairs")]
    public bool streetRepairs; // �Ÿ� ������ ���õ� ����� �����ؾ� �ϴ����� ����
    public int streetRepairsHousePrice = 40;
    public int streetRepairsHotelPrice = 115;

}
