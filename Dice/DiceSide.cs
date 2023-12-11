using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceSide : MonoBehaviour
{
    bool onGround;
    public bool OnGround => onGround;

     void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            onGround = true;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            onGround = false;
        }
    }
    public int SideValue()
    {
        int value = Int32.Parse(name);
        return value;
      
    }
    //Int32.Parse(name): �� �κ��� Int32 Ŭ������ Parse �޼��带 ȣ���մϴ�. �� �޼���� ���ڿ��� ����(int)�� ��ȯ�մϴ�,
}
