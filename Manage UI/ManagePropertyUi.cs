using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class ManagePropertyUi : MonoBehaviour
{
    [SerializeField] Transform cardHolder;// ī�带 ��ġ�� ��ġ(�θ� ������Ʈ)�� Transform
    [SerializeField] GameObject cardPrefab; // �ε��� ī���� ������
    [SerializeField] Button buyHouseButton, sellHouseButton; // ���� ��� �ȱ� ���� ��ư
    [SerializeField] TMP_Text buyHousePriceText, sellHousePriceText;// ���� ��� �Ĵ� ������ ǥ���� �ؽ�Ʈ
    [SerializeField] GameObject buttonBox;


    Player playerReference;// ������ �÷��̾� ��ü
    List<MonopolyNode> nodesInSet = new List<MonopolyNode>();// ������ �ε��� ������ ����Ʈ
    List<GameObject> cardsInSet = new List<GameObject>(); // UI�� ǥ�õ� ī�� ��ü���� ����Ʈ
    public void SetProperty(List<MonopolyNode> nodes, Player owner)// �ε��� ��Ʈ�� �����ϴ� �޼����Դϴ�.
    {
        playerReference = owner;// �÷��̾� ������ �����ڷ� �����մϴ�.
        nodesInSet.AddRange(nodes);// ���޹��� �ε��� ������ ����Ʈ�� �߰��մϴ�.
        for (int i = 0; i < nodes.Count; i++)
        {
            GameObject newCard = Instantiate(cardPrefab, cardHolder, false);// ī�� �������� �ν��Ͻ�ȭ�Ͽ� cardHolder ��ġ�� ��ġ�մϴ�.
                                                                            //�� ���� false�� ��, �� ���� ������Ʈ�� �θ� ������Ʈ�� ��ġ, ȸ��, �������� ��ӹ��� �ʽ��ϴ�.
                                                                            //���, �θ� ������Ʈ�� ���� ��ǥ�迡 ���� (0, 0, 0) ��ġ�� �����Ǹ�, 
                                                                            //���� ȸ���� (0, 0, 0)�� �ǰ�, ���� �������� (1, 1, 1)�� �˴ϴ�.
            ManageCardUi manageCardUi = newCard.GetComponent<ManageCardUi>();// ������ ī�忡�� ManageCardUi ������Ʈ�� �����ɴϴ�.
            cardsInSet.Add(newCard); // ������ ī�带 ī�� ����Ʈ�� �߰��մϴ�.
            manageCardUi.SetCard(nodesInSet[i],owner,this);// ī�� UI�� �����մϴ�.
        }
        var (list,allsame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(nodesInSet[0]);// �ε��� ��Ʈ�� ���¸� Ȯ���ϰ�, �÷��̾ �ش� ��Ʈ�� ��� �ε����� �����ϰ� �ִ��� Ȯ���մϴ�
        buyHouseButton.interactable = allsame && checkIfBuyAllowed();// �÷��̾ ��Ʈ�� ��� �ε����� �����ϰ� �ְ�, ������� ���ٸ�, ���� ������ �� �ִ� ��ư�� Ȱ��ȭ�մϴ�.
        sellHouseButton.interactable = CheckIfSellAllowed();

        buyHousePriceText.text = "-$" + nodesInSet[0].houseCost;
        sellHousePriceText.text = "+$" + nodesInSet[0].houseCost / 2;
        if (nodes[0].monopolyNodeType != MonopolyNodeType.Property)
        {
            buttonBox.SetActive(false);
        }
    }
    //cardPrefab �����տ� �̹� ManageCardUi ��ũ��Ʈ ������Ʈ�� ���ԵǾ� �ִ���, 
    //Instantiate �Լ��� �� ���� ������Ʈ�� ������ ������, 
    //�ش� ������Ʈ�� ���Ե� ManageCardUi ������Ʈ�� �����ϱ� ���ؼ��� GetComponent �޼��带 ����ؾ� �մϴ�.
    //Instantiate �Լ��� �������� ���纻�� ���� ����ϴ�.
    //�� ���纻�� ���� �����հ� ������ ������Ʈ�� ������ ��������, �޸� �󿡼� ������ ���ο� �ν��Ͻ��Դϴ�.
    //����, �� ���ο� �ν��Ͻ����� Ư�� ������Ʈ�� �����Ϸ��� GetComponent�� ����Ͽ� �ش� ������Ʈ�� ������ ���߸� �մϴ�.
    //-------------------------------------PlayerHasAllNodesOfSet(nodesInSet[0])------------------------------
    //�ڵ��� �ƶ��� ����� ��, nodesInSet[0]�� �־��� �ε��� ��Ʈ�� ù ��° �ε��� ��带 �����մϴ�.
    //�̴� ���� ������� ���ӿ��� �� ��Ʈ ���� ��� ���(�ε���)�� ������ ��Ʈ�� ���� �ִٴ� ���� �Ͽ� ���˴ϴ�.
    //��, �� ��Ʈ�� ��� �ε����� ������ ���� �׷쿡 ���ϰ�, �׷� ���� ��� ��尡 ���� ����(��� �����Ǿ��ų�, ��� ����� ���� ��)�� �����Ѵٰ� �����ϴ� ���Դϴ�.

    public void BuyHouseButton()
    {
        if (!checkIfBuyAllowed())//false�� ���, �ε��� ���� ���� ������� ������ ��尡 �ִٸ� �����Ѵ�
        {
            string message = "One or moreProperties are mortgaged, you can't build a house";
            ManageUi.instance.UpdateSystemMessage(message);
            return;
        }
        if (playerReference.CanAffordHouse(nodesInSet[0].houseCost))
        {
            playerReference.BuildHouseOrHotelEvenly(nodesInSet);
            UpdateHouseVisulas();
            string message = "you build a house";
            ManageUi.instance.UpdateSystemMessage(message);
            return;
        }
        else
        {
            string message = "you don't have enough money";
            ManageUi.instance.UpdateSystemMessage(message);
            return;
        }
#pragma warning disable CS0162 // ������ �� ���� �ڵ尡 �ֽ��ϴ�.
        sellHouseButton.interactable = CheckIfSellAllowed();
#pragma warning restore CS0162 // ������ �� ���� �ڵ尡 �ֽ��ϴ�.
        ManageUi.instance.UpdateMoneyText();
    }

    public void SellHouseButton()
    {
        playerReference.SellHouseEvenly(nodesInSet);
        UpdateHouseVisulas();
        sellHouseButton.interactable = CheckIfSellAllowed();
        ManageUi.instance.UpdateMoneyText();
    }

    bool CheckIfSellAllowed()
    {
        if (nodesInSet.Any(n => n.NumberOfHouses > 0))
        {
            return true;    
        }
        return false;
    }

    bool checkIfBuyAllowed()
    {
        if (nodesInSet.Any(n => n.IsMortgaged == true))
        {
            return false;
        }
        return true;
    }

    public bool CheckIfMortgageAllowed()
    {
        if (nodesInSet.Any(n => n.NumberOfHouses > 0))
        {
            return false;
        }
        return true;
    }
    //CheckIfSellAllowed(): �� �Լ��� �ε��� ����(nodesInSet) �� ��� ���� �ǹ�(NumberOfHouses)�� �ִ��� Ȯ���մϴ�.
    // ���� �ϳ��� �ǹ��� �ִٸ�, �ǸŰ� �����ϹǷ� true�� ��ȯ�մϴ�.
    //checkIfBuyAllowed(): �� �Լ��� ��� �ε����� ����� ���°� �ƴ� ���� ���Ű� �����ϵ��� Ȯ���մϴ�
    //CheckIfMortgageAllowed(): �� �Լ��� �ε��� ���� �� �ǹ��� ���� ��쿡�� ������� ����մϴ�. 
    //� ��忡�� �ǹ��� ������ true�� ��ȯ�ϰ�, �׷��� ������ false�� ��ȯ�մϴ�.

    void UpdateHouseVisulas()//�ǹ�(ȣ��) UI ��Ҹ� Ȱ��ȭ�մϴ�.
    {
        foreach (var card in cardsInSet)
        {
            card.GetComponent<ManageCardUi>().ShowBuildings();

        }
    }
}

