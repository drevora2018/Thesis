using System;
using UnityEngine;

public class RayCastAroundCrane : MonoBehaviour
{

    public GameObject handle;
    public AudioSource audioSource;
    public AudioSource PutDownBeep;
    public int numRays = 12; // Number of rays to cast
    public float rayDistance = 5f; // Distance for the rays
    public float raySpreadAngle = 45f; // Angle between each ray in degrees
    public LayerMask layerMask; // Layer mask to determine which objects the rays can hit
    public ManualControlQuay controlQuay;
    public CollisionDetection CollisionDetection;

    private Collider target;

    public bool Audio1, 
        Audio2 = false;

    [Range(0f, 5f)]public float Margin;

    bool TruckBeep;

    public AudioClip Guidance1, Guidance2;


    void Start()
    {
        audioSource.pitch = 1;
        TruckBeep = false;
    }

    private void Update()
    {
        if(Audio1 || Audio2)
        {
            BeepWhenPutDown();
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
            rayDistance = 1 + (15 * Math.Abs(direction.z));
            
            if (Physics.Raycast(Origin, direction, out hit, rayDistance))
            {
                anyRayHit = true;
                if (hit.distance < distanceToRay)
                {
                    distanceToRay = hit.distance;
                    target = hit.collider;
                }
                Debug.DrawLine(Origin, hit.point, Color.red);
            }
            else
            {
                // If the ray doesn't hit anything, draw a debug line to visualize the ray's path
                Debug.DrawRay(Origin, direction * rayDistance, Color.green);
            }
        }

        // Check if any ray hit something and perform the desired action
        if (!anyRayHit) return;
        var dist = CollisionDetection.CalculateDistToCollider(target: target);
        if (dist > 2)
        {
            if (!audioSource.isPlaying) return;
            print("Stopped audio");
            audioSource.Stop();
        }
        else
        {
            audioSource.pitch = (float)(1 + (1 * (1  - dist / 2)));
            if(!audioSource.isPlaying) audioSource.Play();
        }
    }

    void BeepWhenPutDown()
    {
        if (Audio1) PutDownBeep.clip = Guidance1;
        else PutDownBeep.clip = Guidance2;
        if (controlQuay.distance > 3) PutDownBeep.Stop();
        else if (!TruckBeep)
        {
            PutDownBeep.pitch = (float)(2.0 - (controlQuay.distance / 3.0));
            if (!PutDownBeep.isPlaying) PutDownBeep.Play();
        }
        else PutDownBeep.Stop();
    }
}
