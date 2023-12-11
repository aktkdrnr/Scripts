using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UiShowUtility : MonoBehaviour
{
    MonopolyNode nodeReference;
    Player playerReference;

    [Header("Buy Utility UI")]
    [SerializeField] GameObject UtilityUiPanel;
    [SerializeField] TMP_Text UtilityNameText;
    [SerializeField] Image colorField;
    [Space]
    [SerializeField] TMP_Text mortgagePriceText;
    [Space]
    [SerializeField] Button buyUtilityButton;
    [Space]
    [SerializeField] TMP_Text UtilityPriceText;
    [SerializeField] TMP_Text playerMoneyText;

    void OnEnable()
    {
        MonopolyNode.OnShowUtilityBuyPanel += ShowBuyUtilityPanelUi;
    }

    void OnDisable()
    {
        MonopolyNode.OnShowUtilityBuyPanel -= ShowBuyUtilityPanelUi;
    }
    void Start()
    {
        UtilityUiPanel.SetActive(false);
    }
    void ShowBuyUtilityPanelUi(MonopolyNode node, Player currentPlayer)
    {
        nodeReference = node;
        playerReference = currentPlayer;

        // 상단 UI 내용 업데이트
        UtilityNameText.text = node.name;
        //colorField.color = node.propertyColorField.color;
        mortgagePriceText.text = "$" + node.MorgageValue;
        // 하단 바 내용 업데이트
        UtilityPriceText.text = "Price: $" + node.price; // 속성 가격 표시
        playerMoneyText.text = "You have: $" + currentPlayer.ReadMoney; // 현재 플레이어의 돈 표시
        // 속성 구매 버튼 업데이트
        if (currentPlayer.CanAffordNode(node.price)) // 현재 플레이어가 노드의 가격을 지불할 수 있다면
        {
            buyUtilityButton.interactable = true; // 구매 버튼을 활성화
        }
        else
        {
            buyUtilityButton.interactable = false; // 구매 버튼을 비활성화

        }
        UtilityUiPanel.SetActive(true);
    }
    public void BuyUtilityButton()
    {
        playerReference.BuyProperty(nodeReference); 
        buyUtilityButton.interactable = false;
    }
    public void CloseUtilityButton()
    {
        UtilityUiPanel.SetActive(false);
        nodeReference = null;
        playerReference = null;
    }
}
