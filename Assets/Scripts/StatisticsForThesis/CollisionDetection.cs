using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    public int LowerBound;
    public UnityFFB.UnityFFB joyControl;
    public float speed = 10;

    public List<double> collisions = new List<double>();
    
    private void OnCollisionEnter(Collision collision)
    {
        var vect = new Vector2(
            Math.Abs((-joyControl.Axis_X * speed) * Time.deltaTime),
            Math.Abs((-joyControl.Axis_Y * speed) * Time.deltaTime)
        );
        
        var CollisionsMagnitude = vect.magnitude * 3000 / Time.deltaTime;
        print($"" +
              $"Collision Detected - Magnitude: " +
              $"{vect.magnitude * 3000 / Time.deltaTime}. " +
              $"Vector - X:{vect.x}, Y:{vect.y}"
        );
        if (collision.gameObject.CompareTag("PickedUpContainer"))
        {
            collisions.Add(CollisionsMagnitude);
        }
    }
}
