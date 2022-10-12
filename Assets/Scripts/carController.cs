using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class CarController : MonoBehaviour
{
    [Header("Engine settings")]
    [Tooltip("More torque - more speed")]
    public float Torque = 1000f; 
    public float[] Gears;
    public float maxRPM, minRPM;
    public AnimationCurve AccelerationCurve;
    [Header("Wheel settings")]
    public WheelCollider[] wheelColliders;
    public Transform[] wheelTransforms;
    [SerializeField] float handBrakeForce;
    [SerializeField] float brakeForce;
    [SerializeField] float wheelRollResistance;
    private Quaternion wheelRotation;
    private Vector3 wheelPosition;
    [HideInInspector] public inputs input;
    [HideInInspector] Rigidbody rb;
    int GearNum = 0;
    private float wheelsRPM, GearChangeRate, tr = 0, vertical, horizontal, finalTurnAngle, radius, time;
    private bool reverse;
    


    void Awake()
    {
        time = Time.time / 10;
        rb = gameObject.GetComponent<Rigidbody>();
        Gears = new float[8]{0f, 4.78f, 3.056f, 2.153f, 1.678f, 1.390f, 1.203f, 1.0f};
    }

    void Update()
    { 
        CurrentBrakeForce();
        CurrentEngineRPM();
        for (int i = 0; i < wheelColliders.Length; i++)
        {
            WheelsRPM(i);
        }
        updateWheels();
        HandBrake();
        rotateWheels();
        Brakes();
    }

    void WheelsRPM(int i)
    {
        if (Input.GetAxis("Vertical") != 0)
            wheelColliders[i].motorTorque = Input.GetAxis("Vertical") * Torque * TransmissionRatio() * maxRPM * Time.deltaTime / wheelRollResistance;
        else
            wheelColliders[i].motorTorque = 0;
    }

    //Counting wheel RPM based on number of wheels and .rpm
    void wheelRPM()
    {
        float sum = 0;
        int R = 0;
        for (int i = 0; i < 4; i++)
        {
            sum += wheelColliders[i].rpm;
            R++;
        }
        wheelsRPM = (R != 0) ? sum / R : 0;
 
        if(wheelsRPM < 0 && !reverse ){
            reverse = true;
            //if (gameObject.tag != "AI") manager.changeGear();
        }
        else if(wheelsRPM > 0 && reverse){
            reverse = false;
            //if (gameObject.tag != "AI") manager.changeGear();
        }
    }

    //Change gear method
    int CurrentGear()
    {
        if (Input.GetKeyDown(KeyCode.C) && GearNum < Gears.Length && Time.time >= GearChangeRate)
        {
            GearNum++;
            GearChangeRate = Time.time + 1f/3f ;
        }
        else if (Input.GetKeyDown(KeyCode.X) && GearNum > 0 && Time.time >= GearChangeRate)
        {
            GearNum--;
            GearChangeRate = Time.time + 1f/3f ;
        }
        return GearNum;
    }

    //Choose transmission ratio based on current gear
    float TransmissionRatio()
    {   
        for(int i = 0; i < Gears.Length; i++)
        {
            if (i == CurrentGear())
            {
                tr = Gears[i];
            }
        }
        return tr;
    }

    void updateWheels(){

        for (int i = 0; i < wheelColliders.Length; i++) 
        {
            wheelColliders[i].GetWorldPose(out wheelPosition, out wheelRotation);
            wheelTransforms[i].transform.rotation = wheelRotation;
            wheelTransforms[i].transform.position = wheelPosition;
        }
    }

    void CurrentEngineRPM()
    {
        switch (CurrentGear())
        {
            case 1:
                maxRPM = 4000;
                break;
            case 2:
                maxRPM = 5000;
                break;
            case 3:
                maxRPM = 6000;
                break;
            case 4:
                maxRPM = 7000;
                break;
            case 5: 
                maxRPM = 8000;
                break;
            case 6:
                maxRPM = 9000;
                break;
            case 7:
                maxRPM = 10000;
                break;
        }
    }

    public void HandBrake()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            for (int i = 0; i < wheelColliders.Length; i++)
        {
            wheelColliders[i].brakeTorque = handBrakeForce;
        }
        }
        else
        {   
            for (int i = 0; i < wheelColliders.Length; i++)
            {
                wheelColliders[i].brakeTorque = 0;
            }
        }
    }

    void rotateWheels(){

        vertical = Input.GetAxis("Vertical");
        horizontal = Mathf.Lerp(horizontal , Input.GetAxis("Horizontal") , (Input.GetAxis("Horizontal") != 0) ? 2 * Time.deltaTime : 3 * 2 * Time.deltaTime);

        finalTurnAngle = (radius > 5 ) ? radius : 5  ;

        if (horizontal > 0 ) 
        {
				//rear tracks size is set to 1.5f       wheel base has been set to 2.55f
            wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (finalTurnAngle - (1.5f / 2))) * horizontal;
            wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (finalTurnAngle + (1.5f / 2))) * horizontal;
        } 
        else if (horizontal < 0 ) 
        {                                                          
            wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (finalTurnAngle + (1.5f / 2))) * horizontal;
            wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (finalTurnAngle - (1.5f / 2))) * horizontal;
			//transform.Rotate(Vector3.up * steerHelping);

        } 
        else 
        {
            wheelColliders[0].steerAngle =0;
            wheelColliders[1].steerAngle =0;
        }

    }

    public void Brakes()
    {
        if(Input.GetAxis("Vertical") == 0)
        {
            for (int i = 0; i < wheelColliders.Length; i++)
            {
                wheelColliders[i].brakeTorque = brakeForce;
            }
        }
        else
        {
            for (int i = 0; i < wheelColliders.Length; i++)
            {
                wheelColliders[i].brakeTorque = 0;
            }
        }
    }

    void CurrentBrakeForce()
    {
        switch (CurrentGear())
        {
            case 1:
                brakeForce = 10000;
                break;
            case 2:
                brakeForce = 8000;
                break;
            case 3:
                brakeForce = 6000;
                break;
            case 4:
                brakeForce = 2000;
                break;
            case 5: 
                brakeForce = 1000;
                break;
            case 6:
                brakeForce = 500;
                break;
            case 7:
                brakeForce = 100;
                break;
        }
    }

}