using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public TMP_Dropdown DropdownGearBox;
    public TMP_Dropdown DropdownDriveType;
    public TMP_Dropdown DropdownDriveMode;

    
    public carControlScript _carControlScript;
    
    public GameObject needle;
    
    private float startPosition=22f, endPosition=-202f, desiredPosition;
    
    public Text kph,gearNum;
    
    public vehicleList list;
    
    private float currentNeedleRotation = 0f;
    private float needleVelocity = 0f;

    public void updateNeedle()
    {
        desiredPosition = startPosition - endPosition;
        float targetValue = _carControlScript.engineRPM / 10000f;
        float targetRotation = startPosition - targetValue * desiredPosition;

        currentNeedleRotation = Mathf.SmoothDamp(currentNeedleRotation, targetRotation, ref needleVelocity, 0.1f);

        needle.transform.eulerAngles = new Vector3(0, 0, currentNeedleRotation);
    }


    public void changeGear()
    {
        gearNum.text = (!_carControlScript.reverse)? (_carControlScript.gearNum+1).ToString() : "R";

    }

    
    private void Awake()
    {
        var vehicle = Instantiate(list.Vehicles[PlayerPrefs.GetInt("pointer")], new Vector3(-35.6f,0.76f,-0.05f), Quaternion.identity);
        _carControlScript = vehicle.GetComponent<carControlScript>();
    }

    private void Start()
    {
        DropdownDriveType.onValueChanged.AddListener(OnDriveTypeChanged);

        DropdownGearBox.onValueChanged.AddListener(OnGearboxChanged);

        DropdownDriveType.value = (int)_carControlScript.drive;
        DropdownGearBox.value = (int)_carControlScript.gearChangeType;
        
    
        DropdownDriveMode.onValueChanged.AddListener(OnDriveModeChanged);
        OnDriveModeChanged(DropdownDriveMode.value);

    }

    void FixedUpdate()
    {
        kph.text = _carControlScript.KPH.ToString("0");
        updateNeedle();
        
    }
    
    public void OnDriveTypeChanged(int index)
    {
        switch (index)
        {
            case 0:
                _carControlScript.drive = (carControlScript.driveType)index;
                break;
            
            case 1:
                _carControlScript.drive = (carControlScript.driveType)index;
                break;
            
            case 2:
                _carControlScript.drive = (carControlScript.driveType)index;
                break;
        }
    }

    public void OnGearboxChanged(int index)
    {
        switch (index)
        {
            case 0:
                _carControlScript.gearChangeType = (carControlScript.gearBox)index;
                break;
            case 1:
                _carControlScript.gearChangeType = (carControlScript.gearBox)index;
                break;
        }
    }
    
    public void OnDriveModeChanged(int index)
        {
            Debug.Log("Current index: " + index);

            switch (index)
            {
                case 0:
                  
                    for (int i = 0; i < 4; i++)
                    {
                        Debug.Log("Casual Mode");
                        WheelFrictionCurve forwardFriction = _carControlScript.wheels[i].forwardFriction;
                        forwardFriction.extremumSlip = 0.1f;
                        forwardFriction.stiffness = 2.5f;
                        _carControlScript.wheels[i].forwardFriction = forwardFriction;
                        
                        WheelFrictionCurve sidewaysFriction = _carControlScript.wheels[i].sidewaysFriction;
                        sidewaysFriction.extremumSlip = 0.2f;
                        sidewaysFriction.stiffness = 1.5f;
                        _carControlScript.wheels[i].sidewaysFriction = sidewaysFriction;
                        
                    }

                    break;
                
                
                
                case 1:
                
                    for (int i = 0; i < 4; i++)
                    {
                        Debug.Log("Drift Mode");

                        WheelFrictionCurve forwardFriction = _carControlScript.wheels[i].forwardFriction;
                        forwardFriction.extremumSlip = 0.65f;
                        forwardFriction.stiffness = 1.5f;
                        _carControlScript.wheels[i].forwardFriction = forwardFriction;
                        
                        WheelFrictionCurve sidewaysFriction = _carControlScript.wheels[i].sidewaysFriction;
                        sidewaysFriction.extremumSlip = 1f;
                        sidewaysFriction.stiffness = 1.5f;
                        _carControlScript.wheels[i].sidewaysFriction = sidewaysFriction;
                        
                    }   
                    break;
            }
        }
    
    
}
