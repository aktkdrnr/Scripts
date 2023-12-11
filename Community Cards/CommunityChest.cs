using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class CommunityChest : MonoBehaviour
{
    public static CommunityChest instance;
    [SerializeField] List<SCR_CommunityCards> cards = new List<SCR_CommunityCards>();
    [SerializeField] TMP_Text cardText;//ī���� �ؽ�Ʈ�� ǥ��
    [SerializeField] GameObject cardHolderBackground;//Ŀ�´�Ƽ ī���� ����� ���
    [SerializeField] float showTime = 3;//ī�带 ȭ�鿡 ǥ���ϴ� �ð�
    [SerializeField] Button closeCardButton;//ī�带 ���� �� ����� ��ư

    List<SCR_CommunityCards> cardPool = new List<SCR_CommunityCards>();//���� �߿� ���� ī����� ����
    List<SCR_CommunityCards> usedCardPool = new List<SCR_CommunityCards>();//�̹� ���� ī����� ����

    SCR_CommunityCards jailFreeCard;
    SCR_CommunityCards pickedCard;//�÷��̾ ���� ī�带 ����
    Player currentPlayer;//���� ���� ���� ���� �÷��̾�

    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool hasChanceJailCard, bool hasCommunityJailCard);//��������Ʈ�� �޼��带 ������ �� �ִ� Ÿ��
    public static ShowHumanPanel OnShowHumanPanel;//�ٸ� ��ũ��Ʈ���� �� ��ũ��Ʈ�� �ִ� �޼��带 ȣ���� �� ���

    private void OnEnable()
    {
        MonopolyNode.OnDrawCommunityCard += DrawCard;
    }

    private void OnDisable()
    {
        MonopolyNode.OnDrawCommunityCard -= DrawCard;
    }

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        cardHolderBackground.SetActive(false);//ī�� Ȧ���� ����� ������ �ʰ� ����
        cardPool.AddRange(cards);//cards ����Ʈ�� �ִ� ��� Ŀ�´�Ƽ ī�带 cardPool ����Ʈ�� �߰�
        ShuffleCards();
    }
    
    void ShuffleCards()  //�� �ݺ����� �� ī���� ��ġ�� �������� ��ȯ�Ͽ�, ���������� ī����� ������ �����ϰ� �Ǵ� ���Դϴ�.
    {
        for (int i = 0; i < cardPool.Count; i++)
        {
            int index = Random.Range(0, cardPool.Count); //�� ����Ͽ� ������ �ε����� �����մϴ�. �̴� 0���� cardPool.Count - 1 ���� ������ ������ ���ڸ� ��ȯ�մϴ�.
            SCR_CommunityCards tempCard = cardPool[index]; // �������� ���õ� ī�带 �ӽ� ������ �����մϴ�.
            cardPool[index] = cardPool[i];//���� �ε���(i)�� ī�带 ������ �ε���(index) ��ġ�� �����ϴ�.
            cardPool[i] = tempCard; //�ӽ� ������ ����� ������ ī�带 ���� �ε����� �����ϴ�.
        }
    }

    void DrawCard(Player cardTaker)
    {
        pickedCard = cardPool[0];//cardPool�� ù ��° ī�� (0 �ε���)�� pickedCard ������ �Ҵ��մϴ�. 
        cardPool.RemoveAt(0);//cardPool���� ù ��° ī�带 �����մϴ�. 


        if (pickedCard.jailFreeCard)
        {jailFreeCard = pickedCard;}
        else
        { usedCardPool.Add(pickedCard);};//��� ���� ī�带 usedCardPool ����Ʈ�� �߰��մϴ�. �� ����Ʈ�� �̹� ���� ī�带 �����մϴ�.

        if (cardPool.Count == 0)//�� �̻� ī�尡 ������
        {
            cardPool.AddRange(usedCardPool);//usedCardPool�� ��� ī�带 cardPool�� �ٽ� �߰��ϰ�
            usedCardPool.Clear();//usedCardPool�� ��� ����
            ShuffleCards();// ī�带 �ٽ� �����ϴ�
        }

        currentPlayer = cardTaker; //���� ī�带 �̴� �÷��̾ currentPlayer�� �����մϴ�.
        cardHolderBackground.SetActive(true);//ī�� Ȧ���� ����� Ȱ��ȭ
        cardText.text = pickedCard.textOnCard;//���� ī���� �ؽ�Ʈ�� ǥ��
        if (currentPlayer.playerType == Player.PlayerType.AI)
        {
            closeCardButton.interactable = false;
            Invoke("ApplyCardEffect", showTime);//������ �ð�(showTime) �Ŀ� ApplyCardEffect �޼��带 ȣ��
        }
        else
        {
            closeCardButton.interactable = true;
        }
    }

    public void ApplyCardEffect()
    {
        bool isMoving = false;
        if (pickedCard.rewardMoney != 0 && !pickedCard.collectFromPlayer)
        {
            currentPlayer.CollectMoney(pickedCard.rewardMoney);
        }
        else if (pickedCard.penalityMoney != 0)
        {
            currentPlayer.PayMoney(pickedCard.penalityMoney);
        }
        else if(pickedCard.moveToBoardIndex != -1)//-1�� �ƴ� ���, �÷��̾�� ������ Ư�� ��ġ�� �̵��ؾ� �մϴ�.
        {
            isMoving = true;

            int currentIndex = MonopolyBoard.instance.route.IndexOf(currentPlayer.MymonopolyNode);//���� �÷��̾��� ��ġ�� ��Ÿ���ϴ�. 
            int lengthOfBoard = MonopolyBoard.instance.route.Count;//������ ��ü ���̸� ��Ÿ���ϴ�. 
            int stepsToMove = 0;//�̵��ؾ� �� �Ÿ��� �ʱ�ȭ.
            if (currentIndex < pickedCard.moveToBoardIndex)//�÷��̾�� ������ �̵�
            {
                stepsToMove = pickedCard.moveToBoardIndex - currentIndex;
            }
            else if (currentIndex > pickedCard.moveToBoardIndex)//�÷��̾�� ���带 �� ���� ���� �������� ����
            {
                stepsToMove = lengthOfBoard - currentIndex + pickedCard.moveToBoardIndex;
            }
            MonopolyBoard.instance.MovePlayerToken(stepsToMove, currentPlayer); //���� �Ÿ���ŭ currentPlayer�� �̵���Ű�� �Լ��� ȣ���մϴ�.
        }
        else if (pickedCard.collectFromPlayer)//���� ���� ī�尡 �ٸ� �÷��̾��κ��� ���� �����ϴ� ī������ Ȯ���մϴ�.
        {
            int totalCollected = 0; //�ٸ� �÷��̾��κ��� ������ �� �ݾ��� �ʱ�ȭ
            List<Player> allPlayer = GameManager.instance.GetPlayers; //���ӿ� ���� ���� ��� �÷��̾��� ���

            foreach (var player in allPlayer)
            {
                if (player != currentPlayer)//���� �÷��̾ ������ �ٸ� �÷��̾��
                {
                    int amount = Mathf.Min(player.ReadMoney, pickedCard.rewardMoney);// �÷��̾��� ���� �ݾװ� ī�忡 ��õ� �ݾ� �� ���� �ݾ��� �����մϴ�.
                    player.PayMoney(amount);//�ٸ� �÷��̾�κ��� �ش� �ݾ��� �����մϴ�.
                    totalCollected += amount;
                }
            }
            currentPlayer.CollectMoney(totalCollected);//���� �÷��̾ ������ �� �ݾ��� �ڽ��� �ڻ꿡 �߰��մϴ�.
        }
        else if (pickedCard.streetRepairs)
        {
            int[] allBuildings = currentPlayer.CountHousesAndHotels();//���� �÷��̾ ������ ���� ȣ���� ���� ��ȯ
            int totalCosts = pickedCard.streetRepairsHousePrice * allBuildings[0] + pickedCard.streetRepairsHotelPrice * allBuildings[1];
            currentPlayer.PayMoney(totalCosts);
        }
        else if (pickedCard.goToJail)
        {
            isMoving = true;
            currentPlayer.GoToJail(MonopolyBoard.instance.route.IndexOf(currentPlayer.MymonopolyNode));
        }
        else if (pickedCard.jailFreeCard)
        {
            currentPlayer.AddCommunityJailFreeCard();
        }
        cardHolderBackground.SetActive(false); //ī�� ȿ���� ������ �� ī��Ȧ�� ����� ��Ȱ��ȭ.
        ContinueGame(isMoving);// ī�� ȿ���� �����ߴٸ�, ContinueGame�� ȣ��
    }

    void ContinueGame(bool isMoving)//������ ��Ģ�� ���� �÷��̾��� �ൿ�� �����ϰ�, ������ ���� �ܰ�� �Ѿ�ϴ�.
    {
        if (currentPlayer.playerType == Player.PlayerType.AI)
        {
            if (!isMoving)
            {
                GameManager.instance.Continue();
            }
        }
        else
        {
            if (!isMoving)
            {
                bool jail1 = currentPlayer.HasChanceJailFreeCard;
                bool jail2 = currentPlayer.HasCommunityJailFreeCard;
                OnShowHumanPanel.Invoke(true, GameManager.instance.RolledADouble, !GameManager.instance.RolledADouble, jail1 ,jail2);
            }
        }
    }
    public void AddBackJailFreeCard()
    {
        usedCardPool.Add(jailFreeCard);
        jailFreeCard = null;
    }
}


