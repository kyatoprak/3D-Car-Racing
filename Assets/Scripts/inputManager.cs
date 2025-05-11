using System;
using UnityEngine;

public class inputManager : MonoBehaviour
{
    
    public float vertical;
    public float horizontal;
    public bool handbrake;


    private void FixedUpdate()
    {
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");
        handbrake = (Input.GetAxis("Jump")!=0) ? true:false;
    }

 
}
