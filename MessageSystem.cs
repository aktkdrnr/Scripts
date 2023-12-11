using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MessageSystem : MonoBehaviour
{
    // 메시지를 표시할 TMP_Text 컴포넌트의 참조입니다.
    [SerializeField] TMP_Text messageText;

    // 객체가 활성화될 때 호출됩니다.
    private void OnEnable()
    {
        // 기존 메시지를 지우는 함수를 호출합니다.
        ClearMessage();
        // GameManager, Player, MonopolyNode 클래스에서 보내는 메시지를 받을 수 있도록 리스너를 추가합니다.
        GameManager.OnUpdateMessage += ReceiveMessage;
        Player.OnUpdateMessage += ReceiveMessage;
        MonopolyNode.OnUpdateMessage += ReceiveMessage;
        TradingSystem.OnUpdateMessage += ReceiveMessage;
    }

    // 객체가 비활성화될 때 호출됩니다.
    private void OnDisable()
    {
        // GameManager, Player, MonopolyNode 클래스에서 보내는 메시지를 받지 않도록 리스너를 제거합니다.
        GameManager.OnUpdateMessage -= ReceiveMessage;
        Player.OnUpdateMessage -= ReceiveMessage;
        MonopolyNode.OnUpdateMessage -= ReceiveMessage;
        TradingSystem.OnUpdateMessage -= ReceiveMessage;
    }

    // 메시지를 받아 표시하는 함수입니다.
    void ReceiveMessage(string _message)
    {
        // 메시지 텍스트에 받은 메시지를 설정합니다.
        messageText.text = _message;
    }

    // 메시지를 지우는 함수입니다.
    void ClearMessage()
    {
        // 메시지 텍스트를 빈 문자열로 설정하여 화면에서 메시지를 지웁니다.
        messageText.text = "";
    }
}
