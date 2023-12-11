using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChanceFiled : MonoBehaviour
{
    public static ChanceFiled instance;
    [SerializeField] List<SCR_ChanceCard> cards = new List<SCR_ChanceCard>();
    [SerializeField] TMP_Text cardText;
    [SerializeField] GameObject cardHolderBackground;
    [SerializeField] float showTime = 3;
    [SerializeField] Button closeCardButton;

    List<SCR_ChanceCard> cardPool = new List<SCR_ChanceCard>();
    List<SCR_ChanceCard> usedCardPool = new List<SCR_ChanceCard>();

    SCR_ChanceCard jailFreeCard;
    SCR_ChanceCard pickedCard;
    Player currentPlayer;

    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool hasChanceJailCard, bool hasCommunityJailCard);
    public static ShowHumanPanel OnShowHumanPanel;
    private void OnEnable()
    {
        MonopolyNode.OnDrawChanceCard += DrawCard;
    }
    private void OnDisable()
    {
        MonopolyNode.OnDrawChanceCard -= DrawCard;
    }
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        cardHolderBackground.SetActive(false);
        cardPool.AddRange(cards);
        ShuffleCards();
    }
    void ShuffleCards()
    {
        for (int i = 0; i < cardPool.Count; i++)
        {
            int index = Random.Range(0, cardPool.Count);
            SCR_ChanceCard tempCard = cardPool[index];
            cardPool[index] = cardPool[i];
            cardPool[i] = tempCard;
        }
    }
    void DrawCard(Player cardTaker)
    {
        pickedCard = cardPool[0];
        cardPool.RemoveAt(0);

        if (pickedCard.jailFreeCard)
        { jailFreeCard = pickedCard;}
        else
        {usedCardPool.Add(pickedCard);}

        if (cardPool.Count == 0)
        {
            cardPool.AddRange(usedCardPool);
            usedCardPool.Clear();
            ShuffleCards();
        }

        currentPlayer = cardTaker;
        cardHolderBackground.SetActive(true);
        cardText.text = pickedCard.textOnCard;
        if (currentPlayer.playerType == Player.PlayerType.AI)
        {
            closeCardButton.interactable = false;
            Invoke("ApplyCardEffect", showTime);
        }
        else
        {
            closeCardButton.interactable = true;
        }
    }
    public void ApplyCardEffect()
    {
        bool isMoving = false;
        if (pickedCard.rewardMoney != 0)
        {
            currentPlayer.CollectMoney(pickedCard.rewardMoney);
        }
        else if (pickedCard.penalityMoney != 0 && !pickedCard.payToPlayer)
        {
            currentPlayer.PayMoney(pickedCard.penalityMoney);
        }
        else if (pickedCard.moveToBoardIndex != -1)
        {
            isMoving = true;

            int currentIndex = MonopolyBoard.instance.route.IndexOf(currentPlayer.MymonopolyNode);
            int lengthOfBoard = MonopolyBoard.instance.route.Count;
            int stepsToMove = 0;
            if (currentIndex < pickedCard.moveToBoardIndex)
            {
                stepsToMove = pickedCard.moveToBoardIndex - currentIndex;
            }
            else if (currentIndex > pickedCard.moveToBoardIndex)
            {
                stepsToMove = lengthOfBoard - currentIndex + pickedCard.moveToBoardIndex;
            }
            MonopolyBoard.instance.MovePlayerToken(stepsToMove, currentPlayer);
        }
        else if (pickedCard.payToPlayer)
        {
            int totalCollected = 0;
            List<Player> allPlayer = GameManager.instance.GetPlayers;

            foreach (var player in allPlayer)
            {
                if (player != currentPlayer)
                {
                    int amount = Mathf.Min(currentPlayer.ReadMoney, pickedCard.penalityMoney);
                    player.CollectMoney(amount);
                    totalCollected += amount;
                }
            }
            currentPlayer.PayMoney(totalCollected);
        }
        else if (pickedCard.streetRepairs)
        {
            int[] allBuildings = currentPlayer.CountHousesAndHotels();
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
            currentPlayer.AddChanceJailFreeCard();
        }
        else if (pickedCard.moveStepsBackwards != 0)
        {
            int steps = Mathf.Abs(pickedCard.moveStepsBackwards);
            MonopolyBoard.instance.MovePlayerToken(-steps, currentPlayer);
            isMoving = true;
        }
        else if (pickedCard.nextRailroad)
        {
            MonopolyBoard.instance.MovePlayerToken(MonopolyNodeType.Railroad, currentPlayer);
            isMoving = true;

        }
        else if (pickedCard.nextUtility)
        {
            MonopolyBoard.instance.MovePlayerToken(MonopolyNodeType.Utility, currentPlayer);
            isMoving = true;

        }
        cardHolderBackground.SetActive(false);
        ContinueGame(isMoving);
    }

    void ContinueGame(bool isMoving)
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
                OnShowHumanPanel.Invoke(true, GameManager.instance.RolledADouble, !GameManager.instance.RolledADouble ,jail1 ,jail2);
            }
        }
    }

    public void AddBackJailFreeCard()
    {
        usedCardPool.Add(jailFreeCard);
        jailFreeCard = null;
    }
}
