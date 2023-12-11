using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] MonopolyBoard gameBoard;
    [SerializeField] List<Player> playerList = new List<Player>();
    [SerializeField] int currentPlayer;
    [Header("Global Game Settings")]
    [SerializeField] int maxTurnsInJail = 3;
    [SerializeField] int startMoney = 1500;
    [SerializeField] int goMoney = 500;
    [SerializeField] float secondsBetweenTurns = 3;
    [Header("PlayerInfo")]
    [SerializeField] GameObject playerInfoPrefab;
    [SerializeField] Transform playerPanel;
    [SerializeField] List<GameObject> playerTokenList = new List<GameObject>();
    [Header("Game Over/ Win Info")]
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] TMP_Text winnerNameText;
    [Header("Dice")]
    [SerializeField] Dice _dice1;
    [SerializeField] Dice _dice2;


    List<int>rolledDice = new List<int>();
    bool rolledADouble;
    public bool RolledADouble => rolledADouble;
    public void ResetRolledADouble() => rolledADouble = false;//이는 플레이어가 주사위를 굴려서 더블이 아닌 결과가 나왔을 때 사용됩니다.
    int doubleRollCount;
    bool hasRolledDice;
    public bool HasRolledDice => hasRolledDice;

    int taxPool = 0;




    public int GetGoMoney => goMoney;
    public float SecondsBetweenTurns => secondsBetweenTurns;
    public List<Player> GetPlayers => playerList;//게임에 참여하는 모든 플레이어의 목록을 반환하는 프로퍼티입니다.
    public Player GetCurrentPlayer => playerList[currentPlayer];
    //현재 턴을 진행하고 있는 플레이어 객체를 반환하는 프로퍼티입니다. 이는 현재 턴의 플레이어가 행동을 취할 때 사용됩니다.




    public delegate void UpdateMessage(string message);//이 델리게이트를 사용하면 문자열 매개변수를 받고 반환값이 없는 모든 메서드를 참조할 수 있습니다.
    public static UpdateMessage OnUpdateMessage;//static 메서드는 클래스의 인스턴스가 아니라 클래스 자체에 속합니다. 따라서 static 메서드는 클래스의 모든 인스턴스가 아니라, 클래스 자체를 통해 접근되고 호출됩니다.
                                                //static 메서드는 클래스의 모든 인스턴스에 공통으로 적용되는 기능을 제공하는 데 적합합니다. 예를 들어, 유틸리티 함수나 계산 함수와 같은 것들이 static 메서드로 자주 구현됩니다.
                                                // 클래스의 각 인스턴스에 대해 별도의 메모리를 할당하지 않기 때문에, 여러 인스턴스에서 공통의 메서드를 사용해야 할 때 유용합니다.

    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn , bool hasChanceJailCard, bool hasCommunityJailCard);
    public static ShowHumanPanel OnShowHumanPanel;



    //-------------------- DEBUG---------------------------
   // [SerializeField] bool alwaysDoubleRoll = false;// 주사위가 더블만 계속 나오게 하는 테스트용.
    [SerializeField] bool forceDiceRolls;
    [SerializeField] int dice1;
    [SerializeField] int dice2;

    void Awake()
    {instance = this;}
     void Start()
    {
        currentPlayer = Random.Range(0, playerList.Count);
        gameOverPanel.SetActive(false);
        Inititialize();
        if (playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            //RollDice();
            RollPhysicalDice();
        }
        else
        {
            OnShowHumanPanel.Invoke(true, true, false, false, false);
        }
    }
    void Inititialize()//게임 시작시 초기화 셋팅
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            GameObject infoObject = Instantiate(playerInfoPrefab, playerPanel, false);
            PlayerInfo info = infoObject.GetComponent<PlayerInfo>();

            int randomIndex = Random.Range(0, playerTokenList.Count);
            GameObject newToken = Instantiate(playerTokenList[randomIndex], gameBoard.route[0].transform.position, Quaternion.identity);


            playerList[i].Inititialize(gameBoard.route[0], startMoney, info, newToken);
        }
        playerList[currentPlayer].ActivateSelector(true);//현재 플레이어의 이름 옆에 화살표 모양의 에니메이션을 활성화 합니다.
        if (playerList[currentPlayer].playerType == Player.PlayerType.HUMAN)//플레이어가 인간이라면 쇼패널을 활성화 합니다.
        {
            bool jail1 = playerList[currentPlayer].HasChanceJailFreeCard;
            bool jail2 = playerList[currentPlayer].HasCommunityJailFreeCard;
            OnShowHumanPanel.Invoke(true, true, false, jail1, jail2);
        }
        else
        {
            bool jail1 = playerList[currentPlayer].HasChanceJailFreeCard;
            bool jail2 = playerList[currentPlayer].HasCommunityJailFreeCard;
            OnShowHumanPanel.Invoke(false, false, false, jail1, jail2);
        }
    }
    //-------------------GameObject infoObject = Instantiate(playerInfoPrefab, playerPanel, false);------------------------------------
    //Instantiate 함수에서 false 값은 부모 오브젝트에 대한 infoObject의 위치, 회전, 그리고 크기를 전역 좌표 기준이 아닌 
    //로컬 좌표 기준으로 설정할 것인지를 결정합니다. 여기서 false는 infoObject가 playerPanel의 자식으로 생성될 때, 
    //playerPanel의 변환 정보를 상속받지 않고, 대신 자체적인 위치, 회전, 크기 정보를 유지하게 하는 설정입니다. 
    //즉, infoObject는 playerPanel의 위치나 회전에 영향을 받지 않고 원점(0, 0, 0) 위치에 생성됩니다.
    //--GameObject newToken = Instantiate(playerTokenList[randomIndex], gameBoard.route[0].transform.position, Quaternion.identity);------
    //Quaternion.identity는 회전 없이 오브젝트의 기본 방향을 나타내는 용어입니다. 
    //유니티에서 Quaternion은 오브젝트의 회전을 나타내는데 사용되며, 
    //Quaternion.identity는 오브젝트가 어떠한 회전도 적용받지 않을 때의 기본 상태를 의미합니다.
    //이를 사용하면, 새로 생성된 게임 오브젝트(newToken)가 월드의 기본 방향으로 정렬되어 나타납니다.

    public void RollPhysicalDice()
    {
        CheckForJailFree();
        rolledDice.Clear();
        _dice1.RollDice();
        _dice2.RollDice();
    }

    void CheckForJailFree()
    {
        if (playerList[currentPlayer].IsInJail && playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            if (playerList[currentPlayer].HasChanceJailFreeCard)
            {
                playerList[currentPlayer].UseChanceJailFreeCard();
            }
            else if (playerList[currentPlayer].HasCommunityJailFreeCard)
            {
                playerList[currentPlayer].UseCommunityJailFreeCard();
            }
        }

    }
    public void ReportDiceRolled(int diceValue)
    {
        rolledDice.Add(diceValue);
        if (rolledDice.Count==2)
        {
            RollDice();
        }
    }




    void RollDice()
    {
        
        bool allowedToMove = true;// 플레이어가 이동할 수 있는지 여부를 결정하는 변수입니다.
        hasRolledDice = true; // 주사위를 굴렸는지 여부

           // rolledDice = new int[2];// 두 개의 주사위 값을 저장할 배열을 초기화합니다.
/*        rolledDice[0] = Random.Range(1, 7);// 첫 번째와 두 번째 주사위를 각각 굴려 1부터 6 사이의 값을 무작위로 설정합니다.
        rolledDice[1] = Random.Range(1, 7);
//---------------------------------------------------------------------디버그-------------------------------------------------------------------------------------------
        if (alwaysDoubleRoll){ rolledDice[0] = 1;  rolledDice[1] = 1; }// 주사위가 더블만 계속 나오게 하는 테스트용.
        if (forceDiceRolls){ rolledDice[0] = dice1; ; rolledDice[1] = dice2;}
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        rolledADouble = rolledDice[0] == rolledDice[1];// 두 주사위의 값이 같은지 확인하여 더블 여부를 결정합니다.

        if (playerList[currentPlayer].IsInJail)// 현재 플레이어가 감옥에 있는지 확인합니다.
        {
            playerList[currentPlayer].IncreaseNumTurnsInJail();//numTurnsInJail을 증가시킵니다.
            if (rolledADouble)// 주사위가 더블이 나왔다면 플레이어를 감옥에서 해방시킵니다.numTurnsInJail을 리셋합니다.
            {
                playerList[currentPlayer].SetOutOfJail();
                OnUpdateMessage.Invoke(playerList[currentPlayer].name + "<color=green>can leave jail</color>, because a double was rolled");
                //Invoke(...): 델리게이트에 연결된 모든 메서드를 호출하는 메서드입니다. Invoke는 델리게이트가 가리키는 메서드 시그니처와 일치하는 매개변수를 전달받습니다.
                doubleRollCount++;
            }
            else if (playerList[currentPlayer].NumTurnsInJail >= maxTurnsInJail)
            {
                playerList[currentPlayer].SetOutOfJail();
                OnUpdateMessage.Invoke(playerList[currentPlayer].name + "<color=green>can leave jail</color>");
            }
            else// 더블이 아니면 이동할 수 없습니다.
            {
                allowedToMove = false;
            }
        }
        else // 플레이어가 감옥에 있지 않은 경우
        {
            if (!rolledADouble)// 더블이 아닌 경우 더블 굴림 카운트를 리셋합니다.
            {
                doubleRollCount = 0;
            }
            else  // 더블을 굴렸다면 더블 굴림 카운트를 증가시킵니다.
            {
                doubleRollCount++;
                if (doubleRollCount >= 3)// 더블 굴림 카운트가 3 이상이 되면 플레이어를 감옥으로 보냅니다.
                {
                    int indexOnBoard = MonopolyBoard.instance.route.IndexOf(playerList[currentPlayer].MymonopolyNode);
                    playerList[currentPlayer].GoToJail(indexOnBoard);
                    OnUpdateMessage.Invoke(playerList[currentPlayer].name + "has rollrd <b>3 times a double</b>, and has to <b><color=red>go to jail!!</color></b>");
                    // OnUpdateMessage 델리게이트를 호출하고, 현재 플레이어의 이름과 함께
                    // "has rolled 3 times a double, and has to go to jail"이라는 메시지를 전달합니다.
                    // 이 메시지는 현재 플레이어가 연속해서 3번 더블을 던졌고, 감옥에 가야 한다는 사실을 알리는 내용입니다.
                    rolledADouble = false; //리셋
                    return; // 함수를 여기서 종료합니다.
                }
            }
        }
        if (allowedToMove) // 플레이어가 이동할 수 있는 경우
        {
            OnUpdateMessage.Invoke(playerList[currentPlayer].name + " has rolled " + rolledDice[0] + "&" + rolledDice[1]);
            StartCoroutine(DelayBeforeMove(rolledDice[0] + rolledDice[1]));// 지연 후 이동 코루틴을 시작합니다.
        }
        else 
        {
            OnUpdateMessage.Invoke(playerList[currentPlayer].name + " has to stay in jail ");
            StartCoroutine(DelayBetweenSwitchPlayer()); //플레이어가 이동할 수 없는 경우 다음 플레이어에게 순번을 돌립니다.
        }
        if (playerList[currentPlayer].playerType == Player.PlayerType.HUMAN)
        {
            bool jail1 = playerList[currentPlayer].HasChanceJailFreeCard;
            bool jail2 = playerList[currentPlayer].HasCommunityJailFreeCard;
            OnShowHumanPanel.Invoke(true, false, false, jail1, jail2);
        }
    }


    IEnumerator DelayBeforeMove(int rolledDice) 
    {
        yield return new WaitForSeconds(secondsBetweenTurns);  // secondsBetweenTurns초간 기다립니다.
        gameBoard.MovePlayerToken(rolledDice,playerList[currentPlayer]); // 게임 보드에 있는 MovePlayerToken 함수를 호출하여 플레이어를 이동시킵니다.
    }
    IEnumerator DelayBetweenSwitchPlayer()
    {
        yield return new WaitForSeconds(secondsBetweenTurns);
        SwitchPlayer();
    }


    public void SwitchPlayer()
    {
        currentPlayer++;// 다음 플레이어로 변경합니다.
        hasRolledDice = false;// // 주사위를 굴렸는지 여부

        doubleRollCount = 0;
        if (currentPlayer>=playerList.Count)
        { currentPlayer = 0;}// 만약 모든 플레이어의 턴이 끝났다면 첫 번째 플레이어로 돌아갑니다.
       
        DeactivateArrow();// 현재 활성화된 플레이어를 나타내는 화살표를 비활성화합니다. 새 플레이어의 턴이 시작될 때 이전 플레이어에 대한 시각적 표시를 제거하는 데 사용됩니다.
        playerList[currentPlayer].ActivateSelector(true);//현재 플레이어의 이름 옆에 화살표 모양의 에니메이션을 활성화 합니다.
        
        if (playerList[currentPlayer].playerType == Player.PlayerType.AI)// 만약 현재 플레이어가 AI라면 자동으로 주사위를 굴립니다.
        {
            // RollDice();
            RollPhysicalDice();
            OnShowHumanPanel.Invoke(false, false, false, false, false); 
        } 
        else//플레이어가 인간이라면 쇼패널을 활성화 합니다.
        {
            bool jail1 = playerList[currentPlayer].HasChanceJailFreeCard;
            bool jail2 = playerList[currentPlayer].HasCommunityJailFreeCard;
            OnShowHumanPanel.Invoke(true, true, false, jail1, jail2);
        }
    }
    public List<int> LastRolledDice => rolledDice;
    //LastRolledDice 프로퍼티는 마지막으로 굴려진 주사위 값들을 외부에서 접근할 수 있도록 제공합니다. =>는 C#에서 익스프레션 바디 멤버를 나타내며,
    //이 경우 rolledDice 배열을 반환하는 간단한 getter 역할을 합니다. 이 프로퍼티를 통해 다른 클래스나 메서드에서 마지막으로 굴려진 주사위 값들에 접근할 수 있습니다.
    //예를 들어, 게임 로직에서 플레이어가 유틸리티 속성에 도착했을 때 지불해야 하는 금액을 계산할 때 이 값을 사용할 수 있습니다.
    //LastRolledDice 프로퍼티는 주사위를 굴린 후의 결과를 저장하므로, 유틸리티 임대료 계산과 같은 로직에서 주사위의 결과가 필요할 때 유용하게 사용됩니다.
    public void AddTaxToPool(int amount)
    {taxPool += amount; }// 현재 세금 풀에 파라미터로 받은 'amount'의 세금을 더함.
    public int GetTaxPool()
    {
        int currentTaxCollected = taxPool;// 현재 세금 풀에 저장된 세금의 총액을 임시 변수에 저장함.
        taxPool = 0;// 세금 풀을 0으로 재설정하여 다음 세금 수집을 위해 초기화함. 
        return currentTaxCollected; // 임시로 저장한 세금 총액을 반환함.이는 게임 내에서 세금을 지불할 때 사용될 수 있음.
    }
    //------------------------------GAME OVER----------------------------
    public void RemovePlayer(Player player)
    {
        playerList.Remove(player);
        CheckForGameOver();
    }
    void CheckForGameOver()
    {
        if (playerList.Count == 1)
        {OnUpdateMessage.Invoke(playerList[0].name + "IS THE WINNER");}
        gameOverPanel.SetActive(true);
        winnerNameText.text = playerList[0].name;
    }
    //-------------------------------UI-------------------------------------
    void DeactivateArrow()// 현재 활성화된 플레이어를 나타내는 화살표를 비활성화합니다.
    {
        foreach (var player in playerList)
        {
            player.ActivateSelector(false);
        }
    }

    public void Continue()
    {
        if (playerList.Count > 1)
        {
            Invoke("ContinueGame", SecondsBetweenTurns);
        }
    }

    void ContinueGame()
    {
        if (RolledADouble)
        {
           // RollDice();
            RollPhysicalDice();
        }
        else
        {   SwitchPlayer();  }
    }

    //-------------------------------------HUMAN BANKRUPT---------------------------------HUMAN
    public void HumanBankrupt()
    {
        playerList[currentPlayer].Bankrupt();
    }

    //--------------------------------JAIL FREE CARDS---------------------------------------------HUMAN
    public void UseJail1Card()
    {
        playerList[currentPlayer].UseChanceJailFreeCard();
    }
    public void UseJail2Card()
    {
        playerList[currentPlayer].UseCommunityJailFreeCard();
    }
}



