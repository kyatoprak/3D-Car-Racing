using UnityEngine;

public class RaceInfo : MonoBehaviour
{
    public string playerName;
    public float startTime;
    public float endTime = -1f; 

    public bool HasFinished => endTime > 0f;
}