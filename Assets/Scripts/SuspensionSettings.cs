using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuspensionSettings : MonoBehaviour
{
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
}
