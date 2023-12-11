using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public enum MonopolyNodeType
{
    Property,
    Utility,
    Railroad,
    Tax,
    Chance,
    CommunityChest,
    Go,
    Jail,
    FreeParking,
    GoToJail
}
public class MonopolyNode : MonoBehaviour
{
    public MonopolyNodeType monopolyNodeType;
    public Image propertyColorField;
    [Header("Property Name")]
    [SerializeField] internal new string name;
    [SerializeField] TMP_Text nameText;
    [Header("Property Price")]
    public int price;
    public int houseCost;
    [SerializeField] TMP_Text priceText;
    [Header("Property Rent")]
    [SerializeField] bool calculateRentAuto;
    [SerializeField] int currentRent;
    [SerializeField] internal int baseRent;
    [SerializeField] internal List<int> rentWithHouses = new List<int>();
    int numberOfHouses;
    public int NumberOfHouses => numberOfHouses;
    [SerializeField] GameObject[] houses;
    [SerializeField] GameObject hotel;

    [Header("Property Mortgage")]
    [SerializeField] GameObject mortgageImage;
    [SerializeField] GameObject propertyImage;
    [SerializeField] bool isMortgaged;
    [SerializeField] int mortgageValue;
    [Header("Property Owner")]
    [SerializeField] GameObject ownerBar;
    [SerializeField] TMP_Text ownerText;
    Player owner;

    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    public delegate void DrawCommunityCard(Player player);
    public static DrawCommunityCard OnDrawCommunityCard;

    public delegate void DrawChanceCard(Player player);
    public static DrawChanceCard OnDrawChanceCard;

    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool hasChanceJailCard, bool hasCommunityJailCard);
    public static ShowHumanPanel OnShowHumanPanel;

    public delegate void ShowPropertyBuyPanel(MonopolyNode node, Player player);
    public static ShowPropertyBuyPanel OnShowPropertyBuyPanel;

    public delegate void ShowRailroadBuyPanel(MonopolyNode node, Player player);
    public static ShowRailroadBuyPanel OnShowRailroadBuyPanel;

    public delegate void ShowUtilityBuyPanel(MonopolyNode node, Player player);
    public static ShowUtilityBuyPanel OnShowUtilityBuyPanel;

    public Player Owner => owner;
    public void SetOwner(Player newOwner)
    {
        owner = newOwner;
        OnOwnerUpdated();
    }

    void OnValidate()//부동산의 가치와 임대료를 자동으로 계산하여 게임의 실시간 데이터를 관리하는 데 사용됩니다.
    {
        if (nameText != null)
        { nameText.text = name; }// UI에 표시될 이름 텍스트를 업데이트합니다.
        if (calculateRentAuto) //calculateRentAuto가 true로 설정되어 있다면,
        { 
            if(monopolyNodeType == MonopolyNodeType.Property) // 속성 타입이 'Property'인 경우의 자동 계산
            {
                if (baseRent > 0) // 가격, 모기지 가치, 각 집/호텔에 따른 임대료를 계산합니다.
                {
                    price = 3 * (baseRent * 10);
                    mortgageValue = price / 2;
                    rentWithHouses.Clear();
                    rentWithHouses.Add(baseRent * 5);
                    rentWithHouses.Add(baseRent * 5*2);
                    rentWithHouses.Add(baseRent * 5*3);
                    rentWithHouses.Add(baseRent * 5*4);
                    rentWithHouses.Add(baseRent * 5*5);
                }
                else if (baseRent <= 0)// 기본 임대료가 0 이하인 경우 모든 값을 0으로 설정합니다.
                {
                    price = 0;
                    baseRent = 0;
                    rentWithHouses.Clear();
                    mortgageValue = 0;
                }
            }
            if (monopolyNodeType == MonopolyNodeType.Utility)// 유틸리티 타입인 경우 모기지 가치만 계산합니다.
            { mortgageValue = price / 2;}
            if (monopolyNodeType == MonopolyNodeType.Railroad)// 철도 타입인 경우 모기지 가치만 계산합니다.
            { mortgageValue = price / 2; }
        }
        if (priceText != null) 
        { priceText.text = "$" + price; }  // UI에 표시될 가격 텍스트를 업데이트합니다.
        OnOwnerUpdated(); // 소유자 정보가 업데이트될 때 호출되는 메서드입니다.
        UnMortgageProperty(); // 모기지 해제 메서드를 호출합니다.
    }

    public void UpdateColorField(Color color)
    {
        if (propertyColorField != null)
        {
            propertyColorField.color = color;
        }
    }
    public int MortgageProperty()
    {
        isMortgaged = true;
        if (mortgageImage != null)
        { mortgageImage.SetActive(true); }
        if (propertyImage != null)
        { propertyImage.SetActive(false); }
        return mortgageValue;
    }
    public void UnMortgageProperty()
    {
        isMortgaged = false;
        if (mortgageImage != null)
        { mortgageImage.SetActive(false);}
        if (propertyImage != null)
        { propertyImage.SetActive(true);}
    }
    public bool IsMortgaged => isMortgaged;
    public int MorgageValue => mortgageValue;
    public void OnOwnerUpdated()
    {
        if (ownerBar != null)
        {
            if (owner != null)
            {
                ownerBar.SetActive(true);
                ownerText.text = owner.name;
            }
            else
            {
                ownerBar.SetActive(false);
                ownerText.text = "";
            }
        }
    }
    public void PlayerLandedOnNode(Player currentPlayer)
    {
        bool playerIsHuman = currentPlayer.playerType == Player.PlayerType.HUMAN;
        bool continueTurn = true;
        switch (monopolyNodeType)
        {
            case MonopolyNodeType.Property:
                if (!playerIsHuman)
                {
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        int rentToPay = CalculatePropertyRent();
                        currentPlayer.PayRent(rentToPay, owner);
                        OnUpdateMessage.Invoke(currentPlayer.name + "pays rent of:" + rentToPay + "to" + owner.name);
                    }
                    else if (owner == null && currentPlayer.CanAffordNode(price))
                    {
                        OnUpdateMessage.Invoke(currentPlayer.name + "buys" + this.name);
                        currentPlayer.BuyProperty(this);
                    }
                    else
                    {
                         
                    }
                }
                else
                {

                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        int rentToPay = CalculatePropertyRent();
                        currentPlayer.PayRent(rentToPay, owner);
                    }
                    else if (owner == null)
                    {
                        OnShowPropertyBuyPanel.Invoke(this, currentPlayer);
                    }
                    else
                    {

                    }
                }
                break;
            case MonopolyNodeType.Utility:
                if (!playerIsHuman)
                {
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        int rentToPay = CalculateUtilityRent();
                        currentRent = rentToPay;
                        currentPlayer.PayRent(rentToPay, owner);
                        OnUpdateMessage.Invoke(currentPlayer.name + "pays rent of:" + rentToPay + "to" + owner.name);
                    }
                    else if (owner == null && currentPlayer.CanAffordNode(price)) // 유틸리티에 주인이 없고, 현재 플레이어가 이 노드를 살 수 있는 돈이 있다면
                    {
                        OnUpdateMessage.Invoke(currentPlayer.name + "buys" + this.name);
                        currentPlayer.BuyProperty(this); // 현재 플레이어가 유틸리티를 구매합니다.
                        OnOwnerUpdated();// 소유주 정보를 업데이트합니다.
                    }
                    else
                    {

                    }
                }
                else
                {

                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        int rentToPay = CalculateUtilityRent();
                        currentRent = rentToPay;
                        currentPlayer.PayRent(rentToPay, owner);
                    }
                    else if (owner == null)
                    {
                        OnShowUtilityBuyPanel.Invoke(this, currentPlayer);
                    }
                    else
                    {

                    }
                }
                break;
            case MonopolyNodeType.Railroad:
                if (!playerIsHuman)
                {
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        int rentToPay = CalculateRailroadRent();
                        currentRent = rentToPay;
                        currentPlayer.PayRent(rentToPay, owner);
                        OnUpdateMessage.Invoke(currentPlayer.name + "pays rent of:" + rentToPay + "to" + owner.name);
                    }
                    else if (owner == null && currentPlayer.CanAffordNode(price)) // 유틸리티에 주인이 없고, 현재 플레이어가 이 노드를 살 수 있는 돈이 있다면
                    {
                        OnUpdateMessage.Invoke(currentPlayer.name + "buys" + this.name);
                        currentPlayer.BuyProperty(this); // 현재 플레이어가 유틸리티를 구매합니다.
                        OnOwnerUpdated();// 소유주 정보를 업데이트합니다.
                    }
                    else
                    {

                    }
                }
                else
                {

                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        int rentToPay = CalculateRailroadRent();
                        currentRent = rentToPay;
                        currentPlayer.PayRent(rentToPay, owner);
                    }
                    else if (owner == null)
                    {
                        OnShowRailroadBuyPanel.Invoke(this, currentPlayer);
                    }
                    else
                    {

                    }
                }
                break;
            case MonopolyNodeType.Tax:// 현재 플레이어가 세금 칸에 도착했을 때 처리
                GameManager.instance.AddTaxToPool(price);// 세금을 게임의 세금 풀에 추가함*
                currentPlayer.PayMoney(price);// 플레이어로 하여금 세금을 지불하게 함
                OnUpdateMessage.Invoke(currentPlayer.name + "pays tax of:" + price);
                break;
            case MonopolyNodeType.FreeParking:// 플레이어가 무료 주차 칸에 도착했을 때 처리
                int tax = GameManager.instance.GetTaxPool();// 세금 풀에서 현재 저장된 세금을 가져옴
                currentPlayer.CollectMoney(tax);// 가져온 세금을 플레이어에게 지급함
                OnUpdateMessage.Invoke(currentPlayer.name + "gets tax of:" + tax);
                break;
            case MonopolyNodeType.GoToJail:
                int indexOnBoard = MonopolyBoard.instance.route.IndexOf(currentPlayer.MymonopolyNode);
                currentPlayer.GoToJail(indexOnBoard);
                OnUpdateMessage.Invoke(currentPlayer.name + "has to go to jail!");
                continueTurn = false;
                break;
            case MonopolyNodeType.Chance:
                OnDrawChanceCard.Invoke(currentPlayer);
                continueTurn = false;
                break;
            case MonopolyNodeType.CommunityChest:
                OnDrawCommunityCard.Invoke(currentPlayer);
                continueTurn = false;
                break;
        }
        if (!continueTurn)
        {
            return;
        }

        if (!playerIsHuman)
        {
            //Invoke("ContinueGame", GameManager.instance.SecondsBetweenTurns);
            currentPlayer.ChangeState(Player.AiState.TRADING);
         }
        else
        {
            bool canEndTurn = !GameManager.instance.RolledADouble && currentPlayer.ReadMoney >= 0;
            bool canRollDice = GameManager.instance.RolledADouble && currentPlayer.ReadMoney >= 0;
            bool jail1 = currentPlayer.HasChanceJailFreeCard;
            bool jail2 = currentPlayer.HasCommunityJailFreeCard;

            OnShowHumanPanel.Invoke(true, canRollDice, canEndTurn, jail1, jail2);
        }
    }
   /* void ContinueGame()
    {
        if (GameManager.instance.RolledADouble)
        { GameManager.instance.RollDice(); }
        else
        { GameManager.instance.SwitchPlayer();}
    }*/

    int CalculatePropertyRent()
    {
        switch (numberOfHouses)
        {
            case 0:
                var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(this);
               
                if (allSame)
                {currentRent = baseRent * 2; }
                else
                { currentRent = baseRent;}
                break;
            case 1:
                currentRent = rentWithHouses[0];
                break;
            case 2:
                currentRent = rentWithHouses[1];
                break;
            case 3:
                currentRent = rentWithHouses[2];
                break;
            case 4:
                currentRent = rentWithHouses[3];
                break;
            case 5:
                currentRent = rentWithHouses[4];
                break;
        }
        return currentRent;
    }

    int CalculateUtilityRent()// 게임 매니저에서 마지막에 굴려진 주사위 값들을 가져옵니다.
    {
        List<int> lastRolledDice = GameManager.instance.LastRolledDice;
        int result = 0;
        var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(this);
        // 현재 유틸리티가 속한 세트의 모든 노드를 소유하고 있는지 확인합니다.
        if (allSame)
        { result = (lastRolledDice[0] + lastRolledDice[1]) * 10;}
        else
        { result = (lastRolledDice[0] + lastRolledDice[1]) * 4;}
        return result;
    }
    int CalculateRailroadRent()
    {
        int result = 0; // 결과를 저장할 변수를 초기화합니다.
        var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(this);// 현재 속성 세트의 모든 노드를 가져옵니다.
        int amount = 0; // 소유한 철도 속성의 수를 저장할 변수를 0으로 초기화합니다.
        
        foreach (var item in list)// list에 있는 모든 항목(노드)을 반복합니다.
        { amount += (item.owner == this.owner) ? 1 : 0; }  // 만약 현재 항목의 소유자가 이 속성의 소유자와 같다면 amount를 1 증가시킵니다. 삼항 연산자 (조건 ? 참일 때 값 : 거짓일 때 값)를 사용하여 검사합니다.

      
        result = baseRent * (int)Mathf.Pow(2, amount-1);
        // 최종 임대료를 계산합니다. baseRent(기본 임대료)에 2의 amount 제곱을 곱합니다.
        // 예를 들어, amount가 2라면 2의 2승, 즉 4를 baseRent에 곱합니다.
        
        return result; // 계산된 임대료를 반환합니다.
    }
    void VisualizeHouses()// 부동산 노드에 건설된 집과 호텔의 시각적 표현을 업데이트합니다.
    {
        switch (numberOfHouses)
        {
            case 0:
                houses[0].SetActive(false);
                houses[1].SetActive(false);
                houses[2].SetActive(false);
                houses[3].SetActive(false);
                hotel.SetActive(false);
                break;
            case 1:
                houses[0].SetActive(true);
                houses[1].SetActive(false);
                houses[2].SetActive(false);
                houses[3].SetActive(false);
                hotel.SetActive(false);
                break;
            case 2:
                houses[0].SetActive(true);
                houses[1].SetActive(true);
                houses[2].SetActive(false);
                houses[3].SetActive(false);
                hotel.SetActive(false);
                break;
            case 3:
                houses[0].SetActive(true);
                houses[1].SetActive(true);
                houses[2].SetActive(true);
                houses[3].SetActive(false);
                hotel.SetActive(false);
                break;
            case 4:
                houses[0].SetActive(true);
                houses[1].SetActive(true);
                houses[2].SetActive(true);
                houses[3].SetActive(true);
                hotel.SetActive(false);
                break;
            case 5:
                houses[0].SetActive(false);
                houses[1].SetActive(false);
                houses[2].SetActive(false);
                houses[3].SetActive(false);
                hotel.SetActive(true);
                break;
        }
    }
   
    public void BuildHouseOrHotel() // 부동산 노드에 집이나 호텔을 건설하는 기능입니다.
    {
        if (monopolyNodeType == MonopolyNodeType.Property)// 노드가 부동산 유형일 경우에만 집을 건설할 수 있습니다.
        {
            numberOfHouses++; // 집의 수를 하나 증가시킵니다.
            VisualizeHouses(); // 시각적 표현을 업데이트합니다.
        }
    }
    public int SellHouseOrHotel() // 부동산 노드에 건설된 집이나 호텔을 판매하는 기능입니다.
    { 
        if (monopolyNodeType == MonopolyNodeType.Property && numberOfHouses > 0)// 노드가 부동산 유형일 경우에만 집을 판매할 수 있습니다.
        {
            numberOfHouses--; // 집의 수를 하나 감소시킵니다.
            VisualizeHouses(); // 시각적 표현을 업데이트합니다.
            return houseCost / 2;
        }
        return 0;

    }
    public void ResetNode()// 부동산 노드를 초기 상태로 리셋하는 기능입니다.
    // (예를들어, 해당 노드를 가진 플레이어가 파산 한 경우,새 게임을 시작할 때,플레이어 간에 부동산을 거래할 때,특정 게임 규칙이나 카드에 의해)
    {
        if (isMortgaged) // 노드가 저당 잡혔을 경우, 저당 잡힌 표시를 제거합니다.
        {
            propertyImage.SetActive(true); // 부동산 이미지를 활성화합니다.
            mortgageImage.SetActive(false); // 저당 잡힌 이미지를 비활성화합니다.
            isMortgaged = false; // 저당 상태를 해제합니다.
        }
        if (monopolyNodeType == MonopolyNodeType.Property)  // 부동산 유형일 경우 집의 수를 0으로 리셋하고 시각적 표현을 업데이트합니다.
        {
            numberOfHouses = 0;
            VisualizeHouses();
        }
        owner.RemoveProperty(this);//현재 내가 가진 부동산을 제거합니다.
        owner.name = "";
        owner.ActivateSelector(false);
        owner = null;
        OnOwnerUpdated();// 소유자 정보를 초기화합니다.
    }

    public void ChangeOwner(Player newOwner)
    {
        owner.RemoveProperty(this);
        newOwner.AddProperty(this);
        SetOwner(newOwner);
    }
}
