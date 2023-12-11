using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable] // 이 클래스의 인스턴스는 Unity 인스펙터에서 볼 수 있게 직렬화 가능
public class Player
{
    // 플레이어 유형을 정의하는 열거형 (사람 또는 AI)
    public enum PlayerType
    {
        HUMAN,
        AI
    }

    public PlayerType playerType; // 플레이어 유형 (사람 또는 AI)
    public string name; // 플레이어의 이름
    int money; // 플레이어가 가진 돈

    MonopolyNode currentnode; // 플레이어가 현재 위치한 보드의 노드
    bool isInJail; // 플레이어가 감옥에 있는지 여부
    int numTurnsInJail = 0; // 플레이어가 감옥에서 보낸 턴 수
    [SerializeField] GameObject myToken; // 플레이어의 토큰 (게임 오브젝트)
    [SerializeField] List<MonopolyNode> myMonopolyNodes = new List<MonopolyNode>(); // 플레이어가 소유한 노드 목록
    public List<MonopolyNode> GetMonopolyNodes => myMonopolyNodes;// 플레이어가 소유한 노드 목록

    bool hasChanceJailFreeCard, hasCommunityJailFreeCard;

    public bool HasChanceJailFreeCard => hasChanceJailFreeCard;
    public bool HasCommunityJailFreeCard => hasCommunityJailFreeCard;

    PlayerInfo myInfo; // 플레이어 정보 UI를 업데이트하는데 사용되는 PlayerInfo 객체

    int aiMoneySavity = 200; // AI가 사용할 돈의 양

    public enum AiState
    {
        IDLE,
        TRADING
    }
    public AiState aiState;


    // 접근자 메서드들
    public bool IsInJail => isInJail; // 플레이어가 감옥에 있는지 여부를 반환,읽기 전용 메서드
    public GameObject MyToken => myToken; // 플레이어의 토큰을 반환,읽기 전용 메서드
    public MonopolyNode MymonopolyNode => currentnode; // 플레이어의 현재 노드를 반환,읽기 전용 메서드
    public int ReadMoney => money;

    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool hasChanceJailCard, bool hasCommunityJailCard);
    public static ShowHumanPanel OnShowHumanPanel;

    public void Inititialize(MonopolyNode startNode, int startMoney, PlayerInfo info, GameObject token)
    {
        currentnode = startNode; // 시작 노드 설정
        money = startMoney; // 시작 자금 설정
        myInfo = info; // PlayerInfo 객체 설정
        myInfo.SetPlayerNameAndCash(name, money); // UI에 플레이어 이름과 자금 표시
        myToken = token; // 플레이어의 토큰 설정
        myInfo.ActivateArrow(false);
    }
    public void SetMyCurrentNode(MonopolyNode newNode) //플레이어가 새로운 노드(부동산 칸)에 도착했을 때 호출
    {
        currentnode = newNode; // 현재 노드 업데이트
        newNode.PlayerLandedOnNode(this); // 해당 노드에 플레이어가 도착했을 때의 로직 처리

        if (playerType == PlayerType.AI)
        {
            CheckIfPlayerHasASet();//완전한 부동산 세트를 소유하고 있는지 확인하고, 조건이 충족되면 건물을 건설합니다.
            UnMortgageProperties();//충분한 자금이 있을 경우 저당 잡힌 부동산을 해제합니다.
            //TradingSystem.instance.FindMissingProperty(this);
        }
         
    }
    public void CollectMoney(int amount)
    {
        money += amount; // 플레이어의 돈에 amount를 추가
        myInfo.SetPlayerCash(money); // UI에 플레이어의 업데이트된 자금 표시
        if (playerType == PlayerType.HUMAN && GameManager.instance.GetCurrentPlayer == this )
        {
            bool canEndTurn = !GameManager.instance.RolledADouble && ReadMoney >= 0 && GameManager.instance.HasRolledDice;
            bool canRollDice = (GameManager.instance.RolledADouble && ReadMoney >= 0) || (!GameManager.instance.HasRolledDice && ReadMoney >= 0);
            OnShowHumanPanel.Invoke(true, canRollDice, canEndTurn, hasChanceJailFreeCard, hasCommunityJailFreeCard);
        }
    }
    internal bool CanAffordNode(int price)//게임 로직에서 플레이어가 노드를 구매하려 할 때 호출되어, 노드를 구매할 수 있는지를 판단
    { return price <= money;}
    //예를 들어, 노드를 구매하는 결정, 경매 참여, 건물 건설 등의 다양한 상황에서 사용될 수 있습니다.
    public void BuyProperty(MonopolyNode node)//플레이어가 게임 내에서 부동산을 구매할 때 필요한 금전적 처리를 수행
                                              //해당 부동산 노드의 소유권을 플레이어에게 이전하며, 플레이어의 UI 정보를 업데이트하는 기능을 수행
    {
        money -= node.price;
        node.SetOwner(this);
        myInfo.SetPlayerCash(money);
        myMonopolyNodes.Add(node);
        SortPropertiesByPrice();
    }
    void SortPropertiesByPrice()//이 메서드는 플레이어가 소유한 부동산 리스트(myMonopolyNodes)를 가격(price)에 따라 정렬합니다.
    { myMonopolyNodes = myMonopolyNodes.OrderBy(_node => _node.price).ToList();}
    internal void PayRent(int rentAmount, Player owner)//이 함수는 플레이어가 임대료를 지불할 때 호출되며, 다음과 같이 동작합니다:
    {
        if (money < rentAmount)
        {
            if (playerType == PlayerType.AI)
            { HandleInsufficientFunds(rentAmount); }
            else
            { OnShowHumanPanel(true, false, false, hasChanceJailFreeCard, hasCommunityJailFreeCard); }
        }
        money -= rentAmount;
        owner.CollectMoney(rentAmount);
        myInfo.SetPlayerCash(money);
    }
    //임대료 지불 여부 확인: 플레이어의 현재 금액(money)이 임대료(rentAmount)보다 적을 경우, 즉 플레이어가 임대료를 지불할 
    //수없을 경우, 주석에서 언급하는 대로 부족한 자금을 처리하는 로직을 실행합니다. 이 부분에서는 실제 코드가 주석으로 
    //대체되어 있어, AI 플레이어의 경우 어떤 특별한 로직이 수행될지 추정할 수 있습니다. 예를 들어, 부족한 경우 다른 자산을 
    //매각하거나, 게임 오버 처리를 할 수 있습니다.
    //임대료 차감 및 수집: 플레이어의 금액에서 임대료를 차감(money -= rentAmount)하고, 임대료를 받을 소유주(owner)에게 
    //CollectMoney 함수를 호출하여 임대료를 수집합니다.
    //UI 업데이트: myInfo.SetPlayerCash(money)를 호출하여 UI에 표시되는 플레이어의 현재 금액을 업데이트합니다.
    //함수의 기능적인 측면에서 internal 접근 한정자는 해당 함수가 동일한 어셈블리 내에서만 접근 가능함을 의미합니다. 
    //즉, 다른 프로젝트나 어셈블리에서는 이 함수를 사용할 수 없습니다. PayRent 함수는 게임의 핵심 로직 중 하나로, 플레이어가
    //임대료를 지불해야 할 때 게임 규칙에 따라 금액을 조정하는 데 사용됩니다.
    internal void PayMoney(int amount)
    {
        if (money < amount)
        {
            if (playerType == PlayerType.AI)
            {HandleInsufficientFunds(amount);}
           /* else
            { OnShowHumanPanel(true, false, false); }*/
        }
        money -= amount;
        myInfo.SetPlayerCash(money);
        if (playerType == PlayerType.HUMAN && GameManager.instance.GetCurrentPlayer == this)
        {
            bool canEndTurn = !GameManager.instance.RolledADouble && ReadMoney >= 0 && GameManager.instance.HasRolledDice;
            bool canRollDice = (GameManager.instance.RolledADouble && ReadMoney >= 0) || (!GameManager.instance.HasRolledDice && ReadMoney >= 0);
            OnShowHumanPanel.Invoke(true, canRollDice, canEndTurn, hasChanceJailFreeCard, hasCommunityJailFreeCard);
        }
    }
    public void GoToJail(int indexOnBoard) // 플레이어가 감옥으로 가는 메서드입니다.> PlayerLandedOnNode의 MonopolyNodeType.GoToJail로.
    {
        isInJail = true; // 플레이어의 감옥 상태를 true로 설정합니다.

        // 플레이어를 감옥까지 이동시키는 함수를 호출합니다.
        // 이동 거리는 현재 위치에서 감옥까지의 거리를 계산하여 결정합니다.
        MonopolyBoard.instance.MovePlayerToken(CalculateDistanceFromJail(indexOnBoard), this);
        GameManager.instance.ResetRolledADouble();
    }
    public void SetOutOfJail()// 플레이어를 감옥에서 해방시키는 메서드입니다.
    {
        isInJail = false; // 플레이어의 감옥 상태를 false로 설정하여 감옥에서 해방시킵니다.
        numTurnsInJail = 0;
    }
    int CalculateDistanceFromJail(int indexOnBoard) // 현재 플레이어의 위치에서 감옥까지의 거리를 계산하는 메서드입니다. > GoToJail로.
    {
        int result = 0; // 결과를 저장할 변수를 초기화합니다.
        int indexOfJail = 10; // 감옥의 인덱스를 나타내는 변수입니다.

        // 현재 플레이어의 인덱스가 감옥 인덱스보다 큰 경우 (감옥을 지나친 경우)
        if (indexOnBoard > indexOfJail)
        {
            // 감옥까지의 거리를 음수로 계산합니다 (뒤로 이동해야 함을 의미).
            result = (indexOnBoard - indexOfJail) * -1;
        }
        else
        {
            // 감옥까지의 거리를 양수로 계산합니다 (앞으로 이동해야 함을 의미).
            result = (indexOfJail - indexOnBoard);
        }
        return result; // 계산된 거리를 반환합니다.
    }
    public int NumTurnsInJail => numTurnsInJail;
    public void IncreaseNumTurnsInJail()
    {numTurnsInJail++;}
    public void HandleInsufficientFunds(int amountToPay)//플레이어가 지불해야 할 금액(amountToPay)을 현재 가지고 있는 돈(money)으로 지불할 수 없을 때 호출
    {
        int housesToSell = 0; // 판매할 집의 수
        int allHouses = 0; // 플레이어가 소유한 집의 총 수
        int propertiesToMortgage = 0; // 저당 잡을 수 있는 부동산의 수
        int allpropertiesToMortgage = 0; // 플레이어가 소유한 저당 잡을 수 있는 부동산의 총 수

        foreach (var node in myMonopolyNodes) // 플레이어가 소유한 모든 부동산에서 집의 수를 합산합니다.
        { allHouses += node.NumberOfHouses;}
        while (money < amountToPay && allHouses > 0)
        {
            foreach (var node in myMonopolyNodes)
            {
                housesToSell = node.NumberOfHouses;
                if (housesToSell > 0)// 집을 팔고 그 금액을 수금합니다.
                {
                    CollectMoney(node.SellHouseOrHotel());
                    allHouses--;
                    if (money >= amountToPay) // 필요한 금액이 충당되면 함수를 종료합니다.
                    { return;}
                }
            }
        }
        foreach (var node in myMonopolyNodes)// 부동산을 저당 잡아서 부족한 금액을 충당하려고 시도합니다.
        {
            allpropertiesToMortgage += (!node.IsMortgaged) ? 1 : 0;
        }
        while (money < amountToPay && allpropertiesToMortgage > 0)// 지불해야 할 금액이 부족하고, 저당 잡히지 않은 내 부동산이 1개 이상이라면
        {
            foreach (var node in myMonopolyNodes)
            {
                propertiesToMortgage += (!node.IsMortgaged) ? 1 : 0;//저당잡히지 않은 부동산이 몇 개인지 확인합니다.
                if (propertiesToMortgage > 0) //저당잡히지 않은 나의 부동산이 한개 이상이라면,
                {
                    CollectMoney(node.MortgageProperty()); // 부동산을 저당 잡고 그 금액을 수금합니다.
                    allpropertiesToMortgage--;
                    if (money >= amountToPay)
                    { return; } // 필요한 금액이 충당되면 함수를 종료합니다.
                }
            }
        }
        if (playerType == PlayerType.AI)// 모든 방법을 시도한 후에도 금액이 부족하면 ai플레이어를 파산 처리합니다.
        { Bankrupt(); }
    }
    internal void Bankrupt() // 파산 메서드: 플레이어가 파산했을 때 호출됩니다.  
    {
        OnUpdateMessage.Invoke(name + "is Bankrupt");
        
        for (int i = myMonopolyNodes.Count-1; i >= 0 ; i--)
        {
            myMonopolyNodes[i].ResetNode();// 각 부동산 노드를 초기 상태로 리셋합니다.
        }
        if (hasChanceJailFreeCard)
        {
            ChanceFiled.instance.AddBackJailFreeCard();
        }
       
        if (hasCommunityJailFreeCard)
        {
            CommunityChest.instance.AddBackJailFreeCard();
        }

        GameManager.instance.RemovePlayer(this); // 게임 매니저에서 플레이어를 게임에서 제거합니다.
    }
    //for (int i = myMonopolyNodes.Count-1; i >= 0; i--) 이 코드는 역순으로 리스트를 반복 처리하기 위해 사용됩니다. 
    //이 방식은 주로 리스트 내의 요소를 제거할 때 사용되는데, 정방향으로 반복하면서 요소를 제거할 경우 리스트의 크기가 변하고,
    //이로 인해 인덱스 오류가 발생할 수 있기 때문입니다.
    //예를 들어, 플레이어가 3개의 부동산을 소유하고 있다고 가정해 보겠습니다. myMonopolyNodes의 인덱스는 0부터 2까지입니다. 
    //이제 플레이어가 파산했을 때, 이 부동산을 초기화해야 합니다.
    //정방향 반복(for (int i = 0; i < myMonopolyNodes.Count; i++))을 사용하면 첫 번째 요소(인덱스 0)를 제거한 후, 
    //리스트의 크기가 줄어들어 인덱스 1과 2의 요소가 각각 인덱스 0과 1로 이동합니다. 그 결과, 원래 인덱스 2에 있던 요소는 반복문에서 누락될 수 있습니다.
    //역순 반복을 사용하면 리스트의 마지막 요소부터 시작하여 각 요소를 제거하고, 리스트의 크기가 줄어들더라도 이미 처리된 요소에는 영향을 주지 않습니다. 
    //이 방법으로 리스트의 모든 요소를 안전하게 처리할 수 있습니다.

    //myMonopolyNodes.Count - 1을 사용하는 이유는 배열과 리스트의 인덱스가 0부터 시작하기 때문입니다.
    //Count 속성은 리스트의 총 요소 수를 반환하며, 인덱스는 0부터 시작하므로 마지막 요소의 인덱스는 Count - 1이 됩니다.
    //예를 들어, 리스트에 5개의 요소가 있다면 Count는 5이지만, 마지막 요소의 인덱스는 4(0, 1, 2, 3, 4)입니다. 
    //따라서 for 루프를 사용하여 리스트의 모든 요소에 접근하려면 myMonopolyNodes.Count - 1부터 시작하여
    //0에 도달할 때까지 감소시키는 것이 올바른 방법입니다.
    
    void UnMortgageProperties()//플레이어가 충분한 자금을 가지고 있을 때만 저당을 해제하도록 설계되어 있으며, AI 플레이어의 경우 특정 금액(aiMoneySavity)을 유지하기 위해 사용됩니다.
    {
        foreach (var node in myMonopolyNodes)
        {
            if (node.IsMortgaged)//만약 노드가 저당 잡혔다면(IsMortgaged가 true), 저당 해제 비용을 계산합니다. 이 비용은 저당 금액(MorgageValue)에 10%의 추가 비용(이자)을 더한 금액입니다.
            {
                int cost = node.MorgageValue + (int)(node.MorgageValue * 0.1f);
                
                if (money >= aiMoneySavity + cost)//플레이어의 현재 금액(money)이 AI의 안전 금액(aiMoneySavity)에 저당 해제 비용을 더한 금액보다 많다면, 저당을 해제할 수 있습니다.
                {
                    PayMoney(cost);
                    node.UnMortgageProperty();
                }
            }
        }
    }
    public int[] CountHousesAndHotels()// 모노폴리 게임에서 플레이어가 소유한 집과 호텔의 수를 계산합니다.
    {
        int houses = 0; // 집의 수를 셀 변수입니다. 배열의 인덱스 0에 해당합니다.
        int hotels = 0; // 호텔의 수를 셀 변수입니다. 배열의 인덱스 1에 해당합니다.
        foreach (var node in myMonopolyNodes) // 플레이어가 소유한 모든 노드를 순회합니다.
        {
            if (node.NumberOfHouses != 5)
            {houses += node.NumberOfHouses; }// 집의 수를 해당 노드의 집 개수만큼 증가시킵니다.
            else { hotels += 1;}
        }
        int[] allBuildings = new int[2] { houses, hotels };// 집과 호텔의 총 수를 담은 배열을 생성합니다.
        return allBuildings;       // 생성한 배열을 반환합니다.
    }
    void CheckIfPlayerHasASet() // 플레이어가 완전한 부동산 세트(예: 모든 오렌지색 부동산)를 소유하고 있는지 체크후 조건 충족시 건물을 건설하는 함수입니다.
    {
        List<MonopolyNode> processedSet = null; // 한번에 하나의 건물 만 짓기위한 방법 중에 하나, break; 문을 사용해 도 가능하나 장단점이 있다. 1번
        foreach (var node in myMonopolyNodes) // 플레이어가 소유한 모든 노드에 대해 반복합니다.
        {
            var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(node); // 현재 노드가 속한 세트의 모든 노드를 가져옵니다. 'allSame'은 플레이어가 그 세트의 모든 노드를 소유하고 있는지 여부입니다.
            if (!allSame) // 만약 플레이어가 세트의 모든 노드를 소유하고 있지 않다면, 다음 세트로 넘어갑니다.
            { continue;}
            List<MonopolyNode> nodeSet = list;// 가져온 리스트를 노드세트에 저장합니다.
            if (nodeSet != null && nodeSet != processedSet) // nodeSet != processedSet 한번에 하나의 건물 만 짓기위한 방법 중에 하나, break; 문을 사용해 도 가능하나 장단점이 있다. 2번
            {
                bool hasMortgagedNode = nodeSet.Any(node => node.IsMortgaged) ? true : false;
                //코드의 이 부분은 LINQ(Language Integrated Query)를 사용하여 nodeSet 컬렉션에서 어떤 조건을 만족하는 요소가 있는지 확인합니다. 구체적으로,
                //nodeSet 내의 모든 MonopolyNode 객체들 중에서 IsMortgaged 속성이 true인 객체가 하나라도 있는지 검사합니다.
                if (!hasMortgagedNode)// 저당 잡힌 노드가 없다면, 건물을 건설할 수 있는 조건을 체크합니다.
                {
                    if (nodeSet[0].monopolyNodeType == MonopolyNodeType.Property)// 해당 노드 세트의 첫 번째 노드가 부동산 유형이라면, BuildHouseOrHotelEvenly 메서드를 호출합니다.
                    {
                        BuildHouseOrHotelEvenly(nodeSet);//건물을 공정하게 건설하는 메서드입니다.
                        processedSet = nodeSet; // 한 번에 하나의 건물 만 짓기위한 방법 중에 하나, break; 문을 사용해 도 가능하나 장단점이 있다. 3번
                    }
                }
            }
        }
    }
    internal void BuildHouseOrHotelEvenly(List<MonopolyNode> nodesToBuildOn)// 부동산 세트에 집이나 호텔을 공정하게 건설하기 위한 메서드입니다.
    {
        // 최소 및 최대 집 개수를 저장하기 위한 변수를 초기화합니다.
        int minHouses = int.MaxValue; // 가능한 가장 큰 값으로 시작합니다. 위에서 아래로,
                                      //minHouses는 가장 적은 수의 집을 가진 부동산을 찾기 위해 사용되며, 
                                      //시작할 때는 가능한 가장 큰 값인 int.MaxValue로 초기화됩니다. 
                                      //이는 어떤 부동산의 집 수도 이 값보다 작기 때문에, 첫 비교에서 즉시 업데이트됩니다.
        int maxHouses = int.MinValue; // 가능한 가장 작은 값으로 시작합니다. 아래에서 위로
                                      //maxHouses는 가장 많은 수의 집을 가진 부동산을 찾기 위해 사용되며, 
                                      //시작할 때는 가능한 가장 작은 값인 int.MinValue로 초기화됩니다. 
                                      //이는 어떤 부동산의 집 수도 이 값보다 크기 때문에, 첫 비교에서 즉시 업데이트됩니다.

        foreach (var node in nodesToBuildOn)// 주어진 부동산 세트에 현재 건설된 집의 최소 및 최대 개수를 찾습니다.
        {
            int numOfHouses = node.NumberOfHouses; // 현재 노드의 집 개수를 가져옵니다.

            if (numOfHouses < minHouses) //numOfHouses를 minHouses에 업데이트 합니다.
            { minHouses = numOfHouses;}
            if (numOfHouses > maxHouses && numOfHouses < 5)
            {maxHouses = numOfHouses; }
        }
        foreach (var node in nodesToBuildOn)// 부동산 세트에 최대 허용 개수만큼 집을 구매합니다.
        {
            if (node.NumberOfHouses == minHouses && node.NumberOfHouses < 5 && CanAffordHouse(node.houseCost))// 현재 노드의 집 개수가 최소 집 개수와 같고,
                                                                                                              // 아직 5개 미만의 집이 있다면,(0,1,2,3,4)해당 노드에 집을 건설할 수 있습니다.
            {
                node.BuildHouseOrHotel();// 집을 건설하는 메서드를 호출합니다.
                PayMoney(node.houseCost);// 플레이어의 돈을 집 비용만큼 차감하는 코드입니다.
                break;                
            }
        }
    }
    //break; 문은 반복문을 제어하는 데 사용되는 키워드입니다. 이 코드에서 break; 문은 foreach 루프를 즉시 종료하고 루프 바깥의 코드 실행을 계속합니다.
    //이 코드에서 break; 문은 if 조건문이 참일 때 실행됩니다. 즉, 현재 노드의 집 개수가 최소 집 개수와 같고, 아직 5개 미만의 집이 있으며, 
    //플레이어가 집을 살 수 있는 경우에만 실행됩니다. 이 경우, node.BuildHouseOrHotel(); 메서드를 호출하여 집을 건설하고, 
    //PayMoney(node.houseCost);를 호출하여 플레이어의 돈을 집 비용만큼 차감한 후, break; 문을 실행하여 foreach 루프를 즉시 종료합니다.
    //이렇게 하면 한 번의 루프 반복에서 한 개의 집만 건설할 수 있습니다. 즉, break; 문은 부동산 세트에 집을 공정하게 분배하는 데 도움이 됩니다. 
    //각 루프 반복에서 한 개의 집만 건설하므로, 모든 부동산에 동일한 개수의 집이 건설될 때까지 이 과정을 반복할 수 있습니다. 
    //이렇게 하면 모든 부동산에 집이 공정하게 분배되며, 이는 모노폴리 게임의 규칙에 따른 것입니다. 이 규칙에 따르면, 플레이어는 한 부동산에 
    //모든 집을 건설하기 전에 모든 부동산에 동일한 개수의 집을 건설해야 합니다. 따라서 break; 문은 이 규칙을 준수하는 데 도움이 됩니다.

    //세 부동산 A, B, C가 있고 각각 다음과 같은 수의 집을 가지고 있다고 가정해 봅시다:
    //A 부동산: 1채의 집 B 부동산: 2채의 집 C 부동산: 1채의 집
    //이제 이 부동산들을 nodesToBuildOn 리스트에 넣고 foreach 루프를 실행합니다.
    //첫 번째 반복에서 A 부동산을 보면, numOfHouses는 1입니다. minHouses는 int.MaxValue이므로 A 부동산의 집 수가 더 작습니다. 
    //따라서 minHouses는 1로 업데이트됩니다. maxHouses는 int.MinValue이므로 A 부동산의 집 수가 더 큽니다. maxHouses도 1로 업데이트됩니다.
    //다음 B 부동산을 보면, numOfHouses는 2입니다. minHouses는 현재 1이므로 변경되지 않습니다. 
    //maxHouses는 1이므로 B 부동산의 집 수가 더 크고 5 미만이므로 maxHouses는 2로 업데이트됩니다.
    //마지막으로 C 부동산을 보면, numOfHouses는 1입니다. minHouses는 이미 1이므로 변경되지 않습니다. maxHouses도 변경되지 않습니다.
    //루프가 끝나면 minHouses는 1, maxHouses는 2가 됩니다. 이 정보를 사용하여 플레이어는 A나 C 부동산 중 한 곳에 추가로 집을 지을 수 있습니다
    //(둘 다 minHouses와 동일한 수의 집을 가지고 있기 때문에). B 부동산에는 이미 2채의 집이 있으므로, 이번 건설 차례에는 집을 추가하지 않습니다.

    internal void SellHouseEvenly(List<MonopolyNode> nodesToSellFrom)// 내부 함수로 선언된 SellHouseEvenly,
                                                                     // 다른 클래스에서 직접 접근할 수 없지만 같은 어셈블리 내에서는 호출할 수 있습니다.
    {
        int minHouses = int.MaxValue;// 각 부동산에 지어진 집의 최소 수를 저장할 변수를 초기화합니다
        bool houseSold = false;
        foreach (var node in nodesToSellFrom)
        {
            minHouses = Mathf.Min(minHouses, node.NumberOfHouses); // Mathf.Min은 둘 중에 더 작은 값을 받습니다.
        }
        for (int i = nodesToSellFrom.Count-1; i >= 0; i--)// 부동산 목록을 역순으로 순회합니다. 이는 가장 마지막에 추가된 부동산부터 확인하기 위함입니다.
        {
            if (nodesToSellFrom[i].NumberOfHouses > minHouses)// 현재 노드에 지어진 집의 수가 minHouses보다 많으면, 집을 판매합니다.
            {
                CollectMoney(nodesToSellFrom[i].SellHouseOrHotel());
                houseSold = true;
                break;
            }
        }
        if (!houseSold)
        {
            CollectMoney(nodesToSellFrom[nodesToSellFrom.Count-1].SellHouseOrHotel());
        }
    }
    //for 문의 초기화 부분에서 i를 nodesToSellFrom.Count - 1로 설정합니다. 
    //이는 리스트의 마지막 인덱스를 나타냅니다. (리스트는 0부터 시작하므로, 요소의 개수에서 1을 빼야 마지막 요소의 인덱스가 됩니다.)
    //조건 부분에서 i >= 0은 i가 0 이상인 동안 루프를 계속 실행하라는 것을 의미합니다. 즉, 첫 번째 요소에 도달할 때까지 루프가 계속됩니다.
    //각 반복 후에 i--는 i의 값을 1씩 감소시킵니다. 즉, 다음 반복 때는 이전 요소를 확인하게 됩니다.
    //예시:
    //부동산 세트가 다음과 같이 있다고 가정해 봅시다:
    //인덱스 0 (A 부동산): 2채의 집  
    //인덱스 1 (B 부동산): 3채의 집  
    //인덱스 2 (C 부동산): 2채의 집
    //부동산 세트의 집을 팔 때, 가장 많은 집이 있는 부동산부터 판매를 시작해야 합니다. 이 경우 B 부동산입니다.

    //for 루프는 다음과 같이 실행됩니다:
    //i는 2로 시작합니다 (nodesToSellFrom.Count - 1이므로). i가 2인 경우 (C 부동산), 집의 수는 2채입니다. 
    //이는 minHouses (가장 적은 집의 수)와 같으므로 이 부동산은 무시하고 다음 반복으로 넘어갑니다.
    //i를 1로 감소시킵니다 (i--). i가 1인 경우 (B 부동산), 집의 수는 3채입니다. 
    //이는 minHouses보다 많으므로 이 부동산에서 집을 판매할 수 있습니다. 집을 하나 판매하고 루프를 빠져나옵니다 (break).
    //만약 B 부동산이 판매 대상이 아니었다면, i를 0으로 감소시켜 A 부동산을 확인합니다. A 부동산 역시 집이 2채이므로 판매 대상이 아닙니다.
    //이 로직으로 플레이어는 세트 내에서 가장 많은 집이 있는 부동산부터 집을 공평하게 판매할 수 있습니다.

    //------------------------------------ if (!houseSold)-------------------------------------
    //모든 노드에 동일한 수의 집: 만약 모든 노드 세트가 동일한 수의 집을 가지고 있으면, minHouses 값은 그 집의 수와 같게 됩니다. 이 경우, 
    //어떤 노드에서도 if (nodesToSellFrom[i].NumberOfHouses > minHouses) 조건을 만족시키지 못하므로, 집이 판매되지 않습니다.

    //집을 판매하지 못한 상황에 대한 대비: houseSold 변수는 집이 판매되었는지를 추적합니다.
    //만약 모든 노드를 검토했음에도 불구하고 집이 판매되지 않았다면 (houseSold == false), 이 구문이 실행됩니다.

    //마지막 노드에서의 집 판매 시도: if (!houseSold) 구문은 부동산 목록의 마지막 노드에서 집을 판매하려고 시도합니다. 
    //이는 플레이어가 자금을 얻을 수 있도록 하는 "비상 조치"로 볼 수 있습니다.

    //따라서, 이 구문은 모든 노드가 동일한 수의 집을 가지고 있어서 일반적인 로직으로는 집을 판매할 수 없는 상황에 대한 예외 처리로 중요한 역할을 합니다. 
    //이러한 방식으로 게임의 균형을 유지하고, 플레이어가 완전히 자금을 얻지 못하는 상황을 방지할 수 있습니다.

    //nodesToSellFrom.Count-1는 리스트의 마지막 인덱스를 나타냅니다. 이는 리스트의 인덱스가 0부터 시작하기 때문에, 리스트의 크기(Count)에서 1을 빼면 마지막 요소의 인덱스가 됩니다.
    //예를 들어, nodesToSellFrom 리스트에 5개의 모노폴리 노드가 있다고 가정합시다. 이 경우, nodesToSellFrom.Count는 5가 됩니다. 리스트의 인덱스는 0부터 시작하므로,
    //마지막 요소의 인덱스는 5 - 1 = 4가 됩니다. 따라서, nodesToSellFrom[4]는 리스트의 마지막 노드를 참조하게 됩니다.


    public bool CanAffordHouse(int price)//집을 구매할 여력이 있는지 확인하는 메서드
    {
        if (playerType == PlayerType.AI)// ai가 가진 총 머니에서 최소 금액200을 제외한 나머지 금액이, 집의 가격보다 크거나 같다면 참을 반환한다.
        { return (money - aiMoneySavity) >= price;}
        return money >= price; 
    }
    public void ActivateSelector(bool active)
    {
        myInfo.ActivateArrow(active);
    }

    public void AddProperty(MonopolyNode node)//내 부동산 리스트에 부동산을 추가합니다.
    {
        myMonopolyNodes.Add(node);
        SortPropertiesByPrice();
    }
    public void RemoveProperty(MonopolyNode node)//내 부동산 리스트에서 부동산을 제거합니다.
    {
        myMonopolyNodes.Remove(node);
        SortPropertiesByPrice();
    }

    public void ChangeState(AiState state)
    {
        if (playerType == PlayerType.HUMAN)
        {
            return;
        }
        aiState = state;
        switch (aiState)
        {
            case AiState.IDLE:
                {
                    GameManager.instance.Continue();
                }
                break;
            case AiState.TRADING:
                {
                    TradingSystem.instance.FindMissingProperty(this);
                }
                break;
            default:
                break;
        }
    }

    public void AddChanceJailFreeCard()
    {
        hasChanceJailFreeCard = true;
    }
    public void AddCommunityJailFreeCard()
    {
        hasCommunityJailFreeCard = true;
    }
    public void UseCommunityJailFreeCard()
    {
        if (!isInJail)
        {return;}
        hasCommunityJailFreeCard = false;
        SetOutOfJail();
        CommunityChest.instance.AddBackJailFreeCard();
        OnUpdateMessage.Invoke(name + "used jail free card.");
    }
    public void UseChanceJailFreeCard()
    {
        if (!isInJail)
        { return; }
        SetOutOfJail();
        hasChanceJailFreeCard = false;
        ChanceFiled.instance.AddBackJailFreeCard();
        OnUpdateMessage.Invoke(name + "used jail free card.");

    }
}
