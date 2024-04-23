using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    public int LowerBound;
    Vector3 lastpos;
    public UnityFFB.UnityFFB joyControl;
    public float speed = 10;

    public List<double> collisions = new List<double>();


    private void Start()
    {
        lastpos = transform.position;
    }

    private void Update()
    {
        lastpos = transform.position;
    }
    private void OnCollisionEnter(Collision collision)
    {
        var X0 = 0;
        var Y0 = 0;
        var X1 = -joyControl.Axis_X;
        var Y1 = -joyControl.Axis_Y;
        
        var CollisionsMagnitude = Math.Sqrt(Math.Pow(X1 - X0, 2) + Math.Pow(Y1 - Y0, 2)) / Time.deltaTime;
        if (collision.gameObject.CompareTag("PickedUpContainer"))
        {
            collisions.Add(CollisionsMagnitude);
        }
    }
}
