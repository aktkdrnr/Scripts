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

    // �÷��̾��� ��ū�� �־��� ���� ����ŭ �̵���Ű�� �Լ��Դϴ�.
    public void MovePlayerToken(int steps, Player player)
    {
        // MovePlayerInSteps �ڷ�ƾ�� �����մϴ�. �ڷ�ƾ�� ������ �ܰ踦 ���� ��ū�� �̵���ŵ�ϴ�.
        StartCoroutine(MovePlayerInSteps(steps, player));
    }

    public void MovePlayerToken(MonopolyNodeType type, Player player)//�� �ڵ�� ���带 ��ȯ�ϸ� Ư�� ������ ��带 ã�� ������ ��� �ε����� �ϳ��� ������Ű�� ������ �����ϰ� �ֽ��ϴ�.
        //���ϴ� ������ ��带 ã�� ������ ���, ���� �޽����� ����ϰ� �Լ��� �����Ͽ�, ���� ������ ������ �ʵ��� �մϴ�.
    {
        // ã���� �ϴ� ��� Ÿ���� �ε����� ������ ������ �ʱ�ȭ�մϴ�.
        int indexOfNextNodeType = -1; // ã���� �ϴ� ��� Ÿ���� ������ -1�� �����˴ϴ�.
      
        // ���� �÷��̾� ��ġ�� �ε����� ã���ϴ�.
        int indexOnBoard = route.IndexOf(player.MymonopolyNode); // �÷��̾��� ���� ��� �ε���
      
        // �˻��� ������ �ε����� �����մϴ�. ������ ���� �����ϸ� �ٽ� ó������ ���ư��ϴ�.
        int startSearchIndex = (indexOnBoard + 1) % route.Count; // ���� ������ �˻��� ����

        // �˻��� ����� ���� ����ϴ� �����Դϴ�.
        int nodeSearches = 0; // �˻��� ������ �� 0���� �ʱ�ȭ

        // ���ϴ� ��� Ÿ���� ã�ų�, ������ ��� ��带 �˻��� ������ ������ �����մϴ�.
        while (indexOfNextNodeType == -1 && nodeSearches < route.Count) // ���ǿ� ���� ������ �ݺ�
        {
            // ���� �˻� �ε����� ��� Ÿ���� ã���� �ϴ� Ÿ�԰� ��ġ�ϴ��� Ȯ���մϴ�.
            if (route[startSearchIndex].monopolyNodeType == type)
            {
                // ��ġ�Ѵٸ�, �� �ε����� indexOfNextNodeType�� �����մϴ�.
                indexOfNextNodeType = startSearchIndex;
            }
            startSearchIndex = (startSearchIndex + 1) % route.Count;
            //���� �ε���(startSearchIndex)���� ���ϴ� ��� Ÿ���� ã�� ������ ���, ���� �ε����� �̵��ؾ� �մϴ�. �̸� ���� ���� startSearchIndex�� 1�� ���ϰ� �ֽ��ϴ�.
            //���� ���� % route.Count�� �ε����� ������ ���� �������� ��(��, route.Count�� ���� ���ų� ū ���� �Ǿ��� ��) �ε����� 0���� �����մϴ�.�̷��� �ϸ� ������ ��带 ��ȯ�� �� �ֽ��ϴ�.
            //���� ����� ��ȯ ������ ������ �����Ƿ�, ������ ��带 �˻��� �Ŀ��� �ٽ� ù ��° ���� ���ư��� �˻��� ����ؾ� �մϴ�. �� �� ���� ������ ��ȯ ������ ������ �ݴϴ�.

            // �˻��� ����� ���� �ϳ� ������ŵ�ϴ�.
            nodeSearches++;
        }

        // ���ϴ� ��� Ÿ���� ã�� ������ ���, ���� �޽����� ����ϰ� �Լ��� �����մϴ�.
        if (indexOfNextNodeType == -1)
        {
            Debug.LogError("NO NODE FOUND"); // �α׿� ���� �޽��� ���
            return; // �Լ� ����
        }

        // �÷��̾ ã�� ������ �̵���Ű�� �ڷ�ƾ�� �����մϴ�.
        StartCoroutine(MovePlayerInSteps(nodeSearches, player));
    }
    


    public

    // �÷��̾��� ��ū�� ������ ���� ����ŭ �̵���Ű�� �ڷ�ƾ�Դϴ�.
    IEnumerator MovePlayerInSteps(int steps, Player player)
    {
        yield return new WaitForSeconds(0.5f);

        // ���� �̵� ���� ���� �����մϴ�.
        int stepsLeft = steps;

        // �̵��� �÷��̾��� ��ū ��ü�� �����ɴϴ�.
        GameObject tokenToMove = player.MyToken;

        // �÷��̾��� ���� ��� �ε����� ã���ϴ�.
        int indexOnBoard = route.IndexOf(player.MymonopolyNode);

        // 'Go' ������ �����ƴ��� ���θ� ��Ÿ���� �÷����Դϴ�.
        bool moveOverGo = false;

        // �÷��̾ ������ �̵��ϴ��� ���θ� ��Ÿ���� �÷����Դϴ�.
        bool isMovingFowerd = steps > 0;

        // �÷��̾ ������ �̵��ϴ� ���
        if (isMovingFowerd)
        {
            // ���� �̵� ������ 0���� Ŭ ������ �ݺ��մϴ�.
            while (stepsLeft > 0)
            {
                // ���� ���� �ε����� �̵��մϴ�.
                indexOnBoard++;
                // �ε����� ��Ʈ �迭�� ������ �ε����� �ʰ��ϸ� ó������ ���ư��ϴ�.
                if (indexOnBoard > route.Count - 1)
                {
                    indexOnBoard = 0;
                    moveOverGo = true; // 'Go' ������ �������� ��Ÿ���ϴ�.
                }

                // ��ǥ ��ġ�� �����մϴ�.
                Vector3 endPos = route[indexOnBoard].transform.position;
                // ��ǥ ��ġ���� �̵��ϴ� ���� ����մϴ�.
                while (MoveToNextNode(tokenToMove, endPos, 20))
                {
                    yield return null; // ���� �����ӱ��� ����մϴ�.
                }
                // �� ���� �̵��� �Ϸ��ϰ� ���� ���� ���� ���ҽ�ŵ�ϴ�.
                stepsLeft--;
            }
        }
        else// �÷��̾ �ڷ� �̵��ϴ� ��� (��: ī�� ȿ���� ����,������ ������ ��忡 ����,)
        {
            // ���� �̵� ������ 0���� ���� ������ �ݺ��մϴ�.
            while (stepsLeft < 0)
            {
                // ���� ���� �ε����� �̵��մϴ�.
                indexOnBoard--;
                // �ε����� 0���� �۾����� ������ �ε����� ���ư��ϴ�.
                if (indexOnBoard < 0)
                {
                    indexOnBoard = route.Count - 1;
                }

                // ��ǥ ��ġ�� �����մϴ�.
                Vector3 endPos = route[indexOnBoard].transform.position;
                // ��ǥ ��ġ���� �̵��ϴ� ���� ����մϴ�.
                while (MoveToNextNode(tokenToMove, endPos, 20))
                {
                    yield return null; // ���� �����ӱ��� ����մϴ�.
                }
                // �� ���� �̵��� �Ϸ��ϰ� ���� ���� ���� ������ŵ�ϴ�.
                stepsLeft++;
            }
        }
        //stepsLeft++�� ���ڸ� �ø��� ���� �ƴ϶�, �ֻ��� ����� ���� ��(�� ���� 5)�� stepsLeft�� �ʱ�ȭ�ϰ�, 
        //�÷��̾ �� ĭ�� �̵��� ������ stepsLeft--�� ����Ͽ� ���� �̵� ĭ ���� �ϳ��� �ٿ� �����ϴ�. 
        //�̴� �÷��̾ ��Ȯ�� �ֻ����� ���� ĭ ����ŭ �̵��ϵ��� �����մϴ�. stepsLeft�� 0�� �Ǹ�, 
        //�÷��̾��� �̵��� �Ϸ�� ������ ó���˴ϴ�.

        // 'Go' ������ �����ٸ�, 'Go' �������� ���� ���� �÷��̾�� �־�� �մϴ�.
        if (moveOverGo)
        {
            player.CollectMoney(GameManager.instance.GetGoMoney);
        }
        // �÷��̾��� ���� ��带 ������Ʈ�մϴ�.
        player.SetMyCurrentNode(route[indexOnBoard]);
    }

    
    bool MoveToNextNode(GameObject tokenToMove, Vector3 endPos, float speed)// �÷��̾��� ��ū�� ��ǥ ��ġ(endPos)�� �̵���Ű�� �޼����Դϴ�.
    {
        // Vector3.MoveTowards�� ����Ͽ� ���� ��ġ���� ��ǥ ��ġ�� ���� �ӵ�(speed)�� �̵��մϴ�.
        // Time.deltaTime�� ���Ͽ� ������ �ӵ��� ������� �ϰ��� �ӵ��� �̵��մϴ�.
        tokenToMove.transform.position = Vector3.MoveTowards(tokenToMove.transform.position, endPos, speed * Time.deltaTime);

        // �� �޼���� ��ū�� ���� ��ǥ ��ġ�� �������� �ʾ����� true��, �����ߴٸ� false�� ��ȯ�մϴ�.
        return endPos != tokenToMove.transform.position;
    }


    internal (List<MonopolyNode> list, bool allSame) PlayerHasAllNodesOfSet(MonopolyNode node)// �÷��̾ ���� ��尡 ���� ��Ʈ�� ��� ��带 �����ϰ� �ִ��� Ȯ���ϴ� �޼����Դϴ�.
    {
        bool allSame = false; // ��� ��带 �����ϰ� �ִ����� ���θ� ������ �����Դϴ�.
        foreach (var nodeSet in nodeSetList)// nodeSetList�� ��� ��� ��Ʈ�� ��ȸ�մϴ�.
        {
            if (nodeSet.nodesInSetList.Contains(node)) // ���� ��尡 �ش� ��� ��Ʈ�� ���ԵǾ� �ִ��� Ȯ���մϴ�.
            {
                allSame = nodeSet.nodesInSetList.All(_node => _node.Owner == node.Owner);// �ش� ��� ��Ʈ�� ���Ե� ��� ��尡 ������ �÷��̾�� �����Ǿ����� Ȯ���մϴ�.
                return (nodeSet.nodesInSetList, allSame);   // �ش� ��� ��Ʈ�� ���� ���θ� ��ȯ�մϴ�.
            }
        }
        return (null, allSame);// �ϳ��� ��� ��Ʈ�� ������ �������� ���� ��� null�� false�� ��ȯ�մϴ�.
    }

}
