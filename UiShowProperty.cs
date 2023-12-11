using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;   

public class UiShowProperty : MonoBehaviour
{
        MonopolyNode nodeReference; // Monopoly ������ Ư�� ��带 �����ϱ� ���� ����
        Player playerReference;

        [Header("Buy Property UI")] // �ν����� â���� �� ������ UI ��Ҹ� �����ϱ� ���� ���
        [SerializeField] GameObject propertyUiPanel; // �Ӽ� ���� UI �г� ������Ʈ
        [SerializeField] TMP_Text propertyNameText; // �Ӽ� �̸��� ǥ���ϴ� �ؽ�Ʈ �ʵ�
        [SerializeField] Image colorField; // �Ӽ��� ������ ǥ���ϴ� �̹��� �ʵ�
        [Space] // �ν����� â���� ��� ���̿� ���� �߰�
        [SerializeField] TMP_Text rentPriceText; // ���� ���� ���� �Ӵ�Ḧ ǥ���ϴ� �ؽ�Ʈ �ʵ�
        [SerializeField] TMP_Text oneHouseRentText; // �� ä�� ���� ���� ���� �Ӵ�Ḧ ǥ���ϴ� �ؽ�Ʈ �ʵ�
        [SerializeField] TMP_Text twoHouseRentText; // �� ä�� ���� ���� ���� �Ӵ�Ḧ ǥ���ϴ� �ؽ�Ʈ �ʵ�
        [SerializeField] TMP_Text threeHouseRentText; // �� ä�� ���� ���� ���� �Ӵ�Ḧ ǥ���ϴ� �ؽ�Ʈ �ʵ�
        [SerializeField] TMP_Text fourHouseRentText; // �� ä�� ���� ���� ���� �Ӵ�Ḧ ǥ���ϴ� �ؽ�Ʈ �ʵ�
        [SerializeField] TMP_Text hotelRentText; // ȣ���� ���� ���� �Ӵ�Ḧ ǥ���ϴ� �ؽ�Ʈ �ʵ�
        [Space] // �ν����� â���� ��� ���̿� ���� �߰�
        [SerializeField] TMP_Text housePriceText; // �� ������ ǥ���ϴ� �ؽ�Ʈ �ʵ�
        [SerializeField] TMP_Text mortgagePriceText; // ȣ�� ������ ǥ���ϴ� �ؽ�Ʈ �ʵ�
        [Space] // �ν����� â���� ��� ���̿� ���� �߰�
        [SerializeField] Button buyPropertyButton; // �Ӽ� ���� ��ư
        [Space]
        [SerializeField] TMP_Text propertyPriceText;
        [SerializeField] TMP_Text playerMoneyText;

    void OnEnable()
    {
        MonopolyNode.OnShowPropertyBuyPanel += ShowBuyPropertyUi;
    }

    void OnDisable()
    {
        MonopolyNode.OnShowPropertyBuyPanel -= ShowBuyPropertyUi;
    }

    void Start()
    {
        propertyUiPanel.SetActive(false);
    }


    void ShowBuyPropertyUi(MonopolyNode node, Player currentPlayer)
    {
        nodeReference = node; // ���� ��带 ���� ������ �Ҵ�
        playerReference = currentPlayer;
        
        // ��� UI ���� ������Ʈ
            propertyNameText.text = node.name; // ����� �̸����� �Ӽ� �̸� �ؽ�Ʈ ������Ʈ
            colorField.color = node.propertyColorField.color; // ����� �������� ���� �ʵ� ������Ʈ

            // ī�� �߾� ���� ������Ʈ
            rentPriceText.text = "$" + node.baseRent; // �⺻ �Ӵ�� ǥ��
            oneHouseRentText.text = "$" + node.rentWithHouses[0]; // �� ä�� ���� ���� �� �Ӵ�� ǥ��
            twoHouseRentText.text = "$" + node.rentWithHouses[1]; // �� ä�� ���� ���� �� �Ӵ�� ǥ��
            threeHouseRentText.text = "$" + node.rentWithHouses[2]; // �� ä�� ���� ���� �� �Ӵ�� ǥ��
            fourHouseRentText.text = "$" + node.rentWithHouses[3]; // �� ä�� ���� ���� �� �Ӵ�� ǥ��
            hotelRentText.text = "$" + node.rentWithHouses[4]; // ȣ���� ���� �� �Ӵ�� ǥ��

            // �ǹ� ��� ������Ʈ
            housePriceText.text = "$" + node.houseCost; // �� ���� ǥ��
            mortgagePriceText.text = "$" + node.MorgageValue;

            // �ϴ� �� ���� ������Ʈ
            propertyPriceText.text = "Price: $" + node.price; // �Ӽ� ���� ǥ��
            playerMoneyText.text = "You have: $" + currentPlayer.ReadMoney; // ���� �÷��̾��� �� ǥ�� 

        if (currentPlayer.CanAffordNode(node.price))
        { 
            buyPropertyButton.interactable = true; 
        }
        else
        {
            buyPropertyButton.interactable = false; 
        }
        propertyUiPanel.SetActive(true);
    }
    public void BuyPropertyButton()
    {
        playerReference.BuyProperty(nodeReference);
        buyPropertyButton.interactable = false;
    }
    //BuyPropertyButton �Լ����� playerReference�� BuyProperty �޼ҵ带 ȣ���Ͽ�
    //nodeReference�� ����Ű�� �ε����� �����մϴ�. ����, buyPropertyButton�� interactable �Ӽ��� false�� �����Ͽ�,
    //����ڰ� ��ư�� �� �̻� Ŭ������ ���ϰ� �մϴ�. �̴� �Ϲ������� ����ڰ� �� �� �ε����� �����ϸ�, 
    //���� �ε����� �ݺ��ؼ� ������ �� ������ �ϱ� �����Դϴ�.
    public void ClosePropertyButton()
    {
        propertyUiPanel.SetActive(false);
        nodeReference = null;
        playerReference = null;
    }
    //nodeReference�� playerReference�� null�� �����Ͽ�, ������ �����մϴ�. 
    //�̴� ���� UI ��� �� ������ ���� ������ ���� ���� �ʵ��� �޸𸮸� �����ϴ� �� ������ �˴ϴ�.
    //���� ���, �÷��̾ "Park Place" �ε����� �����Ϸ��� �� ��, BuyPropertyButton�� Ŭ���ϸ� �ش� �ε����� �÷��̾��� ������ �ǰ�, 
    //���� ��ư�� ��Ȱ��ȭ�˴ϴ�. �÷��̾ â�� ������, ClosePropertyButton�� ȣ��Ǿ� UI�� �������, �������� �����˴ϴ�.
    //---------------------------------
    //�������� null�� �����Ǹ�, �ٸ� ��ũ��Ʈ���� �ش� ������ ����Ϸ� �� �� NullReferenceException ������ �߻��� ���ɼ��� �ֽ��ϴ�. 
    //Ư��, nodeReference�� playerReference�� ���������� ���� �����ϰ� ����ϰ� �ִٸ�, 
    //�� �������� null�� ������ ���Ŀ� �׵��� �����ϴ� �ٸ� �ڵ尡 ����� �� ������ ���� �� �ֽ��ϴ�.
    //�̸� �����ϱ� ���ؼ��� null�� �����ϱ� ����, �ش� �������� ���ǰ� �ִ� ��� ������ �� �������� null�� �ƴ��� üũ�ϴ� ������ �߰��ؾ� �մϴ�
    //����, �ش� �������� null�� �����ϴ� ������ ������ ����, �� �� �̻� ������ �ʿ� ���� �������� ����ǵ��� �ؾ� �մϴ�.
}
