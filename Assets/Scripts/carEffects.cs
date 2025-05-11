using System;
using UnityEngine;

public class carEffects : MonoBehaviour
{
    private carControlScript _carControlScript;
    public ParticleSystem[] smoke;
    void Start()
    {
        _carControlScript = gameObject.GetComponent<carControlScript>();
    }

    private void FixedUpdate()
    {
        if(_carControlScript.playPauseSmoke)
            startSmoke();
        
        else
            stopSmoke();
    }

    // Update is called once per frame
    public void startSmoke()
    {
        for (int i = 0; i < smoke.Length; i++)
        {
            smoke[i].Play();
        }
    }
    
    public void stopSmoke()
    {
        for (int i = 0; i < smoke.Length; i++)
        {
            smoke[i].Stop();
        }
    }
}
