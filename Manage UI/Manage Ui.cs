using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ManageUi : MonoBehaviour
{
    public static ManageUi instance;

    [SerializeField] GameObject managerPanel;
    [SerializeField] Transform propertyGrid;
    [SerializeField] GameObject prpertySetPrefab;
    Player playerReference;
    List<GameObject> propertyPrefab = new List<GameObject>();
    [SerializeField] TMP_Text yourMoneyText;
    [SerializeField] TMP_Text systemMessageText;

    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        managerPanel.SetActive(false);
    }

    public void OpenManager()
    {
        playerReference = GameManager.instance.GetCurrentPlayer;
        CreateProperties();
        managerPanel.SetActive(true);
        UpdateMoneyText();
    }

    void CreateProperties()
    {
        List<MonopolyNode> processedSet = null;

        foreach (var node in playerReference.GetMonopolyNodes)
        {
            var (list, allsame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(node);
            //List<MonopolyNode> nodeSet = list ==== List<MonopolyNode> nodeSet = new List<MonopolyNode>(); nodeSet.AddRange(list);
            List<MonopolyNode> nodeSet = new List<MonopolyNode>();
            nodeSet.AddRange(list);

            if (nodeSet != null && list != processedSet)
            {
                processedSet = list;
                nodeSet.RemoveAll(n => n.Owner != playerReference);
                GameObject newPropertySet = Instantiate(prpertySetPrefab, propertyGrid, false);
                newPropertySet.GetComponent<ManagePropertyUi>().SetProperty(nodeSet, playerReference);// UI�� �ε��� ��Ʈ�� �÷��̾� ������ �����մϴ�.
                propertyPrefab.Add(newPropertySet);
            }
        }
    }

    //---------------------------------var (list, allsame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(node);-----------------------
    //�÷��̾ ���� ��尡 ���� ��弼Ʈ�� ����Ʈ���� ��ȯ�մϴ�.
    //�÷��̾ ���� ���� ��� ��Ʈ�� ������ ���� ���ٸ�, ���� �ƴ϶�� ������ ��ȯ�մϴ�.
    //--------------------------------------------------List<MonopolyNode> processedSet = null;--------------------------------------
    //processedSet�� null ���� �ʱ�ȭ�ϴ� ���� �Ŵ��� �гο� ���� �ƹ��͵� ó������ �ʾ����� ��Ÿ���ϴ�. 
    //�̷��� �����ν�, ���߿� �÷��̾ ������ ��� ��Ʈ�� �ϳ��� ó���ϰ� �Ŵ��� �гο� ǥ���� �� �ֽ��ϴ�. 
    //null�� ���������ν�, �츮�� '���� �ƹ��͵� Ȯ�ε��� �ʾҴ�'�� �ʱ� ���¸� �����ϴ� ���Դϴ�.
    //if (nodeSet != null && nodeSet != processedSet) ���ǹ��� �� ������ Ȯ���մϴ�:
    //nodeSet != null: ���� ��� ��Ʈ(nodeSet)�� ������ �����ϸ�, �÷��̾ ������ ��� ��Ʈ�� ������ ��Ÿ���ϴ�.
    //nodeSet != processedSet: ���� ��� ��Ʈ�� ���� processedSet�� ���Ե��� �ʾ����� ��Ÿ���ϴ�. ��, ���� ó������ ���� ���ο� ��Ʈ���� �ǹ��մϴ�.
    //--------------------------------------------------nodeSet.RemoveAll ------------------------------------------
    //�ڵ�� nodeSet���� ���� �÷��̾ �������� ���� ��� ��带 �����Ͽ�, 
    //nodeSet�� ���� �÷��̾ ������ ����θ� �����ǵ��� �մϴ�. �̴� ���� �������� �÷��̾��� ���� �ε��길�� ����ϱ� ���� �ʿ��� ó���Դϴ�.
    //RemoveAl ������ ��(true)�� ��ҵ鸸 ����Ʈ���� ����
    //-----------------------------------------propertyPrefab.Add(newPropertySet);----------------------------------
    //UI ��� ����: newPropertySet�� �÷��̾ ������ Ư�� �ε��� ��Ʈ�� ��Ÿ���� UI ����Դϴ�. 
    //�� ������Ʈ�� ���ӿ��� �÷��̾��� �������� �ð������� ǥ���ϴ� �� ���˴ϴ�.
    //���� ���� �� ����: ���� ������ �÷��̾��� �ε��� ���� ���´� �ð��� ������ ���� ����� �� �ֽ��ϴ�. 
    //����, �÷��̾ ���ο� �ε����� ����ϰų� ���� �ε����� �Ǹ��� ������, �̿� �����ϴ� UI ��Ҹ� �����ϰ�, �� ����Ʈ�� �߰��մϴ�.
    //�ļ� ó�� ���̼�: propertyPrefab ����Ʈ�� �߰������ν�, ���߿� �̷��� UI ��ҵ��� ���� ã�Ƽ� ������Ʈ�ϰų� ������ �� �ֽ��ϴ�. 
    //���� ���, �÷��̾ �ε����� �Ҿ��� ��, ���õ� UI ��Ҹ� ����Ʈ���� ã�� ������ �� �ֽ��ϴ�.
    //�޸� ����: ������ �÷����ϸ鼭 ���� UI ��ҵ��� �����ǰ� �ı��� �� �ֽ��ϴ�. 
    //propertyPrefab ����Ʈ�� ����ϸ� ������ UI ��ҵ��� �����Ͽ�, �ʿ��� �� �޸𸮿��� �����ϴ� ���� ������ ���������ϴ�.
    //����ϸ�, propertyPrefab.Add(newPropertySet);�� ������ UI�� �����ϴ� �� �߿��� ������ �ϸ�, 
    //�÷��̾��� �ε��� ���� ���¸� ȿ�������� �ð�ȭ�ϱ� ���� ���˴ϴ�.
    //-------------List<MonopolyNode> nodeSet = list; �ڵ带 List<MonopolyNode> nodeSet = new List<MonopolyNode>(); nodeSet.AddRange(list);�� ������ ����-----------


    //�ֿ� ������ ���� ����Ʈ(list)�� ������ ���纻�� �����ϱ� ���ؼ��Դϴ�. 
    //�� ������ nodeSet�� list ���� ���� ���踦 ����, �� ����Ʈ�� ���� ���������� �����ϵ��� �մϴ�. 

    //List<MonopolyNode> nodeSet = new List<MonopolyNode>();
    //�� �ڵ�� nodeSet�̶�� ���ο� List<MonopolyNode> ��ü�� �����մϴ�. �̷��� �����ν�, nodeSet�� ó������ ����ִ� ���°� �˴ϴ�.

    //nodeSet.AddRange(list);�� list�� �ִ� ��� ��ҵ��� nodeSet�� �߰��մϴ�. �� �۾��� list�� ��ҵ��� nodeSet���� "����"�ϴ� �Ͱ� �����մϴ�.
    //�� ������ ����, nodeSet�� list�� ������ ��� ������, list�� �����Ǵ��� nodeSet���� ������ ��ġ�� �ʽ��ϴ�. 
    //����������, nodeSet�� �����ص� list���� ������ ��ġ�� �ʽ��ϴ�.

    //������ ����
    //������: nodeSet�� list�� �������� ���纻�� ������ �Ǿ�, �� ����Ʈ ���� ��ȣ�ۿ��� �߻����� �ʽ��ϴ�.
    //�̴� �Ŀ� nodeSet �Ǵ� list�� ������ �� ���ο��� ������ ���� �ʰ� ���������� �۾��� �� �ְ� �մϴ�.
    //������ ������: �̷��� ���� ����� ���α׷��ֿ��� �������� �������� ����Ű�� �� ������ �˴ϴ�. 
    //nodeSet�� list�� ���� �ٸ� ��ü�� �����ϹǷ�, �ϳ��� �����ص� �ٸ� �ϳ��� ����ġ �ʰ� ����Ǵ� ���� ������ �� �ֽ��ϴ�.
    //���������, �� ������ ���α׷��� ���� ���ɼ��� �������� ���̴� �� ������ �� �� ������, Ư�� ������ ���� �����̳� ��Ը� ������Ʈ���� �߿��� �� �ֽ��ϴ�.

    public void CloseManager()
    {
        managerPanel.SetActive(false);
        ClearProperties();
    }
    //�� for �ݺ����� propertyPrefab ����Ʈ�� �ִ� ��� UI ��Ҹ� �Ųٷ� ��ȸ�ϸ鼭 ������ �����մϴ�.
    //propertyPrefab.Count-1�� �����ϴ� ������ ����Ʈ�� ������ ��ҿ��� �����ؼ� ù ��° ��ҷ� �Ž��� �ö󰡱� �����Դϴ�.
    //Destroy(propertyPrefab[i]);�� propertyPrefab ����Ʈ�� �ִ� �� UI ��Ҹ� �޸𸮿��� �����մϴ�. 
    //�̰��� ������ ������ ����ȭ�ϰ� �޸� ������ �����ϴ� �� �߿��մϴ�.
    //-------------------------------------------------------------
    //propertyPrefab.Clear();�� propertyPrefab ����Ʈ�� ���ϴ�. 
    //�̴� ����Ʈ�� ����� ��� ������ �����Ͽ� ����Ʈ�� �ʱ� ���·� �ǵ����� �۾��Դϴ�. 
    //�̷��� �����ν�, ���� ���� ������ �г��� ������ �� ���Ӱ� ��Ȯ�� ������ UI�� ä�� �� �ֽ��ϴ�.

    void ClearProperties()
    {
        for (int i = propertyPrefab.Count - 1; i >= 0; i--)
        {Destroy(propertyPrefab[i]);}
        propertyPrefab.Clear();
    }
    //�������� ����Ʈ ��ȸ: for �ݺ����� ����Ͽ� propertyPrefab ����Ʈ�� ������ ��Һ��� ù ��° ��ұ��� �������� ��ȸ�մϴ�.
    //�̴� ����Ʈ���� ��Ҹ� ������ �� �ε����� ��ȭ�� ���� ������ �����ϱ� �����Դϴ�.
    //��ü ����:Destroy(propertyPrefab[i]); �ڵ�� Unity�� Destroy �Լ��� ȣ���Ͽ�, 
    //���� �ε��� i�� �ش��ϴ� ���� ������Ʈ�� �����մϴ�. �̴� ������ �޸� ������ �߿��մϴ�.
    //����Ʈ �ʱ�ȭ:propertyPrefab.Clear(); �ڵ�� ����Ʈ�� ��� ������ �����Ͽ�, ����Ʈ�� �ʱ� ���·� �ǵ����ϴ�.
    //�̴� ����Ʈ�� ����ϰ� ����, ���ο� ��ҵ��� �߰��ϱ� ���� �غ� �մϴ�.

   

    public void UpdateMoneyText()
    {
        string showMoney = (playerReference.ReadMoney > 0) ? "<color=green>&" + playerReference.ReadMoney : "<color=red>&" + playerReference.ReadMoney;
        yourMoneyText.text = "<color=black>Your Money</color>" + showMoney;
    }

    public void UpdateSystemMessage(string message)
    {systemMessageText.text = message;}
    public void AutoHandleFunds()
    {
        if (playerReference.ReadMoney > 0)
        {
            UpdateSystemMessage("you don't need to do that, you have enough money!");
            return;           
        }
        playerReference.HandleInsufficientFunds(Mathf.Abs(playerReference.ReadMoney));
        ClearProperties();
        CreateProperties();
        UpdateMoneyText();
    }
    //�� �޼ҵ�� �÷��̾��� ���� �ݾ��� ������ ��, �� �ݾ��� ���밪�� HandleInsufficientFunds �޼ҵ�� �����մϴ�.
    //��, �÷��̾ ��ä�� ������ �ִٸ�, �� ��ä�� �ݾ��� ó���ϴ� �� ���˴ϴ�.\
    //�÷��̾��� ���� -500�� ��: Mathf.Abs(playerReference.ReadMoney) �� -500�� ���밪�� ����Ͽ� 500�� ��ȯ�մϴ�.
    //HandleInsufficientFunds(500) �޼ҵ尡 ȣ��Ǹ�, �÷��̾�� 500�� ��ä�� ó���ϱ� ���� ��ġ�� ���ؾ� �մϴ�.
}
