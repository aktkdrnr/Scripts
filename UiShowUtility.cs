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

        // ��� UI ���� ������Ʈ
        UtilityNameText.text = node.name;
        //colorField.color = node.propertyColorField.color;
        mortgagePriceText.text = "$" + node.MorgageValue;
        // �ϴ� �� ���� ������Ʈ
        UtilityPriceText.text = "Price: $" + node.price; // �Ӽ� ���� ǥ��
        playerMoneyText.text = "You have: $" + currentPlayer.ReadMoney; // ���� �÷��̾��� �� ǥ��
        // �Ӽ� ���� ��ư ������Ʈ
        if (currentPlayer.CanAffordNode(node.price)) // ���� �÷��̾ ����� ������ ������ �� �ִٸ�
        {
            buyUtilityButton.interactable = true; // ���� ��ư�� Ȱ��ȭ
        }
        else
        {
            buyUtilityButton.interactable = false; // ���� ��ư�� ��Ȱ��ȭ

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
