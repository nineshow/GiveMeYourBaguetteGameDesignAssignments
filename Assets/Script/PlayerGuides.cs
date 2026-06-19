using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerGuides : MonoBehaviour
{
    public static PlayerGuides Instance;
    [Header("Guide Texts")]
     [SerializeField] private List<string> Guides=new List<string>();

    [Header("UI")]
    [SerializeField] private GameObject guidePanel;
    [SerializeField] private TMP_Text guideText;

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

    public void ShowGuide(int index)
    {
        if(index<0||index>=Guides.Count)
        {
            Debug.Log("Invalid guide index: "+index);
            return;
        }

        guidePanel.SetActive(true);
        guideText.text=Guides[index];

    }


    public void HideGuide()
    {
        guidePanel.SetActive(false);
    }
}
