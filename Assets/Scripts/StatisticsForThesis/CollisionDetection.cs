using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    public int LowerBound;
    public UnityFFB.UnityFFB joyControl;
    public float speed = 10;

    public List<double> collisions = new List<double>();


    public double CalculateDistToCollider(Collider target)
    {
        if (target.gameObject.CompareTag("PickedUpContainer"))
        {
            var contPoint = target.ClosestPointOnBounds(gameObject.transform.position);
            var trolleyPoint = gameObject.GetComponent<Collider>().ClosestPointOnBounds(contPoint);

            return Vector3.Distance(contPoint, trolleyPoint) - .7;
        }
        return 999;
    }

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
