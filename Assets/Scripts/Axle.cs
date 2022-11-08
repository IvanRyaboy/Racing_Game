using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axle : MonoBehaviour
{
    [Header("Debug Settings")]
    [Tooltip("Debug name of axle")]
    [SerializeField] private string debugName;

    [Tooltip("Debug color of axle")]
    [SerializeField] public Color debugColor = Color.white;

    [Header("Axle settings")]
    
    [Tooltip("Axle width")]
    [SerializeField] public float width = 0.4f;

    [Tooltip("Axle offset")]
    [SerializeField] public Vector2 offset = Vector2.zero;

    [Tooltip("Current steering angle (in degrees)")]
    [SerializeField] public float steerAngle = 0.0f;

    [Header("Wheel settings")]

    [Tooltip("Wheel radius in meter")]
    [SerializeField] public float radius = 0.3f;

    [Range(0.0f, 1.0f)]
    [Tooltip("Tire lateral friction normalized 0...1")]
    [SerializeField] public float lateralFriction = 0.1f;

    [Range(0.0f, 1.0f)]
    [Tooltip("Rolling friction, normalized 0...1")]
    [SerializeField] public float rollingFriction = 0.1f;

    [HideInInspector]
    [Tooltip("Brake left")]
    public bool brakeLeft = false;

    [HideInInspector]
    [Tooltip("Brake right")]
    public bool brakeRight = false;

    [HideInInspector]
    [Tooltip("Hand brake left")]
    public bool handBrakeLeft = false;

    [HideInInspector]
    [Tooltip("Hand brake right")]
    public bool handBrakeRight = false;

    [Tooltip("Brake force magnitude")]
    public float brakeForceMag = 4.0f;

    [Header("Suspension settings")]

    [Tooltip("Suspension stiffness (Suspension 'power')")]
    [SerializeField] public float stiffness = 8500.0f;
    [Tooltip("Suspension damping (Suspension 'bounce')")]
    [SerializeField] public float damping = 3000.0f;
    [Tooltip("Suspension Restruction (Not using now)")]
    [SerializeField] public float restruction = 1.0f;

    [Tooltip("Relaxed suspension lenght")]
    [SerializeField] public float lenghtRelaxed = 0.55f;
    [Tooltip("Stabeliser bar anti-roll force")]
    [SerializeField] public float antiRollForce = 100000.0f;

    [HideInInspector]
    public WheelData wheelDataL = new WheelData();
    [HideInInspector]
    public WheelData wheelDataR = new WheelData();

    [Header("Visual settings")]

    [Tooltip("Visual scale for wheels")]
    [SerializeField] public float visualScale = 0.03270531f;
    [Tooltip("Wheel actor left")]
    [SerializeField] public GameObject wheelVisualLeft;
    [Tooltip("Wheel actor right")]
    [SerializeField] public GameObject wheelVisualRight;
    [Tooltip("Is axle powered by engine")]
    [SerializeField] public bool isPowered = false;

    [Tooltip("After flight slippery coefficent (0 - no friction)")]
    [SerializeField] public float afterFlightSlipperyK = 0.02f;
    [Tooltip("Brake slippery coefficent (0 - no friction)")]
    [SerializeField] public float brakeSlipperyK = 0.5f;
    [Tooltip("Hand brake slippery coefficent (0 - no friction)")]
    [SerializeField] public float handBrakeSlipperyK = 0.01f;
}
