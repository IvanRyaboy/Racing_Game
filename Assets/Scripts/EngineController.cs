using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineController : MonoBehaviour
{
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
    public AxleSettings[] axles = new AxleSettings[2];


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
}
