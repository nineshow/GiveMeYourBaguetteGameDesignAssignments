using UnityEngine;

public class WeaponDamage : MonoBehaviour
{
    // set damage number
    public int damage = 10;

    // if is triggered
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if it has HealthPoint component
        HealthPoint health = other.GetComponent<HealthPoint>();

        // if no health component, just return and end
        if(health==null)
        {
            return;
        }

        // player will not be hurt by player weapon
        if(gameObject.CompareTag("PlayerWeapon") 
        && other.CompareTag("Player"))
        {
            return;
        }


        // monster will not be hurt by monster weapon
        if(gameObject.CompareTag("MonsterWeapon") 
        && other.CompareTag("Monster"))
        {
            return;
        }

        // if reach here, can just do damage as usual
        health.TakeDamage(damage);
    }
}