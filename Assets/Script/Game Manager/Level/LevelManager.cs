using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    private bool isGamePaused = false;

    public GameObject gameOverPanel;

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

    public void LoadLevel(int nextSceneID)
    {
        //check if scene exist first
        if(nextSceneID<0||nextSceneID>= UnityEngine.
        SceneManagement.SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError("Invalid scene ID: "+nextSceneID);
            return;
        }
        SceneManager.LoadScene(nextSceneID);
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
        SceneManager.LoadScene(0);
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
