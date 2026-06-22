using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] private ItemType itemType;
    [SerializeField] private int amount = 1;
    [SerializeField] private string achievementID;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        InventoryManager.Instance.PickupItem(
            itemType,
            amount,
            achievementID
        );

        Destroy(gameObject);
    }
}