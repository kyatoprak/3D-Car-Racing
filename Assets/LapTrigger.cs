using System;
using TMPro;
using UnityEngine;

public class LapTrigger : MonoBehaviour
{
    public int LapCounter = 0;
    public TMP_Text LapCounterText;
    public bool endRace = false;
    public int lapCount = 2;

    private void OnTriggerEnter(Collider other)
    {
        var raceInfo = other.GetComponent<RaceInfo>();
        if (raceInfo == null || raceInfo.HasFinished) return;

        if (other.CompareTag("Player"))
        {
            if (LapCounter == lapCount - 1)
            {
                LapCounterText.text = "Lap: " + lapCount + "/" + lapCount;
                endRace = true;
                raceInfo.endTime = Time.time - raceInfo.startTime;
                FindObjectOfType<GameManager>().ShowResults();
                return; 
            }
            else
            {
                LapCounter++;
                LapCounterText.text = "Lap: " + LapCounter + "/" + lapCount;
            }
        }
    }


}

