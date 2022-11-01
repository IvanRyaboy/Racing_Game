using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelData : MonoBehaviour
{
    //Is wheels touch the ground
    [HideInInspector]
    public bool isOnGround;

    //Wheel ground touch point
    [HideInInspector]
    public RaycastHit touchPoint = new RaycastHit();

    //Real yaw, after steering correction
    [HideInInspector]
    public float yawRad = 0.0f;

    //Visual rotation
    [HideInInspector]
    public float visualRotationRad = 0.0f;

    //Suspension compression
    [HideInInspector]
    public float compression = 0.0f;

    //Suspension compression on previous update
    [HideInInspector]
    public float compressionPrev = 0.0f;

    [HideInInspector]
    public string debugText = "-";

}