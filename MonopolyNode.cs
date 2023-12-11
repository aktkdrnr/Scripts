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

    void OnValidate()//�ε����� ��ġ�� �Ӵ�Ḧ �ڵ����� ����Ͽ� ������ �ǽð� �����͸� �����ϴ� �� ���˴ϴ�.
    {
        if (nameText != null)
        { nameText.text = name; }// UI�� ǥ�õ� �̸� �ؽ�Ʈ�� ������Ʈ�մϴ�.
        if (calculateRentAuto) //calculateRentAuto�� true�� �����Ǿ� �ִٸ�,
        { 
            if(monopolyNodeType == MonopolyNodeType.Property) // �Ӽ� Ÿ���� 'Property'�� ����� �ڵ� ���
            {
                if (baseRent > 0) // ����, ����� ��ġ, �� ��/ȣ�ڿ� ���� �Ӵ�Ḧ ����մϴ�.
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
                else if (baseRent <= 0)// �⺻ �Ӵ�ᰡ 0 ������ ��� ��� ���� 0���� �����մϴ�.
                {
                    price = 0;
                    baseRent = 0;
                    rentWithHouses.Clear();
                    mortgageValue = 0;
                }
            }
            if (monopolyNodeType == MonopolyNodeType.Utility)// ��ƿ��Ƽ Ÿ���� ��� ����� ��ġ�� ����մϴ�.
            { mortgageValue = price / 2;}
            if (monopolyNodeType == MonopolyNodeType.Railroad)// ö�� Ÿ���� ��� ����� ��ġ�� ����մϴ�.
            { mortgageValue = price / 2; }
        }
        if (priceText != null) 
        { priceText.text = "$" + price; }  // UI�� ǥ�õ� ���� �ؽ�Ʈ�� ������Ʈ�մϴ�.
        OnOwnerUpdated(); // ������ ������ ������Ʈ�� �� ȣ��Ǵ� �޼����Դϴ�.
        UnMortgageProperty(); // ����� ���� �޼��带 ȣ���մϴ�.
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
                    else if (owner == null && currentPlayer.CanAffordNode(price)) // ��ƿ��Ƽ�� ������ ����, ���� �÷��̾ �� ��带 �� �� �ִ� ���� �ִٸ�
                    {
                        OnUpdateMessage.Invoke(currentPlayer.name + "buys" + this.name);
                        currentPlayer.BuyProperty(this); // ���� �÷��̾ ��ƿ��Ƽ�� �����մϴ�.
                        OnOwnerUpdated();// ������ ������ ������Ʈ�մϴ�.
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
                    else if (owner == null && currentPlayer.CanAffordNode(price)) // ��ƿ��Ƽ�� ������ ����, ���� �÷��̾ �� ��带 �� �� �ִ� ���� �ִٸ�
                    {
                        OnUpdateMessage.Invoke(currentPlayer.name + "buys" + this.name);
                        currentPlayer.BuyProperty(this); // ���� �÷��̾ ��ƿ��Ƽ�� �����մϴ�.
                        OnOwnerUpdated();// ������ ������ ������Ʈ�մϴ�.
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
            case MonopolyNodeType.Tax:// ���� �÷��̾ ���� ĭ�� �������� �� ó��
                GameManager.instance.AddTaxToPool(price);// ������ ������ ���� Ǯ�� �߰���*
                currentPlayer.PayMoney(price);// �÷��̾�� �Ͽ��� ������ �����ϰ� ��
                OnUpdateMessage.Invoke(currentPlayer.name + "pays tax of:" + price);
                break;
            case MonopolyNodeType.FreeParking:// �÷��̾ ���� ���� ĭ�� �������� �� ó��
                int tax = GameManager.instance.GetTaxPool();// ���� Ǯ���� ���� ����� ������ ������
                currentPlayer.CollectMoney(tax);// ������ ������ �÷��̾�� ������
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

    int CalculateUtilityRent()// ���� �Ŵ������� �������� ������ �ֻ��� ������ �����ɴϴ�.
    {
        List<int> lastRolledDice = GameManager.instance.LastRolledDice;
        int result = 0;
        var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(this);
        // ���� ��ƿ��Ƽ�� ���� ��Ʈ�� ��� ��带 �����ϰ� �ִ��� Ȯ���մϴ�.
        if (allSame)
        { result = (lastRolledDice[0] + lastRolledDice[1]) * 10;}
        else
        { result = (lastRolledDice[0] + lastRolledDice[1]) * 4;}
        return result;
    }
    int CalculateRailroadRent()
    {
        int result = 0; // ����� ������ ������ �ʱ�ȭ�մϴ�.
        var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(this);// ���� �Ӽ� ��Ʈ�� ��� ��带 �����ɴϴ�.
        int amount = 0; // ������ ö�� �Ӽ��� ���� ������ ������ 0���� �ʱ�ȭ�մϴ�.
        
        foreach (var item in list)// list�� �ִ� ��� �׸�(���)�� �ݺ��մϴ�.
        { amount += (item.owner == this.owner) ? 1 : 0; }  // ���� ���� �׸��� �����ڰ� �� �Ӽ��� �����ڿ� ���ٸ� amount�� 1 ������ŵ�ϴ�. ���� ������ (���� ? ���� �� �� : ������ �� ��)�� ����Ͽ� �˻��մϴ�.

      
        result = baseRent * (int)Mathf.Pow(2, amount-1);
        // ���� �Ӵ�Ḧ ����մϴ�. baseRent(�⺻ �Ӵ��)�� 2�� amount ������ ���մϴ�.
        // ���� ���, amount�� 2��� 2�� 2��, �� 4�� baseRent�� ���մϴ�.
        
        return result; // ���� �Ӵ�Ḧ ��ȯ�մϴ�.
    }
    void VisualizeHouses()// �ε��� ��忡 �Ǽ��� ���� ȣ���� �ð��� ǥ���� ������Ʈ�մϴ�.
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
   
    public void BuildHouseOrHotel() // �ε��� ��忡 ���̳� ȣ���� �Ǽ��ϴ� ����Դϴ�.
    {
        if (monopolyNodeType == MonopolyNodeType.Property)// ��尡 �ε��� ������ ��쿡�� ���� �Ǽ��� �� �ֽ��ϴ�.
        {
            numberOfHouses++; // ���� ���� �ϳ� ������ŵ�ϴ�.
            VisualizeHouses(); // �ð��� ǥ���� ������Ʈ�մϴ�.
        }
    }
    public int SellHouseOrHotel() // �ε��� ��忡 �Ǽ��� ���̳� ȣ���� �Ǹ��ϴ� ����Դϴ�.
    { 
        if (monopolyNodeType == MonopolyNodeType.Property && numberOfHouses > 0)// ��尡 �ε��� ������ ��쿡�� ���� �Ǹ��� �� �ֽ��ϴ�.
        {
            numberOfHouses--; // ���� ���� �ϳ� ���ҽ�ŵ�ϴ�.
            VisualizeHouses(); // �ð��� ǥ���� ������Ʈ�մϴ�.
            return houseCost / 2;
        }
        return 0;

    }
    public void ResetNode()// �ε��� ��带 �ʱ� ���·� �����ϴ� ����Դϴ�.
    // (�������, �ش� ��带 ���� �÷��̾ �Ļ� �� ���,�� ������ ������ ��,�÷��̾� ���� �ε����� �ŷ��� ��,Ư�� ���� ��Ģ�̳� ī�忡 ����)
    {
        if (isMortgaged) // ��尡 ���� ������ ���, ���� ���� ǥ�ø� �����մϴ�.
        {
            propertyImage.SetActive(true); // �ε��� �̹����� Ȱ��ȭ�մϴ�.
            mortgageImage.SetActive(false); // ���� ���� �̹����� ��Ȱ��ȭ�մϴ�.
            isMortgaged = false; // ���� ���¸� �����մϴ�.
        }
        if (monopolyNodeType == MonopolyNodeType.Property)  // �ε��� ������ ��� ���� ���� 0���� �����ϰ� �ð��� ǥ���� ������Ʈ�մϴ�.
        {
            numberOfHouses = 0;
            VisualizeHouses();
        }
        owner.RemoveProperty(this);//���� ���� ���� �ε����� �����մϴ�.
        owner.name = "";
        owner.ActivateSelector(false);
        owner = null;
        OnOwnerUpdated();// ������ ������ �ʱ�ȭ�մϴ�.
    }

    public void ChangeOwner(Player newOwner)
    {
        owner.RemoveProperty(this);
        newOwner.AddProperty(this);
        SetOwner(newOwner);
    }
}
