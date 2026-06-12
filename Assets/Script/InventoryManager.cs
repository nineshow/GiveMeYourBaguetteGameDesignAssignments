using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Player Data")]
    public int gold = 0;
    public int potionCount = 0;

    [Header("Shop Settings")]
    public int potionPrice = 20;
    public int healAmount = 30;

    [Header("UI Elements")]
    public Text goldText;
    public GameObject inventoryPanel;
    public Text potionText; // New text for potions

    private bool isInventoryOpen = false;
    private PlayerHealth playerHealth;

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
            playerHealth = player.GetComponent<PlayerHealth>();
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
            if (playerHealth != null)
            {
                potionCount--;
                playerHealth.Heal(healAmount);
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
        if (potionText != null) potionText.text = "Potions: " + potionCount.ToString();
    }

    private void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        if (inventoryPanel != null) inventoryPanel.SetActive(isInventoryOpen);
    }
}