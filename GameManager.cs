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
    public void ResetRolledADouble() => rolledADouble = false;//�̴� �÷��̾ �ֻ����� ������ ������ �ƴ� ����� ������ �� ���˴ϴ�.
    int doubleRollCount;
    bool hasRolledDice;
    public bool HasRolledDice => hasRolledDice;

    int taxPool = 0;




    public int GetGoMoney => goMoney;
    public float SecondsBetweenTurns => secondsBetweenTurns;
    public List<Player> GetPlayers => playerList;//���ӿ� �����ϴ� ��� �÷��̾��� ����� ��ȯ�ϴ� ������Ƽ�Դϴ�.
    public Player GetCurrentPlayer => playerList[currentPlayer];
    //���� ���� �����ϰ� �ִ� �÷��̾� ��ü�� ��ȯ�ϴ� ������Ƽ�Դϴ�. �̴� ���� ���� �÷��̾ �ൿ�� ���� �� ���˴ϴ�.




    public delegate void UpdateMessage(string message);//�� ��������Ʈ�� ����ϸ� ���ڿ� �Ű������� �ް� ��ȯ���� ���� ��� �޼��带 ������ �� �ֽ��ϴ�.
    public static UpdateMessage OnUpdateMessage;//static �޼���� Ŭ������ �ν��Ͻ��� �ƴ϶� Ŭ���� ��ü�� ���մϴ�. ���� static �޼���� Ŭ������ ��� �ν��Ͻ��� �ƴ϶�, Ŭ���� ��ü�� ���� ���ٵǰ� ȣ��˴ϴ�.
                                                //static �޼���� Ŭ������ ��� �ν��Ͻ��� �������� ����Ǵ� ����� �����ϴ� �� �����մϴ�. ���� ���, ��ƿ��Ƽ �Լ��� ��� �Լ��� ���� �͵��� static �޼���� ���� �����˴ϴ�.
                                                // Ŭ������ �� �ν��Ͻ��� ���� ������ �޸𸮸� �Ҵ����� �ʱ� ������, ���� �ν��Ͻ����� ������ �޼��带 ����ؾ� �� �� �����մϴ�.

    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn , bool hasChanceJailCard, bool hasCommunityJailCard);
    public static ShowHumanPanel OnShowHumanPanel;



    //-------------------- DEBUG---------------------------
   // [SerializeField] bool alwaysDoubleRoll = false;// �ֻ����� ���� ��� ������ �ϴ� �׽�Ʈ��.
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
    void Inititialize()//���� ���۽� �ʱ�ȭ ����
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            GameObject infoObject = Instantiate(playerInfoPrefab, playerPanel, false);
            PlayerInfo info = infoObject.GetComponent<PlayerInfo>();

            int randomIndex = Random.Range(0, playerTokenList.Count);
            GameObject newToken = Instantiate(playerTokenList[randomIndex], gameBoard.route[0].transform.position, Quaternion.identity);


            playerList[i].Inititialize(gameBoard.route[0], startMoney, info, newToken);
        }
        playerList[currentPlayer].ActivateSelector(true);//���� �÷��̾��� �̸� ���� ȭ��ǥ ����� ���ϸ��̼��� Ȱ��ȭ �մϴ�.
        if (playerList[currentPlayer].playerType == Player.PlayerType.HUMAN)//�÷��̾ �ΰ��̶�� ���г��� Ȱ��ȭ �մϴ�.
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
    //Instantiate �Լ����� false ���� �θ� ������Ʈ�� ���� infoObject�� ��ġ, ȸ��, �׸��� ũ�⸦ ���� ��ǥ ������ �ƴ� 
    //���� ��ǥ �������� ������ �������� �����մϴ�. ���⼭ false�� infoObject�� playerPanel�� �ڽ����� ������ ��, 
    //playerPanel�� ��ȯ ������ ��ӹ��� �ʰ�, ��� ��ü���� ��ġ, ȸ��, ũ�� ������ �����ϰ� �ϴ� �����Դϴ�. 
    //��, infoObject�� playerPanel�� ��ġ�� ȸ���� ������ ���� �ʰ� ����(0, 0, 0) ��ġ�� �����˴ϴ�.
    //--GameObject newToken = Instantiate(playerTokenList[randomIndex], gameBoard.route[0].transform.position, Quaternion.identity);------
    //Quaternion.identity�� ȸ�� ���� ������Ʈ�� �⺻ ������ ��Ÿ���� ����Դϴ�. 
    //����Ƽ���� Quaternion�� ������Ʈ�� ȸ���� ��Ÿ���µ� ���Ǹ�, 
    //Quaternion.identity�� ������Ʈ�� ��� ȸ���� ������� ���� ���� �⺻ ���¸� �ǹ��մϴ�.
    //�̸� ����ϸ�, ���� ������ ���� ������Ʈ(newToken)�� ������ �⺻ �������� ���ĵǾ� ��Ÿ���ϴ�.

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
        
        bool allowedToMove = true;// �÷��̾ �̵��� �� �ִ��� ���θ� �����ϴ� �����Դϴ�.
        hasRolledDice = true; // �ֻ����� ���ȴ��� ����

           // rolledDice = new int[2];// �� ���� �ֻ��� ���� ������ �迭�� �ʱ�ȭ�մϴ�.
/*        rolledDice[0] = Random.Range(1, 7);// ù ��°�� �� ��° �ֻ����� ���� ���� 1���� 6 ������ ���� �������� �����մϴ�.
        rolledDice[1] = Random.Range(1, 7);
//---------------------------------------------------------------------�����-------------------------------------------------------------------------------------------
        if (alwaysDoubleRoll){ rolledDice[0] = 1;  rolledDice[1] = 1; }// �ֻ����� ���� ��� ������ �ϴ� �׽�Ʈ��.
        if (forceDiceRolls){ rolledDice[0] = dice1; ; rolledDice[1] = dice2;}
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        rolledADouble = rolledDice[0] == rolledDice[1];// �� �ֻ����� ���� ������ Ȯ���Ͽ� ���� ���θ� �����մϴ�.

        if (playerList[currentPlayer].IsInJail)// ���� �÷��̾ ������ �ִ��� Ȯ���մϴ�.
        {
            playerList[currentPlayer].IncreaseNumTurnsInJail();//numTurnsInJail�� ������ŵ�ϴ�.
            if (rolledADouble)// �ֻ����� ������ ���Դٸ� �÷��̾ �������� �ع��ŵ�ϴ�.numTurnsInJail�� �����մϴ�.
            {
                playerList[currentPlayer].SetOutOfJail();
                OnUpdateMessage.Invoke(playerList[currentPlayer].name + "<color=green>can leave jail</color>, because a double was rolled");
                //Invoke(...): ��������Ʈ�� ����� ��� �޼��带 ȣ���ϴ� �޼����Դϴ�. Invoke�� ��������Ʈ�� ����Ű�� �޼��� �ñ״�ó�� ��ġ�ϴ� �Ű������� ���޹޽��ϴ�.
                doubleRollCount++;
            }
            else if (playerList[currentPlayer].NumTurnsInJail >= maxTurnsInJail)
            {
                playerList[currentPlayer].SetOutOfJail();
                OnUpdateMessage.Invoke(playerList[currentPlayer].name + "<color=green>can leave jail</color>");
            }
            else// ������ �ƴϸ� �̵��� �� �����ϴ�.
            {
                allowedToMove = false;
            }
        }
        else // �÷��̾ ������ ���� ���� ���
        {
            if (!rolledADouble)// ������ �ƴ� ��� ���� ���� ī��Ʈ�� �����մϴ�.
            {
                doubleRollCount = 0;
            }
            else  // ������ ���ȴٸ� ���� ���� ī��Ʈ�� ������ŵ�ϴ�.
            {
                doubleRollCount++;
                if (doubleRollCount >= 3)// ���� ���� ī��Ʈ�� 3 �̻��� �Ǹ� �÷��̾ �������� �����ϴ�.
                {
                    int indexOnBoard = MonopolyBoard.instance.route.IndexOf(playerList[currentPlayer].MymonopolyNode);
                    playerList[currentPlayer].GoToJail(indexOnBoard);
                    OnUpdateMessage.Invoke(playerList[currentPlayer].name + "has rollrd <b>3 times a double</b>, and has to <b><color=red>go to jail!!</color></b>");
                    // OnUpdateMessage ��������Ʈ�� ȣ���ϰ�, ���� �÷��̾��� �̸��� �Բ�
                    // "has rolled 3 times a double, and has to go to jail"�̶�� �޽����� �����մϴ�.
                    // �� �޽����� ���� �÷��̾ �����ؼ� 3�� ������ ������, ������ ���� �Ѵٴ� ����� �˸��� �����Դϴ�.
                    rolledADouble = false; //����
                    return; // �Լ��� ���⼭ �����մϴ�.
                }
            }
        }
        if (allowedToMove) // �÷��̾ �̵��� �� �ִ� ���
        {
            OnUpdateMessage.Invoke(playerList[currentPlayer].name + " has rolled " + rolledDice[0] + "&" + rolledDice[1]);
            StartCoroutine(DelayBeforeMove(rolledDice[0] + rolledDice[1]));// ���� �� �̵� �ڷ�ƾ�� �����մϴ�.
        }
        else 
        {
            OnUpdateMessage.Invoke(playerList[currentPlayer].name + " has to stay in jail ");
            StartCoroutine(DelayBetweenSwitchPlayer()); //�÷��̾ �̵��� �� ���� ��� ���� �÷��̾�� ������ �����ϴ�.
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
        yield return new WaitForSeconds(secondsBetweenTurns);  // secondsBetweenTurns�ʰ� ��ٸ��ϴ�.
        gameBoard.MovePlayerToken(rolledDice,playerList[currentPlayer]); // ���� ���忡 �ִ� MovePlayerToken �Լ��� ȣ���Ͽ� �÷��̾ �̵���ŵ�ϴ�.
    }
    IEnumerator DelayBetweenSwitchPlayer()
    {
        yield return new WaitForSeconds(secondsBetweenTurns);
        SwitchPlayer();
    }


    public void SwitchPlayer()
    {
        currentPlayer++;// ���� �÷��̾�� �����մϴ�.
        hasRolledDice = false;// // �ֻ����� ���ȴ��� ����

        doubleRollCount = 0;
        if (currentPlayer>=playerList.Count)
        { currentPlayer = 0;}// ���� ��� �÷��̾��� ���� �����ٸ� ù ��° �÷��̾�� ���ư��ϴ�.
       
        DeactivateArrow();// ���� Ȱ��ȭ�� �÷��̾ ��Ÿ���� ȭ��ǥ�� ��Ȱ��ȭ�մϴ�. �� �÷��̾��� ���� ���۵� �� ���� �÷��̾ ���� �ð��� ǥ�ø� �����ϴ� �� ���˴ϴ�.
        playerList[currentPlayer].ActivateSelector(true);//���� �÷��̾��� �̸� ���� ȭ��ǥ ����� ���ϸ��̼��� Ȱ��ȭ �մϴ�.
        
        if (playerList[currentPlayer].playerType == Player.PlayerType.AI)// ���� ���� �÷��̾ AI��� �ڵ����� �ֻ����� �����ϴ�.
        {
            // RollDice();
            RollPhysicalDice();
            OnShowHumanPanel.Invoke(false, false, false, false, false); 
        } 
        else//�÷��̾ �ΰ��̶�� ���г��� Ȱ��ȭ �մϴ�.
        {
            bool jail1 = playerList[currentPlayer].HasChanceJailFreeCard;
            bool jail2 = playerList[currentPlayer].HasCommunityJailFreeCard;
            OnShowHumanPanel.Invoke(true, true, false, jail1, jail2);
        }
    }
    public List<int> LastRolledDice => rolledDice;
    //LastRolledDice ������Ƽ�� ���������� ������ �ֻ��� ������ �ܺο��� ������ �� �ֵ��� �����մϴ�. =>�� C#���� �ͽ������� �ٵ� ����� ��Ÿ����,
    //�� ��� rolledDice �迭�� ��ȯ�ϴ� ������ getter ������ �մϴ�. �� ������Ƽ�� ���� �ٸ� Ŭ������ �޼��忡�� ���������� ������ �ֻ��� ���鿡 ������ �� �ֽ��ϴ�.
    //���� ���, ���� �������� �÷��̾ ��ƿ��Ƽ �Ӽ��� �������� �� �����ؾ� �ϴ� �ݾ��� ����� �� �� ���� ����� �� �ֽ��ϴ�.
    //LastRolledDice ������Ƽ�� �ֻ����� ���� ���� ����� �����ϹǷ�, ��ƿ��Ƽ �Ӵ�� ���� ���� �������� �ֻ����� ����� �ʿ��� �� �����ϰ� ���˴ϴ�.
    public void AddTaxToPool(int amount)
    {taxPool += amount; }// ���� ���� Ǯ�� �Ķ���ͷ� ���� 'amount'�� ������ ����.
    public int GetTaxPool()
    {
        int currentTaxCollected = taxPool;// ���� ���� Ǯ�� ����� ������ �Ѿ��� �ӽ� ������ ������.
        taxPool = 0;// ���� Ǯ�� 0���� �缳���Ͽ� ���� ���� ������ ���� �ʱ�ȭ��. 
        return currentTaxCollected; // �ӽ÷� ������ ���� �Ѿ��� ��ȯ��.�̴� ���� ������ ������ ������ �� ���� �� ����.
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
    void DeactivateArrow()// ���� Ȱ��ȭ�� �÷��̾ ��Ÿ���� ȭ��ǥ�� ��Ȱ��ȭ�մϴ�.
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



