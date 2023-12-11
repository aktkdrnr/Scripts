using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable] // �� Ŭ������ �ν��Ͻ��� Unity �ν����Ϳ��� �� �� �ְ� ����ȭ ����
public class Player
{
    // �÷��̾� ������ �����ϴ� ������ (��� �Ǵ� AI)
    public enum PlayerType
    {
        HUMAN,
        AI
    }

    public PlayerType playerType; // �÷��̾� ���� (��� �Ǵ� AI)
    public string name; // �÷��̾��� �̸�
    int money; // �÷��̾ ���� ��

    MonopolyNode currentnode; // �÷��̾ ���� ��ġ�� ������ ���
    bool isInJail; // �÷��̾ ������ �ִ��� ����
    int numTurnsInJail = 0; // �÷��̾ �������� ���� �� ��
    [SerializeField] GameObject myToken; // �÷��̾��� ��ū (���� ������Ʈ)
    [SerializeField] List<MonopolyNode> myMonopolyNodes = new List<MonopolyNode>(); // �÷��̾ ������ ��� ���
    public List<MonopolyNode> GetMonopolyNodes => myMonopolyNodes;// �÷��̾ ������ ��� ���

    bool hasChanceJailFreeCard, hasCommunityJailFreeCard;

    public bool HasChanceJailFreeCard => hasChanceJailFreeCard;
    public bool HasCommunityJailFreeCard => hasCommunityJailFreeCard;

    PlayerInfo myInfo; // �÷��̾� ���� UI�� ������Ʈ�ϴµ� ���Ǵ� PlayerInfo ��ü

    int aiMoneySavity = 200; // AI�� ����� ���� ��

    public enum AiState
    {
        IDLE,
        TRADING
    }
    public AiState aiState;


    // ������ �޼����
    public bool IsInJail => isInJail; // �÷��̾ ������ �ִ��� ���θ� ��ȯ,�б� ���� �޼���
    public GameObject MyToken => myToken; // �÷��̾��� ��ū�� ��ȯ,�б� ���� �޼���
    public MonopolyNode MymonopolyNode => currentnode; // �÷��̾��� ���� ��带 ��ȯ,�б� ���� �޼���
    public int ReadMoney => money;

    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool hasChanceJailCard, bool hasCommunityJailCard);
    public static ShowHumanPanel OnShowHumanPanel;

    public void Inititialize(MonopolyNode startNode, int startMoney, PlayerInfo info, GameObject token)
    {
        currentnode = startNode; // ���� ��� ����
        money = startMoney; // ���� �ڱ� ����
        myInfo = info; // PlayerInfo ��ü ����
        myInfo.SetPlayerNameAndCash(name, money); // UI�� �÷��̾� �̸��� �ڱ� ǥ��
        myToken = token; // �÷��̾��� ��ū ����
        myInfo.ActivateArrow(false);
    }
    public void SetMyCurrentNode(MonopolyNode newNode) //�÷��̾ ���ο� ���(�ε��� ĭ)�� �������� �� ȣ��
    {
        currentnode = newNode; // ���� ��� ������Ʈ
        newNode.PlayerLandedOnNode(this); // �ش� ��忡 �÷��̾ �������� ���� ���� ó��

        if (playerType == PlayerType.AI)
        {
            CheckIfPlayerHasASet();//������ �ε��� ��Ʈ�� �����ϰ� �ִ��� Ȯ���ϰ�, ������ �����Ǹ� �ǹ��� �Ǽ��մϴ�.
            UnMortgageProperties();//����� �ڱ��� ���� ��� ���� ���� �ε����� �����մϴ�.
            //TradingSystem.instance.FindMissingProperty(this);
        }
         
    }
    public void CollectMoney(int amount)
    {
        money += amount; // �÷��̾��� ���� amount�� �߰�
        myInfo.SetPlayerCash(money); // UI�� �÷��̾��� ������Ʈ�� �ڱ� ǥ��
        if (playerType == PlayerType.HUMAN && GameManager.instance.GetCurrentPlayer == this )
        {
            bool canEndTurn = !GameManager.instance.RolledADouble && ReadMoney >= 0 && GameManager.instance.HasRolledDice;
            bool canRollDice = (GameManager.instance.RolledADouble && ReadMoney >= 0) || (!GameManager.instance.HasRolledDice && ReadMoney >= 0);
            OnShowHumanPanel.Invoke(true, canRollDice, canEndTurn, hasChanceJailFreeCard, hasCommunityJailFreeCard);
        }
    }
    internal bool CanAffordNode(int price)//���� �������� �÷��̾ ��带 �����Ϸ� �� �� ȣ��Ǿ�, ��带 ������ �� �ִ����� �Ǵ�
    { return price <= money;}
    //���� ���, ��带 �����ϴ� ����, ��� ����, �ǹ� �Ǽ� ���� �پ��� ��Ȳ���� ���� �� �ֽ��ϴ�.
    public void BuyProperty(MonopolyNode node)//�÷��̾ ���� ������ �ε����� ������ �� �ʿ��� ������ ó���� ����
                                              //�ش� �ε��� ����� �������� �÷��̾�� �����ϸ�, �÷��̾��� UI ������ ������Ʈ�ϴ� ����� ����
    {
        money -= node.price;
        node.SetOwner(this);
        myInfo.SetPlayerCash(money);
        myMonopolyNodes.Add(node);
        SortPropertiesByPrice();
    }
    void SortPropertiesByPrice()//�� �޼���� �÷��̾ ������ �ε��� ����Ʈ(myMonopolyNodes)�� ����(price)�� ���� �����մϴ�.
    { myMonopolyNodes = myMonopolyNodes.OrderBy(_node => _node.price).ToList();}
    internal void PayRent(int rentAmount, Player owner)//�� �Լ��� �÷��̾ �Ӵ�Ḧ ������ �� ȣ��Ǹ�, ������ ���� �����մϴ�:
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
    //�Ӵ�� ���� ���� Ȯ��: �÷��̾��� ���� �ݾ�(money)�� �Ӵ��(rentAmount)���� ���� ���, �� �÷��̾ �Ӵ�Ḧ ������ 
    //������ ���, �ּ����� ����ϴ� ��� ������ �ڱ��� ó���ϴ� ������ �����մϴ�. �� �κп����� ���� �ڵ尡 �ּ����� 
    //��ü�Ǿ� �־�, AI �÷��̾��� ��� � Ư���� ������ ������� ������ �� �ֽ��ϴ�. ���� ���, ������ ��� �ٸ� �ڻ��� 
    //�Ű��ϰų�, ���� ���� ó���� �� �� �ֽ��ϴ�.
    //�Ӵ�� ���� �� ����: �÷��̾��� �ݾ׿��� �Ӵ�Ḧ ����(money -= rentAmount)�ϰ�, �Ӵ�Ḧ ���� ������(owner)���� 
    //CollectMoney �Լ��� ȣ���Ͽ� �Ӵ�Ḧ �����մϴ�.
    //UI ������Ʈ: myInfo.SetPlayerCash(money)�� ȣ���Ͽ� UI�� ǥ�õǴ� �÷��̾��� ���� �ݾ��� ������Ʈ�մϴ�.
    //�Լ��� ������� ���鿡�� internal ���� �����ڴ� �ش� �Լ��� ������ ����� �������� ���� �������� �ǹ��մϴ�. 
    //��, �ٸ� ������Ʈ�� ����������� �� �Լ��� ����� �� �����ϴ�. PayRent �Լ��� ������ �ٽ� ���� �� �ϳ���, �÷��̾
    //�Ӵ�Ḧ �����ؾ� �� �� ���� ��Ģ�� ���� �ݾ��� �����ϴ� �� ���˴ϴ�.
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
    public void GoToJail(int indexOnBoard) // �÷��̾ �������� ���� �޼����Դϴ�.> PlayerLandedOnNode�� MonopolyNodeType.GoToJail��.
    {
        isInJail = true; // �÷��̾��� ���� ���¸� true�� �����մϴ�.

        // �÷��̾ �������� �̵���Ű�� �Լ��� ȣ���մϴ�.
        // �̵� �Ÿ��� ���� ��ġ���� ���������� �Ÿ��� ����Ͽ� �����մϴ�.
        MonopolyBoard.instance.MovePlayerToken(CalculateDistanceFromJail(indexOnBoard), this);
        GameManager.instance.ResetRolledADouble();
    }
    public void SetOutOfJail()// �÷��̾ �������� �ع��Ű�� �޼����Դϴ�.
    {
        isInJail = false; // �÷��̾��� ���� ���¸� false�� �����Ͽ� �������� �ع��ŵ�ϴ�.
        numTurnsInJail = 0;
    }
    int CalculateDistanceFromJail(int indexOnBoard) // ���� �÷��̾��� ��ġ���� ���������� �Ÿ��� ����ϴ� �޼����Դϴ�. > GoToJail��.
    {
        int result = 0; // ����� ������ ������ �ʱ�ȭ�մϴ�.
        int indexOfJail = 10; // ������ �ε����� ��Ÿ���� �����Դϴ�.

        // ���� �÷��̾��� �ε����� ���� �ε������� ū ��� (������ ����ģ ���)
        if (indexOnBoard > indexOfJail)
        {
            // ���������� �Ÿ��� ������ ����մϴ� (�ڷ� �̵��ؾ� ���� �ǹ�).
            result = (indexOnBoard - indexOfJail) * -1;
        }
        else
        {
            // ���������� �Ÿ��� ����� ����մϴ� (������ �̵��ؾ� ���� �ǹ�).
            result = (indexOfJail - indexOnBoard);
        }
        return result; // ���� �Ÿ��� ��ȯ�մϴ�.
    }
    public int NumTurnsInJail => numTurnsInJail;
    public void IncreaseNumTurnsInJail()
    {numTurnsInJail++;}
    public void HandleInsufficientFunds(int amountToPay)//�÷��̾ �����ؾ� �� �ݾ�(amountToPay)�� ���� ������ �ִ� ��(money)���� ������ �� ���� �� ȣ��
    {
        int housesToSell = 0; // �Ǹ��� ���� ��
        int allHouses = 0; // �÷��̾ ������ ���� �� ��
        int propertiesToMortgage = 0; // ���� ���� �� �ִ� �ε����� ��
        int allpropertiesToMortgage = 0; // �÷��̾ ������ ���� ���� �� �ִ� �ε����� �� ��

        foreach (var node in myMonopolyNodes) // �÷��̾ ������ ��� �ε��꿡�� ���� ���� �ջ��մϴ�.
        { allHouses += node.NumberOfHouses;}
        while (money < amountToPay && allHouses > 0)
        {
            foreach (var node in myMonopolyNodes)
            {
                housesToSell = node.NumberOfHouses;
                if (housesToSell > 0)// ���� �Ȱ� �� �ݾ��� �����մϴ�.
                {
                    CollectMoney(node.SellHouseOrHotel());
                    allHouses--;
                    if (money >= amountToPay) // �ʿ��� �ݾ��� ���Ǹ� �Լ��� �����մϴ�.
                    { return;}
                }
            }
        }
        foreach (var node in myMonopolyNodes)// �ε����� ���� ��Ƽ� ������ �ݾ��� ����Ϸ��� �õ��մϴ�.
        {
            allpropertiesToMortgage += (!node.IsMortgaged) ? 1 : 0;
        }
        while (money < amountToPay && allpropertiesToMortgage > 0)// �����ؾ� �� �ݾ��� �����ϰ�, ���� ������ ���� �� �ε����� 1�� �̻��̶��
        {
            foreach (var node in myMonopolyNodes)
            {
                propertiesToMortgage += (!node.IsMortgaged) ? 1 : 0;//���������� ���� �ε����� �� ������ Ȯ���մϴ�.
                if (propertiesToMortgage > 0) //���������� ���� ���� �ε����� �Ѱ� �̻��̶��,
                {
                    CollectMoney(node.MortgageProperty()); // �ε����� ���� ��� �� �ݾ��� �����մϴ�.
                    allpropertiesToMortgage--;
                    if (money >= amountToPay)
                    { return; } // �ʿ��� �ݾ��� ���Ǹ� �Լ��� �����մϴ�.
                }
            }
        }
        if (playerType == PlayerType.AI)// ��� ����� �õ��� �Ŀ��� �ݾ��� �����ϸ� ai�÷��̾ �Ļ� ó���մϴ�.
        { Bankrupt(); }
    }
    internal void Bankrupt() // �Ļ� �޼���: �÷��̾ �Ļ����� �� ȣ��˴ϴ�.  
    {
        OnUpdateMessage.Invoke(name + "is Bankrupt");
        
        for (int i = myMonopolyNodes.Count-1; i >= 0 ; i--)
        {
            myMonopolyNodes[i].ResetNode();// �� �ε��� ��带 �ʱ� ���·� �����մϴ�.
        }
        if (hasChanceJailFreeCard)
        {
            ChanceFiled.instance.AddBackJailFreeCard();
        }
       
        if (hasCommunityJailFreeCard)
        {
            CommunityChest.instance.AddBackJailFreeCard();
        }

        GameManager.instance.RemovePlayer(this); // ���� �Ŵ������� �÷��̾ ���ӿ��� �����մϴ�.
    }
    //for (int i = myMonopolyNodes.Count-1; i >= 0; i--) �� �ڵ�� �������� ����Ʈ�� �ݺ� ó���ϱ� ���� ���˴ϴ�. 
    //�� ����� �ַ� ����Ʈ ���� ��Ҹ� ������ �� ���Ǵµ�, ���������� �ݺ��ϸ鼭 ��Ҹ� ������ ��� ����Ʈ�� ũ�Ⱑ ���ϰ�,
    //�̷� ���� �ε��� ������ �߻��� �� �ֱ� �����Դϴ�.
    //���� ���, �÷��̾ 3���� �ε����� �����ϰ� �ִٰ� ������ ���ڽ��ϴ�. myMonopolyNodes�� �ε����� 0���� 2�����Դϴ�. 
    //���� �÷��̾ �Ļ����� ��, �� �ε����� �ʱ�ȭ�ؾ� �մϴ�.
    //������ �ݺ�(for (int i = 0; i < myMonopolyNodes.Count; i++))�� ����ϸ� ù ��° ���(�ε��� 0)�� ������ ��, 
    //����Ʈ�� ũ�Ⱑ �پ��� �ε��� 1�� 2�� ��Ұ� ���� �ε��� 0�� 1�� �̵��մϴ�. �� ���, ���� �ε��� 2�� �ִ� ��Ҵ� �ݺ������� ������ �� �ֽ��ϴ�.
    //���� �ݺ��� ����ϸ� ����Ʈ�� ������ ��Һ��� �����Ͽ� �� ��Ҹ� �����ϰ�, ����Ʈ�� ũ�Ⱑ �پ����� �̹� ó���� ��ҿ��� ������ ���� �ʽ��ϴ�. 
    //�� ������� ����Ʈ�� ��� ��Ҹ� �����ϰ� ó���� �� �ֽ��ϴ�.

    //myMonopolyNodes.Count - 1�� ����ϴ� ������ �迭�� ����Ʈ�� �ε����� 0���� �����ϱ� �����Դϴ�.
    //Count �Ӽ��� ����Ʈ�� �� ��� ���� ��ȯ�ϸ�, �ε����� 0���� �����ϹǷ� ������ ����� �ε����� Count - 1�� �˴ϴ�.
    //���� ���, ����Ʈ�� 5���� ��Ұ� �ִٸ� Count�� 5������, ������ ����� �ε����� 4(0, 1, 2, 3, 4)�Դϴ�. 
    //���� for ������ ����Ͽ� ����Ʈ�� ��� ��ҿ� �����Ϸ��� myMonopolyNodes.Count - 1���� �����Ͽ�
    //0�� ������ ������ ���ҽ�Ű�� ���� �ùٸ� ����Դϴ�.
    
    void UnMortgageProperties()//�÷��̾ ����� �ڱ��� ������ ���� ���� ������ �����ϵ��� ����Ǿ� ������, AI �÷��̾��� ��� Ư�� �ݾ�(aiMoneySavity)�� �����ϱ� ���� ���˴ϴ�.
    {
        foreach (var node in myMonopolyNodes)
        {
            if (node.IsMortgaged)//���� ��尡 ���� �����ٸ�(IsMortgaged�� true), ���� ���� ����� ����մϴ�. �� ����� ���� �ݾ�(MorgageValue)�� 10%�� �߰� ���(����)�� ���� �ݾ��Դϴ�.
            {
                int cost = node.MorgageValue + (int)(node.MorgageValue * 0.1f);
                
                if (money >= aiMoneySavity + cost)//�÷��̾��� ���� �ݾ�(money)�� AI�� ���� �ݾ�(aiMoneySavity)�� ���� ���� ����� ���� �ݾ׺��� ���ٸ�, ������ ������ �� �ֽ��ϴ�.
                {
                    PayMoney(cost);
                    node.UnMortgageProperty();
                }
            }
        }
    }
    public int[] CountHousesAndHotels()// ������� ���ӿ��� �÷��̾ ������ ���� ȣ���� ���� ����մϴ�.
    {
        int houses = 0; // ���� ���� �� �����Դϴ�. �迭�� �ε��� 0�� �ش��մϴ�.
        int hotels = 0; // ȣ���� ���� �� �����Դϴ�. �迭�� �ε��� 1�� �ش��մϴ�.
        foreach (var node in myMonopolyNodes) // �÷��̾ ������ ��� ��带 ��ȸ�մϴ�.
        {
            if (node.NumberOfHouses != 5)
            {houses += node.NumberOfHouses; }// ���� ���� �ش� ����� �� ������ŭ ������ŵ�ϴ�.
            else { hotels += 1;}
        }
        int[] allBuildings = new int[2] { houses, hotels };// ���� ȣ���� �� ���� ���� �迭�� �����մϴ�.
        return allBuildings;       // ������ �迭�� ��ȯ�մϴ�.
    }
    void CheckIfPlayerHasASet() // �÷��̾ ������ �ε��� ��Ʈ(��: ��� �������� �ε���)�� �����ϰ� �ִ��� üũ�� ���� ������ �ǹ��� �Ǽ��ϴ� �Լ��Դϴ�.
    {
        List<MonopolyNode> processedSet = null; // �ѹ��� �ϳ��� �ǹ� �� �������� ��� �߿� �ϳ�, break; ���� ����� �� �����ϳ� ������� �ִ�. 1��
        foreach (var node in myMonopolyNodes) // �÷��̾ ������ ��� ��忡 ���� �ݺ��մϴ�.
        {
            var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(node); // ���� ��尡 ���� ��Ʈ�� ��� ��带 �����ɴϴ�. 'allSame'�� �÷��̾ �� ��Ʈ�� ��� ��带 �����ϰ� �ִ��� �����Դϴ�.
            if (!allSame) // ���� �÷��̾ ��Ʈ�� ��� ��带 �����ϰ� ���� �ʴٸ�, ���� ��Ʈ�� �Ѿ�ϴ�.
            { continue;}
            List<MonopolyNode> nodeSet = list;// ������ ����Ʈ�� ��弼Ʈ�� �����մϴ�.
            if (nodeSet != null && nodeSet != processedSet) // nodeSet != processedSet �ѹ��� �ϳ��� �ǹ� �� �������� ��� �߿� �ϳ�, break; ���� ����� �� �����ϳ� ������� �ִ�. 2��
            {
                bool hasMortgagedNode = nodeSet.Any(node => node.IsMortgaged) ? true : false;
                //�ڵ��� �� �κ��� LINQ(Language Integrated Query)�� ����Ͽ� nodeSet �÷��ǿ��� � ������ �����ϴ� ��Ұ� �ִ��� Ȯ���մϴ�. ��ü������,
                //nodeSet ���� ��� MonopolyNode ��ü�� �߿��� IsMortgaged �Ӽ��� true�� ��ü�� �ϳ��� �ִ��� �˻��մϴ�.
                if (!hasMortgagedNode)// ���� ���� ��尡 ���ٸ�, �ǹ��� �Ǽ��� �� �ִ� ������ üũ�մϴ�.
                {
                    if (nodeSet[0].monopolyNodeType == MonopolyNodeType.Property)// �ش� ��� ��Ʈ�� ù ��° ��尡 �ε��� �����̶��, BuildHouseOrHotelEvenly �޼��带 ȣ���մϴ�.
                    {
                        BuildHouseOrHotelEvenly(nodeSet);//�ǹ��� �����ϰ� �Ǽ��ϴ� �޼����Դϴ�.
                        processedSet = nodeSet; // �� ���� �ϳ��� �ǹ� �� �������� ��� �߿� �ϳ�, break; ���� ����� �� �����ϳ� ������� �ִ�. 3��
                    }
                }
            }
        }
    }
    internal void BuildHouseOrHotelEvenly(List<MonopolyNode> nodesToBuildOn)// �ε��� ��Ʈ�� ���̳� ȣ���� �����ϰ� �Ǽ��ϱ� ���� �޼����Դϴ�.
    {
        // �ּ� �� �ִ� �� ������ �����ϱ� ���� ������ �ʱ�ȭ�մϴ�.
        int minHouses = int.MaxValue; // ������ ���� ū ������ �����մϴ�. ������ �Ʒ���,
                                      //minHouses�� ���� ���� ���� ���� ���� �ε����� ã�� ���� ���Ǹ�, 
                                      //������ ���� ������ ���� ū ���� int.MaxValue�� �ʱ�ȭ�˴ϴ�. 
                                      //�̴� � �ε����� �� ���� �� ������ �۱� ������, ù �񱳿��� ��� ������Ʈ�˴ϴ�.
        int maxHouses = int.MinValue; // ������ ���� ���� ������ �����մϴ�. �Ʒ����� ����
                                      //maxHouses�� ���� ���� ���� ���� ���� �ε����� ã�� ���� ���Ǹ�, 
                                      //������ ���� ������ ���� ���� ���� int.MinValue�� �ʱ�ȭ�˴ϴ�. 
                                      //�̴� � �ε����� �� ���� �� ������ ũ�� ������, ù �񱳿��� ��� ������Ʈ�˴ϴ�.

        foreach (var node in nodesToBuildOn)// �־��� �ε��� ��Ʈ�� ���� �Ǽ��� ���� �ּ� �� �ִ� ������ ã���ϴ�.
        {
            int numOfHouses = node.NumberOfHouses; // ���� ����� �� ������ �����ɴϴ�.

            if (numOfHouses < minHouses) //numOfHouses�� minHouses�� ������Ʈ �մϴ�.
            { minHouses = numOfHouses;}
            if (numOfHouses > maxHouses && numOfHouses < 5)
            {maxHouses = numOfHouses; }
        }
        foreach (var node in nodesToBuildOn)// �ε��� ��Ʈ�� �ִ� ��� ������ŭ ���� �����մϴ�.
        {
            if (node.NumberOfHouses == minHouses && node.NumberOfHouses < 5 && CanAffordHouse(node.houseCost))// ���� ����� �� ������ �ּ� �� ������ ����,
                                                                                                              // ���� 5�� �̸��� ���� �ִٸ�,(0,1,2,3,4)�ش� ��忡 ���� �Ǽ��� �� �ֽ��ϴ�.
            {
                node.BuildHouseOrHotel();// ���� �Ǽ��ϴ� �޼��带 ȣ���մϴ�.
                PayMoney(node.houseCost);// �÷��̾��� ���� �� ��븸ŭ �����ϴ� �ڵ��Դϴ�.
                break;                
            }
        }
    }
    //break; ���� �ݺ����� �����ϴ� �� ���Ǵ� Ű�����Դϴ�. �� �ڵ忡�� break; ���� foreach ������ ��� �����ϰ� ���� �ٱ��� �ڵ� ������ ����մϴ�.
    //�� �ڵ忡�� break; ���� if ���ǹ��� ���� �� ����˴ϴ�. ��, ���� ����� �� ������ �ּ� �� ������ ����, ���� 5�� �̸��� ���� ������, 
    //�÷��̾ ���� �� �� �ִ� ��쿡�� ����˴ϴ�. �� ���, node.BuildHouseOrHotel(); �޼��带 ȣ���Ͽ� ���� �Ǽ��ϰ�, 
    //PayMoney(node.houseCost);�� ȣ���Ͽ� �÷��̾��� ���� �� ��븸ŭ ������ ��, break; ���� �����Ͽ� foreach ������ ��� �����մϴ�.
    //�̷��� �ϸ� �� ���� ���� �ݺ����� �� ���� ���� �Ǽ��� �� �ֽ��ϴ�. ��, break; ���� �ε��� ��Ʈ�� ���� �����ϰ� �й��ϴ� �� ������ �˴ϴ�. 
    //�� ���� �ݺ����� �� ���� ���� �Ǽ��ϹǷ�, ��� �ε��꿡 ������ ������ ���� �Ǽ��� ������ �� ������ �ݺ��� �� �ֽ��ϴ�. 
    //�̷��� �ϸ� ��� �ε��꿡 ���� �����ϰ� �й�Ǹ�, �̴� ������� ������ ��Ģ�� ���� ���Դϴ�. �� ��Ģ�� ������, �÷��̾�� �� �ε��꿡 
    //��� ���� �Ǽ��ϱ� ���� ��� �ε��꿡 ������ ������ ���� �Ǽ��ؾ� �մϴ�. ���� break; ���� �� ��Ģ�� �ؼ��ϴ� �� ������ �˴ϴ�.

    //�� �ε��� A, B, C�� �ְ� ���� ������ ���� ���� ���� ������ �ִٰ� ������ ���ô�:
    //A �ε���: 1ä�� �� B �ε���: 2ä�� �� C �ε���: 1ä�� ��
    //���� �� �ε������ nodesToBuildOn ����Ʈ�� �ְ� foreach ������ �����մϴ�.
    //ù ��° �ݺ����� A �ε����� ����, numOfHouses�� 1�Դϴ�. minHouses�� int.MaxValue�̹Ƿ� A �ε����� �� ���� �� �۽��ϴ�. 
    //���� minHouses�� 1�� ������Ʈ�˴ϴ�. maxHouses�� int.MinValue�̹Ƿ� A �ε����� �� ���� �� Ů�ϴ�. maxHouses�� 1�� ������Ʈ�˴ϴ�.
    //���� B �ε����� ����, numOfHouses�� 2�Դϴ�. minHouses�� ���� 1�̹Ƿ� ������� �ʽ��ϴ�. 
    //maxHouses�� 1�̹Ƿ� B �ε����� �� ���� �� ũ�� 5 �̸��̹Ƿ� maxHouses�� 2�� ������Ʈ�˴ϴ�.
    //���������� C �ε����� ����, numOfHouses�� 1�Դϴ�. minHouses�� �̹� 1�̹Ƿ� ������� �ʽ��ϴ�. maxHouses�� ������� �ʽ��ϴ�.
    //������ ������ minHouses�� 1, maxHouses�� 2�� �˴ϴ�. �� ������ ����Ͽ� �÷��̾�� A�� C �ε��� �� �� ���� �߰��� ���� ���� �� �ֽ��ϴ�
    //(�� �� minHouses�� ������ ���� ���� ������ �ֱ� ������). B �ε��꿡�� �̹� 2ä�� ���� �����Ƿ�, �̹� �Ǽ� ���ʿ��� ���� �߰����� �ʽ��ϴ�.

    internal void SellHouseEvenly(List<MonopolyNode> nodesToSellFrom)// ���� �Լ��� ����� SellHouseEvenly,
                                                                     // �ٸ� Ŭ�������� ���� ������ �� ������ ���� ����� �������� ȣ���� �� �ֽ��ϴ�.
    {
        int minHouses = int.MaxValue;// �� �ε��꿡 ������ ���� �ּ� ���� ������ ������ �ʱ�ȭ�մϴ�
        bool houseSold = false;
        foreach (var node in nodesToSellFrom)
        {
            minHouses = Mathf.Min(minHouses, node.NumberOfHouses); // Mathf.Min�� �� �߿� �� ���� ���� �޽��ϴ�.
        }
        for (int i = nodesToSellFrom.Count-1; i >= 0; i--)// �ε��� ����� �������� ��ȸ�մϴ�. �̴� ���� �������� �߰��� �ε������ Ȯ���ϱ� �����Դϴ�.
        {
            if (nodesToSellFrom[i].NumberOfHouses > minHouses)// ���� ��忡 ������ ���� ���� minHouses���� ������, ���� �Ǹ��մϴ�.
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
    //for ���� �ʱ�ȭ �κп��� i�� nodesToSellFrom.Count - 1�� �����մϴ�. 
    //�̴� ����Ʈ�� ������ �ε����� ��Ÿ���ϴ�. (����Ʈ�� 0���� �����ϹǷ�, ����� �������� 1�� ���� ������ ����� �ε����� �˴ϴ�.)
    //���� �κп��� i >= 0�� i�� 0 �̻��� ���� ������ ��� �����϶�� ���� �ǹ��մϴ�. ��, ù ��° ��ҿ� ������ ������ ������ ��ӵ˴ϴ�.
    //�� �ݺ� �Ŀ� i--�� i�� ���� 1�� ���ҽ�ŵ�ϴ�. ��, ���� �ݺ� ���� ���� ��Ҹ� Ȯ���ϰ� �˴ϴ�.
    //����:
    //�ε��� ��Ʈ�� ������ ���� �ִٰ� ������ ���ô�:
    //�ε��� 0 (A �ε���): 2ä�� ��  
    //�ε��� 1 (B �ε���): 3ä�� ��  
    //�ε��� 2 (C �ε���): 2ä�� ��
    //�ε��� ��Ʈ�� ���� �� ��, ���� ���� ���� �ִ� �ε������ �ǸŸ� �����ؾ� �մϴ�. �� ��� B �ε����Դϴ�.

    //for ������ ������ ���� ����˴ϴ�:
    //i�� 2�� �����մϴ� (nodesToSellFrom.Count - 1�̹Ƿ�). i�� 2�� ��� (C �ε���), ���� ���� 2ä�Դϴ�. 
    //�̴� minHouses (���� ���� ���� ��)�� �����Ƿ� �� �ε����� �����ϰ� ���� �ݺ����� �Ѿ�ϴ�.
    //i�� 1�� ���ҽ�ŵ�ϴ� (i--). i�� 1�� ��� (B �ε���), ���� ���� 3ä�Դϴ�. 
    //�̴� minHouses���� �����Ƿ� �� �ε��꿡�� ���� �Ǹ��� �� �ֽ��ϴ�. ���� �ϳ� �Ǹ��ϰ� ������ �������ɴϴ� (break).
    //���� B �ε����� �Ǹ� ����� �ƴϾ��ٸ�, i�� 0���� ���ҽ��� A �ε����� Ȯ���մϴ�. A �ε��� ���� ���� 2ä�̹Ƿ� �Ǹ� ����� �ƴմϴ�.
    //�� �������� �÷��̾�� ��Ʈ ������ ���� ���� ���� �ִ� �ε������ ���� �����ϰ� �Ǹ��� �� �ֽ��ϴ�.

    //------------------------------------ if (!houseSold)-------------------------------------
    //��� ��忡 ������ ���� ��: ���� ��� ��� ��Ʈ�� ������ ���� ���� ������ ������, minHouses ���� �� ���� ���� ���� �˴ϴ�. �� ���, 
    //� ��忡���� if (nodesToSellFrom[i].NumberOfHouses > minHouses) ������ ������Ű�� ���ϹǷ�, ���� �Ǹŵ��� �ʽ��ϴ�.

    //���� �Ǹ����� ���� ��Ȳ�� ���� ���: houseSold ������ ���� �ǸŵǾ������� �����մϴ�.
    //���� ��� ��带 ������������ �ұ��ϰ� ���� �Ǹŵ��� �ʾҴٸ� (houseSold == false), �� ������ ����˴ϴ�.

    //������ ��忡���� �� �Ǹ� �õ�: if (!houseSold) ������ �ε��� ����� ������ ��忡�� ���� �Ǹ��Ϸ��� �õ��մϴ�. 
    //�̴� �÷��̾ �ڱ��� ���� �� �ֵ��� �ϴ� "��� ��ġ"�� �� �� �ֽ��ϴ�.

    //����, �� ������ ��� ��尡 ������ ���� ���� ������ �־ �Ϲ����� �������δ� ���� �Ǹ��� �� ���� ��Ȳ�� ���� ���� ó���� �߿��� ������ �մϴ�. 
    //�̷��� ������� ������ ������ �����ϰ�, �÷��̾ ������ �ڱ��� ���� ���ϴ� ��Ȳ�� ������ �� �ֽ��ϴ�.

    //nodesToSellFrom.Count-1�� ����Ʈ�� ������ �ε����� ��Ÿ���ϴ�. �̴� ����Ʈ�� �ε����� 0���� �����ϱ� ������, ����Ʈ�� ũ��(Count)���� 1�� ���� ������ ����� �ε����� �˴ϴ�.
    //���� ���, nodesToSellFrom ����Ʈ�� 5���� ������� ��尡 �ִٰ� �����սô�. �� ���, nodesToSellFrom.Count�� 5�� �˴ϴ�. ����Ʈ�� �ε����� 0���� �����ϹǷ�,
    //������ ����� �ε����� 5 - 1 = 4�� �˴ϴ�. ����, nodesToSellFrom[4]�� ����Ʈ�� ������ ��带 �����ϰ� �˴ϴ�.


    public bool CanAffordHouse(int price)//���� ������ ������ �ִ��� Ȯ���ϴ� �޼���
    {
        if (playerType == PlayerType.AI)// ai�� ���� �� �ӴϿ��� �ּ� �ݾ�200�� ������ ������ �ݾ���, ���� ���ݺ��� ũ�ų� ���ٸ� ���� ��ȯ�Ѵ�.
        { return (money - aiMoneySavity) >= price;}
        return money >= price; 
    }
    public void ActivateSelector(bool active)
    {
        myInfo.ActivateArrow(active);
    }

    public void AddProperty(MonopolyNode node)//�� �ε��� ����Ʈ�� �ε����� �߰��մϴ�.
    {
        myMonopolyNodes.Add(node);
        SortPropertiesByPrice();
    }
    public void RemoveProperty(MonopolyNode node)//�� �ε��� ����Ʈ���� �ε����� �����մϴ�.
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
