using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 
using UnityEngine.UI; 

public class TradePropertyCard : MonoBehaviour
{
    MonopolyNode nodeReference;

    [SerializeField] Image colorField; // �ε��� ������ ���� UI �̹���
    [SerializeField] TMP_Text propertyNameText; // �ε��� �̸��� ���� �ؽ�Ʈ �ʵ�
    [SerializeField] Image typeImage; // �ε��� ����(��, ö��, �����ü� ��)�� ��Ÿ���� �̹���
    [SerializeField] Sprite houseSprite, railroadSprite, utilitySprite; // �پ��� �ε��� ������ ���� ��������Ʈ
    [SerializeField] GameObject mortgageImage; // �ε����� �㺸 �����Ǿ����� ��Ÿ���� �̹���
    [SerializeField] TMP_Text propertyPriceText; // �ε��� ������ ���� �ؽ�Ʈ �ʵ�
    [SerializeField] Toggle toggleButton; // �ŷ����� �ε����� �����ϱ� ���� ��� ��ư

    public void SetTradeCard(MonopolyNode node, ToggleGroup toggleGroup)
    {
        nodeReference = node; 
        colorField.color = (node.propertyColorField != null) ? node.propertyColorField.color : Color.black; 
        propertyNameText.text = node.name; 

        switch (node.monopolyNodeType)
        {
            case MonopolyNodeType.Property:
                typeImage.sprite = houseSprite;
                typeImage.color = Color.blue;
                break;
            case MonopolyNodeType.Railroad:
                typeImage.sprite = railroadSprite;
                typeImage.color = Color.white;
                break;
            case MonopolyNodeType.Utility:
                typeImage.sprite = utilitySprite;
                typeImage.color = Color.black;
                break;
        }
        mortgageImage.SetActive(node.IsMortgaged);
        propertyPriceText.text = "$" + node.price;
        toggleButton.isOn = false;//isOn �Ӽ��� ����� ����(����/����)
        toggleButton.group = toggleGroup;
    }

    public MonopolyNode Node()
    {
        return nodeReference;
    }
}
