using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;   

public class UiShowProperty : MonoBehaviour
{
        MonopolyNode nodeReference; // Monopoly 게임의 특정 노드를 참조하기 위한 변수
        Player playerReference;

        [Header("Buy Property UI")] // 인스펙터 창에서 이 섹션의 UI 요소를 구분하기 위한 헤더
        [SerializeField] GameObject propertyUiPanel; // 속성 구매 UI 패널 오브젝트
        [SerializeField] TMP_Text propertyNameText; // 속성 이름을 표시하는 텍스트 필드
        [SerializeField] Image colorField; // 속성의 색상을 표시하는 이미지 필드
        [Space] // 인스펙터 창에서 요소 사이에 공간 추가
        [SerializeField] TMP_Text rentPriceText; // 집이 없을 때의 임대료를 표시하는 텍스트 필드
        [SerializeField] TMP_Text oneHouseRentText; // 한 채의 집이 있을 때의 임대료를 표시하는 텍스트 필드
        [SerializeField] TMP_Text twoHouseRentText; // 두 채의 집이 있을 때의 임대료를 표시하는 텍스트 필드
        [SerializeField] TMP_Text threeHouseRentText; // 세 채의 집이 있을 때의 임대료를 표시하는 텍스트 필드
        [SerializeField] TMP_Text fourHouseRentText; // 네 채의 집이 있을 때의 임대료를 표시하는 텍스트 필드
        [SerializeField] TMP_Text hotelRentText; // 호텔이 있을 때의 임대료를 표시하는 텍스트 필드
        [Space] // 인스펙터 창에서 요소 사이에 공간 추가
        [SerializeField] TMP_Text housePriceText; // 집 가격을 표시하는 텍스트 필드
        [SerializeField] TMP_Text mortgagePriceText; // 호텔 가격을 표시하는 텍스트 필드
        [Space] // 인스펙터 창에서 요소 사이에 공간 추가
        [SerializeField] Button buyPropertyButton; // 속성 구매 버튼
        [Space]
        [SerializeField] TMP_Text propertyPriceText;
        [SerializeField] TMP_Text playerMoneyText;

    void OnEnable()
    {
        MonopolyNode.OnShowPropertyBuyPanel += ShowBuyPropertyUi;
    }

    void OnDisable()
    {
        MonopolyNode.OnShowPropertyBuyPanel -= ShowBuyPropertyUi;
    }

    void Start()
    {
        propertyUiPanel.SetActive(false);
    }


    void ShowBuyPropertyUi(MonopolyNode node, Player currentPlayer)
    {
        nodeReference = node; // 현재 노드를 참조 변수에 할당
        playerReference = currentPlayer;
        
        // 상단 UI 내용 업데이트
            propertyNameText.text = node.name; // 노드의 이름으로 속성 이름 텍스트 업데이트
            colorField.color = node.propertyColorField.color; // 노드의 색상으로 색상 필드 업데이트

            // 카드 중앙 내용 업데이트
            rentPriceText.text = "$" + node.baseRent; // 기본 임대료 표시
            oneHouseRentText.text = "$" + node.rentWithHouses[0]; // 한 채의 집이 있을 때 임대료 표시
            twoHouseRentText.text = "$" + node.rentWithHouses[1]; // 두 채의 집이 있을 때 임대료 표시
            threeHouseRentText.text = "$" + node.rentWithHouses[2]; // 세 채의 집이 있을 때 임대료 표시
            fourHouseRentText.text = "$" + node.rentWithHouses[3]; // 네 채의 집이 있을 때 임대료 표시
            hotelRentText.text = "$" + node.rentWithHouses[4]; // 호텔이 있을 때 임대료 표시

            // 건물 비용 업데이트
            housePriceText.text = "$" + node.houseCost; // 집 가격 표시
            mortgagePriceText.text = "$" + node.MorgageValue;

            // 하단 바 내용 업데이트
            propertyPriceText.text = "Price: $" + node.price; // 속성 가격 표시
            playerMoneyText.text = "You have: $" + currentPlayer.ReadMoney; // 현재 플레이어의 돈 표시 

        if (currentPlayer.CanAffordNode(node.price))
        { 
            buyPropertyButton.interactable = true; 
        }
        else
        {
            buyPropertyButton.interactable = false; 
        }
        propertyUiPanel.SetActive(true);
    }
    public void BuyPropertyButton()
    {
        playerReference.BuyProperty(nodeReference);
        buyPropertyButton.interactable = false;
    }
    //BuyPropertyButton 함수에서 playerReference의 BuyProperty 메소드를 호출하여
    //nodeReference가 가리키는 부동산을 구매합니다. 이후, buyPropertyButton의 interactable 속성을 false로 설정하여,
    //사용자가 버튼을 더 이상 클릭하지 못하게 합니다. 이는 일반적으로 사용자가 한 번 부동산을 구매하면, 
    //같은 부동산을 반복해서 구매할 수 없도록 하기 위함입니다.
    public void ClosePropertyButton()
    {
        propertyUiPanel.SetActive(false);
        nodeReference = null;
        playerReference = null;
    }
    //nodeReference와 playerReference를 null로 설정하여, 참조를 제거합니다. 
    //이는 다음 UI 사용 시 오래된 참조 정보가 남아 있지 않도록 메모리를 정리하는 데 도움이 됩니다.
    //예를 들어, 플레이어가 "Park Place" 부동산을 구매하려고 할 때, BuyPropertyButton을 클릭하면 해당 부동산이 플레이어의 소유가 되고, 
    //구매 버튼은 비활성화됩니다. 플레이어가 창을 닫으면, ClosePropertyButton이 호출되어 UI가 사라지고, 참조들이 정리됩니다.
    //---------------------------------
    //참조들이 null로 설정되면, 다른 스크립트에서 해당 참조를 사용하려 할 때 NullReferenceException 오류가 발생할 가능성이 있습니다. 
    //특히, nodeReference나 playerReference를 전역적으로 접근 가능하게 사용하고 있다면, 
    //이 변수들이 null로 설정된 이후에 그들을 참조하는 다른 코드가 실행될 때 문제가 생길 수 있습니다.
    //이를 방지하기 위해서는 null로 설정하기 전에, 해당 참조들이 사용되고 있는 모든 곳에서 이 참조들이 null이 아닌지 체크하는 로직을 추가해야 합니다
    //또한, 해당 참조들을 null로 설정하는 행위가 안전한 시점, 즉 더 이상 참조가 필요 없는 시점에만 수행되도록 해야 합니다.
}
