using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UiShowRailroad : MonoBehaviour
{
    MonopolyNode nodeReference;
    Player playerReference;

    [Header("Buy Railroad UI")]
    [SerializeField] GameObject RailroadUiPanel;
    [SerializeField] TMP_Text RailroadNameText; 
    [SerializeField] Image colorField; 
    [Space] 
    [SerializeField] TMP_Text oneRailroadRentText; 
    [SerializeField] TMP_Text twoRailroadRentText;
    [SerializeField] TMP_Text threeRailroadRentText;
    [SerializeField] TMP_Text fourRailroadRentText;   
    [Space] 
    [SerializeField] TMP_Text mortgagePriceText;
    [Space] 
    [SerializeField] Button buyRailroadButton; 
    [Space]
    [SerializeField] TMP_Text propertyPriceText;
    [SerializeField] TMP_Text playerMoneyText;

    void OnEnable()
    {
        MonopolyNode.OnShowRailroadBuyPanel += ShowBuyRailroadPanelUi;
    }

    void OnDisable()
    {
        MonopolyNode.OnShowRailroadBuyPanel -= ShowBuyRailroadPanelUi;
    }
    void Start()
    {
        RailroadUiPanel.SetActive(false);

    }
    void ShowBuyRailroadPanelUi(MonopolyNode node, Player currentPlayer)
    {
        nodeReference = node; 
        playerReference = currentPlayer;
        
        // ��� UI ���� ������Ʈ
        RailroadNameText.text = node.name;
        //colorField.color = node.propertyColorField.color;
        //        result = baseRent * (int)Mathf.Pow(2, amount-1);
        oneRailroadRentText.text = "$" + node.baseRent * (int)Mathf.Pow(2, 1 - 1);
        twoRailroadRentText.text = "$" + node.baseRent * (int)Mathf.Pow(2, 2 - 1);
        threeRailroadRentText.text = "$" + node.baseRent * (int)Mathf.Pow(2, 3 - 1);
        fourRailroadRentText.text = "$" + node.baseRent * (int)Mathf.Pow(2, 4 - 1);
       
        mortgagePriceText.text = "$" + node.MorgageValue;

        // �ϴ� �� ���� ������Ʈ
        propertyPriceText.text = "Price: $" + node.price; // �Ӽ� ���� ǥ��
        playerMoneyText.text = "You have: $" + currentPlayer.ReadMoney; // ���� �÷��̾��� �� ǥ��

        // �Ӽ� ���� ��ư ������Ʈ
        if (currentPlayer.CanAffordNode(node.price)) // ���� �÷��̾ ����� ������ ������ �� �ִٸ�
        {
            buyRailroadButton.interactable = true; // ���� ��ư�� Ȱ��ȭ
        }
        else
        {
            buyRailroadButton.interactable = false; // ���� ��ư�� ��Ȱ��ȭ

        }
        RailroadUiPanel.SetActive(true);
    }
    public void BuyRailroadButton()
    {
        playerReference.BuyProperty(nodeReference);
        buyRailroadButton.interactable = false;
    }
    public void CloseRailroadButton()
    {
        RailroadUiPanel.SetActive(false);
        nodeReference = null;
        playerReference = null;
    }
}

