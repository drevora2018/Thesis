using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    public int LowerBound;
    Vector3 lastpos;

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
        var RelativeVelocity = transform.position - lastpos;
        if (collision.gameObject.CompareTag("PickedUpContainer"))
        {
            var CollisionsMagnitude = RelativeVelocity.magnitude;
            print("Collision Magnitude "+ CollisionsMagnitude);
        }
    }
}
