using System;
using UnityEngine;

public class cameraScript : MonoBehaviour
{
    public GameObject Car;
    public GameObject child;
    private float speed;
    private float tiltAmount = 10f; 
    private float tiltSpeed = 5f;   
    private float currentTilt = 0f; 
    private float previousYRotation;
    private carControlScript _carControlScript;

    private void Start()
    {
        Car = GameObject.FindGameObjectWithTag("Player");
        child = Car.transform.Find("CameraConstraint").gameObject;
        previousYRotation = Car.transform.eulerAngles.y;
        _carControlScript = Car.GetComponent<carControlScript>();
    }

    private void FixedUpdate()
    {
        FollowCar();
        speed = (_carControlScript.KPH >= 60) ? 15 : _carControlScript.KPH / 4;
    }

    private void FollowCar()
    {
        transform.position = Vector3.Lerp(transform.position, child.transform.position, Time.deltaTime * speed);
        transform.LookAt(Car.transform.position);

        float currentYRotation = Car.transform.eulerAngles.y;
        float deltaRotation = Mathf.DeltaAngle(previousYRotation, currentYRotation);

        float targetTilt = Mathf.Clamp(-deltaRotation, -tiltAmount, tiltAmount);

        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);

        transform.rotation = Quaternion.Euler(currentTilt, transform.eulerAngles.y, 0f);

        previousYRotation = currentYRotation;
    }
}