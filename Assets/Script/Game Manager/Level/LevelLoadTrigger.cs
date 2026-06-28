using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoadTrigger : MonoBehaviour
{
    private int nextLevelID;

    void Start()
    {
        nextLevelID=LevelManager.Instance.GetCurrentLevelID()+1;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            LevelManager.Instance.LoadLevel(nextLevelID);
        }
    }
}
