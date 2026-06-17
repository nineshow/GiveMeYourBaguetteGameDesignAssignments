using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{

    [Range(0f,1f)]
    public float damageReduction=0.2f;

    public bool isDefending;

    void Update()
    {
        //hold K to defend
        isDefending=Input.GetKey(KeyCode.K);
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
}
