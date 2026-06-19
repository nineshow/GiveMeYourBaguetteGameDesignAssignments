using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

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

    public void LoadNextLevel(int nextSceneID)
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
        int currentLevelID=SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentLevelID);
    }

    void Update()
    {
        //for testing
        
        if(Input.GetKeyDown(KeyCode.N))
        {
            LoadNextLevel(1);
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }
    }
}
