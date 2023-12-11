using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 
using UnityEngine.UI; 

public class TradePropertyCard : MonoBehaviour
{
    MonopolyNode nodeReference;

    [SerializeField] Image colorField; // 부동산 색상을 위한 UI 이미지
    [SerializeField] TMP_Text propertyNameText; // 부동산 이름을 위한 텍스트 필드
    [SerializeField] Image typeImage; // 부동산 유형(집, 철도, 공공시설 등)을 나타내는 이미지
    [SerializeField] Sprite houseSprite, railroadSprite, utilitySprite; // 다양한 부동산 유형을 위한 스프라이트
    [SerializeField] GameObject mortgageImage; // 부동산이 담보 설정되었는지 나타내는 이미지
    [SerializeField] TMP_Text propertyPriceText; // 부동산 가격을 위한 텍스트 필드
    [SerializeField] Toggle toggleButton; // 거래에서 부동산을 선택하기 위한 토글 버튼

    public void SetTradeCard(MonopolyNode node, ToggleGroup toggleGroup)
    {
        nodeReference = node; 
        colorField.color = (node.propertyColorField != null) ? node.propertyColorField.color : Color.black; 
        propertyNameText.text = node.name; 

        switch (node.monopolyNodeType)
        {
            case MonopolyNodeType.Property:
                typeImage.sprite = houseSprite;
                typeImage.color = Color.blue;
                break;
            case MonopolyNodeType.Railroad:
                typeImage.sprite = railroadSprite;
                typeImage.color = Color.white;
                break;
            case MonopolyNodeType.Utility:
                typeImage.sprite = utilitySprite;
                typeImage.color = Color.black;
                break;
        }
        mortgageImage.SetActive(node.IsMortgaged);
        propertyPriceText.text = "$" + node.price;
        toggleButton.isOn = false;//isOn 속성은 토글의 상태(켜짐/꺼짐)
        toggleButton.group = toggleGroup;
    }

    public MonopolyNode Node()
    {
        return nodeReference;
    }
}
