using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AchievementManager : MonoBehaviour
{

    public static AchievementManager Instance;

    [SerializeField] private GameObject achievementPanel;
    [SerializeField] private TMP_Text achievementTitleText;
    [SerializeField] private TMP_Text achievementDescriptionText;
    [SerializeField] private Image achievementIconImage;
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
        if(achievementPanel!=null)
        {
            achievementPanel.SetActive(false);
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

        Debug.Log("Achievement Unlocked: " + achievement.title + ": " + achievement.description);
        achievementPanel.SetActive(true);
        StartCoroutine(ShowAchievement(achievement));
    }

    public bool IsUnlocked(string achievementID)
    {
        Achievement achievement =
            achievements.Find(a => a.id == achievementID);

        return achievement != null && achievement.unlocked;
    }
    private IEnumerator ShowAchievement(Achievement achievement)
{
    achievementPanel.SetActive(true);
    
    achievementIconImage.sprite = achievement.icon;
    achievementTitleText.text = achievement.title;
    achievementDescriptionText.text = achievement.description;

    yield return new WaitForSeconds(3f);

    achievementPanel.SetActive(false);
}
}
