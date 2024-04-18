using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RayCastAroundCrane : MonoBehaviour
{

    public GameObject handle;
    public AudioSource audioSource1, audioSource2;
    public int numRays = 12; // Number of rays to cast
    public float rayDistance = 5f; // Distance for the rays
    public float raySpreadAngle = 45f; // Angle between each ray in degrees
    public LayerMask layerMask; // Layer mask to determine which objects the rays can hit
    public ManualControlQuay controlQuay;

    void Start()
    {
        
    }

    private void Update()
    {
        CastRaysInCircle();
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
            if (Physics.Raycast(Origin, direction, out hit, rayDistance))
            {
                if (hit.distance < distanceToRay)
                {
                    distanceToRay = hit.distance;
                }
                Debug.DrawLine(Origin, hit.point, Color.red);
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
            Debug.Log(distanceToRay);
            if (distanceToRay < 5 && distanceToRay > 2)
            {
                if (!audioSource1.isPlaying)
                {
                    audioSource1.Play();
                    audioSource2.Stop();
                }
            }
            else if (distanceToRay <= 2)
            {
                if (!audioSource2.isPlaying)
                {
                    audioSource1.Stop();
                    audioSource2.Play();
                }
            }
            // Perform the action when any ray hits
            Debug.Log("Action performed because at least one ray hit.");
            // Add your code here to perform the desired action
        }

        else
        {
            audioSource1.Stop();
            audioSource2.Stop();
        }

    }
}
