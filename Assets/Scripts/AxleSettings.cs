using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxleSettings : MonoBehaviour
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
    
}
