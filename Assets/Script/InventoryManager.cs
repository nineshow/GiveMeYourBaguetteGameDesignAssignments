using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Player Data")]
    public int gold = 0;
    public int potionCount = 0;

    [Header("Shop Settings")]
    public int potionPrice = 20;
    public int healAmount = 30;

    [Header("UI Elements(Always Visible)")]
    public Text goldText;
    public TextMeshProUGUI potionQuantityText;
    public Button usePotionButton;  

    [Header("Inventory Settings (Tab Panel)")]
    public GameObject inventoryPanel;
    private bool isInventoryOpen = false;
    private HealthPoint currentHP;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        // Automatically find the player by Tag and get the health script
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            currentHP = player.GetComponent<HealthPoint>();
        }

        UpdateUI();
        
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            UsePotion();
        }
    }

    public void AddGold(int amount)
    {
        gold += amount;
        UpdateUI();
        Debug.Log("Gold collected! Current Total: " + gold);
    }

    // Called by the Buy Button
    public void BuyPotion()
    {
        if (gold >= potionPrice)
        {
            gold -= potionPrice;
            potionCount++;
            UpdateUI();
            Debug.Log("Potion bought! Total potions: " + potionCount);
        }
        else
        {
            Debug.Log("Not enough gold!");
        }
    }

    // Called by the Use Button
    public void UsePotion()
    {
        if (potionCount > 0)
        {
            if (currentHP != null)
            {
                potionCount--;
                currentHP.Heal(healAmount);
                UpdateUI();
            }
        }
        else
        {
            Debug.Log("No potions left!");
        }
    }

   private void UpdateUI()
    {
        if (goldText != null) goldText.text = "Gold: " + gold.ToString();
        
        // 【修改】：只显示纯数字，赋值给你右下角的文本
        if (potionQuantityText != null) 
        {
            potionQuantityText.text = potionCount.ToString();
        }
    }

    private void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        if (inventoryPanel != null) inventoryPanel.SetActive(isInventoryOpen);
    }

    public void PickupItem(ItemType itemType, int amount = 1, string achievementID = null)
{
    switch (itemType)
    {
        case ItemType.Potion:
            potionCount += amount;
            UpdateUI();
            break;

        case ItemType.Key:
            Debug.Log("Key picked up");
            break;
    }

    Debug.Log("Picked up: " + itemType + " x" + amount);

    if (!string.IsNullOrEmpty(achievementID))
    {
        AchievementManager.Instance.Unlock(achievementID);
    }
}
}