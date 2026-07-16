using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideTrigger : MonoBehaviour
{
    [SerializeField] private int guideIndex;


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GuideManager.Instance.ShowGuildPanel(guideIndex);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GuideManager.Instance.HideGuide();
        }
    }
}
