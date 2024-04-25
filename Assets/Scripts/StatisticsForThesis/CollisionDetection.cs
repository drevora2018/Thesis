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


    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("PickedUpContainer"))
        {
            var target = other.transform.position;
            var origo = gameObject.transform.position;

            var X0 = origo.x;
            var Y0 = origo.z;

            var X1 = target.x;
            var Y1 = target.z;

            var angle = Math.Abs(Math.Atan2(Y1 - Y0, X1 - X0)); // Adjusted to use Y1 - Y0 and X1 - X0
            var dist = Vector2.Distance(new Vector2(target.x, target.z), new Vector2(origo.x, origo.z));

            var centerX = target.x + 7.00 * Math.Cos(angle); // Calculate X coordinate of the point on the target rectangle
            var centerY = target.z + 1.25 * Math.Sin(angle);  // Calculate Y coordinate of the point on the target rectangle

            var X2 = centerX - origo.x; // Calculate X component relative to origin
            var Y2 = centerY - origo.z; // Calculate Y component relative to origin

            var comp = 2 * Math.Sqrt(Math.Pow(Math.Abs(X2), 2) + Math.Pow(Math.Abs(Y2), 2)); // Adjusted to use absolute values

            var closestPointsDistance = Math.Abs(dist - comp); // Calculate the distance between the two closest points

            print($"Proximity: Dist:  {dist}");
            print($"Proximity: Angle: {angle * Mathf.Rad2Deg}");
            print($"Proximity: Comp:  {comp}");
            print($"Proximity: Res:   {closestPointsDistance}");
        }
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
