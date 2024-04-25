using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class RayCastAroundCrane : MonoBehaviour
{

    public GameObject handle;
    public AudioSource audioSource1, audioSource2;
    public int numRays = 12; // Number of rays to cast
    public float rayDistance = 5f; // Distance for the rays
    public float raySpreadAngle = 45f; // Angle between each ray in degrees
    public LayerMask layerMask; // Layer mask to determine which objects the rays can hit
    public ManualControlQuay controlQuay;
    public AudioSource PositionBeep;
    public AudioSource PutDownBeep;

    public bool EnableCollisionNotification, 
        EnableGuidanceNotificaton = false;

    [Range(0f, 5f)]public float Margin;

    bool TruckBeep;


    void Start()
    {
        audioSource1.pitch = 0;
        TruckBeep = false;
    }

    private void Update()
    {
        BeepWhenPutDown();
        if(EnableCollisionNotification) CastRaysInCircle();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PickedUpContainer"))
        {
            var target = other.transform.position;
            var origo = gameObject.transform.position;

            var X0 = origo.z;
            var Y0 = origo.x;

            var X1 = target.z;
            var Y1 = target.x * -1;

            var angle = Math.Atan2(X1 - X0, Y1 - Y0);
            var dist = Math.Sqrt(Math.Pow(X1 - X0, 2) + Math.Pow(Y1 - Y0, 2));

            var start = 3 * Math.Cos(angle);

            print($"Dist: {dist}");
            print($"Angle: {angle}");

        }
    }

    void CastRaysInCircle()
    {
        Vector3 Origin;
        if (controlQuay.IsHeldObj()) { Origin = handle.transform.position + -(transform.up * 3.5f); }
        else { Origin = handle.transform.position + -(transform.up * 1f); }
        
        // Calculate the angle between each ray
        float angleIncrement = raySpreadAngle / numRays;

        // Calculate the starting angle
        float startAngle = -raySpreadAngle / 2f;

        // Track if any ray hits
        bool anyRayHit = false;

        float distanceToRay = 1000f;

        for (int i = 0; i < numRays; i++)
        {
            float angle = startAngle + i * angleIncrement;

            float radians = angle * Mathf.Deg2Rad;

            Vector3 direction = new Vector3(Mathf.Sin(radians), 0f, Mathf.Cos(radians));

            RaycastHit hit;
            rayDistance = 1 + (8 * Math.Abs(direction.z));


            if (Physics.Raycast(Origin, direction, out hit, rayDistance))
            {
                if (hit.distance < distanceToRay) distanceToRay = hit.distance;
                Debug.DrawLine(Origin, hit.point, Color.red);
                if (hit.transform.CompareTag("PickedUpContainer"))
                    anyRayHit = true;
            }
            else
            {
                // If the ray doesn't hit anything, draw a debug line to visualize the ray's path
                Debug.DrawRay(Origin, direction * rayDistance, Color.green);
            }
        }

        // Check if any ray hit something and perform the desired action
        if (anyRayHit)
        {
            Debug.Log("Picked Up Container Distance: "+ distanceToRay);
            if (distanceToRay < 3)
            {
                audioSource1.pitch = (1 - +(1 / (distanceToRay / 3))) * -1;
                if (!audioSource1.isPlaying) audioSource1.Play();
            }
            else
            {
                audioSource2.Stop();
            }

            // Perform the action when any ray hits
            Debug.Log("Action performed because at least one ray hit." + distanceToRay);
            // Add your code here to perform the desired action
        }

        else
        {
            audioSource1.Stop();
            audioSource2.Stop();
        }

    }

    void BeepWhenPutDown()
    {
        if(EnableGuidanceNotificaton)
        {
            if (controlQuay.distance > 3) PutDownBeep.Stop();
            else if (!TruckBeep)
            {
                PutDownBeep.pitch = (float)(2.0 - (controlQuay.distance / 3.0));
                if (!PutDownBeep.isPlaying) PutDownBeep.Play();
            }
            else { PutDownBeep.Stop(); }
        }
        else
        {
            PutDownBeep.Stop();
        }
    }
}
