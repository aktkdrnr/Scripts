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
    //Int32.Parse(name): 이 부분은 Int32 클래스의 Parse 메서드를 호출합니다. 이 메서드는 문자열을 정수(int)로 변환합니다,
}
