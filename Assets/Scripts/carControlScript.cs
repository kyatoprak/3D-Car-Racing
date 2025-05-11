using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class carControlScript : MonoBehaviour
{
    
    public enum  driveType
    {
        frontWheelDrive,
        rearWheelDrive,
        allWheelDrive,
    }
    public enum  gearBox
    {
        automatic,
        manuel,
    }
    
    [SerializeField] public gearBox gearChangeType;
    [SerializeField] public driveType drive; 
    
   public bool playPauseSmoke = false,hasFinished;
   [HideInInspector]public bool test;
   [HideInInspector] public bool reverse=false;

   public GameObject wheelMeshes, wheelCollider,centerOfMass;
   public GameObject[] wheelMesh = new GameObject[4];

   public TrailRenderer[] tireMarks;

   public AudioSource skidClip;

   private WheelFrictionCurve  forwardFriction,sidewaysFriction;

    public WheelCollider[] wheels = new WheelCollider[4];
    
    private inputManager _inputManager;
    
    private Rigidbody rb;

    public Light leftRearLight;
    public Light rightRearLight;

    
    public float maxRPM, minRPM, totalPower, wheelsRPM,KPH, engineRPM, smoothTime = 0.01f, addDownForceValue = 50,torque = 200,radius = 6,brakePower = 200,driftFactor,handBrakeFrictionMultiplier = 2f;

    public float[] slip = new float[4];
    public float[] gears;
    public int gearNum = 0;
  
    //Engine RPM
    public AnimationCurve enginePower;
    public GameManager gameManager;
    private bool tireMarksFlag;
    
    void Start()
    {
        if(SceneManager.GetActiveScene().name == "AwakeScene")return;
        getObjects();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

    }

    // Update is called once per fr
    private void FixedUpdate()
    {
       
        if (SceneManager.GetActiveScene().name == "AwakeScene") return;
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
        if (_inputManager.vertical < 0)
        {
            rightRearLight.intensity = 0.4f;
            leftRearLight.intensity = 0.4f;
        }
        else
        {
               rightRearLight.intensity = 0.1f;
                leftRearLight.intensity = 0.1f;
        }
   
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
            if (Input.GetKeyDown(KeyCode.E)&& gearNum < gears.Length - 1)
            {
                gearNum++;

                gameManager.changeGear();
            }
        }
       
        if (engineRPM < minRPM && gearNum >0)
        {
            gearNum--;
            gameManager.changeGear();
        }
    }
    //let the car drift forward-sideslip
    private void getFriction()
    {

        for (int i = 0; i < wheels.Length; i++)
        {
            WheelHit wheelHit;
            wheels[i].GetGroundHit(out wheelHit);
            slip[i] = wheelHit.sidewaysSlip;
        }
        
    }

    private void vehicleMovement()
    {
        if (drive == driveType.allWheelDrive)
        {
            for (int i = 0; i < wheels.Length; i++)
            {
                wheels[i].motorTorque = _inputManager.vertical*(torque/4);
            }

        }
        
        else if (drive == driveType.frontWheelDrive)
        {
            for (int i = 0; i < wheels.Length-2; i++)
            {
                wheels[i].motorTorque = _inputManager.vertical*(torque/2);
            }
        }
        
        else if (drive == driveType.rearWheelDrive)
        {
            for (int i = 2; i < wheels.Length; i++)
            {
                wheels[i].motorTorque = _inputManager.vertical*(torque/2);
            }
        }

        KPH = rb.linearVelocity.magnitude * 3.6f;

        if (_inputManager.handbrake)
        {
            wheels[2].brakeTorque = wheels[3].brakeTorque = brakePower;
        }
        else
        {
            wheels[2].brakeTorque = wheels[3].brakeTorque = 0;

        }
    }

    private void steerVehicle()
    {
      
            if (_inputManager.horizontal > 0) {
                wheels[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * _inputManager.horizontal;
                wheels[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius - (1.5f / 2))) * _inputManager.horizontal;
            } else if (_inputManager.horizontal < 0) {
                wheels[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius - (1.5f / 2))) * _inputManager.horizontal;
                wheels[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * _inputManager.horizontal;
            } else {
                wheels[0].steerAngle = 0;
                wheels[1].steerAngle = 0;
            }
        
    }
    
    
    private void wheelAnimation()
    {
        Vector3 wheelPosition = Vector3.zero;
        Quaternion wheelRotation = Quaternion.identity;

        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].GetWorldPose(out wheelPosition, out wheelRotation);
            wheelMesh[i].transform.position = wheelPosition;
            wheelMesh[i].transform.rotation = wheelRotation;
            
        }
    }

    private void getObjects()
    {
        _inputManager = GetComponent<inputManager>();
        rb = GetComponent<Rigidbody>();
        centerOfMass = GameObject.Find("Mass");
        rb.centerOfMass = centerOfMass.transform.localPosition;
        wheelCollider = GameObject.Find("WheelColliders");
        wheelMeshes = GameObject.Find("WheelMesh");
        
        //assing the wheels 
        wheelMesh[0] = wheelMeshes.transform.Find("0").gameObject;
        wheelMesh[1] = wheelMeshes.transform.Find("1").gameObject;        
        wheelMesh[2] = wheelMeshes.transform.Find("2").gameObject;
        wheelMesh[3] = wheelMeshes.transform.Find("3").gameObject;
        
        wheels[0] = wheelCollider.transform.Find("0").gameObject.GetComponent<WheelCollider>();
        wheels[1] = wheelCollider.transform.Find("1").gameObject.GetComponent<WheelCollider>();     
        wheels[2] = wheelCollider.transform.Find("2").gameObject.GetComponent<WheelCollider>();
        wheels[3] = wheelCollider.transform.Find("3").gameObject.GetComponent<WheelCollider>();


    }

    private void downForce()
    {
        rb.AddForce(-transform.up * addDownForceValue * rb.linearVelocity.magnitude);
    }

    private void calculateEnginePower()
    {
        wheelRPM();
         totalPower = enginePower.Evaluate(engineRPM) * (gears[gearNum]) * _inputManager.vertical;
             float velocity = 0.0f;
                 engineRPM = Mathf.SmoothDamp(engineRPM, 1000 + (Mathf.Abs(wheelsRPM) * (gears[gearNum])), ref velocity,
                     smoothTime);
    }

    private void wheelRPM()
    {
        float sum = 0;
        int R = 0;
        
        for (int i = 0; i < 4; i++)
        {
            sum += wheels[i].rpm;
            R++;
            
        }
        
        wheelsRPM = (R != 0) ? sum / R : 0;
        
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
        if (wheels[0].isGrounded && wheels[1].isGrounded && wheels[2].isGrounded && wheels[3].isGrounded)
            return true;
        else
            return false;
    }
    
    
     private void adjustTraction(){
            //tine it takes to go from normal drive to drift 
        float driftSmothFactor = .7f * Time.deltaTime;

		if(_inputManager.handbrake){
            sidewaysFriction = wheels[0].sidewaysFriction;
            forwardFriction = wheels[0].forwardFriction;

            float velocity = 0;
            sidewaysFriction.extremumValue =sidewaysFriction.asymptoteValue = forwardFriction.extremumValue = forwardFriction.asymptoteValue =
                Mathf.SmoothDamp(forwardFriction.asymptoteValue,driftFactor * handBrakeFrictionMultiplier,ref velocity ,driftSmothFactor );

            for (int i = 0; i < 4; i++) {
                wheels [i].sidewaysFriction = sidewaysFriction;
                wheels [i].forwardFriction = forwardFriction;
            }

            sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue = forwardFriction.extremumValue = forwardFriction.asymptoteValue =  1.1f;
                //extra grip for the front wheels
            for (int i = 0; i < 2; i++) {
                wheels [i].sidewaysFriction = sidewaysFriction;
                wheels [i].forwardFriction = forwardFriction;
            }
            GetComponent<Rigidbody>().AddForce(transform.forward * (KPH / 400) * 10000 );
		}
            //executed when handbrake is being held
        else{

			forwardFriction = wheels[0].forwardFriction;
			sidewaysFriction = wheels[0].sidewaysFriction;

			forwardFriction.extremumValue = forwardFriction.asymptoteValue = sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue = 
                ((KPH * handBrakeFrictionMultiplier) / 300) + 1;

			for (int i = 0; i < 4; i++) {
				wheels [i].forwardFriction = forwardFriction;
				wheels [i].sidewaysFriction = sidewaysFriction;

			}
        }

            //checks the amount of slip to control the drift
		for(int i = 2;i<4 ;i++){

            WheelHit wheelHit;

            wheels[i].GetGroundHit(out wheelHit);
                //smoke
            if(wheelHit.sidewaysSlip >= .8f || wheelHit.sidewaysSlip <= -.8f ||wheelHit.forwardSlip >= .8f || wheelHit.forwardSlip <= -.8f)
                playPauseSmoke = true;
            else
                playPauseSmoke = false;
                        

			if(wheelHit.sidewaysSlip < 0 )	driftFactor = (1 + -_inputManager.horizontal) * Mathf.Abs(wheelHit.sidewaysSlip) ;

			if(wheelHit.sidewaysSlip > 0 )	driftFactor = (1 + _inputManager.horizontal )* Mathf.Abs(wheelHit.sidewaysSlip );
		}	
		
	}


     private void checkDrift()
     {
         if (_inputManager.handbrake && KPH>=6)
             startEmitter();
         else
         {
             stopEmitter();
         }
     }


     private void startEmitter()
     {
         if (tireMarksFlag) return;
         
         foreach ( TrailRenderer T in tireMarks)
         {
             T.transform.rotation = Quaternion.Euler(0, 0, 0);

             T.emitting = true;
         }
         
         skidClip.Play();
         tireMarksFlag = true;
     }

     private void stopEmitter()
     {
         if (!tireMarksFlag) return;
         
         foreach ( TrailRenderer T in tireMarks)
         {
             T.emitting = false;
             
         }
         
         skidClip.Stop();
         tireMarksFlag = false;
     }
}
