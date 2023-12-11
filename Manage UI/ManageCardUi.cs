using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ManageCardUi : MonoBehaviour
{
    [SerializeField] Image colorFiled;// ī�� ������ ǥ���ϴ� UI ���
    [SerializeField] TMP_Text propertyNameText;
    [SerializeField] GameObject[] buildings;// �ǹ��� ǥ���ϴ� UI ��ҵ��� �迭
    [SerializeField] GameObject mortgageImage;// ����� ���¸� ǥ���ϴ� UI �̹���
    [SerializeField] TMP_Text mortgageValueText;// ����� ��ġ�� ǥ���ϴ� �ؽ�Ʈ
    [SerializeField] Button mortgageButton, unMortgageButton; // ����� �� ����� ���� ��ư

    [SerializeField] Image iconImage;
    [SerializeField] Sprite houseSprite, railroadSprite, utilitySprite;

    Player playerReference;
    MonopolyNode nodeReference;
    ManagePropertyUi propertyReference;


    public void SetCard(MonopolyNode node, Player owner, ManagePropertyUi propertySet) //������ī�忡 ����� �޼��� �Դϴ�. SetProperty�� �پ ��� �˴ϴ�.

    {
        nodeReference = node;
        playerReference = owner;
        propertyReference = propertySet;
        if (node.propertyColorField != null)// ī�� ������ ������ �������� �����մϴ�.
        {
            colorFiled.color = node.propertyColorField.color;
        }
        else
        {
            colorFiled.color = Color.black;
        }
        ShowBuildings(); //�ǹ� UI ��Ҹ� Ȱ��ȭ�մϴ�.


        mortgageImage.SetActive(node.IsMortgaged); // ����� ���¿� ���� ����� �̹����� ǥ���ϰų� ����ϴ�.
        mortgageValueText.text = "Mortgage Value $" + node.MorgageValue;// ����� ��ġ�� �ؽ�Ʈ�� ǥ���մϴ�.
        mortgageButton.interactable = !node.IsMortgaged;// ����� ���°� �ƴϸ� ����� ��ư�� Ȱ��ȭ�մϴ�.
        unMortgageButton.interactable = node.IsMortgaged;// ����� �����̸� ����� ���� ��ư�� Ȱ��ȭ�մϴ�.
        switch (nodeReference.monopolyNodeType)
        {
            case MonopolyNodeType.Property:
                iconImage.sprite = houseSprite;
                iconImage.color = Color.blue;
                break;
            case MonopolyNodeType.Railroad:
                iconImage.sprite = railroadSprite;
                iconImage.color = Color.white;
                break;
            case MonopolyNodeType.Utility:
                iconImage.sprite = utilitySprite;
                iconImage.color = Color.black;
                break;
        }
        propertyNameText.text = nodeReference.name;
    }
   
    public void MortgageButton()
    {
        if (!propertyReference.CheckIfMortgageAllowed()) 
        {
            string message = "you have houses on one or more Properties, you can't mortgage!";
            ManageUi.instance.UpdateSystemMessage(message);
            return;
        }
        if (nodeReference.IsMortgaged)// �̹� ������Ǿ��ٸ�, �߰� �۾� ���� �Լ��� �������ɴϴ�.
        {
            string message = "It's mortgaged already!";
            ManageUi.instance.UpdateSystemMessage(message);
            return;
        }
        playerReference.CollectMoney(nodeReference.MortgageProperty());// �÷��̾�� ����� ��ġ��ŭ�� ���� �ְ�, �ش� �ε����� ����� ���·� �����մϴ�.
        mortgageImage.SetActive(true);
        mortgageButton.interactable = false;// ����� ��ư�� ��Ȱ��ȭ�Ͽ� �� �̻� ���� �� ���� �մϴ�.
        unMortgageButton.interactable = true; // ����� ���� ��ư�� Ȱ��ȭ�Ͽ� ���� �� �ְ� �մϴ�.
        ManageUi.instance.UpdateMoneyText();
    }
    //--------------------------------if (!propertyReference.CheckIfMortgageAllowed()) { return;}---------------------------------
    //CheckIfMortgageAllowed �޼��尡 false�� ��ȯ�Ѵٸ�, �� �ǹ��� �־ ������� ������ �� ���ٸ�, ���� �Լ����� �� �̻� �������� ���� �����Ѵ�

    public void UnMortgageButton()
    {
        if (!nodeReference.IsMortgaged)
        {
            string message = "It's unmortgaged already!";
            ManageUi.instance.UpdateSystemMessage(message);
            return;
        }// ����� ���°� �ƴ϶��, �߰� �۾� ���� �Լ��� �������ɴϴ�.
        if (playerReference.ReadMoney < nodeReference.MorgageValue) //�÷��̾��� ����� ���簡�� ����, ���ٸ�, �Լ��� �����մϴ�.
        {
            string message = "you don't have enough money!";
            ManageUi.instance.UpdateSystemMessage(message);
            return;
        }
        playerReference.PayMoney(nodeReference.MorgageValue);// �÷��̾�κ��� ����� ��ġ��ŭ�� ���� �ް�, 
        nodeReference.UnMortgageProperty();//�ش� �ε����� ������� �����մϴ�.
        mortgageImage.SetActive(false);
        unMortgageButton.interactable = false;
        mortgageButton.interactable = true;
        ManageUi.instance.UpdateMoneyText();
    }

    public void ShowBuildings()//�ǹ�(ȣ��) UI ��Ҹ� Ȱ��ȭ�մϴ�. , ��Ȱ��ȭ �մϴ�.
    {
        foreach (var icon in buildings)
        {
            icon.SetActive(false);
        }
        if (nodeReference.NumberOfHouses < 5)
        {
            for (int i = 0; i < nodeReference.NumberOfHouses; i++)
            {
                buildings[i].SetActive(true);
            }// �ǹ��� ���� 5�� �̸��̸�, �ش��ϴ� ����ŭ(0,1,2,3,4) �ǹ� UI ��Ҹ� Ȱ��ȭ�մϴ�.
        }
        else
        {
            buildings[4].SetActive(true);
        }// �ǹ��� ���� 4�� �̻��̸�, ������ �ǹ�(ȣ��) UI ��Ҹ� Ȱ��ȭ�մϴ�.
    }
}
