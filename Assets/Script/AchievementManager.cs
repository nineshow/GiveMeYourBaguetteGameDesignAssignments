using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{

    public static AchievementManager Instance;
    [SerializeField] private List<Achievement> achievements =
        new List<Achievement>();

    private HashSet<string> unlockedAchievements =
        new HashSet<string>();


    void Awake()
    {
        if(Instance==null)
        {
            Instance=this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void Unlock(string achievementID)
    {
        Achievement achievement =
            achievements.Find(a => a.id == achievementID);

        if (achievement == null)
        {
            Debug.LogWarning("Achievement not found: " + achievementID);
            return;
        }

        if (achievement.unlocked)
            return;

        achievement.unlocked = true;

        Debug.Log("Achievement Unlocked: " + achievement.title + achievement.description);
       
    }

    public bool IsUnlocked(string achievementID)
    {
        Achievement achievement =
            achievements.Find(a => a.id == achievementID);

        return achievement != null && achievement.unlocked;
    }
}
