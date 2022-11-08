using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EngineController : MonoBehaviour
{
    const int WHEEL_LEFT_INDEX = 0;
    const int WHEEL_RIGHT_INDEX = 1;

    const float wheelWidth = 0.085f;
    public Vector3 centerOfMass = Vector3.zero;

    [Header("Engine")]
    [Tooltip("Y - Desired vehicle speed (km/h). X - Time (seconds)")]
    public AnimationCurve accelerationCurve = AnimationCurve.Linear(0.0f, 0.0f, 5.0f, 100.0f);
    [Tooltip("Y - Desired vehicle speed (km/h). X - Time (seconds)")]
    public AnimationCurve accelerationCurveReverse = AnimationCurve.Linear(0.0f, 0.0f, 5.0f, 20.0f);
    [Tooltip("Number of times to iterate reverse evaluation of Acceleration Curve. May need to increase with higher max vehicle speed. ")]
    public int reverseEvaluationAccuracy = 25;

    [Header("Steering")]
    //x - steering in km/h
    //y - steering in degrees
    [Tooltip("Y = Steering angle limit (deg). X - Vehicle speed (km/h)")]
    public AnimationCurve steerAngleLimit = AnimationCurve.Linear(0.0f, 35.0f, 100.0f, 5.0f);

    // x - speed in km/h
    // y - angle in degrees (speed of returning wheels to zero position)
    [Tooltip("Y - Steereing reset speed (deg/sec). X - Vehicle speed (km/h)")]
    public AnimationCurve steeringResetSpeed = AnimationCurve.EaseInOut(0.0f, 30.0f, 100.0f, 10.0f);

    // x - speed in km/h
    // y - angle in degrees
    [Tooltip("Y - Steereing speed (deg/sec). X - Vehicle speed (km/h)")]
    public AnimationCurve steeringSpeed = AnimationCurve.Linear(0.0f, 2.0f, 100.0f, 0.5f);

    [Header("Debug")]
    public bool debugDeaw = true;

    [Header("Other")]
    [Tooltip("Stabilization in flight (torque)")]
    [SerializeField] public float flihgtstabilizatonForce = 8.0f;
    [Tooltip("Stabilization in flight (Ang velocity damping)")]
    [SerializeField] public float flightStabilizationDamping = 0.0f;
    [Tooltip("Hand brake slippery time in seconds")]
    [SerializeField] public float handBrakeSlipperyTime = 2.2f;

    public bool controllable = true;

    // x - speed in km/h
    // y - Downforce percentage
    [Tooltip("Y - Downforce (percentage 0%..100%). X - Vehicle speed (km/h)")]
    public AnimationCurve downForceCurve = AnimationCurve.Linear(0.0f, 0.0f, 200.0f, 100.0f);
    [Tooltip("Downforce")]
    [SerializeField] public float downForce = 5.0f;

    [Header("Axles")]
    public Axle[] axles = new Axle[2];


    //Variables for project 
    float afterFlightSlipperyTiresTime = 0.0f;
    float brakeSlipperyTiresTime = 0.0f;
    float handBrakeSlipperyTiresTime = 0.0f;
    bool isBrake = false;
    bool isHandBrake = false;
    bool isAcceleration = false;
    bool isReverseAcceleration = false;
    float accelerationForceMagnitude = 0.0f;
    Rigidbody rb = null;

    //UI style for debug render
    static GUIStyle style = new GUIStyle();

    //For alloc-free force 
    Ray wheelRay = new Ray();
    RaycastHit[] wheelRayHits = new RaycastHit[16];

    void Reset(Vector3 position)
    {
        position += new Vector3(UnityEngine.Random.Range(-1.0f, 1.0f), 0.0f, UnityEngine.Random.Range(-1.0f, 1.0f));
        float yaw = transform.eulerAngles.y + UnityEngine.Random.Range(-10.0f, 10.0f);

        transform.position = position;
        transform.rotation = Quaternion.Euler(new Vector3(0.0f, yaw, 0.0f));

        rb.velocity = new Vector3(0f, 0f, 0f);
        rb.angularVelocity = new Vector3(0f, 0f, 0f);

        for (int axleIndex = 0; axleIndex < axles.Length; axleIndex++)
        {
            axles[axleIndex].steerAngle = 0.0f;
        }

        Debug.Log(string.Format("Reset {0}, {1}, {2}, Rot {3}", position.x, position.y, position.z, yaw));
    }

    void Start()
    {
        style.normal.textColor = Color.red;

        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass;
    }

    void OnValidate()
    {
        //HACK: to apply steering in editor
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        ApplyVisual();
        CalculateAckermanSteering();
    }

    float GetHandBrakeK()
    {
        float x = handBrakeSlipperyTiresTime / Math.Max(0.1f, handBrakeSlipperyTime);
        //Smoother step
        x = x * x * x * (x * (x * 6 - 15) + 10);
        return x; 
    }

    void CalculateAckermanSteering()
    {
        //Copy desired steerig
        for (int axleIndex = 0; axleIndex < axles.Length; axleIndex++)
        {
            float steerAngleRad = axles[axleIndex].steerAngle * Mathf.Deg2Rad;

            axles[axleIndex].wheelDataL.yawRad = steerAngleRad;
            axles[axleIndex].wheelDataR.yawRad = steerAngleRad;
        }

        if (axles.Length != 2)
        {
            Debug.LogWarning("Ackerman work only for 2 axle vehicles");
            return;
        }

        Axle frontAxle = axles[0];
        Axle rearAxle = axles[1];

        if (Mathf.Abs(rearAxle.steerAngle) > 0.0001f)
        {
            Debug.LogWarning("Ackerman work only for vehicles with forvard steering axle");
            return;
        }

        //Calculate our chassis (remove scale)
        Vector3 axleDiff = transform.TransformPoint(new Vector3(0.0f, frontAxle.offset.y, frontAxle.offset.x)) - transform.TransformPoint(new Vector3(0.0f, rearAxle.offset.y, rearAxle.offset.x));
        float axleSeparation = axleDiff.magnitude;

        Vector3 wheelDiff = transform.TransformPoint(new Vector3(frontAxle.width * -0.5f, frontAxle.offset.y, frontAxle.offset.x)) - transform.TransformPoint(new Vector3(frontAxle.width * 0.5f, frontAxle.offset.y, frontAxle.offset.x));
        float wheelSeparation = wheelDiff.magnitude;

        //Get turning circle radius for steering angle input
        float turningCircleRadius = axleSeparation / Mathf.Tan(frontAxle.steerAngle * Mathf.Deg2Rad);

        //Make front inside tire turn sharper and outside tire less sharp based on turning circle radius
        float steerAngleLeft = Mathf.Atan(axleSeparation / (turningCircleRadius + (wheelSeparation / 2)));
        float steerAngleRight = Mathf.Atan(axleSeparation / (turningCircleRadius - ( wheelSeparation / 2)));

        frontAxle.wheelDataL.yawRad = steerAngleLeft;
        frontAxle.wheelDataR.yawRad = steerAngleRight;
    }

    void CalculateWheelVisualTransform(Vector3 wsAttachPoint, Vector3 wsDownDirection, Axle axle, WheelData data, int wheelIndex, float wisualRotationRad, out Vector3 pos, out Quaternion rot)
    {
        float suspCurrentLen = Mathf.Clamp01(1.0f - data.compression) * axle.lenghtRelaxed;

        pos = wsAttachPoint + wsDownDirection * suspCurrentLen;

        float additionalYaw = 0.0f;
        float additionalMul = Mathf.Rad2Deg;
        if (wheelIndex == WHEEL_LEFT_INDEX)
        {
            additionalYaw = 180.0f;
            additionalMul = -Mathf.Rad2Deg;
        }
        Quaternion localWheelRot = Quaternion.Euler(new Vector3(data.visualRotationRad * additionalMul, additionalYaw + data.yawRad * Mathf.Rad2Deg, 0.0f));
        rot = transform.rotation * localWheelRot;   
    }

    void CalculateWheelRotationFromSpeed(Axle axle, WheelData data, Vector3 wsPos)
    {
        if (rb == null)
        {
            data.visualRotationRad = 0.0f;
            return;
        }

        Quaternion localWheelRot = Quaternion.Euler(new Vector3(0.0f, data.yawRad * Mathf.Rad2Deg, 0.0f));
        Quaternion wsWheelRot = transform.rotation * localWheelRot;

        Vector3 wsWheelForward = wsWheelRot * Vector3.forward;
        Vector3 velosityQueryPos = data.isOnGround ? data.touchPoint.point : wsPos;
        Vector3 wheelVelosity = rb.GetPointVelocity(velosityQueryPos);
        //Longitudinal speed (m/s)
        float tireLongSpeed = Vector3.Dot(wheelVelosity, wsWheelForward);

        //Circle lenght = 2 * PI * R
        float wheelLenghtMeters = 2 * Mathf.PI * axle.radius;

        //Wheels turns per second
        float rps = tireLongSpeed / wheelLenghtMeters;

        float deltaRot = Mathf.PI * 2.0f * rps * Time.deltaTime;

        data.visualRotationRad += deltaRot; 
    }

    void ApplyVisual()
    {
        Vector3 wsDownDirection = transform.TransformDirection(Vector3.down);
        wsDownDirection.Normalize();

        for(int axleIndex = 0; axleIndex < axles.Length; axleIndex++)
        {
            Axle axle = axles[axleIndex];
            Vector3 localL = new Vector3(axle.width * -0.5f, axle.offset.y, axle.offset.x);
            Vector3 localR = new Vector3(axle.width * 0.5f, axle.offset.y, axle.offset.x);

            Vector3 wsL = transform.TransformPoint(localL);
            Vector3 wsR = transform.TransformPoint(localR);

            Vector3 wsPos;
            Quaternion wsRot;

            if (axle.wheelVisualLeft != null)
            {
                CalculateWheelVisualTransform(wsL, wsDownDirection, axle, axle.wheelDataL, WHEEL_LEFT_INDEX, axle.wheelDataL.visualRotationRad, out wsPos, out wsRot);
                axle.wheelVisualLeft.transform.position = wsPos;
                axle.wheelVisualLeft.transform.rotation = wsRot;
                axle.wheelVisualLeft.transform.localScale = new Vector3(axle.radius, axle.radius, axle.radius) * axle.visualScale;

                if (!isBrake)
                {
                    CalculateWheelRotationFromSpeed(axle, axle.wheelDataL, wsPos);
                }
            }

            if (axle.wheelVisualRight != null)
            {
                CalculateWheelVisualTransform(wsR, wsDownDirection, axle, axle.wheelDataR, WHEEL_RIGHT_INDEX, axle.wheelDataR.visualRotationRad, out wsPos, out wsRot);
                axle.wheelVisualRight.transform.position = wsPos;
                axle.wheelVisualRight.transform.rotation = wsRot;
                axle.wheelVisualRight.transform.localScale = new Vector3(axle.radius, axle.radius, axle.radius) * axle.visualScale;

                if (!isBrake)
                {
                    CalculateWheelRotationFromSpeed(axle, axle.wheelDataR, wsPos);
                }
            }
        }
    }

}
