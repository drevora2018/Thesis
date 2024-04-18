using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class TargetCollision : MonoBehaviour
{
    Targets targetParent;

    void Start()
    {
        targetParent = transform.parent.GetComponent<Targets>();
    }

    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (transform.name == "TruckTarget" && collision.collider.gameObject.CompareTag("CraneSpreader"))
        {
            targetParent.NextTarget();
        }
        else if (collision.collider.gameObject.CompareTag("PickedUpContainer"))
        {
            targetParent.TargetReached(collision.collider.gameObject);
            //var dist = Vector3.Distance(transform.position, collision.collider.gameObject.transform.position);
            //targetParent.userTestLog.AddTargetDistance(dist);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (transform.name == "TruckTarget" && other.gameObject.CompareTag("CraneSpreader"))
        {
            targetParent.NextTarget();
        }
        else if (other.gameObject.CompareTag("PickedUpContainer"))
        {
            targetParent.TargetReached(other.gameObject);
            //var dist = Vector3.Distance(transform.position, other.gameObject.transform.position);
            //targetParent.userTestLog.AddTargetDistance(dist);
        }
    }
}
