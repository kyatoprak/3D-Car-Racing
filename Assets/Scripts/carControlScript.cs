using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class carControlScript : MonoBehaviour
{
    
    public enum driveType { frontWheelDrive, rearWheelDrive, allWheelDrive }
    public enum gearBox { automatic, manuel }

    [Header("Gear and Drive Settings")]
    [SerializeField] public gearBox gearChangeType;
    [SerializeField] public driveType drive;

    [Header("Vehicle Status Flags")]
    public bool playPauseSmoke = false;
    public bool hasFinished;
    [HideInInspector] public bool test;
    [HideInInspector] public bool reverse = false;
    

    [Header("Wheels")]
    public WheelCollider[] wheels = new WheelCollider[4];
    public float[] slip = new float[4];
    private WheelFrictionCurve forwardFriction, sidewaysFriction;
    public GameObject wheelMeshes;
    public GameObject wheelCollider;
    public GameObject[] wheelMesh = new GameObject[4];

    
    [Header("Trail and Audio Effects")]
    public TrailRenderer[] tireMarks;
    public AudioSource skidClip;

    [Header("Rear Lights")]
    public Light leftRearLight;
    public Light rightRearLight;

    [Header("Physics and Movement Parameters")]
    public float torque = 200;
    public float radius = 6;
    public float brakePower = 200;
    public float addDownForceValue = 50;
    public float driftFactor;
    public float handBrakeFrictionMultiplier = 2f;
    public float smoothTime = 0.01f;
    public GameObject centerOfMass;


    [Header("Gear and RPM Settings")]
    public float[] gears;
    public int gearNum = 0;
    public float maxRPM;
    public float minRPM;
    public float totalPower;
    public float wheelsRPM;
    public float KPH;
    public float engineRPM;
    public AnimationCurve enginePower;

    [Header("Input")]
    private float verticalInput;
    private float horizontalInput;
    private bool handbrakeInput;

    [Header("References")]
    private Rigidbody rb;
    private IInputProvider inputManager;
    public GameManager gameManager;

    [Header("Time")]
    public float startTime;
    public float endTime;
    
    [Header("Internal Flags")]
    private bool tireMarksFlag;
    private bool tireMarksBool = false;

    private LapTrigger _lapTrigger;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "AwakeScene") return;

        getObjects();
        
    }

    private void FixedUpdate()
    {
            
        if (!gameManager)
            {
                gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
            }

        if (SceneManager.GetActiveScene().name == "raceScene" && !gameManager.raceStarts)
        {
            endTime = Time.time;
            return;
        }
        
        
            if (SceneManager.GetActiveScene().name == "AwakeScene") return;

            verticalInput = inputManager.VerticalInput;
            horizontalInput = inputManager.HorizontalInput;
            handbrakeInput = inputManager.HandbrakeInput;

            wheelAnimation();
            vehicleMovement();
            steerVehicle();
            downForce();
            getFriction();
            calculateEnginePower();
            gearShifting();
            isGrounded();
            adjustTraction();
            checkDrift();
            VehicleLights();
    }
    

    private void VehicleLights()
    {
        float lightIntensity = (verticalInput < 0) ? 0.4f : 0.1f;
        leftRearLight.intensity = lightIntensity;
        rightRearLight.intensity = lightIntensity;
    }

    private void gearShifting()
    {
        if (gearChangeType == gearBox.automatic)
        {
            if (engineRPM > maxRPM && gearNum < gears.Length - 1 && !reverse)
            {
                gearNum++;
                gameManager.changeGear();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.E) && gearNum < gears.Length - 1)
            {
                gearNum++;
                gameManager.changeGear();
            }
        }

        if (engineRPM < minRPM && gearNum > 0)
        {
            gearNum--;
            gameManager.changeGear();
        }
    }

    private void getFriction()
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].GetGroundHit(out WheelHit wheelHit);
            slip[i] = wheelHit.sidewaysSlip;
        }
    }

    private void vehicleMovement()
    {
        float torqueValue = verticalInput * torque;

        switch (drive)
        {
            case driveType.allWheelDrive:
                for (int i = 0; i < wheels.Length; i++)
                    wheels[i].motorTorque = torqueValue / 4;
                break;

            case driveType.frontWheelDrive:
                for (int i = 0; i < 2; i++)
                    wheels[i].motorTorque = torqueValue / 2;
                break;

            case driveType.rearWheelDrive:
                for (int i = 2; i < 4; i++)
                    wheels[i].motorTorque = torqueValue / 2;
                break;
        }

        KPH = rb.linearVelocity.magnitude * 3.6f;

        float brakeTorque = handbrakeInput ? brakePower : 0;
        wheels[2].brakeTorque = wheels[3].brakeTorque = brakeTorque;
    }

    private void steerVehicle()
    {
        float steerAngle0 = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2)));
        float steerAngle1 = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius - (1.5f / 2)));

        if (horizontalInput > 0)
        {
            wheels[0].steerAngle = steerAngle0 * horizontalInput;
            wheels[1].steerAngle = steerAngle1 * horizontalInput;
        }
        else if (horizontalInput < 0)
        {
            wheels[0].steerAngle = steerAngle1 * horizontalInput;
            wheels[1].steerAngle = steerAngle0 * horizontalInput;
        }
        else
        {
            wheels[0].steerAngle = wheels[1].steerAngle = 0;
        }
    }

    private void wheelAnimation()
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].GetWorldPose(out Vector3 wheelPosition, out Quaternion wheelRotation);
            wheelMesh[i].transform.position = wheelPosition;
            wheelMesh[i].transform.rotation = wheelRotation;
        }
    }

    private void getObjects()
    {
        if (SceneManager.GetActiveScene().name == "raceScene")
        _lapTrigger = GameObject.FindGameObjectWithTag("LapTrigger").GetComponent<LapTrigger>();
        
        
        
        if (!TryGetComponent(out inputManager))
        {
            Debug.LogError("No IInputProvider (PlayerInputManager or AIInputManager) found on: " + gameObject.name);
        }

        rb = GetComponent<Rigidbody>();
        centerOfMass = GameObject.Find("Mass");
        rb.centerOfMass = centerOfMass.transform.localPosition;

        wheelCollider = GameObject.Find("WheelColliders");
        wheelMeshes = GameObject.Find("WheelMesh");

        for (int i = 0; i < 4; i++)
        {
            wheelMesh[i] = wheelMeshes.transform.Find(i.ToString()).gameObject;
            wheels[i] = wheelCollider.transform.Find(i.ToString()).gameObject.GetComponent<WheelCollider>();
        }
    }

    private void downForce()
    {
        rb.AddForce(-transform.up * addDownForceValue * rb.linearVelocity.magnitude);
    }

    private void calculateEnginePower()
    {
        wheelRPM();

        totalPower = enginePower.Evaluate(engineRPM) * gears[gearNum] * verticalInput;

        float velocity = 0.0f;
        engineRPM = Mathf.SmoothDamp(engineRPM, 1000 + Mathf.Abs(wheelsRPM) * gears[gearNum], ref velocity, smoothTime);
    }

    private void wheelRPM()
    {
        float sum = 0;
        for (int i = 0; i < 4; i++)
            sum += wheels[i].rpm;

        wheelsRPM = sum / 4;

        if (wheelsRPM < 0 && !reverse)
        {
            reverse = true;
            gameManager.changeGear();
        }
        else if (wheelsRPM > 0 && reverse)
        {
            reverse = false;
            gameManager.changeGear();
        }
    }

    private bool isGrounded()
    {
        return wheels[0].isGrounded && wheels[1].isGrounded && wheels[2].isGrounded && wheels[3].isGrounded;
    }

    private void adjustTraction()
    {
        float driftSmoothFactor = 0.7f * Time.deltaTime;

        if (handbrakeInput)
        {
            sidewaysFriction = wheels[0].sidewaysFriction;
            forwardFriction = wheels[0].forwardFriction;

            float velocity = 0;
            float targetValue = Mathf.SmoothDamp(forwardFriction.asymptoteValue, driftFactor * handBrakeFrictionMultiplier, ref velocity, driftSmoothFactor);

            sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue = targetValue;
            forwardFriction.extremumValue = forwardFriction.asymptoteValue = targetValue;

            for (int i = 0; i < 4; i++)
            {
                wheels[i].sidewaysFriction = sidewaysFriction;
                wheels[i].forwardFriction = forwardFriction;
            }

            sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue = 1.1f;
            forwardFriction.extremumValue = forwardFriction.asymptoteValue = 1.1f;

            for (int i = 0; i < 2; i++)
            {
                wheels[i].sidewaysFriction = sidewaysFriction;
                wheels[i].forwardFriction = forwardFriction;
            }

            rb.AddForce(transform.forward * (KPH / 400f) * 10000f);
        }
        else
        {
            forwardFriction = wheels[0].forwardFriction;
            sidewaysFriction = wheels[0].sidewaysFriction;

            float baseFriction = ((KPH * handBrakeFrictionMultiplier) / 300f) + 1;

            forwardFriction.extremumValue = forwardFriction.asymptoteValue = baseFriction;
            sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue = baseFriction;

            for (int i = 0; i < 4; i++)
            {
                wheels[i].forwardFriction = forwardFriction;
                wheels[i].sidewaysFriction = sidewaysFriction;
            }
        }

        for (int i = 2; i < 4; i++)
        {
            wheels[i].GetGroundHit(out WheelHit wheelHit);

            if (gameManager.DropdownDriveMode.value == 0)
            {
                playPauseSmoke = Mathf.Abs(wheelHit.sidewaysSlip) >= 0.8f || Mathf.Abs(wheelHit.forwardSlip) >= 0.8f;
            }
            else if (gameManager.DropdownDriveMode.value == 1)
            {
                tireMarksBool = playPauseSmoke = Mathf.Abs(wheelHit.sidewaysSlip) >= 0.3f || Mathf.Abs(wheelHit.forwardSlip) >= 0.3f;
            }

            if (wheelHit.sidewaysSlip < 0)
                driftFactor = (1 + -horizontalInput) * Mathf.Abs(wheelHit.sidewaysSlip);

            if (wheelHit.sidewaysSlip > 0)
                driftFactor = (1 + horizontalInput) * Mathf.Abs(wheelHit.sidewaysSlip);
        }
    }

    private void checkDrift()
    {
        bool shouldEmit = (tireMarksBool && !handbrakeInput && KPH > 25) || (handbrakeInput && KPH >= 6);

        if (shouldEmit)
            startEmitter();
        else
            stopEmitter();
    }

    private void startEmitter()
    {
        if (tireMarksFlag) return;

        foreach (TrailRenderer t in tireMarks)
            t.emitting = true;

        skidClip.Play();
        tireMarksFlag = true;
    }

    private void stopEmitter()
    {
        if (!tireMarksFlag) return;

        foreach (TrailRenderer t in tireMarks)
            t.emitting = false;

        skidClip.Stop();
        tireMarksFlag = false;
    }
}
