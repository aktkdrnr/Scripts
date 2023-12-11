using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ManageCardUi : MonoBehaviour
{
    [SerializeField] Image colorFiled;// 카드 색상을 표시하는 UI 요소
    [SerializeField] TMP_Text propertyNameText;
    [SerializeField] GameObject[] buildings;// 건물을 표시하는 UI 요소들의 배열
    [SerializeField] GameObject mortgageImage;// 모기지 상태를 표시하는 UI 이미지
    [SerializeField] TMP_Text mortgageValueText;// 모기지 가치를 표시하는 텍스트
    [SerializeField] Button mortgageButton, unMortgageButton; // 모기지 및 모기지 해제 버튼

    [SerializeField] Image iconImage;
    [SerializeField] Sprite houseSprite, railroadSprite, utilitySprite;

    Player playerReference;
    MonopolyNode nodeReference;
    ManagePropertyUi propertyReference;


    public void SetCard(MonopolyNode node, Player owner, ManagePropertyUi propertySet) //프리팹카드에 적용될 메서드 입니다. SetProperty에 붙어서 사용 됩니다.

    {
        nodeReference = node;
        playerReference = owner;
        propertyReference = propertySet;
        if (node.propertyColorField != null)// 카드 색상을 설정된 색상으로 변경합니다.
        {
            colorFiled.color = node.propertyColorField.color;
        }
        else
        {
            colorFiled.color = Color.black;
        }
        ShowBuildings(); //건물 UI 요소를 활성화합니다.


        mortgageImage.SetActive(node.IsMortgaged); // 모기지 상태에 따라 모기지 이미지를 표시하거나 숨깁니다.
        mortgageValueText.text = "Mortgage Value $" + node.MorgageValue;// 모기지 가치를 텍스트로 표시합니다.
        mortgageButton.interactable = !node.IsMortgaged;// 모기지 상태가 아니면 모기지 버튼을 활성화합니다.
        unMortgageButton.interactable = node.IsMortgaged;// 모기지 상태이면 모기지 해제 버튼을 활성화합니다.
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
        if (nodeReference.IsMortgaged)// 이미 모기지되었다면, 추가 작업 없이 함수를 빠져나옵니다.
        {
            string message = "It's mortgaged already!";
            ManageUi.instance.UpdateSystemMessage(message);
            return;
        }
        playerReference.CollectMoney(nodeReference.MortgageProperty());// 플레이어에게 모기지 가치만큼의 돈을 주고, 해당 부동산을 모기지 상태로 설정합니다.
        mortgageImage.SetActive(true);
        mortgageButton.interactable = false;// 모기지 버튼을 비활성화하여 더 이상 누를 수 없게 합니다.
        unMortgageButton.interactable = true; // 모기지 해제 버튼을 활성화하여 누를 수 있게 합니다.
        ManageUi.instance.UpdateMoneyText();
    }
    //--------------------------------if (!propertyReference.CheckIfMortgageAllowed()) { return;}---------------------------------
    //CheckIfMortgageAllowed 메서드가 false를 반환한다면, 즉 건물이 있어서 모기지를 설정할 수 없다면, 현재 함수에서 더 이상 진행하지 말고 종료한다

    public void UnMortgageButton()
    {
        if (!nodeReference.IsMortgaged)
        {
            string message = "It's unmortgaged already!";
            ManageUi.instance.UpdateSystemMessage(message);
            return;
        }// 모기지 상태가 아니라면, 추가 작업 없이 함수를 빠져나옵니다.
        if (playerReference.ReadMoney < nodeReference.MorgageValue) //플레이어의 재산이 저당가격 보다, 낮다면, 함수를 종료합니다.
        {
            string message = "you don't have enough money!";
            ManageUi.instance.UpdateSystemMessage(message);
            return;
        }
        playerReference.PayMoney(nodeReference.MorgageValue);// 플레이어로부터 모기지 가치만큼의 돈을 받고, 
        nodeReference.UnMortgageProperty();//해당 부동산의 모기지를 해제합니다.
        mortgageImage.SetActive(false);
        unMortgageButton.interactable = false;
        mortgageButton.interactable = true;
        ManageUi.instance.UpdateMoneyText();
    }

    public void ShowBuildings()//건물(호텔) UI 요소를 활성화합니다. , 비활성화 합니다.
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
            }// 건물의 수가 5개 미만이면, 해당하는 수만큼(0,1,2,3,4) 건물 UI 요소를 활성화합니다.
        }
        else
        {
            buildings[4].SetActive(true);
        }// 건물의 수가 4개 이상이면, 마지막 건물(호텔) UI 요소를 활성화합니다.
    }
}
