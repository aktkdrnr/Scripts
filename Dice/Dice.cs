using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{
    Rigidbody rb;
    bool hasLanded;
    bool thrown;

    Vector3 initPossition;
    int diceValue;

    [SerializeField] DiceSide[] diceSides;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        initPossition = transform.position;
        rb.useGravity = false;
        rb.isKinematic = true;
    }
    //isKinematic을 true로 설정하면 해당 객체는 더 이상 중력의 영향을 받지 않고, 충돌 시 물리적 힘을 전달하지 않으며,
    //다른 Rigidbody 객체들과의 상호작용도 하지 않게 됩니다.
    //반대로 isKinematic을 false로 설정하면 객체는 Unity의 물리 엔진에 의해 관리되며 중력, 충돌, 힘 등에 반응하게 됩니다.


    void Update()
    {
        if (rb.IsSleeping() && !hasLanded && thrown)
        {
            hasLanded = true;
            rb.useGravity = false;
            rb.isKinematic = true;
            SideValueCheck();
        }
        else if (rb.IsSleeping() && hasLanded && diceValue == 0)
        {
            ReRollDice();
        }
    }

    public void RollDice()
    {
        Reset();
        if (!thrown && !hasLanded)
        {
            thrown = true;
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.AddTorque(Random.Range(0, 500), Random.Range(0, 500), Random.Range(0, 500));
        }
      /*  else if (thrown && hasLanded)
        {
            Reset();
        }*/
    }
    void Reset()
    {
        transform.position = initPossition;
        thrown = false;
        hasLanded = false;
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    void ReRollDice()
    {
        Reset();
        thrown = true;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.AddTorque(Random.Range(0, 500), Random.Range(0, 500), Random.Range(0, 500));
    }

    void SideValueCheck()
    {
        diceValue = 0;
        foreach (var side in diceSides)
        {
            if (side.OnGround)
            {
                diceValue = side.SideValue();
                Debug.Log("ROLLED NUMBER" + diceValue);
                break;
            }
        }
        GameManager.instance.ReportDiceRolled(diceValue);
    }
}

