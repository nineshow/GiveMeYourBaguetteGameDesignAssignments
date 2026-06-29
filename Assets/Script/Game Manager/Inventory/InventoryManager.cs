using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Core Data Asset (連接你的PlayerData檔案)")]
    public PlayerData playerData; // 👈 這裡就是把剛才在 Project 裡建的 NewPlayerData 檔案拖進來！

    [Header("Shop Settings")]
    public int potionPrice = 20;
    public int healAmount = 30;

    [Header("UI Elements(Always Visible)")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI potionQuantityText;
    public Button usePotionButton;  

    [Header("Inventory Settings (Tab Panel)")]
    public GameObject inventoryPanel;
    private bool isInventoryOpen = false;
    private HealthPoint currentHP;

    // 用於本關開局的備份變數（實現死亡回溯）
    private int backupGold;
    private int backupPotionCount;
    private int backupHealth;

    void Awake()
    {
        // 這裡不需要用 DontDestroyOnLoad 了，因為 PlayerData 檔案本身就不會隨場景銷毀！
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        // 1. 自動尋找當前關卡的玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            currentHP = player.GetComponent<HealthPoint>();
        }

        // 2. 備份剛進這一關時的初始狀態（方便死掉時回溯物資）
        if (playerData != null)
        {
            backupGold = playerData.gold;
            backupPotionCount = playerData.potionCount;
            backupHealth = playerData.currentHealth;
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

    //  專門給 LevelManager 的 RestartLevel() 呼叫
    public void ResetToLevelStart()
    {
        if (playerData != null)
        {
            playerData.gold = backupGold;
            playerData.potionCount = backupPotionCount;
            playerData.currentHealth = backupHealth;
        }
        UpdateUI();
        Debug.Log("【Restart 成功】數據已完美回溯到本關開局狀態！");
    }

    public void AddGold(int amount)
    {
        if (playerData != null)
        {
            playerData.gold += amount; //  直接修改檔案數據
            UpdateUI();
        }
    }

    // 🛒 點擊購買按鈕觸發這個
    public void BuyPotion()
    {
        if (playerData == null) return;

        if (playerData.gold >= potionPrice)
        {
            playerData.gold -= potionPrice; //  直接修改檔案數據
            playerData.potionCount++;       //  直接修改檔案數據
            UpdateUI();
            Debug.Log("Potion bought! Total potions: " + playerData.potionCount);
        }
        else
        {
            Debug.Log("Not enough gold!");
        }
    }

    //  點擊使用藥水或按 H 觸發
    public void UsePotion()
    {
        if (playerData == null) return;

        if (playerData.potionCount > 0)
        {
            if (currentHP != null)
            {
                playerData.potionCount--; // 直接修改檔案數據
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
        if (playerData == null) return;

        if (goldText != null) goldText.text = playerData.gold.ToString();
        if (potionQuantityText != null) potionQuantityText.text = playerData.potionCount.ToString();
    }

    private void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        if (inventoryPanel != null) inventoryPanel.SetActive(isInventoryOpen);
    }

    public void PickupItem(ItemType itemType, int amount = 1, string achievementID = null)
    {
        if (playerData == null) return;

        switch (itemType)
        {
            case ItemType.Potion:
                playerData.potionCount += amount;
                UpdateUI();
                break;

            case ItemType.Key:
                Debug.Log("Key picked up");
                break;
        }

        if (!string.IsNullOrEmpty(achievementID))
        {
            AchievementManager.Instance.Unlock(achievementID);
        }
    }
}