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
                newPropertySet.GetComponent<ManagePropertyUi>().SetProperty(nodeSet, playerReference);// UI에 부동산 세트와 플레이어 정보를 설정합니다.
                propertyPrefab.Add(newPropertySet);
            }
        }
    }

    //---------------------------------var (list, allsame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(node);-----------------------
    //플레이어가 가진 노드가 속한 노드세트의 리스트들을 반환합니다.
    //플레이어가 속한 노드와 노드 세트의 주인이 전부 같다면, 참을 아니라면 거짓을 반환합니다.
    //--------------------------------------------------List<MonopolyNode> processedSet = null;--------------------------------------
    //processedSet에 null 값을 초기화하는 것은 매니저 패널에 아직 아무것도 처리되지 않았음을 나타냅니다. 
    //이렇게 함으로써, 나중에 플레이어가 소유한 노드 세트를 하나씩 처리하고 매니저 패널에 표시할 수 있습니다. 
    //null로 시작함으로써, 우리는 '아직 아무것도 확인되지 않았다'는 초기 상태를 설정하는 것입니다.
    //if (nodeSet != null && nodeSet != processedSet) 조건문은 두 가지를 확인합니다:
    //nodeSet != null: 현재 노드 세트(nodeSet)가 실제로 존재하며, 플레이어가 소유한 노드 세트가 있음을 나타냅니다.
    //nodeSet != processedSet: 현재 노드 세트가 아직 processedSet에 포함되지 않았음을 나타냅니다. 즉, 아직 처리되지 않은 새로운 세트임을 의미합니다.
    //--------------------------------------------------nodeSet.RemoveAll ------------------------------------------
    //코드는 nodeSet에서 현재 플레이어가 소유하지 않은 모든 노드를 제거하여, 
    //nodeSet이 오직 플레이어가 소유한 노드들로만 구성되도록 합니다. 이는 게임 로직에서 플레이어의 소유 부동산만을 고려하기 위해 필요한 처리입니다.
    //RemoveAl 조건이 참(true)인 요소들만 리스트에서 제거
    //-----------------------------------------propertyPrefab.Add(newPropertySet);----------------------------------
    //UI 요소 관리: newPropertySet는 플레이어가 소유한 특정 부동산 세트를 나타내는 UI 요소입니다. 
    //이 오브젝트는 게임에서 플레이어의 소유물을 시각적으로 표시하는 데 사용됩니다.
    //동적 생성 및 추적: 게임 내에서 플레이어의 부동산 소유 상태는 시간이 지남에 따라 변경될 수 있습니다. 
    //따라서, 플레이어가 새로운 부동산을 취득하거나 기존 부동산을 판매할 때마다, 이에 대응하는 UI 요소를 생성하고, 이 리스트에 추가합니다.
    //후속 처리 용이성: propertyPrefab 리스트에 추가함으로써, 나중에 이러한 UI 요소들을 쉽게 찾아서 업데이트하거나 제거할 수 있습니다. 
    //예를 들어, 플레이어가 부동산을 잃었을 때, 관련된 UI 요소를 리스트에서 찾아 제거할 수 있습니다.
    //메모리 관리: 게임을 플레이하면서 많은 UI 요소들이 생성되고 파괴될 수 있습니다. 
    //propertyPrefab 리스트를 사용하면 생성된 UI 요소들을 추적하여, 필요할 때 메모리에서 제거하는 등의 관리가 용이해집니다.
    //요약하면, propertyPrefab.Add(newPropertySet);는 게임의 UI를 관리하는 데 중요한 역할을 하며, 
    //플레이어의 부동산 소유 상태를 효과적으로 시각화하기 위해 사용됩니다.
    //-------------List<MonopolyNode> nodeSet = list; 코드를 List<MonopolyNode> nodeSet = new List<MonopolyNode>(); nodeSet.AddRange(list);로 변경한 이유-----------


    //주요 이유는 원본 리스트(list)의 독립된 복사본을 생성하기 위해서입니다. 
    //이 변경은 nodeSet과 list 간의 참조 관계를 끊고, 두 리스트가 서로 독립적으로 동작하도록 합니다. 

    //List<MonopolyNode> nodeSet = new List<MonopolyNode>();
    //이 코드는 nodeSet이라는 새로운 List<MonopolyNode> 객체를 생성합니다. 이렇게 함으로써, nodeSet는 처음에는 비어있는 상태가 됩니다.

    //nodeSet.AddRange(list);는 list에 있는 모든 요소들을 nodeSet에 추가합니다. 이 작업은 list의 요소들을 nodeSet으로 "복사"하는 것과 유사합니다.
    //이 과정을 통해, nodeSet은 list의 내용을 담게 되지만, list가 수정되더라도 nodeSet에는 영향을 미치지 않습니다. 
    //마찬가지로, nodeSet을 수정해도 list에는 영향을 미치지 않습니다.

    //변경의 목적
    //독립성: nodeSet이 list의 독립적인 복사본을 가지게 되어, 두 리스트 간의 상호작용이 발생하지 않습니다.
    //이는 후에 nodeSet 또는 list를 수정할 때 서로에게 영향을 주지 않고 독립적으로 작업할 수 있게 합니다.
    //데이터 안정성: 이러한 접근 방식은 프로그래밍에서 데이터의 안정성을 향상시키는 데 도움이 됩니다. 
    //nodeSet과 list가 서로 다른 객체를 참조하므로, 하나를 변경해도 다른 하나가 예기치 않게 변경되는 것을 방지할 수 있습니다.
    //결론적으로, 이 변경은 프로그램의 예측 가능성과 안정성을 높이는 데 도움이 될 수 있으며, 특히 복잡한 게임 로직이나 대규모 프로젝트에서 중요할 수 있습니다.

    public void CloseManager()
    {
        managerPanel.SetActive(false);
        ClearProperties();
    }
    //이 for 반복문은 propertyPrefab 리스트에 있는 모든 UI 요소를 거꾸로 순회하면서 각각을 제거합니다.
    //propertyPrefab.Count-1로 시작하는 이유는 리스트의 마지막 요소에서 시작해서 첫 번째 요소로 거슬러 올라가기 위함입니다.
    //Destroy(propertyPrefab[i]);는 propertyPrefab 리스트에 있는 각 UI 요소를 메모리에서 제거합니다. 
    //이것은 게임의 성능을 최적화하고 메모리 누수를 방지하는 데 중요합니다.
    //-------------------------------------------------------------
    //propertyPrefab.Clear();는 propertyPrefab 리스트를 비웁니다. 
    //이는 리스트에 저장된 모든 참조를 제거하여 리스트를 초기 상태로 되돌리는 작업입니다. 
    //이렇게 함으로써, 다음 번에 관리자 패널을 열었을 때 새롭고 정확한 정보로 UI를 채울 수 있습니다.

    void ClearProperties()
    {
        for (int i = propertyPrefab.Count - 1; i >= 0; i--)
        {Destroy(propertyPrefab[i]);}
        propertyPrefab.Clear();
    }
    //역순으로 리스트 순회: for 반복문을 사용하여 propertyPrefab 리스트의 마지막 요소부터 첫 번째 요소까지 역순으로 순회합니다.
    //이는 리스트에서 요소를 제거할 때 인덱스의 변화로 인한 오류를 방지하기 위함입니다.
    //객체 제거:Destroy(propertyPrefab[i]); 코드는 Unity의 Destroy 함수를 호출하여, 
    //현재 인덱스 i에 해당하는 게임 오브젝트를 제거합니다. 이는 게임의 메모리 관리에 중요합니다.
    //리스트 초기화:propertyPrefab.Clear(); 코드는 리스트의 모든 참조를 제거하여, 리스트를 초기 상태로 되돌립니다.
    //이는 리스트를 깔끔하게 비우고, 새로운 요소들을 추가하기 위한 준비를 합니다.

   

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
    //이 메소드는 플레이어의 현재 금액이 음수일 때, 그 금액의 절대값을 HandleInsufficientFunds 메소드로 전달합니다.
    //즉, 플레이어가 부채를 가지고 있다면, 그 부채의 금액을 처리하는 데 사용됩니다.\
    //플레이어의 돈이 -500일 때: Mathf.Abs(playerReference.ReadMoney) 는 -500의 절대값을 계산하여 500을 반환합니다.
    //HandleInsufficientFunds(500) 메소드가 호출되며, 플레이어는 500의 부채를 처리하기 위한 조치를 취해야 합니다.
}
