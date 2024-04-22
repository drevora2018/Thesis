using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraneJoyInput : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed = 5.0f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float moveHorizontal = Input.GetAxis("Joy_X");
        float moveVertical = Input.GetAxis("Joy_Y");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        transform.Translate(movement * (speed * Time.deltaTime));
    }
}
