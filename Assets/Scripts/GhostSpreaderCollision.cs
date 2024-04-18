using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostSpreaderCollision : MonoBehaviour
{
    public ManualControl craneControl;

    private void Start()
    {
        //craneControl = GetComponent<ManualControl>();
    }
    private void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "LimitCrane")
        {
            if (other.gameObject.name == "left")
            {
                craneControl.stop1Ghost = true;
            }
            if (other.gameObject.name == "right")
            {
                craneControl.stop2Ghost = true;
            }
            if (other.gameObject.name == "forward")
            {
                craneControl.stop3Ghost = true;
            }
            if (other.gameObject.name == "back")
            {
                craneControl.stop4Ghost = true;
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "LimitCrane")
        {
            if (other.gameObject.name == "left")
            {
                craneControl.stop1Ghost = false;
            }
            if (other.gameObject.name == "right")
            {
                craneControl.stop2Ghost = false;
            }
            if (other.gameObject.name == "forward")
            {
                craneControl.stop3Ghost = false;
            }
            if (other.gameObject.name == "back")
            {
                craneControl.stop4Ghost = false;
            }
        }
    }
}
