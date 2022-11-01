using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelSettings : MonoBehaviour
{
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
}
