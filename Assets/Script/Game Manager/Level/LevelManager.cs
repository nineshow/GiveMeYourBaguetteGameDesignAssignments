using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    private bool isGamePaused = false;

    public GameObject gameOverPanel;
    private int pendingLevelID;

    [SerializeField] private GameObject pausePanel;

    private void Awake()
    {
        if(Instance==null)
        {
            Instance=this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if(gameOverPanel!=null)
        {
            gameOverPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Game Over panel is not assigned.");
        }

        if(pausePanel!=null)
        {
            pausePanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Pause panel is not assigned.");
        }
    }

    void Update()
    {
       // Check for the Escape key press to toggle pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
        
    }

    public void LoadLevel(int id)
    {
        // 1. 先把要去的關卡 ID 存起來
        pendingLevelID = id;

        // 2. 【核心動作】：立刻觸發你的黑幕開始變黑 (假設你的黑幕腳本叫 TransitionManager)
        if (TransitionManager.instance != null)
        {
            // 讓黑幕立刻開始由透明變黑 (我們只讓它跑前半段的變黑，不讓它在裡面切場景)
            TransitionManager.instance.StartFadeOut(); 
        }

        // 3. 【強行煞車】：等待 0.5 秒鐘之後，才去執行真正的場景切換
        Invoke("ExecuteSceneLoad", 0.5f); 
    }

    // 4. 1秒後被 Invoke 呼叫的真正切換方法
    private void ExecuteSceneLoad()
    {
        SceneManager.LoadScene(pendingLevelID);
    }

    public void LoadNextLevel()
    {
        int currentLevelID = GetCurrentLevelID();
        int nextLevelID = currentLevelID + 1;
       
        int lastSceneID = 15;

         if(currentLevelID==lastSceneID)
        {
            Debug.Log("Last scene completed. Loading Main Menu.");
            LoadMainMenu();
            return;
        }
        
        Debug.Log("Loading next level: " + nextLevelID);
        LoadLevel(nextLevelID);
        
    }

    public void RestartLevel()
    {
        int currentLevelID=GetCurrentLevelID();
        ResumeGame(); // 确保在重新加载关卡时恢复游戏时间
        SceneManager.LoadScene(currentLevelID);
        
    }

    public int GetCurrentLevelID()
    {
        return SceneManager.GetActiveScene().buildIndex;
    }

    public void LoadMainMenu()
    {
        LoadLevel(0); 
        if(gameOverPanel!=null)
        {
            gameOverPanel.SetActive(false);
        }
        ResumeGame(); // 确保在返回主菜单时恢复游戏时间
    }

    public void PauseGame()
    {
        isGamePaused = true;
        
        Time.timeScale = 0f; // 暂停游戏
    }

    public void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1f; // 恢复游戏时间
    }

    public void TogglePause()
    {
        if (isGamePaused)
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
            ResumeGame();
        }
        else
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
            }
            PauseGame();
        }
    }

}
