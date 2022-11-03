using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualSettings : MonoBehaviour
{
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
