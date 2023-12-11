using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class ManagePropertyUi : MonoBehaviour
{
    [SerializeField] Transform cardHolder;// 카드를 배치할 위치(부모 오브젝트)의 Transform
    [SerializeField] GameObject cardPrefab; // 부동산 카드의 프리팹
    [SerializeField] Button buyHouseButton, sellHouseButton; // 집을 사고 팔기 위한 버튼
    [SerializeField] TMP_Text buyHousePriceText, sellHousePriceText;// 집을 사고 파는 가격을 표시할 텍스트
    [SerializeField] GameObject buttonBox;


    Player playerReference;// 참조할 플레이어 객체
    List<MonopolyNode> nodesInSet = new List<MonopolyNode>();// 소유한 부동산 노드들의 리스트
    List<GameObject> cardsInSet = new List<GameObject>(); // UI에 표시될 카드 객체들의 리스트
    public void SetProperty(List<MonopolyNode> nodes, Player owner)// 부동산 세트를 설정하는 메서드입니다.
    {
        playerReference = owner;// 플레이어 참조를 소유자로 설정합니다.
        nodesInSet.AddRange(nodes);// 전달받은 부동산 노드들을 리스트에 추가합니다.
        for (int i = 0; i < nodes.Count; i++)
        {
            GameObject newCard = Instantiate(cardPrefab, cardHolder, false);// 카드 프리팹을 인스턴스화하여 cardHolder 위치에 배치합니다.
                                                                            //이 값이 false일 때, 새 게임 오브젝트는 부모 오브젝트의 위치, 회전, 스케일을 상속받지 않습니다.
                                                                            //대신, 부모 오브젝트의 로컬 좌표계에 대해 (0, 0, 0) 위치에 생성되며, 
                                                                            //로컬 회전은 (0, 0, 0)이 되고, 로컬 스케일은 (1, 1, 1)이 됩니다.
            ManageCardUi manageCardUi = newCard.GetComponent<ManageCardUi>();// 생성된 카드에서 ManageCardUi 컴포넌트를 가져옵니다.
            cardsInSet.Add(newCard); // 생성된 카드를 카드 리스트에 추가합니다.
            manageCardUi.SetCard(nodesInSet[i],owner,this);// 카드 UI를 설정합니다.
        }
        var (list,allsame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(nodesInSet[0]);// 부동산 세트의 상태를 확인하고, 플레이어가 해당 세트의 모든 부동산을 소유하고 있는지 확인합니다
        buyHouseButton.interactable = allsame && checkIfBuyAllowed();// 플레이어가 세트의 모든 부동산을 소유하고 있고, 저당권이 없다면, 집을 구입할 수 있는 버튼을 활성화합니다.
        sellHouseButton.interactable = CheckIfSellAllowed();

        buyHousePriceText.text = "-$" + nodesInSet[0].houseCost;
        sellHousePriceText.text = "+$" + nodesInSet[0].houseCost / 2;
        if (nodes[0].monopolyNodeType != MonopolyNodeType.Property)
        {
            buttonBox.SetActive(false);
        }
    }
    //cardPrefab 프리팹에 이미 ManageCardUi 스크립트 컴포넌트가 포함되어 있더라도, 
    //Instantiate 함수로 새 게임 오브젝트를 생성할 때마다, 
    //해당 오브젝트에 포함된 ManageCardUi 컴포넌트에 접근하기 위해서는 GetComponent 메서드를 사용해야 합니다.
    //Instantiate 함수는 프리팹의 복사본을 새로 만듭니다.
    //이 복사본은 원본 프리팹과 동일한 컴포넌트와 설정을 가지지만, 메모리 상에서 완전히 새로운 인스턴스입니다.
    //따라서, 이 새로운 인스턴스에서 특정 컴포넌트에 접근하려면 GetComponent를 사용하여 해당 컴포넌트의 참조를 얻어야만 합니다.
    //-------------------------------------PlayerHasAllNodesOfSet(nodesInSet[0])------------------------------
    //코드의 맥락을 고려할 때, nodesInSet[0]는 주어진 부동산 세트의 첫 번째 부동산 노드를 참조합니다.
    //이는 보통 모노폴리 게임에서 한 세트 내의 모든 노드(부동산)가 동일한 세트에 속해 있다는 가정 하에 사용됩니다.
    //즉, 한 세트의 모든 부동산이 동일한 색상 그룹에 속하고, 그룹 내의 모든 노드가 같은 상태(모두 소유되었거나, 모두 모기지 상태 등)를 공유한다고 가정하는 것입니다.

    public void BuyHouseButton()
    {
        if (!checkIfBuyAllowed())//false인 경우, 부동산 집합 내에 모기지가 설정된 노드가 있다면 종료한다
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
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
        sellHouseButton.interactable = CheckIfSellAllowed();
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
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
    //CheckIfSellAllowed(): 이 함수는 부동산 집합(nodesInSet) 내 어느 노드라도 건물(NumberOfHouses)이 있는지 확인합니다.
    // 만약 하나라도 건물이 있다면, 판매가 가능하므로 true를 반환합니다.
    //checkIfBuyAllowed(): 이 함수는 모든 부동산이 모기지 상태가 아닐 때만 구매가 가능하도록 확인합니다
    //CheckIfMortgageAllowed(): 이 함수는 부동산 집합 내 건물이 없는 경우에만 모기지를 허용합니다. 
    //어떤 노드에도 건물이 없으면 true를 반환하고, 그렇지 않으면 false를 반환합니다.

    void UpdateHouseVisulas()//건물(호텔) UI 요소를 활성화합니다.
    {
        foreach (var card in cardsInSet)
        {
            card.GetComponent<ManageCardUi>().ShowBuildings();

        }
    }
}

