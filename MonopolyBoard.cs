using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MonopolyBoard : MonoBehaviour
{
    public static MonopolyBoard instance;

    public List<MonopolyNode> route = new List<MonopolyNode>();

    [System.Serializable]
    public class NodeSet
    {
        public Color setColor = Color.white;
        public List<MonopolyNode> nodesInSetList = new List<MonopolyNode>();

    }
    public List<NodeSet> nodeSetList = new List<NodeSet>();

    private void Awake()
    { instance = this; }

    void OnValidate()
    {
        route.Clear();
        foreach (Transform node in transform.GetComponentInChildren<Transform>())
        {
            route.Add(node.GetComponent<MonopolyNode>());
        }

    }
    void OnDrawGizmos()
    {
        if (route.Count > 1)
        {
            for (int i = 0; i < route.Count; i++)
            {
                Vector3 current = route[i].transform.position;
                Vector3 next = (i + 1 < route.Count) ? route[i + 1].transform.position : current;

                Gizmos.color = Color.green;
                Gizmos.DrawLine(current, next);
            }
        }
    }

    // 플레이어의 토큰을 주어진 스텝 수만큼 이동시키는 함수입니다.
    public void MovePlayerToken(int steps, Player player)
    {
        // MovePlayerInSteps 코루틴을 시작합니다. 코루틴은 지정된 단계를 거쳐 토큰을 이동시킵니다.
        StartCoroutine(MovePlayerInSteps(steps, player));
    }

    public void MovePlayerToken(MonopolyNodeType type, Player player)//이 코드는 보드를 순환하며 특정 유형의 노드를 찾을 때까지 노드 인덱스를 하나씩 증가시키는 로직을 구현하고 있습니다.
        //원하는 유형의 노드를 찾지 못했을 경우, 오류 메시지를 출력하고 함수를 종료하여, 무한 루프에 빠지지 않도록 합니다.
    {
        // 찾고자 하는 노드 타입의 인덱스를 저장할 변수를 초기화합니다.
        int indexOfNextNodeType = -1; // 찾고자 하는 노드 타입이 없으면 -1로 유지됩니다.
      
        // 현재 플레이어 위치의 인덱스를 찾습니다.
        int indexOnBoard = route.IndexOf(player.MymonopolyNode); // 플레이어의 현재 노드 인덱스
      
        // 검색을 시작할 인덱스를 설정합니다. 보드의 끝에 도달하면 다시 처음으로 돌아갑니다.
        int startSearchIndex = (indexOnBoard + 1) % route.Count; // 다음 노드부터 검색을 시작

        // 검색한 노드의 수를 계산하는 변수입니다.
        int nodeSearches = 0; // 검색을 시작할 때 0으로 초기화

        // 원하는 노드 타입을 찾거나, 보드의 모든 노드를 검색할 때까지 루프를 실행합니다.
        while (indexOfNextNodeType == -1 && nodeSearches < route.Count) // 조건에 맞을 때까지 반복
        {
            // 현재 검색 인덱스의 노드 타입이 찾고자 하는 타입과 일치하는지 확인합니다.
            if (route[startSearchIndex].monopolyNodeType == type)
            {
                // 일치한다면, 그 인덱스를 indexOfNextNodeType에 저장합니다.
                indexOfNextNodeType = startSearchIndex;
            }
            startSearchIndex = (startSearchIndex + 1) % route.Count;
            //현재 인덱스(startSearchIndex)에서 원하는 노드 타입을 찾지 못했을 경우, 다음 인덱스로 이동해야 합니다. 이를 위해 현재 startSearchIndex에 1을 더하고 있습니다.
            //모듈로 연산 % route.Count는 인덱스가 보드의 끝에 도달했을 때(즉, route.Count의 값과 같거나 큰 값이 되었을 때) 인덱스를 0으로 리셋합니다.이렇게 하면 보드의 노드를 순환할 수 있습니다.
            //게임 보드는 순환 구조를 가지고 있으므로, 마지막 노드를 검사한 후에는 다시 첫 번째 노드로 돌아가서 검색을 계속해야 합니다. 이 때 모듈로 연산이 순환 로직을 지원해 줍니다.

            // 검색한 노드의 수를 하나 증가시킵니다.
            nodeSearches++;
        }

        // 원하는 노드 타입을 찾지 못했을 경우, 오류 메시지를 출력하고 함수를 종료합니다.
        if (indexOfNextNodeType == -1)
        {
            Debug.LogError("NO NODE FOUND"); // 로그에 에러 메시지 출력
            return; // 함수 종료
        }

        // 플레이어를 찾은 노드까지 이동시키는 코루틴을 시작합니다.
        StartCoroutine(MovePlayerInSteps(nodeSearches, player));
    }
    


    public

    // 플레이어의 토큰을 지정된 스텝 수만큼 이동시키는 코루틴입니다.
    IEnumerator MovePlayerInSteps(int steps, Player player)
    {
        yield return new WaitForSeconds(0.5f);

        // 남은 이동 스텝 수를 저장합니다.
        int stepsLeft = steps;

        // 이동할 플레이어의 토큰 객체를 가져옵니다.
        GameObject tokenToMove = player.MyToken;

        // 플레이어의 현재 노드 인덱스를 찾습니다.
        int indexOnBoard = route.IndexOf(player.MymonopolyNode);

        // 'Go' 지점을 지나쳤는지 여부를 나타내는 플래그입니다.
        bool moveOverGo = false;

        // 플레이어가 앞으로 이동하는지 여부를 나타내는 플래그입니다.
        bool isMovingFowerd = steps > 0;

        // 플레이어가 앞으로 이동하는 경우
        if (isMovingFowerd)
        {
            // 남은 이동 스텝이 0보다 클 때까지 반복합니다.
            while (stepsLeft > 0)
            {
                // 다음 노드로 인덱스를 이동합니다.
                indexOnBoard++;
                // 인덱스가 루트 배열의 마지막 인덱스를 초과하면 처음으로 돌아갑니다.
                if (indexOnBoard > route.Count - 1)
                {
                    indexOnBoard = 0;
                    moveOverGo = true; // 'Go' 지점을 지났음을 나타냅니다.
                }

                // 목표 위치를 설정합니다.
                Vector3 endPos = route[indexOnBoard].transform.position;
                // 목표 위치까지 이동하는 동안 대기합니다.
                while (MoveToNextNode(tokenToMove, endPos, 20))
                {
                    yield return null; // 다음 프레임까지 대기합니다.
                }
                // 한 스텝 이동을 완료하고 남은 스텝 수를 감소시킵니다.
                stepsLeft--;
            }
        }
        else// 플레이어가 뒤로 이동하는 경우 (예: 카드 효과에 의해,감옥에 가세요 노드에 의해,)
        {
            // 남은 이동 스텝이 0보다 작을 때까지 반복합니다.
            while (stepsLeft < 0)
            {
                // 이전 노드로 인덱스를 이동합니다.
                indexOnBoard--;
                // 인덱스가 0보다 작아지면 마지막 인덱스로 돌아갑니다.
                if (indexOnBoard < 0)
                {
                    indexOnBoard = route.Count - 1;
                }

                // 목표 위치를 설정합니다.
                Vector3 endPos = route[indexOnBoard].transform.position;
                // 목표 위치까지 이동하는 동안 대기합니다.
                while (MoveToNextNode(tokenToMove, endPos, 20))
                {
                    yield return null; // 다음 프레임까지 대기합니다.
                }
                // 한 스텝 이동을 완료하고 남은 스텝 수를 증가시킵니다.
                stepsLeft++;
            }
        }
        //stepsLeft++로 숫자를 늘리는 것이 아니라, 주사위 결과로 얻은 수(이 경우는 5)로 stepsLeft를 초기화하고, 
        //플레이어가 한 칸씩 이동할 때마다 stepsLeft--를 사용하여 남은 이동 칸 수를 하나씩 줄여 나갑니다. 
        //이는 플레이어가 정확히 주사위에 나온 칸 수만큼 이동하도록 보장합니다. stepsLeft가 0이 되면, 
        //플레이어의 이동이 완료된 것으로 처리됩니다.

        // 'Go' 지점을 지났다면, 'Go' 지점에서 받을 돈을 플레이어에게 주어야 합니다.
        if (moveOverGo)
        {
            player.CollectMoney(GameManager.instance.GetGoMoney);
        }
        // 플레이어의 현재 노드를 업데이트합니다.
        player.SetMyCurrentNode(route[indexOnBoard]);
    }

    
    bool MoveToNextNode(GameObject tokenToMove, Vector3 endPos, float speed)// 플레이어의 토큰을 목표 위치(endPos)로 이동시키는 메서드입니다.
    {
        // Vector3.MoveTowards를 사용하여 현재 위치에서 목표 위치로 일정 속도(speed)로 이동합니다.
        // Time.deltaTime을 곱하여 프레임 속도에 관계없이 일관된 속도로 이동합니다.
        tokenToMove.transform.position = Vector3.MoveTowards(tokenToMove.transform.position, endPos, speed * Time.deltaTime);

        // 이 메서드는 토큰이 아직 목표 위치에 도달하지 않았으면 true를, 도달했다면 false를 반환합니다.
        return endPos != tokenToMove.transform.position;
    }


    internal (List<MonopolyNode> list, bool allSame) PlayerHasAllNodesOfSet(MonopolyNode node)// 플레이어가 가진 노드가 속한 세트의 모든 노드를 소유하고 있는지 확인하는 메서드입니다.
    {
        bool allSame = false; // 모든 노드를 소유하고 있는지의 여부를 저장할 변수입니다.
        foreach (var nodeSet in nodeSetList)// nodeSetList의 모든 노드 세트를 순회합니다.
        {
            if (nodeSet.nodesInSetList.Contains(node)) // 현재 노드가 해당 노드 세트에 포함되어 있는지 확인합니다.
            {
                allSame = nodeSet.nodesInSetList.All(_node => _node.Owner == node.Owner);// 해당 노드 세트에 포함된 모든 노드가 동일한 플레이어에게 소유되었는지 확인합니다.
                return (nodeSet.nodesInSetList, allSame);   // 해당 노드 세트와 소유 여부를 반환합니다.
            }
        }
        return (null, allSame);// 하나의 노드 세트도 완전히 소유하지 않은 경우 null과 false를 반환합니다.
    }

}
