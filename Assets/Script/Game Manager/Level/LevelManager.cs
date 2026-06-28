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
        SceneManager.LoadScene(currentLevelID);
    }

    public int GetCurrentLevelID()
    {
        return SceneManager.GetActiveScene().buildIndex;
    }

    void Update()
    {
        
    }
}
