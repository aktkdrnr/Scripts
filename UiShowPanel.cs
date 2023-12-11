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
        GameManager.OnShowHumanPanel += ShowPanel;//객체가 활성화될 때 호출되며, GameManager의 이벤트에 ShowPanel 메서드를 연결합니다.
        MonopolyNode.OnShowHumanPanel += ShowPanel;
        CommunityChest.OnShowHumanPanel += ShowPanel;
        ChanceFiled.OnShowHumanPanel += ShowPanel;
        Player.OnShowHumanPanel += ShowPanel;
    }

    void OnDisable()
    {
        GameManager.OnShowHumanPanel -= ShowPanel;// 객체가 비활성화될 때 호출되며, 연결된 ShowPanel 메서드를 이벤트에서 제거합니다.
        MonopolyNode.OnShowHumanPanel -= ShowPanel;
        CommunityChest.OnShowHumanPanel -= ShowPanel;
        ChanceFiled.OnShowHumanPanel -= ShowPanel;
        Player.OnShowHumanPanel -= ShowPanel;


    }
    void ShowPanel(bool showPanel,bool enableRollDice,bool enableEndTurn, bool hasChanceJailCard, bool hasCommunityJailCard)
    {
        humanPanel.SetActive(showPanel);// 인간 플레이어에 대한 UI 패널입니다. SetActive를 통해 보이거나 숨길 수 있습니다.
        rollDiceButton.interactable = enableRollDice;//주사위를 굴리는 버튼입니다. interactable 속성으로 버튼을 활성화하거나 비활성화할 수 있습니다.
        endTurnButton.interactable = enableEndTurn;//턴을 종료하는 버튼입니다. 이것도 interactable 속성으로 제어됩니다.
        jailFreeCard1.interactable = hasChanceJailCard;
        jailFreeCard2.interactable = hasCommunityJailCard;
    }
}
