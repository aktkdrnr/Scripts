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

    //-------------------------------------���� �÷��̾�� ������ �ε����� ã�� �ŷ��� �����ϴ� ����� ����-----------------------------------------AI
    public void FindMissingProperty(Player currentPlayer)
    {
        List<MonopolyNode> processedSet = null; //���� � ��Ʈ�� ó������ �ʾ����� ��Ÿ���ϴ�.�÷��̾ ������ �ε��� ��Ʈ�� ����
        MonopolyNode requestedNode = null;//���� �ŷ� ����� �������� �ʾ����� ��Ÿ���ϴ�.�÷��̾ �ŷ��� ������ ��ǥ �ε���
        foreach (var node in currentPlayer.GetMonopolyNodes)// ���� �÷��̾ ������ �ε��� ��Ʈ���� �˻��մϴ�.
        {
            var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(node);
            //Ư�� ��尡 ���� �ε��� ��Ʈ(list)�� �÷��̾ �ش� ��Ʈ�� ��� �ε����� �����ϰ� �ִ��� ����(allSame)�� ��ȯ
            List<MonopolyNode> nodeSet = new List<MonopolyNode>();
            nodeSet.AddRange(list);
            //nodeSet.AddRange(list);: ���⼭ nodeSet�� ���ο� ����Ʈ�� �����ϰ�, �ռ� ���� ��Ʈ(list)�� ������ nodeSet�� �߰��մϴ�.
            //�̷��� ������ ����Ʈ�� ����� ������ list�� ���� �߰����� �۾��� �����ϱ� �����Դϴ�.

            bool notAllPurchased = list.Any(n => n.Owner == null);
            //���� �÷��̾ ������ �ε��� ��Ʈ(list) �߿��� ���� ���ŵ��� ����, �� �����ڰ� ���� ��尡 �ϳ��� �ִ��� Ȯ��
            //Any �Լ��� ����Ʈ�� ������ �����ϴ� ��Ұ� �ϳ��� �ִ��� �˻��մϴ�.

            if (allSame || processedSet == list || notAllPurchased)
            //������ ���� �˻��ϴ� ��Ʈ�� �̹� ������ �����Ǿ��ų�(allSame),������ ó���Ǿ��ų�(processedSet == list),
            //���� ���ŵ��� ���� �ε����� ���� ��(notAllPurchased) �ش� ��Ʈ�� �ǳʶٵ��� �մϴ�.�̴� ���ʿ��� �ߺ� �˻縦 �����մϴ�.
            {
                processedSet = list;//��尡 ���� ��Ʈ�� processedSet�� �Ҵ�.
                continue;//�����ڵ�� �ǳʶݴϴ�.
            }
            if (list.Count == 2)//��Ʈ�� �� ���� �ε����� ���� �� ����= 
            {requestedNode = list.Find(n => n.Owner != currentPlayer && n.Owner != null);
                //���� �÷��̾ �������� �ʰ� �ٸ� �÷��̾ ������ �ε����� ã���ϴ�.
                if (requestedNode != null)//�ش� �ε���(requestedNode)�� �����ϸ� MakeTradeDecision �Լ��� ȣ���� �ŷ��� �õ���,�����մϴ�.
                {
                    MakeTradeDecision(currentPlayer, requestedNode.Owner, requestedNode);
                    break;
                }
            }
            if (list.Count >= 3)//�ε��� ��Ʈ�� ��� �� ���� ��尡 �ִ��� Ȯ���մϴ�.
            {
                int hasMostOfSet = list.Count(n => n.Owner == currentPlayer);//���� �÷��̾ ��Ʈ ������ ������ ����� ���� ����մϴ�.
                if (hasMostOfSet>=2)// ���� �÷��̾ ��Ʈ �� ��� 2���� ��带 ������ �ִٸ�, 
                {requestedNode = list.Find(n => n.Owner != currentPlayer && n.Owner != null);
                    MakeTradeDecision(currentPlayer, requestedNode.Owner, requestedNode);
                    break;}//���� �������� ���� �ε����, �� �ε����� ������ ã��, �ŷ��� ���� �Ŀ� �����մϴ�.
            }
        }
        if (requestedNode == null)//��ǥ �ε����� ã�� ������ ���, �����·� ���ư��ϴ�.
        {currentPlayer.ChangeState(Player.AiState.IDLE);}
    }
   
    //------------------------------------------MakeTradeDecision �������� �ŷ��� �����ϴ� �Լ�.-------------------------------------AI
    void MakeTradeDecision(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode)
    {
        if (currentPlayer.ReadMoney >= CalculateValueOfNode(requestedNode))//���� �÷��̾ ���� ���� ��ǥ �ε����� ����� ��ġ���� ������,      
        {
            MakeTradeOffer(currentPlayer,nodeOwner,requestedNode,null,CalculateValueOfNode(requestedNode),0);
            return;
        }
        //requestedNode: �����Ϸ��� �ε���./null: �ŷ��� ���ԵǴ� �ٸ� �ε����� ������ ��Ÿ���ϴ�.
        // CalculateValueOfNode(requestedNode): ��û�� �ε����� ��ġ�� ����Ͽ� �����ϴ� �ݾ����� ����մϴ�./ 0: ��û�� �߰� �ݾ��� ������ ��Ÿ���ϴ�.

        bool foundDecision = false;

        foreach (var node in currentPlayer.GetMonopolyNodes)//���� �÷��̾ ������ �ε����� ������ ��ȸ �մϴ�.
        {
            var checkedSet = MonopolyBoard.instance.PlayerHasAllNodesOfSet(node).list;
            //���� �÷��̰� ������ ��尡 ���� ������ ��� ��Ʈ���� ����Ʈ�� checkedSet�� �Ҵ��մϴ�.
            if (checkedSet.Contains(requestedNode))//checkedSet�ȿ� �����÷��̾��� ��ǥ�ε����� �ִٸ� ��� �����մϴ�.
            { continue; }//continue Ű����� ���� �ݺ��� �����ϰ� foreach ������ ���� �ݺ����� �����϶�� �ǹ�

            if (checkedSet.Count(n => n.Owner == currentPlayer) == 1)
            //�÷��̾ ������ ����� ��Ʈ ����Ʈ �ȿ�, ����� ������ ���� �÷��̾��, �� ����� ���ڰ�, �ϳ����,
            {
                if (CalculateValueOfNode(node) + currentPlayer.ReadMoney >= requestedNode.price)
                    //�÷��̾ ������ ���(���ȵ� �ε���)�� ��ġ�� ���� ������ �ִ� �Ӵϰ�, ��ǥ �ε����� ���ݺ��� ũ�ų� ���ٸ�,
                {
                    int difference = Mathf.Abs(CalculateValueOfNode(requestedNode) - CalculateValueOfNode(node));
                    //��ǥ �ε��� �� �÷��̾ ������ �ε��갣(���ȵ� �ε���)�� ��ġ ���̸� ����Ͽ�, ���밪�� ����,difference�� �Ҵ�.
                    if (difference > 0)
                    { MakeTradeOffer(currentPlayer, nodeOwner, requestedNode, node, difference, 0); }
                    //��ǥ �ε����� ��ġ�� ���ȵ� �ε����� ��ġ���� ũ�ٸ�, �ŷ�����ڿ��� �� ���׸�ŭ �����ϴ� �ŷ��� �մϴ�. 
                    else
                    { MakeTradeOffer(currentPlayer, nodeOwner, requestedNode, node, 0, Mathf.Abs(difference)); }
                    //��ǥ �ε����� ��ġ�� ���ȵ� �ε����� ��ġ���� �۴ٸ�, �ŷ�����ڿ��� �� ���׸�ŭ ��û�ϴ� �ŷ��� �մϴ�. 
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

    //---------------------------------MakeTradeOffer() �÷��̾� ���� �ŷ� ������ ó��-------------------------------------------------AI
    void MakeTradeOffer(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode, MonopolyNode offeredNode, int offeredMoney, int requestedMoney)
    {
        if (nodeOwner.playerType == Player.PlayerType.AI)//AI�� ��쿡�� �ŷ��� ����Ͽ�,�ŷ��� �����մϴ�.
        { ConsiderTradeOffer(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney); }
        else if (nodeOwner.playerType == Player.PlayerType.HUMAN)
        {
            ShowTradeOfferPanel(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
        }
    }


    //-----------------------//ConsiderTradeOffer()�� �Լ��� �ŷ��� ��ġ�� �м��ϰ�, AI�� �ŷ��� �������� �������� �����մϴ�.--------------------------------AI
    void ConsiderTradeOffer(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode, MonopolyNode offeredNode, int offeredMoney, int requestedMoney)
    {
        int valueOfTheTrade = (CalculateValueOfNode(requestedNode)+requestedMoney) - (CalculateValueOfNode(offeredNode) + offeredMoney);
        //�÷��̾� A�� �÷��̾� B���� '�����ũ'(��ġ 400) �ε����� ��û�ϰ�, �߰��� 100�� ���� �����ϰ��� �մϴ�.
        // �÷��̾� A�� '��ũ �÷��̽�'(��ġ 300) �ε���� 50�� ���� �����մϴ�.//valueOfTheTrade ���: (400 + 100) - (300 + 50) = 500 - 350 = 150
        //���������, valueOfTheTrade�� 150�� �Ǹ�, �̴� �ŷ��� �÷��̾� A���� 150��ŭ �����ϴٴ� ���� ��Ÿ���ϴ�.
        //�� ����� �ŷ��� �󸶳� �������� �Ǵ� ��� ���� �� ���������� �Ǵ��ϴ� �� ���˴ϴ�.
        
        if (requestedNode == null && offeredNode != null && requestedMoney < nodeOwner.ReadMoney/3 && !MonopolyBoard.instance.PlayerHasAllNodesOfSet(requestedNode).allSame)
        //���� ��û�� �ε����� ����(requestedNode == null), ���ȵ� �ε����� ������(offeredNode != null),
        //��û�� �ݾ��� �ŷ� ������� ���� 1/3 �̸��� ���, �ŷ��� �����մϴ�.
        //���� �÷��̾ ��û�� �ε��� ��Ʈ�� �̹� ��� �����ϰ� ���� ���� ��� �ŷ��մϴ�.
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

        //�ŷ� ����ڰ� ��� �̵��� �� ����, ���� �÷��̾ ��û�� �ε��� ��Ʈ�� �̹� ��� �����ϰ� ���� ���� ��� �ŷ��մϴ�.

        else // �� �ܿ��� �޼���
        {
            if (currentPlayer.playerType == Player.PlayerType.HUMAN)
            {
                TradeResult(false);
            }
            Debug.Log("AI REJECTED TRADE OFFER");
        }
    }

    //------------------------------------------MonopolyNode (�ε���)�� ��ġ�� ���-----------------------------------------------------AI
    int CalculateValueOfNode(MonopolyNode requestedNode)
    {
        int value = 0;//: ���� value�� 0���� �ʱ�ȭ�մϴ�. �� ������ �ε����� �� ��ġ�� �����մϴ�.
        if (requestedNode != null)//�÷��̾ �ŷ��� ������ ��ǥ �ε����� ���� ��쿡�� ���
        {
            if (requestedNode.monopolyNodeType == MonopolyNodeType.Property)//: �ε��� ��� Ÿ���� �Ϲ� �ε����� ���
            { value = requestedNode.price + requestedNode.NumberOfHouses * requestedNode.houseCost;}
            //��ǥ�ε����� ����+ ��ǥ �ε����� ������ �ִ� ���� ���� * ���� ������ �ε����� �� ��ġ�� �Ҵ��մϴ�.
            else
            {value = requestedNode.price; }//�ε��� ��� Ÿ���� �Ϲ� �ε����� �ƴ� ���, �⺻ ���ݸ� ����մϴ�.
            return value;//���� �� ��ġ�� ��ȯ�մϴ�.
        }
        return value;//������ ��ǥ �ε����� ���� ��� 0�� ��ȯ �մϴ�.
    }

    //------------------------------------------�÷��̾� ���� �ŷ��� ó���մϴ�.-----------------------------------------------------AI
    void Trade(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode, MonopolyNode offeredNode, int offeredMoney, int requestedMoney)
    {
        if (requestedNode != null)//�ŷ� ��� �ε����� �����ϴ��� Ȯ��
        {
            currentPlayer.PayMoney(offeredMoney);//���� �÷��̾ ������ ���� ����
            requestedNode.ChangeOwner(currentPlayer);//�ŷ��� �ε����� �������� ���� �÷��̾�� ����
            nodeOwner.CollectMoney(offeredMoney);//�ŷ�����ڰ� ���� ����
            nodeOwner.PayMoney(requestedMoney); //�ŷ�����ڰ� �÷��̾�� ���� ����
            if (offeredNode != null) //�÷��̾ �ŷ�����ڿ��� �߰��� �����ϴ� �ε����� �ִٸ�, �� �����ǵ� ����
            {offeredNode.ChangeOwner(nodeOwner);}
            string offeredNodeName = (offeredNode != null) ? "$" + offeredNode.name : "";
            OnUpdateMessage.Invoke(currentPlayer.name + "trade" + requestedNode.name + "for" + offeredMoney + offeredNodeName +  "to" + nodeOwner.name);
        }
        else if (offeredNode != null && requestedNode == null)
        //�÷��̾ �ŷ�����ڿ��� �����ϴ� �ε����� �ְ�, ��ǥ�� �ϴ� �ε����� ���ٸ�,
        {
            currentPlayer.CollectMoney(requestedMoney);//���� �÷��̾ ���� �����Ѵ�.
            nodeOwner.PayMoney(requestedMoney);//�ŷ�����ڰ� �÷��̾�� ���� �����Ѵ�.
            offeredNode.ChangeOwner(nodeOwner);//�÷��̾ ������ �ε����� �����ְ� �ŷ�����ڷ� ����ȴ�.
            OnUpdateMessage.Invoke(currentPlayer.name + "sold" + offeredNode.name + "to" + nodeOwner.name + "for" + requestedMoney);
        }
        CloseTradePanel();
        if (currentPlayer.playerType == Player.PlayerType.AI)
        {
            currentPlayer.ChangeState(Player.AiState.IDLE);
        }
    }
    //Player currentPlayer: ���� ���� �����ϰ� �ִ� �÷��̾��Դϴ�.
    //Player nodeOwner: �ŷ� ����� �Ǵ� �ε����� �������Դϴ�.
    // MonopolyNode requestedNode: ���� �÷��̾ �䱸�ϴ�, �� �ŷ��� ���� ����� �ϴ� �ε����Դϴ�.
    //MonopolyNode offeredNode: ���� �÷��̾ �����ϰ��� �ϴ� �ε����Դϴ�.
    //int offeredMoney: �ŷ� �����(�ε��� ������) �� ���� �÷��̾�� �����ϰų� �޴� ���Դϴ�.
    //int requestedMoney: ���� �÷��̾ �ŷ� ����ڿ��� �����ϰų� �޴� ���Դϴ�.


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


    //-------------------------------MakeOfferButton()�ŷ� ���� �ý��ۿ��� ����ڰ� ������ �ŷ��� ó��----------------------------------------------HUMAN
    public void MakeOfferButton()
    {
        MonopolyNode requestedNode = null;
        MonopolyNode offeredNode = null;
        if (rightPlayerReference == null)
        { return;}

        Toggle offeredToggle = leftToggleGroup.ActiveToggles().FirstOrDefault();
        //leftToggleGroup ������ ���� Ȱ��ȭ��(����ڰ� ������) ù ��° ����� ã���ϴ�. �̰��� ����ڰ� �ŷ��� �����ϱ� ���� ������ �ε����� ã�� �� ���˴ϴ�.
        if (offeredToggle != null)
        {offeredNode = offeredToggle.GetComponentInParent<TradePropertyCard>().Node();}
        //������ ��ۿ� ����� TradePropertyCard ������Ʈ�� ã��, �ű⿡ ����� Node �Լ��� ȣ���Ͽ� �ش� �ε��� ��ü�� ������ ����ϴ�. �̷��� ���� offeredNode�� �ŷ� ������ �� �� ���˴ϴ�.

        Toggle requestedToggle = rightToggleGroup.ActiveToggles().FirstOrDefault();
        if (requestedToggle != null)
        {requestedNode = requestedToggle.GetComponentInParent<TradePropertyCard>().Node();}

        MakeTradeOffer(leftPlayerReference, rightPlayerReference, requestedNode, offeredNode, (int)leftMoneySlider.value, (int)rightMoneySlider.value);
    }
    //FirstOrDefault() �޼ҵ�� LINQ�� Ȯ�� �޼ҵ� �� �ϳ���, ���������� ù ��° ��Ҹ� ��ȯ�ϰų� �������� ��Ұ� ���� ��� �⺻���� ��ȯ�մϴ�. 
    //ActiveToggles() �Լ��� ��� �׷쿡�� Ȱ��ȭ�� ��� ����� ��ȯ�ϰ�, FirstOrDefault()�� �� �� ù ��°�� �����մϴ�.
    //�̷��� ����ϴ� ������ ���� � ��۵� Ȱ��ȭ���� �ʾ��� ��� null�� ��ȯ�ϱ� �����Դϴ�. 
    //�̴� First() �޼ҵ�� �ٸ���, ��Ұ� ���� �� ���ܸ� �߻���Ű�� �ʰ� �����ϰ� null�� ��ȯ�ϹǷ�, ���� ���� �ڵ��� �������� ���̴� ����Դϴ�.

    //------------------------------- Ʈ���̵� ���� Ȱ��ȭ �Ǵ� ��� â----------------------------------------------HUMAN

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
    //------------------------------- AI�� �ε��� �ŷ� ���� â----------------------------------------------HUMAN
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

        if (leftCard.activeInHierarchy)//leftCard��� ���� ������Ʈ�� ���� ������ ���� �������� Ȱ��ȭ�Ǿ� �ִ���(��, ȭ�鿡 ���̴���)�� Ȯ���մϴ�
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
        if (rightCard.activeInHierarchy)//rightCard��� ���� ������Ʈ�� ���� ������ ���� �������� Ȱ��ȭ�Ǿ� �ִ���(��, ȭ�鿡 ���̴���)�� Ȯ���մϴ�
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
