using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class carController : MonoBehaviour
{
    public float Torque = 100f, maxRPM, minRPM;
    public WheelCollider[] wheelColliders;
    [HideInInspector] public inputs input;
    [HideInInspector] Rigidbody rb;


    void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    void Update()
    {
        for (int i = 0; i < wheelColliders.Length; i++)
        {
            wheelColliders[i].motorTorque = Input.GetAxis("Vertical") * Torque;
        }
    }
}
