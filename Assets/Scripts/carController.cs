using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CarController : MonoBehaviour
{
    public float Torque = 1000f, maxRPM, minRPM;
    public WheelCollider[] wheelColliders;
    public Transform[] wheelTransforms;
    public float[] Gears; 
    [HideInInspector] public inputs input;
    [HideInInspector] Rigidbody rb;
    int GearNum = 0;
    private float wheelsRPM, GearChangeRate, tr = 0, wheelRadius;
    private bool reverse;
    private Quaternion wheelRotation;
    private Vector3 wheelPosition;


    void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        Gears = new float[8]{0f, 4.78f, 3.056f, 2.153f, 1.678f, 1.390f, 1.203f, 1.0f};
    }

    void Update()
    {
        CurrentEngineRPM();
        for (int i = 0; i < wheelColliders.Length; i++)
        {
            WheelsRPM(i);
        }
        updateWheels();
    }

    void WheelsRPM(int i)
    {
        wheelColliders[i].motorTorque = Input.GetAxis("Vertical") * Torque * TransmissionRatio() * maxRPM * Time.deltaTime;
        wheelRadius = wheelColliders[i].radius;
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

        for (int i = 0; i < wheelColliders.Length; i++) {
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

}