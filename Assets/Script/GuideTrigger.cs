using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideTrigger : MonoBehaviour
{
    [SerializeField] private int guideIndex;

    //show corresponding guide text
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            PlayerGuides.Instance.ShowGuide(guideIndex);
        }
    }

    //hide text when player walk away
    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            PlayerGuides.Instance.HideGuide();
        }
    }
}
