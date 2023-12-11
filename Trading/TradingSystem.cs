using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using UnityEngine.UI;
using TMPro;

public class TradingSystem : MonoBehaviour
{
    public static TradingSystem instance;
   
    [SerializeField] GameObject CardPrefab;
    [SerializeField] GameObject tradePanel;
    [SerializeField] GameObject resultPanel;
    [SerializeField] TMP_Text resultMessageText;

    [Header("LEFT SIDE")]
    [SerializeField] TMP_Text leftOfferedNameText;
    [SerializeField] Transform leftCardGrid;
    [SerializeField] ToggleGroup leftToggleGroup;
    [SerializeField] TMP_Text leftYourMoneyText;
    [SerializeField] TMP_Text leftOfferMoneyText;
    [SerializeField] Slider leftMoneySlider;
    List<GameObject> leftCardPrefabList = new List<GameObject>();
    Player leftPlayerReference;
    
    [Header("MIDDLE")]
    [SerializeField] Transform buttonGrid;
    [SerializeField] GameObject playerButtonPrefab;
    List<GameObject> playerButtonList = new List<GameObject>();
   
    [Header("RIGHT SIDE")]
    [SerializeField] TMP_Text rightOfferedNameText;
    [SerializeField] Transform rightCardGrid;
    [SerializeField] ToggleGroup rightToggleGroup;
    [SerializeField] TMP_Text rightYourMoneyText;
    [SerializeField] TMP_Text rightOfferMoneyText;
    [SerializeField] Slider rightMoneySlider;
    List<GameObject> rightCardPrefabList = new List<GameObject>();
    Player rightPlayerReference;

    [Header("Trade Offer Panel")]
    [SerializeField] GameObject tradeOfferPanel;
    [SerializeField] TMP_Text leftMessageText, rightMessageText, leftMoneyText, rightMoneyText;
    [SerializeField] GameObject leftCard, rightCard;
    [SerializeField] Image leftColorField, rightColorField;
    [SerializeField] Image leftPropImage, rightPropImage;
    [SerializeField] Sprite houseSprite, railroadSprite, utilitySprite;

    Player currentPlayer, nodeOwner;
    MonopolyNode requestedNode, offeredNode;
    int requestedMoney, offeredMoney;

    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        tradePanel.SetActive(false);
        resultPanel.SetActive(false);
        tradeOfferPanel.SetActive(false);
    }

    //-------------------------------------현재 플레이어에게 부족한 부동산을 찾아 거래를 제안하는 기능을 수행-----------------------------------------AI
    public void FindMissingProperty(Player currentPlayer)
    {
        List<MonopolyNode> processedSet = null; //아직 어떤 세트도 처리하지 않았음을 나타냅니다.플레이어가 소유한 부동산 세트를 추적
        MonopolyNode requestedNode = null;//아직 거래 대상이 결정되지 않았음을 나타냅니다.플레이어가 거래를 제안할 목표 부동산
        foreach (var node in currentPlayer.GetMonopolyNodes)// 현재 플레이어가 소유한 부동산 세트들을 검사합니다.
        {
            var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(node);
            //특정 노드가 속한 부동산 세트(list)와 플레이어가 해당 세트의 모든 부동산을 소유하고 있는지 여부(allSame)를 반환
            List<MonopolyNode> nodeSet = new List<MonopolyNode>();
            nodeSet.AddRange(list);
            //nodeSet.AddRange(list);: 여기서 nodeSet은 새로운 리스트를 생성하고, 앞서 얻은 세트(list)의 노드들을 nodeSet에 추가합니다.
            //이렇게 별도의 리스트를 만드는 이유는 list에 대한 추가적인 작업을 수행하기 위함입니다.

            bool notAllPurchased = list.Any(n => n.Owner == null);
            //현재 플레이어가 소유한 부동산 세트(list) 중에서 아직 구매되지 않은, 즉 소유자가 없는 노드가 하나라도 있는지 확인
            //Any 함수는 리스트에 조건을 만족하는 요소가 하나라도 있는지 검사합니다.

            if (allSame || processedSet == list || notAllPurchased)
            //조건은 현재 검사하는 세트가 이미 완전히 소유되었거나(allSame),이전에 처리되었거나(processedSet == list),
            //아직 구매되지 않은 부동산이 있을 때(notAllPurchased) 해당 세트를 건너뛰도록 합니다.이는 불필요한 중복 검사를 방지합니다.
            {
                processedSet = list;//노드가 속한 세트를 processedSet에 할당.
                continue;//다음코드로 건너뜁니다.
            }
            if (list.Count == 2)//세트에 두 개의 부동산이 있을 때 실행= 
            {requestedNode = list.Find(n => n.Owner != currentPlayer && n.Owner != null);
                //현재 플레이어가 소유하지 않고 다른 플레이어가 소유한 부동산을 찾습니다.
                if (requestedNode != null)//해당 부동산(requestedNode)이 존재하면 MakeTradeDecision 함수를 호출해 거래를 시도후,종료합니다.
                {
                    MakeTradeDecision(currentPlayer, requestedNode.Owner, requestedNode);
                    break;
                }
            }
            if (list.Count >= 3)//부동산 세트에 적어도 세 개의 노드가 있는지 확인합니다.
            {
                int hasMostOfSet = list.Count(n => n.Owner == currentPlayer);//현재 플레이어가 세트 내에서 소유한 노드의 수를 계산합니다.
                if (hasMostOfSet>=2)// 현재 플레이어가 세트 중 적어도 2개의 노드를 가지고 있다면, 
                {requestedNode = list.Find(n => n.Owner != currentPlayer && n.Owner != null);
                    MakeTradeDecision(currentPlayer, requestedNode.Owner, requestedNode);
                    break;}//아직 소유하지 않은 부동산과, 그 부동산의 주인을 찾아, 거래를 제안 후에 종료합니다.
            }
        }
        if (requestedNode == null)//목표 부동산을 찾지 못했을 경우, 대기상태로 돌아갑니다.
        {currentPlayer.ChangeState(Player.AiState.IDLE);}
    }
   
    //------------------------------------------MakeTradeDecision 최종적을 거래를 결정하는 함수.-------------------------------------AI
    void MakeTradeDecision(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode)
    {
        if (currentPlayer.ReadMoney >= CalculateValueOfNode(requestedNode))//현재 플레이어가 가진 돈이 목표 부동산의 계산한 가치보다 많으면,      
        {
            MakeTradeOffer(currentPlayer,nodeOwner,requestedNode,null,CalculateValueOfNode(requestedNode),0);
            return;
        }
        //requestedNode: 구매하려는 부동산./null: 거래에 포함되는 다른 부동산이 없음을 나타냅니다.
        // CalculateValueOfNode(requestedNode): 요청된 부동산의 가치를 계산하여 제안하는 금액으로 사용합니다./ 0: 요청된 추가 금액이 없음을 나타냅니다.

        bool foundDecision = false;

        foreach (var node in currentPlayer.GetMonopolyNodes)//현재 플레이어가 소유한 부동산의 노드들을 순회 합니다.
        {
            var checkedSet = MonopolyBoard.instance.PlayerHasAllNodesOfSet(node).list;
            //현재 플레이가 소유한 노드가 속한 각각의 노드 세트들의 리스트를 checkedSet에 할당합니다.
            if (checkedSet.Contains(requestedNode))//checkedSet안에 현재플레이어의 목표부동산이 있다면 계속 진행합니다.
            { continue; }//continue 키워드는 현재 반복을 종료하고 foreach 루프의 다음 반복으로 진행하라는 의미

            if (checkedSet.Count(n => n.Owner == currentPlayer) == 1)
            //플레이어가 소유한 노드의 세트 리스트 안에, 노드의 주인이 현재 플레이어고, 그 노드의 숫자가, 하나라면,
            {
                if (CalculateValueOfNode(node) + currentPlayer.ReadMoney >= requestedNode.price)
                    //플레이어가 소유한 노드(제안된 부동산)의 가치와 현재 가지고 있는 머니가, 목표 부동산의 가격보다 크거나 같다면,
                {
                    int difference = Mathf.Abs(CalculateValueOfNode(requestedNode) - CalculateValueOfNode(node));
                    //목표 부동산 과 플레이어가 소유한 부동산간(제안된 부동산)의 가치 차이를 계산하여, 절대값을 산출,difference에 할당.
                    if (difference > 0)
                    { MakeTradeOffer(currentPlayer, nodeOwner, requestedNode, node, difference, 0); }
                    //목표 부동산의 가치가 제안된 부동산의 가치보다 크다면, 거래대상자에게 그 차액만큼 지불하는 거래를 합니다. 
                    else
                    { MakeTradeOffer(currentPlayer, nodeOwner, requestedNode, node, 0, Mathf.Abs(difference)); }
                    //목표 부동산의 가치가 제안된 부동산의 가치보다 작다면, 거래대상자에게 그 차액만큼 요청하는 거래를 합니다. 
                    foundDecision = true;
                    break;
                }
            }
        }
        if (!foundDecision)
        {
            currentPlayer.ChangeState(Player.AiState.IDLE);
        }
    }

    //---------------------------------MakeTradeOffer() 플레이어 간의 거래 제안을 처리-------------------------------------------------AI
    void MakeTradeOffer(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode, MonopolyNode offeredNode, int offeredMoney, int requestedMoney)
    {
        if (nodeOwner.playerType == Player.PlayerType.AI)//AI인 경우에만 거래를 고려하여,거래를 진행합니다.
        { ConsiderTradeOffer(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney); }
        else if (nodeOwner.playerType == Player.PlayerType.HUMAN)
        {
            ShowTradeOfferPanel(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
        }
    }


    //-----------------------//ConsiderTradeOffer()이 함수는 거래의 가치를 분석하고, AI가 거래를 수락할지 거절할지 결정합니다.--------------------------------AI
    void ConsiderTradeOffer(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode, MonopolyNode offeredNode, int offeredMoney, int requestedMoney)
    {
        int valueOfTheTrade = (CalculateValueOfNode(requestedNode)+requestedMoney) - (CalculateValueOfNode(offeredNode) + offeredMoney);
        //플레이어 A가 플레이어 B에게 '보드워크'(가치 400) 부동산을 요청하고, 추가로 100의 돈을 제공하고자 합니다.
        // 플레이어 A는 '파크 플레이스'(가치 300) 부동산과 50의 돈을 제안합니다.//valueOfTheTrade 계산: (400 + 100) - (300 + 50) = 500 - 350 = 150
        //결과적으로, valueOfTheTrade는 150이 되며, 이는 거래가 플레이어 A에게 150만큼 유리하다는 것을 나타냅니다.
        //이 계산은 거래가 얼마나 공평한지 또는 어느 쪽이 더 유리한지를 판단하는 데 사용됩니다.
        
        if (requestedNode == null && offeredNode != null && requestedMoney < nodeOwner.ReadMoney/3 && !MonopolyBoard.instance.PlayerHasAllNodesOfSet(requestedNode).allSame)
        //만약 요청된 부동산이 없고(requestedNode == null), 제안된 부동산이 있으며(offeredNode != null),
        //요청된 금액이 거래 대상자의 돈의 1/3 미만일 경우, 거래를 진행합니다.
        //현재 플레이어가 요청한 부동산 세트를 이미 모두 소유하고 있지 않은 경우 거래합니다.
        {
            Trade(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);

            if (currentPlayer.playerType == Player.PlayerType.HUMAN)
            {
                TradeResult(true);
            }
            return;
        }

        if (valueOfTheTrade <= 0 && !MonopolyBoard.instance.PlayerHasAllNodesOfSet(requestedNode).allSame)
        { 
            Trade(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
            if (currentPlayer.playerType == Player.PlayerType.HUMAN)
            {
                TradeResult(true);
            }
        }

        //거래 대상자가 얻는 이득이 더 많고, 현재 플레이어가 요청한 부동산 세트를 이미 모두 소유하고 있지 않은 경우 거래합니다.

        else // 그 외에는 메세지
        {
            if (currentPlayer.playerType == Player.PlayerType.HUMAN)
            {
                TradeResult(false);
            }
            Debug.Log("AI REJECTED TRADE OFFER");
        }
    }

    //------------------------------------------MonopolyNode (부동산)의 가치를 계산-----------------------------------------------------AI
    int CalculateValueOfNode(MonopolyNode requestedNode)
    {
        int value = 0;//: 변수 value를 0으로 초기화합니다. 이 변수는 부동산의 총 가치를 저장합니다.
        if (requestedNode != null)//플레이어가 거래를 제안할 목표 부동산이 있을 경우에만 계산
        {
            if (requestedNode.monopolyNodeType == MonopolyNodeType.Property)//: 부동산 노드 타입이 일반 부동산일 경우
            { value = requestedNode.price + requestedNode.NumberOfHouses * requestedNode.houseCost;}
            //목표부동산의 가격+ 목표 부동산이 가지고 있는 집의 숫자 * 집의 가격을 부동산의 총 가치에 할당합니다.
            else
            {value = requestedNode.price; }//부동산 노드 타입이 일반 부동산이 아닐 경우, 기본 가격만 고려합니다.
            return value;//계산된 총 가치를 반환합니다.
        }
        return value;//제안할 목표 부동산이 없을 경우 0을 반환 합니다.
    }

    //------------------------------------------플레이어 간의 거래를 처리합니다.-----------------------------------------------------AI
    void Trade(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode, MonopolyNode offeredNode, int offeredMoney, int requestedMoney)
    {
        if (requestedNode != null)//거래 대상 부동산이 존재하는지 확인
        {
            currentPlayer.PayMoney(offeredMoney);//현재 플레이어가 제안한 돈을 지불
            requestedNode.ChangeOwner(currentPlayer);//거래된 부동산의 소유권이 현재 플레이어로 변경
            nodeOwner.CollectMoney(offeredMoney);//거래대상자가 돈을 수집
            nodeOwner.PayMoney(requestedMoney); //거래대상자가 플레이어에게 돈을 지불
            if (offeredNode != null) //플레이어가 거래대상자에게 추가로 제공하는 부동산이 있다면, 그 소유권도 변경
            {offeredNode.ChangeOwner(nodeOwner);}
            string offeredNodeName = (offeredNode != null) ? "$" + offeredNode.name : "";
            OnUpdateMessage.Invoke(currentPlayer.name + "trade" + requestedNode.name + "for" + offeredMoney + offeredNodeName +  "to" + nodeOwner.name);
        }
        else if (offeredNode != null && requestedNode == null)
        //플레이어가 거래대상자에게 제공하는 부동산이 있고, 목표로 하는 부동산이 없다면,
        {
            currentPlayer.CollectMoney(requestedMoney);//현재 플레이어가 돈을 수집한다.
            nodeOwner.PayMoney(requestedMoney);//거래대상자가 플레이어에게 돈을 지불한다.
            offeredNode.ChangeOwner(nodeOwner);//플레이어가 제공한 부동산의 소유주가 거래대상자로 변경된다.
            OnUpdateMessage.Invoke(currentPlayer.name + "sold" + offeredNode.name + "to" + nodeOwner.name + "for" + requestedMoney);
        }
        CloseTradePanel();
        if (currentPlayer.playerType == Player.PlayerType.AI)
        {
            currentPlayer.ChangeState(Player.AiState.IDLE);
        }
    }
    //Player currentPlayer: 현재 턴을 진행하고 있는 플레이어입니다.
    //Player nodeOwner: 거래 대상이 되는 부동산의 소유주입니다.
    // MonopolyNode requestedNode: 현재 플레이어가 요구하는, 즉 거래를 통해 얻고자 하는 부동산입니다.
    //MonopolyNode offeredNode: 현재 플레이어가 제공하고자 하는 부동산입니다.
    //int offeredMoney: 거래 대상자(부동산 소유주) 가 현재 플레이어에게 지불하거나 받는 돈입니다.
    //int requestedMoney: 현재 플레이어가 거래 대상자에게 지불하거나 받는 돈입니다.


    //------------------------------------------CURRENT PLAYER-----------------------------------------------------HUMAN
    void CreateLeftPanel()
    {
        leftOfferedNameText.text = leftPlayerReference.name;
        List<MonopolyNode> referenceNodes = leftPlayerReference.GetMonopolyNodes;

        for (int i = 0; i < referenceNodes.Count; i++)
        {
            GameObject tradeCard = Instantiate(CardPrefab, leftCardGrid, false);
            tradeCard.GetComponent<TradePropertyCard>().SetTradeCard(referenceNodes[i], leftToggleGroup);
            leftCardPrefabList.Add(tradeCard);
        }
        leftYourMoneyText.text = "Your Money: " + leftPlayerReference.ReadMoney;
        leftMoneySlider.maxValue = leftPlayerReference.ReadMoney;
        leftMoneySlider.value = 0;
        UpdateLeftSlider(leftMoneySlider.value);
        tradePanel.SetActive(true);
    }

    public void UpdateLeftSlider(float value)
    {leftOfferMoneyText.text = "Offer Money: $ " + leftMoneySlider.value;}
    
    //---------------------------------------------USER INTERFACE CONTENT-------------------------------------------HUMAN
    public void CloseTradePanel()
    {
        tradePanel.SetActive(false);
        ClearAll();
    }

    public void OpenTradePanel()
    {
        leftPlayerReference = GameManager.instance.GetCurrentPlayer;
        rightOfferedNameText.text = "Select a Player";
        CreateLeftPanel();
        CreateMiddleButtons();
    }

    //---------------------------------------------SELECTED PLAYER-------------------------------------------HUMAN
    public void showRightPlayer(Player player)
    {
        rightPlayerReference = player;
        ClearRightPanel();

        rightOfferedNameText.text = rightPlayerReference.name;
        List<MonopolyNode> referenceNodes = rightPlayerReference.GetMonopolyNodes;

        for (int i = 0; i < rightPlayerReference.GetMonopolyNodes.Count; i++)
        {
            GameObject tradeCard = Instantiate(CardPrefab, rightCardGrid, false);
            tradeCard.GetComponent<TradePropertyCard>().SetTradeCard(referenceNodes[i], rightToggleGroup);
            rightCardPrefabList.Add(tradeCard);
        }
        rightYourMoneyText.text = "Your Money: " + rightPlayerReference.ReadMoney;
        rightMoneySlider.maxValue = rightPlayerReference.ReadMoney;
        rightMoneySlider.value = 0;
        UpdateRightSlider(rightMoneySlider.value);
    }

    public void UpdateRightSlider(float value)
    { rightOfferMoneyText.text = "Requested Money: $ " + rightMoneySlider.value; }

    //----------------------------------------MIDDLE CONTENT-------------------------------------------------------HUMAN
    void CreateMiddleButtons()
    {
        for (int i = playerButtonList.Count-1; i >= 0; i--)
        {Destroy(playerButtonList[i]);}
        playerButtonList.Clear();

        List<Player> allPlayer = new List<Player>(); 
        allPlayer.AddRange(GameManager.instance.GetPlayers);
        allPlayer.Remove(leftPlayerReference);

        foreach (var player in allPlayer)
        {
            GameObject newPlayerButton = Instantiate(playerButtonPrefab, buttonGrid, false);
            newPlayerButton.GetComponent<TradePlayerButton>().SetPlayer(player);
            playerButtonList.Add(newPlayerButton);
        }
    }
    //---------------------------------------CLEAR CONTENT--------------------------------------------------------HUMAN
    void ClearAll()
    {
        rightOfferedNameText.text = "Select a Player";
        rightYourMoneyText.text = "Your Money: $ 0"; 
        rightMoneySlider.maxValue = 0;
        rightMoneySlider.value = 0;
        UpdateRightSlider(rightMoneySlider.value);

        for (int i = playerButtonList.Count - 1; i >= 0; i--)
        { Destroy(playerButtonList[i]); }
        playerButtonList.Clear();

        for (int i = leftCardPrefabList.Count - 1; i >= 0; i--)
        { Destroy(leftCardPrefabList[i]); }
        leftCardPrefabList.Clear();

        for (int i = rightCardPrefabList.Count - 1; i >= 0; i--)
        { Destroy(rightCardPrefabList[i]); }
        rightCardPrefabList.Clear();
    }

    void ClearRightPanel()
    {
        for (int i = rightCardPrefabList.Count - 1; i >= 0; i--)
        { Destroy(rightCardPrefabList[i]); }
        rightCardPrefabList.Clear();
       
        rightMoneySlider.maxValue = 0;
        rightMoneySlider.value = 0;
        UpdateRightSlider(rightMoneySlider.value);
    }


    //-------------------------------MakeOfferButton()거래 제안 시스템에서 사용자가 제안한 거래를 처리----------------------------------------------HUMAN
    public void MakeOfferButton()
    {
        MonopolyNode requestedNode = null;
        MonopolyNode offeredNode = null;
        if (rightPlayerReference == null)
        { return;}

        Toggle offeredToggle = leftToggleGroup.ActiveToggles().FirstOrDefault();
        //leftToggleGroup 내에서 현재 활성화된(사용자가 선택한) 첫 번째 토글을 찾습니다. 이것은 사용자가 거래를 제안하기 위해 선택한 부동산을 찾는 데 사용됩니다.
        if (offeredToggle != null)
        {offeredNode = offeredToggle.GetComponentInParent<TradePropertyCard>().Node();}
        //선택한 토글에 연결된 TradePropertyCard 컴포넌트를 찾고, 거기에 연결된 Node 함수를 호출하여 해당 부동산 객체의 참조를 얻습니다. 이렇게 얻은 offeredNode는 거래 제안을 할 때 사용됩니다.

        Toggle requestedToggle = rightToggleGroup.ActiveToggles().FirstOrDefault();
        if (requestedToggle != null)
        {requestedNode = requestedToggle.GetComponentInParent<TradePropertyCard>().Node();}

        MakeTradeOffer(leftPlayerReference, rightPlayerReference, requestedNode, offeredNode, (int)leftMoneySlider.value, (int)rightMoneySlider.value);
    }
    //FirstOrDefault() 메소드는 LINQ의 확장 메소드 중 하나로, 시퀀스에서 첫 번째 요소를 반환하거나 시퀀스에 요소가 없는 경우 기본값을 반환합니다. 
    //ActiveToggles() 함수는 토글 그룹에서 활성화된 모든 토글을 반환하고, FirstOrDefault()는 그 중 첫 번째를 선택합니다.
    //이렇게 사용하는 이유는 만약 어떤 토글도 활성화되지 않았을 경우 null을 반환하기 위함입니다. 
    //이는 First() 메소드와 다르게, 요소가 없을 때 예외를 발생시키지 않고 안전하게 null을 반환하므로, 에러 없이 코드의 안정성을 높이는 방법입니다.

    //------------------------------- 트레이드 직후 활성화 되는 결과 창----------------------------------------------HUMAN

    void TradeResult(bool accepted)
    {
        if (accepted)
        {
            resultMessageText.text = rightPlayerReference.name + "<b><color=green>accepted</color></b>" + "the trade";
        }
        else
        {
            resultMessageText.text = rightPlayerReference.name + "<b><color=red>rejected</color></b>" + "the trade";
        }

        resultPanel.SetActive(true);
    }
    //------------------------------- AI의 부동산 거래 제안 창----------------------------------------------HUMAN
    void ShowTradeOfferPanel(Player _currentPlayer, Player _nodeOwner, MonopolyNode _requestedNode, MonopolyNode _offeredNode, int _offeredMoney, int _requestedMoney)
    {
        currentPlayer = _currentPlayer;
        nodeOwner = _nodeOwner;
        requestedNode = _requestedNode;
        offeredNode = _offeredNode;
        requestedMoney = _requestedMoney;
        offeredMoney = _offeredMoney;

        tradeOfferPanel.SetActive(true);
        leftMessageText.text = currentPlayer.name + "offers:";
        rightMessageText.text = "For" + nodeOwner.name + "'s";
        leftMoneyText.text = "+$" + offeredMoney;
        rightMoneyText.text = "+$" + requestedMoney;

        leftCard.SetActive(offeredNode != null ? true : false);
        rightCard.SetActive(requestedNode != null ? true : false);

        if (leftCard.activeInHierarchy)//leftCard라는 게임 오브젝트가 현재 게임의 계층 구조에서 활성화되어 있는지(즉, 화면에 보이는지)를 확인합니다
        { 
            leftColorField.color = (offeredNode.propertyColorField != null) ? offeredNode.propertyColorField.color : Color.black;

            switch (offeredNode.monopolyNodeType)
            {
                case MonopolyNodeType.Property:
                    leftPropImage.sprite = houseSprite;
                    leftPropImage.color = Color.blue;
                    break;
                case MonopolyNodeType.Railroad:
                    leftPropImage.sprite = railroadSprite;
                    leftPropImage.color = Color.white;
                    break;
                case MonopolyNodeType.Utility:
                    leftPropImage.sprite = utilitySprite;
                    leftPropImage.color = Color.black;
                    break;
            }
        }
        if (rightCard.activeInHierarchy)//rightCard라는 게임 오브젝트가 현재 게임의 계층 구조에서 활성화되어 있는지(즉, 화면에 보이는지)를 확인합니다
        {
            rightColorField.color = (requestedNode.propertyColorField != null) ? requestedNode.propertyColorField.color : Color.black;

            switch (requestedNode.monopolyNodeType)
            {
                case MonopolyNodeType.Property:
                    rightPropImage.sprite = houseSprite;
                    rightPropImage.color = Color.blue;
                    break;
                case MonopolyNodeType.Railroad:
                    rightPropImage.sprite = railroadSprite;
                    rightPropImage.color = Color.white;
                    break;
                case MonopolyNodeType.Utility:
                    rightPropImage.sprite = utilitySprite;
                    rightPropImage.color = Color.black;
                    break;
            }
        }

    }

    public void AcceptOffer()
    {
        Trade(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
        ResetOffer();
    }
    public void RejectOffer()
    {
        currentPlayer.ChangeState(Player.AiState.IDLE);
        ResetOffer();
    }
    public void ResetOffer()
    {
        currentPlayer = null;
        nodeOwner = null; 
        requestedNode = null;
        offeredNode = null;
        requestedMoney = 0;
        offeredMoney = 0;
    }
}
