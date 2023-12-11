using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UiShowPanel : MonoBehaviour
{
    [SerializeField] GameObject humanPanel;
    [SerializeField] Button rollDiceButton;
    [SerializeField] Button endTurnButton;
    [SerializeField] Button jailFreeCard1;
    [SerializeField] Button jailFreeCard2;

    void OnEnable()
    {
        GameManager.OnShowHumanPanel += ShowPanel;//��ü�� Ȱ��ȭ�� �� ȣ��Ǹ�, GameManager�� �̺�Ʈ�� ShowPanel �޼��带 �����մϴ�.
        MonopolyNode.OnShowHumanPanel += ShowPanel;
        CommunityChest.OnShowHumanPanel += ShowPanel;
        ChanceFiled.OnShowHumanPanel += ShowPanel;
        Player.OnShowHumanPanel += ShowPanel;
    }

    void OnDisable()
    {
        GameManager.OnShowHumanPanel -= ShowPanel;// ��ü�� ��Ȱ��ȭ�� �� ȣ��Ǹ�, ����� ShowPanel �޼��带 �̺�Ʈ���� �����մϴ�.
        MonopolyNode.OnShowHumanPanel -= ShowPanel;
        CommunityChest.OnShowHumanPanel -= ShowPanel;
        ChanceFiled.OnShowHumanPanel -= ShowPanel;
        Player.OnShowHumanPanel -= ShowPanel;


    }
    void ShowPanel(bool showPanel,bool enableRollDice,bool enableEndTurn, bool hasChanceJailCard, bool hasCommunityJailCard)
    {
        humanPanel.SetActive(showPanel);// �ΰ� �÷��̾ ���� UI �г��Դϴ�. SetActive�� ���� ���̰ų� ���� �� �ֽ��ϴ�.
        rollDiceButton.interactable = enableRollDice;//�ֻ����� ������ ��ư�Դϴ�. interactable �Ӽ����� ��ư�� Ȱ��ȭ�ϰų� ��Ȱ��ȭ�� �� �ֽ��ϴ�.
        endTurnButton.interactable = enableEndTurn;//���� �����ϴ� ��ư�Դϴ�. �̰͵� interactable �Ӽ����� ����˴ϴ�.
        jailFreeCard1.interactable = hasChanceJailCard;
        jailFreeCard2.interactable = hasCommunityJailCard;
    }
}
