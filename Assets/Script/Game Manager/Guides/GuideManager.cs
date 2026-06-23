using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GuideManager : MonoBehaviour
{
    public static GuideManager Instance;

    [Header("Guide Data")]
    [SerializeField] private List<string> guides = new List<string>();

    [Header("UI")]
    [SerializeField] private GameObject guidePanel;
    [SerializeField] private TMP_Text guideText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
        }
        else
        {
            Destroy(this);
        }
    }

    public void ShowGuide(int guideIndex)
    {
        if(guideIndex < 0 || guideIndex >= guides.Count)
        {
            Debug.LogWarning("Guide index out of range.");
            return;
        }
        if(guidePanel!= null && guideText != null)
        {
            guideText.text = guides[guideIndex];
            guidePanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Guide panel or text is not assigned.");
        }
    }

    public void HideGuide()
    {
        if (guidePanel!=null)
        {
            guidePanel.SetActive(false);
        }
    }
}
