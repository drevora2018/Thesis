using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualControlQuay : MonoBehaviour
{// public CameraController cameraController;
    public PanamaxCraneMovement anotherScript;
    public UnityFFB.UnityFFB joyControl;

    public GameObject Trolley;
    public GameObject Spreader;
    public GameObject Ropes;
    public GameObject Limit;
    private GameObject ControlUI;
    private GameObject CraneControlUI;
    
    
    public bool KeyboardControl = false;
    public bool JoystickControl = true;


    public Camera[] targetCameras;
    public Camera main;

    float SpreaderUpOffset = 3.7f;
    private GameObject HeldObj;
    private float InitialCranePosition;
    private float InitialTrolleyPosition;
    private float InitialSpreaderPosition;
    private bool stop1, stop2, stop3 = false;
    // private Vector3 min;
    // private Vector3 max;

    public float speed = 3f;
    private float RopeSpeed = 0.052f;

    public TruckDropOff LoadingSpot;


    // Start is called before the first frame update
    void Start()
    {
        // Get the collider component attached to this GameObject.
        //Collider collider = Limit.GetComponent<BoxCollider>();

        // Get the bounds of the collider.
        // Bounds bounds = collider.bounds;

        // Get the minimum and maximum points of the bounds.
        /*  min = bounds.min;
          max = bounds.max;
          Debug.Log("Left edge: " + min.x);
          Debug.Log("Right edge: " + max.x);
          Debug.Log("Bottom edge: " + min.y);
          Debug.Log("Top edge: " + max.y);
          Debug.Log("Front edge: " + min.z);
          Debug.Log("Back edge: " + max.z);*/
        foreach (var cam in targetCameras)
        {
            cam.enabled = true;
        }
        GetComponent<ManualControlQuay>().KeyboardControl = true;
        InitialCranePosition = transform.position.z;
        InitialTrolleyPosition = Trolley.transform.position.x;
        InitialSpreaderPosition = Spreader.transform.position.y;

        ControlUI = GameObject.FindGameObjectWithTag("PauseCanvas").transform.GetChild(0).gameObject;
        CraneControlUI = GameObject.FindGameObjectWithTag("PauseCanvas").transform.GetChild(3).gameObject;

        ControlUI.SetActive(false);
        CraneControlUI.SetActive(true);
        // Vector3 pos = Plane.transform.position + (Plane.transform.localScale);
    }
    public bool IsHeldObj() { if (HeldObj != null) return true; else return false; }
    public void DisableScript()
    {
        enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "LimitCrane")
        {

            if (other.gameObject.name == "left")
            {
                stop1 = true;
            }
            if (other.gameObject.name == "right")
            {
                stop2 = true;
            }
            if (other.gameObject.name == "forward")
            {
                stop3 = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "LimitCrane")
        {

            if (other.gameObject.name == "left")
            {
                stop1 = false;
            }
            if (other.gameObject.name == "right")
            {
                stop2 = false;
            }
            if (other.gameObject.name == "forward")
            {
                stop3 = false;
            }
        }
    }



    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit3;

            if (Physics.Raycast(ray, out hit3))
            {
                if (hit3.collider.tag == "PanamaxCrane")
                {
                    hit3.collider.gameObject.GetComponent<ManualControlQuay>().KeyboardControl = true;
                    CraneControlUI.SetActive(true);
                    ControlUI.SetActive(false);
                }

                // targetCamera.enabled = true;
                // The ray hit an object with a collider.
                // You can use the hit.collider and hit.transform properties
                // to determine which object was hit.

                // For example, you could select the object by changing its color:
                // hit.transform.GetComponent<Renderer>().material.color = Color.red;

                // Or you could perform some other action on the object, such as activating a script or setting a variable.
            }
        }
        RaycastHit hit;
        
        main.GetComponent<CameraController>().enabled = false;
        //      cameraController.enabled= false;
        this.main.enabled = false;
        foreach (var cam in targetCameras)
        {
            cam.enabled = true;

        }
        this.anotherScript.enabled = false;
        
        if (KeyboardControl)
        {
            if (Input.GetKey(KeyCode.W))
            {   // W key is being pressed
                if (!stop3) Trolley.transform.position += transform.forward * Time.deltaTime * speed;
            }

            if (Input.GetKey(KeyCode.S))
            {
                if (InitialTrolleyPosition < Trolley.transform.position.x)
                {   // S key is being pressed
                    Trolley.transform.position += -transform.forward * Time.deltaTime * speed;
                }
            }
            if (Input.GetKey(KeyCode.A))
            {   // A key is being pressed
                if (!stop1) transform.position += -transform.right * Time.deltaTime * speed;
            }
            
            if (Input.GetKey(KeyCode.D))
            {   // D key is being pressed
                if (!stop2) transform.position += transform.right * Time.deltaTime * speed;
            }
        }
        if (JoystickControl)
        {
            if (-joyControl.Axis_X > 0)
            {
                if (!stop1)
                {   
                    transform.position += new Vector3(
                        0, 
                        0, 
                        -joyControl.Axis_X * Time.deltaTime * speed
                    );;
                }
            }
            else
            {
                if (!stop2)
                {   
                    transform.position += new Vector3(
                        0, 
                        0, 
                        -joyControl.Axis_X * Time.deltaTime * speed
                    );;
                }
            }
            
            if (-joyControl.Axis_Y > 0)
            {
                if (!stop3)
                {   
                    Trolley.transform.position += new Vector3(
                        -joyControl.Axis_Y * Time.deltaTime * speed, 
                        0, 
                        0
                    );;
                }
            }
            else
            {
                if (InitialTrolleyPosition < Trolley.transform.position.x)
                {   
                    Trolley.transform.position += new Vector3(
                        -joyControl.Axis_Y * Time.deltaTime * speed, 
                        0, 
                        0
                    );;
                }
            }
        }
        if (Input.GetKey(KeyCode.E)) 
        { 
            if (InitialSpreaderPosition > Spreader.transform.position.y) 
            { 
                Spreader.transform.position += transform.up * Time.deltaTime * speed; 
                Ropes.transform.localScale += RopeSpeed * speed * Time.deltaTime * Spreader.transform.up;
                
            }
        }

        if (Input.GetKey(KeyCode.Q) && (!Physics.Raycast(Spreader.transform.position - Spreader.transform.up * 0.2f, -Spreader.transform.up, out hit, 0.9f) || 
            (HeldObj != null && !Physics.Raycast(new(Spreader.transform.position.x, Spreader.transform.position.y - HeldObj.GetComponent<BoxCollider>().size.y, Spreader.transform.position.z), -Spreader.transform.up, out hit, 0.8f)) || hit.collider.isTrigger || hit.collider.gameObject.CompareTag("CraneSpreader")))
        { 
            Spreader.transform.position += -transform.up * Time.deltaTime * speed; 
            Ropes.transform.localScale += -RopeSpeed * speed * Time.deltaTime * Spreader.transform.up;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            /*
            RaycastHit hit1; 
            if (Physics.Raycast(Spreader.transform.position, -Spreader.transform.up, out hit1, 1.5f) && HeldObj == null) 
            { 
                var obj = hit1.collider.gameObject; 
                if (obj.CompareTag("Container")) 
                { 
                    HeldObj = obj; 
                    HeldObj.transform.parent = Spreader.transform; 
                    HeldObj.GetComponent<Rigidbody>().isKinematic = true; 
                    HeldObj.transform.rotation = Spreader.transform.rotation * Quaternion.Euler(0, 90, 0); 
                    HeldObj.transform.position = Spreader.transform.position - Spreader.transform.up * SpreaderUpOffset; 
                    LoadingSpot.LetTruckLeave();
                    
                    
                }
            }
            */
            RaycastHit hit1;
            if (HeldObj == null && Physics.Raycast(Spreader.transform.position - Spreader.transform.up * 0.2f, -Spreader.transform.up, out hit1, 1.5f))
            {
                var obj = hit1.collider.gameObject;
                if (obj.CompareTag("Container") || obj.CompareTag("PickedUpContainer")) 
                { 
                    HeldObj = obj; 
                    HeldObj.transform.parent = Spreader.transform; 
                    HeldObj.GetComponent<Rigidbody>().isKinematic = true; 
                    HeldObj.tag = "PickedUpContainer"; 
                    HeldObj.transform.rotation = Spreader.transform.rotation * Quaternion.Euler(0, 90, 0); 
                    HeldObj.transform.position = Spreader.transform.position - Spreader.transform.up * SpreaderUpOffset; 
                    LoadingSpot.LetTruckLeave();
                }
            }
            else 
            { 
                HeldObj.GetComponent<Rigidbody>().isKinematic = false; 
                HeldObj = null;
            }
        }
        if (Input.GetKeyDown(KeyCode.F) && HeldObj != null) 
        { 
            HeldObj.transform.parent = null; 
            HeldObj.GetComponent<Rigidbody>().isKinematic = false; 
            HeldObj = null;
        }

        if (Input.GetKey(KeyCode.C)) 
        { 
            main.enabled = !main.enabled; 
            foreach (var cam in targetCameras) 
            { 
                cam.enabled = !cam.enabled;
            }
            
            main.GetComponent<CameraController>().enabled = true; 
            anotherScript.enabled = true; 
            //ControlUI.active = false;
            ////cameraController.enabled = true;
            /// 
            KeyboardControl = false;
            CraneControlUI.SetActive(false); 
            ControlUI.SetActive(true);
        }
    }
}
