using UnityEngine;

public class Coin : MonoBehaviour
{
    public int coinValue = 10; // 这个金币值多少钱

    void OnTriggerEnter2D(Collider2D other)
    {
        // 判断碰到金币的是不是玩家
        if (other.CompareTag("Player"))
        {
            // 增加金币
            InventoryManager.Instance.AddGold(coinValue);
            
            // 销毁金币
            Destroy(gameObject);
        }
    }
}