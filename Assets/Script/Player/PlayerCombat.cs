using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{

    [Range(0f,1f)]
    public float damageReduction=0.2f;

    public bool isDefending;

    [Header("Charge Attack")]
    public int currentCharge=0;
    public int maxCharge=100;
    public bool chargeAttackReady=false;
    public KeyCode chargeAttackKey=KeyCode.R;
    public bool chargeAttackActivated=false;

    private Animator anim; // 【新增】：声明动画控制器

    void Start()
    {
        // 【新增】：游戏开始时获取角色身上的 Animator 组件
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        //hold K to defend
        isDefending = Input.GetKey(KeyCode.K);

        // 【核心新增】：把 isDefending 的状态（true或false）实时传递给动画状态机
        if (anim != null)
        {
            anim.SetBool("isDefending", isDefending);
        }

    if(Input.GetKeyDown(chargeAttackKey) && chargeAttackReady)
    {
        //trigger the charge attack

        chargeAttackActivated=true;
       
       if(anim!=null)
        {
            anim.SetTrigger("ChargeAttack");
        }

       Debug.Log("Charge Attack Activated!");
    }

    }

    // to get the reduction percent
    public float GetDamageMultiplier()
    {   
        //if is defending
        if(isDefending)
        {   
            //return the reduction
            return 1f-damageReduction;
        }

        //else just return 1 (no reduction)
        return 1f;
    }

    public void AddCharge(int amount)
    {
        currentCharge += amount;
        if(currentCharge>=maxCharge)
        {
            currentCharge=maxCharge;
            chargeAttackReady=true;
        }

        Debug.Log("Current Charge: "+currentCharge+"/"+maxCharge);
    }

    public bool IsChargeAttackReady()
    {
        return chargeAttackReady;
    }

    public bool ConsumeChargeAttack()
    {
        if (!chargeAttackActivated|| !chargeAttackReady)
        {
            Debug.Log("Current Charge: "+currentCharge+"/"+maxCharge);
            return false;
        }

        currentCharge = 0;
        chargeAttackReady = false;
        chargeAttackActivated = false;

        Debug.Log("Charge Attack Used!");

        return true;
    }
}
