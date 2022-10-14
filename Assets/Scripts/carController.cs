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
    float steeringAxis;
    float steeringSpeed = 0.5f;
    float maxSteeringAngle = 27f;
    private Quaternion wheelRotation;
    private Vector3 wheelPosition;
    [HideInInspector] public inputs input;
    [HideInInspector] Rigidbody rb;
    private bool isTractionLocked, isDrifting;
    int GearNum = 0;
    private float wheelsRPM, GearChangeRate, tr = 0, vertical, horizontal, finalTurnAngle, radius, time, CarSpeed, driftingAxis, localVelocityX, steeringAngle;
    private bool reverse;
    float handbrakeDriftMultiplier = 2f;
    WheelFrictionCurve FLwheelFriction;
    float FLWextremumSlip;
    WheelFrictionCurve FRwheelFriction;
    float FRWextremumSlip;
    WheelFrictionCurve RLwheelFriction;
    float RLWextremumSlip;
    WheelFrictionCurve RRwheelFriction;
    float RRWextremumSlip;
    


    void Awake()
    {
        time = Time.time / 10;
        rb = gameObject.GetComponent<Rigidbody>();
        Gears = new float[8]{0f, 4.78f, 3.056f, 2.153f, 1.678f, 1.390f, 1.203f, 1.0f};
    }

    void Start()
    {
        FLwheelFriction = new WheelFrictionCurve ();
        FLwheelFriction.extremumSlip = wheelColliders[0].sidewaysFriction.extremumSlip;
        FLWextremumSlip = wheelColliders[0].sidewaysFriction.extremumSlip;
        FLwheelFriction.extremumValue = wheelColliders[0].sidewaysFriction.extremumValue;
        FLwheelFriction.asymptoteSlip = wheelColliders[0].sidewaysFriction.asymptoteSlip;
        FLwheelFriction.asymptoteValue = wheelColliders[0].sidewaysFriction.asymptoteValue;
        FLwheelFriction.stiffness = wheelColliders[0].sidewaysFriction.stiffness;
      FRwheelFriction = new WheelFrictionCurve ();
        FRwheelFriction.extremumSlip = wheelColliders[1].sidewaysFriction.extremumSlip;
        FRWextremumSlip = wheelColliders[1].sidewaysFriction.extremumSlip;
        FRwheelFriction.extremumValue = wheelColliders[1].sidewaysFriction.extremumValue;
        FRwheelFriction.asymptoteSlip = wheelColliders[1].sidewaysFriction.asymptoteSlip;
        FRwheelFriction.asymptoteValue = wheelColliders[1].sidewaysFriction.asymptoteValue;
        FRwheelFriction.stiffness = wheelColliders[1].sidewaysFriction.stiffness;
      RLwheelFriction = new WheelFrictionCurve ();
        RLwheelFriction.extremumSlip = wheelColliders[2].sidewaysFriction.extremumSlip;
        RLWextremumSlip = wheelColliders[2].sidewaysFriction.extremumSlip;
        RLwheelFriction.extremumValue = wheelColliders[2].sidewaysFriction.extremumValue;
        RLwheelFriction.asymptoteSlip = wheelColliders[2].sidewaysFriction.asymptoteSlip;
        RLwheelFriction.asymptoteValue = wheelColliders[2].sidewaysFriction.asymptoteValue;
        RLwheelFriction.stiffness = wheelColliders[2].sidewaysFriction.stiffness;
      RRwheelFriction = new WheelFrictionCurve ();
        RRwheelFriction.extremumSlip = wheelColliders[3].sidewaysFriction.extremumSlip;
        RRWextremumSlip = wheelColliders[3].sidewaysFriction.extremumSlip;
        RRwheelFriction.extremumValue = wheelColliders[3].sidewaysFriction.extremumValue;
        RRwheelFriction.asymptoteSlip = wheelColliders[3].sidewaysFriction.asymptoteSlip;
        RRwheelFriction.asymptoteValue = wheelColliders[3].sidewaysFriction.asymptoteValue;
        RRwheelFriction.stiffness = wheelColliders[3].sidewaysFriction.stiffness;
    }

    void Update()
    { 
        localVelocityX = transform.InverseTransformDirection(rb.velocity).x;
        CurrentEngineRPM();
        for (int i = 0; i < wheelColliders.Length; i++)
        {
            WheelsRPM(i);
        }
        updateWheels();
        Brakes();
        if(!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D) && steeringAxis != 0f){
          ResetSteeringAngle();
        }
        if(Input.GetKey(KeyCode.D))
        {
          TurnRight();
        }
        if(Input.GetKey(KeyCode.A))
        {
          TurnLeft();
        }
        CarSpeed = (2 * Mathf.PI * wheelColliders[0].radius * wheelColliders[0].rpm * 60) / 1000;
        Debug.Log(CarSpeed);
    }

    void WheelsRPM(int i)
    {
        if (Input.GetAxis("Vertical") != 0)
        {
            wheelColliders[i].motorTorque = Input.GetAxis("Vertical") * Torque * TransmissionRatio() * maxRPM * Time.deltaTime / wheelRollResistance;
        }
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


    public void Brakes()
    {   
        if(Input.GetAxis("Vertical") == 0 && Input.GetKey(KeyCode.Space))
        {
            for (int i = 0; i < wheelColliders.Length; i++)
            {
                wheelColliders[i].brakeTorque = brakeForce + handBrakeForce;
                Handbrake();
            }
        }
        else if(Input.GetAxis("Vertical") != 0 && Input.GetKey(KeyCode.Space))
        {
            for (int i = 0; i < wheelColliders.Length; i++)
            {
                wheelColliders[i].brakeTorque = handBrakeForce;
                Handbrake();
            }
        }
    }

    void DecelerateCar()
    {
        if(Math.Abs(localVelocityX) > 2.5)
        {
            isDrifting = true;
        }
        else
        {
            isDrifting = false;
        }
    }

    public void TurnLeft(){
      steeringAxis = steeringAxis - (Time.deltaTime * 10f * steeringSpeed);
      if(steeringAxis < -1f){
        steeringAxis = -1f;
      }
      if (Input.GetAxis("Vertical") != 0)
      {
        steeringAngle = steeringAxis * maxSteeringAngle;
      }
      else
      {
        steeringAngle = steeringAxis * maxSteeringAngle * 1.5f;
      }
      wheelColliders[0].steerAngle = Mathf.Lerp(wheelColliders[0].steerAngle, steeringAngle, steeringSpeed);
      wheelColliders[1].steerAngle = Mathf.Lerp(wheelColliders[1].steerAngle, steeringAngle, steeringSpeed);
    }

    public void TurnRight(){
      steeringAxis = steeringAxis + (Time.deltaTime * 10f * steeringSpeed);
      if(steeringAxis > 1f){
        steeringAxis = 1f;
      }
      if (Input.GetAxis("Vertical") != 0)
      {
        steeringAngle = steeringAxis * maxSteeringAngle;
      }
      else
      {
        steeringAngle = steeringAxis * maxSteeringAngle * 1.5f;
      }
      wheelColliders[0].steerAngle = Mathf.Lerp(wheelColliders[0].steerAngle, steeringAngle, steeringSpeed);
      wheelColliders[1].steerAngle = Mathf.Lerp(wheelColliders[1].steerAngle, steeringAngle, steeringSpeed);
    }

    public void ResetSteeringAngle(){
      if(steeringAxis < 0f){
        steeringAxis = steeringAxis + (Time.deltaTime * 10f * steeringSpeed);
      }else if(steeringAxis > 0f){
        steeringAxis = steeringAxis - (Time.deltaTime * 10f * steeringSpeed);
      }
      if(Mathf.Abs(wheelColliders[0].steerAngle) < 1f){
        steeringAxis = 0f;
      }
      var steeringAngle = steeringAxis * maxSteeringAngle;
      wheelColliders[0].steerAngle = Mathf.Lerp(wheelColliders[0].steerAngle, steeringAngle, steeringSpeed);
      wheelColliders[1].steerAngle = Mathf.Lerp(wheelColliders[1].steerAngle, steeringAngle, steeringSpeed);
    }

    public void RecoverTraction(){
      isTractionLocked = false;
      driftingAxis = driftingAxis - (Time.deltaTime / 1.5f);
      if(driftingAxis < 0f){
        driftingAxis = 0f;
      }

      //If the 'driftingAxis' value is not 0f, it means that the wheels have not recovered their traction.
      //We are going to continue decreasing the sideways friction of the wheels until we reach the initial
      // car's grip.
      if(FLwheelFriction.extremumSlip > FLWextremumSlip){
        FLwheelFriction.extremumSlip = FLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
        wheelColliders[0].sidewaysFriction = FLwheelFriction;

        FRwheelFriction.extremumSlip = FRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
        wheelColliders[1].sidewaysFriction = FRwheelFriction;

        RLwheelFriction.extremumSlip = RLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
        wheelColliders[2].sidewaysFriction = RLwheelFriction;

        RRwheelFriction.extremumSlip = RRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
        wheelColliders[3].sidewaysFriction = RRwheelFriction;

        Invoke("RecoverTraction", Time.deltaTime);

      }
      else if (FLwheelFriction.extremumSlip < FLWextremumSlip){
        FLwheelFriction.extremumSlip = FLWextremumSlip;
        wheelColliders[0].sidewaysFriction = FLwheelFriction;

        FRwheelFriction.extremumSlip = FRWextremumSlip;
        wheelColliders[1].sidewaysFriction = FRwheelFriction;

        RLwheelFriction.extremumSlip = RLWextremumSlip;
        wheelColliders[2].sidewaysFriction = RLwheelFriction;

        RRwheelFriction.extremumSlip = RRWextremumSlip;
        wheelColliders[3].sidewaysFriction = RRwheelFriction;

        driftingAxis = 0f;
      }
    }

    public void Handbrake(){
      CancelInvoke("RecoverTraction");
      // We are going to start losing traction smoothly, there is were our 'driftingAxis' variable takes
      // place. This variable will start from 0 and will reach a top value of 1, which means that the maximum
      // drifting value has been reached. It will increase smoothly by using the variable Time.deltaTime.
      driftingAxis = driftingAxis + (Time.deltaTime);
      float secureStartingPoint = driftingAxis * FLWextremumSlip * handbrakeDriftMultiplier;

      if(secureStartingPoint < FLWextremumSlip){
        driftingAxis = FLWextremumSlip / (FLWextremumSlip * handbrakeDriftMultiplier);
      }
      if(driftingAxis > 1f){
        driftingAxis = 1f;
      }
      //If the forces aplied to the rigidbody in the 'x' asis are greater than
      //3f, it means that the car lost its traction, then the car will start emitting particle systems.
      if(Mathf.Abs(localVelocityX) > 2.5f){
        isDrifting = true;
      }else{
        isDrifting = false;
      }
      //If the 'driftingAxis' value is not 1f, it means that the wheels have not reach their maximum drifting
      //value, so, we are going to continue increasing the sideways friction of the wheels until driftingAxis
      // = 1f.
      if(driftingAxis < 1f){
        FLwheelFriction.extremumSlip = FLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
        wheelColliders[0].sidewaysFriction = FLwheelFriction;

        FRwheelFriction.extremumSlip = FRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
        wheelColliders[1].sidewaysFriction = FRwheelFriction;

        RLwheelFriction.extremumSlip = RLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
        wheelColliders[2].sidewaysFriction = RLwheelFriction;

        RRwheelFriction.extremumSlip = RRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
        wheelColliders[3].sidewaysFriction = RRwheelFriction;
      }

      // Whenever the player uses the handbrake, it means that the wheels are locked, so we set 'isTractionLocked = true'
      // and, as a consequense, the car starts to emit trails to simulate the wheel skids.
      isTractionLocked = true;
    }
    
}