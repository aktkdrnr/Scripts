using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class CommunityChest : MonoBehaviour
{
    public static CommunityChest instance;
    [SerializeField] List<SCR_CommunityCards> cards = new List<SCR_CommunityCards>();
    [SerializeField] TMP_Text cardText;//카드의 텍스트를 표시
    [SerializeField] GameObject cardHolderBackground;//커뮤니티 카드의 배경을 담당
    [SerializeField] float showTime = 3;//카드를 화면에 표시하는 시간
    [SerializeField] Button closeCardButton;//카드를 닫을 때 사용할 버튼

    List<SCR_CommunityCards> cardPool = new List<SCR_CommunityCards>();//게임 중에 사용될 카드들을 보관
    List<SCR_CommunityCards> usedCardPool = new List<SCR_CommunityCards>();//이미 사용된 카드들이 저장

    SCR_CommunityCards jailFreeCard;
    SCR_CommunityCards pickedCard;//플레이어가 뽑은 카드를 저장
    Player currentPlayer;//현재 턴을 진행 중인 플레이어

    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool hasChanceJailCard, bool hasCommunityJailCard);//델리게이트는 메서드를 참조할 수 있는 타입
    public static ShowHumanPanel OnShowHumanPanel;//다른 스크립트에서 이 스크립트에 있는 메서드를 호출할 때 사용

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
        cardHolderBackground.SetActive(false);//카드 홀더의 배경이 보이지 않게 설정
        cardPool.AddRange(cards);//cards 리스트에 있는 모든 커뮤니티 카드를 cardPool 리스트에 추가
        ShuffleCards();
    }
    
    void ShuffleCards()  //각 반복마다 두 카드의 위치를 무작위로 교환하여, 최종적으로 카드들의 순서가 랜덤하게 되는 것입니다.
    {
        for (int i = 0; i < cardPool.Count; i++)
        {
            int index = Random.Range(0, cardPool.Count); //를 사용하여 무작위 인덱스를 생성합니다. 이는 0부터 cardPool.Count - 1 범위 내에서 무작위 숫자를 반환합니다.
            SCR_CommunityCards tempCard = cardPool[index]; // 무작위로 선택된 카드를 임시 변수에 저장합니다.
            cardPool[index] = cardPool[i];//현재 인덱스(i)의 카드를 무작위 인덱스(index) 위치에 놓습니다.
            cardPool[i] = tempCard; //임시 변수에 저장된 무작위 카드를 현재 인덱스에 놓습니다.
        }
    }

    void DrawCard(Player cardTaker)
    {
        pickedCard = cardPool[0];//cardPool의 첫 번째 카드 (0 인덱스)를 pickedCard 변수에 할당합니다. 
        cardPool.RemoveAt(0);//cardPool에서 첫 번째 카드를 제거합니다. 


        if (pickedCard.jailFreeCard)
        {jailFreeCard = pickedCard;}
        else
        { usedCardPool.Add(pickedCard);};//방금 뽑은 카드를 usedCardPool 리스트에 추가합니다. 이 리스트는 이미 사용된 카드를 추적합니다.

        if (cardPool.Count == 0)//더 이상 카드가 없으면
        {
            cardPool.AddRange(usedCardPool);//usedCardPool의 모든 카드를 cardPool에 다시 추가하고
            usedCardPool.Clear();//usedCardPool을 비운 다음
            ShuffleCards();// 카드를 다시 섞습니다
        }

        currentPlayer = cardTaker; //현재 카드를 뽑는 플레이어를 currentPlayer로 설정합니다.
        cardHolderBackground.SetActive(true);//카드 홀더의 배경을 활성화
        cardText.text = pickedCard.textOnCard;//뽑힌 카드의 텍스트를 표시
        if (currentPlayer.playerType == Player.PlayerType.AI)
        {
            closeCardButton.interactable = false;
            Invoke("ApplyCardEffect", showTime);//지정된 시간(showTime) 후에 ApplyCardEffect 메서드를 호출
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
        else if(pickedCard.moveToBoardIndex != -1)//-1이 아닌 경우, 플레이어는 보드의 특정 위치로 이동해야 합니다.
        {
            isMoving = true;

            int currentIndex = MonopolyBoard.instance.route.IndexOf(currentPlayer.MymonopolyNode);//현재 플레이어의 위치를 나타냅니다. 
            int lengthOfBoard = MonopolyBoard.instance.route.Count;//보드의 전체 길이를 나타냅니다. 
            int stepsToMove = 0;//이동해야 할 거리를 초기화.
            if (currentIndex < pickedCard.moveToBoardIndex)//플레이어는 앞으로 이동
            {
                stepsToMove = pickedCard.moveToBoardIndex - currentIndex;
            }
            else if (currentIndex > pickedCard.moveToBoardIndex)//플레이어는 보드를 한 바퀴 돌아 목적지에 도달
            {
                stepsToMove = lengthOfBoard - currentIndex + pickedCard.moveToBoardIndex;
            }
            MonopolyBoard.instance.MovePlayerToken(stepsToMove, currentPlayer); //계산된 거리만큼 currentPlayer를 이동시키는 함수를 호출합니다.
        }
        else if (pickedCard.collectFromPlayer)//현재 뽑힌 카드가 다른 플레이어들로부터 돈을 수집하는 카드인지 확인합니다.
        {
            int totalCollected = 0; //다른 플레이어들로부터 수집한 총 금액을 초기화
            List<Player> allPlayer = GameManager.instance.GetPlayers; //게임에 참여 중인 모든 플레이어의 목록

            foreach (var player in allPlayer)
            {
                if (player != currentPlayer)//현재 플레이어를 제외한 다른 플레이어들
                {
                    int amount = Mathf.Min(player.ReadMoney, pickedCard.rewardMoney);// 플레이어의 현재 금액과 카드에 명시된 금액 중 작은 금액을 선택합니다.
                    player.PayMoney(amount);//다른 플레이어로부터 해당 금액을 수집합니다.
                    totalCollected += amount;
                }
            }
            currentPlayer.CollectMoney(totalCollected);//현재 플레이어가 수집한 총 금액을 자신의 자산에 추가합니다.
        }
        else if (pickedCard.streetRepairs)
        {
            int[] allBuildings = currentPlayer.CountHousesAndHotels();//현재 플레이어가 소유한 집과 호텔의 수를 반환
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
        cardHolderBackground.SetActive(false); //카드 효과를 적용한 후 카드홀더 배경을 비활성화.
        ContinueGame(isMoving);// 카드 효과를 적용했다면, ContinueGame를 호출
    }

    void ContinueGame(bool isMoving)//게임의 규칙에 따라 플레이어의 행동을 결정하고, 게임의 다음 단계로 넘어갑니다.
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


