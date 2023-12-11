using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MessageSystem : MonoBehaviour
{
    // �޽����� ǥ���� TMP_Text ������Ʈ�� �����Դϴ�.
    [SerializeField] TMP_Text messageText;

    // ��ü�� Ȱ��ȭ�� �� ȣ��˴ϴ�.
    private void OnEnable()
    {
        // ���� �޽����� ����� �Լ��� ȣ���մϴ�.
        ClearMessage();
        // GameManager, Player, MonopolyNode Ŭ�������� ������ �޽����� ���� �� �ֵ��� �����ʸ� �߰��մϴ�.
        GameManager.OnUpdateMessage += ReceiveMessage;
        Player.OnUpdateMessage += ReceiveMessage;
        MonopolyNode.OnUpdateMessage += ReceiveMessage;
        TradingSystem.OnUpdateMessage += ReceiveMessage;
    }

    // ��ü�� ��Ȱ��ȭ�� �� ȣ��˴ϴ�.
    private void OnDisable()
    {
        // GameManager, Player, MonopolyNode Ŭ�������� ������ �޽����� ���� �ʵ��� �����ʸ� �����մϴ�.
        GameManager.OnUpdateMessage -= ReceiveMessage;
        Player.OnUpdateMessage -= ReceiveMessage;
        MonopolyNode.OnUpdateMessage -= ReceiveMessage;
        TradingSystem.OnUpdateMessage -= ReceiveMessage;
    }

    // �޽����� �޾� ǥ���ϴ� �Լ��Դϴ�.
    void ReceiveMessage(string _message)
    {
        // �޽��� �ؽ�Ʈ�� ���� �޽����� �����մϴ�.
        messageText.text = _message;
    }

    // �޽����� ����� �Լ��Դϴ�.
    void ClearMessage()
    {
        // �޽��� �ؽ�Ʈ�� �� ���ڿ��� �����Ͽ� ȭ�鿡�� �޽����� ����ϴ�.
        messageText.text = "";
    }
}
