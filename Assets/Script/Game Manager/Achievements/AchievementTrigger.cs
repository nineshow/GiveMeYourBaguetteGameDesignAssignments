using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementTrigger : MonoBehaviour
{
    [SerializeField] private string achievementID;
    [SerializeField] private bool isDie=false;
    

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            AchievementManager.Instance.Unlock(achievementID);
            if(isDie)
            {
                other.GetComponent<HealthPoint>().TakeDamage(100);
            }
        }
    }
}
