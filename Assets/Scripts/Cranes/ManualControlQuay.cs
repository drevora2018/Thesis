using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;

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
    private bool stop1, stop2, stop3, stop4 = false;
    // private Vector3 min;
    // private Vector3 max;

    public float speed = 3f;
    private float RopeSpeed = 0.052f;

    public TruckDropOff LoadingSpot;
    private GameObject TruckToRemove = null;

    public GameObject HighLightPreFab;
    private GameObject highlightedTempObject;

    /// <summary>
    /// Thesis-Specific variables
    /// </summary>
    public AccuracyFinder AccuracyFinder;
    private List<float> AccuracyList = new List<float>();
    bool startTask = false;

    Stopwatch stopwatch = new Stopwatch();


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
        AccuracyFinder = GameObject.FindGameObjectWithTag("Scripts").GetComponent<AccuracyFinder>();

        foreach (var cam in targetCameras)
        {
            cam.enabled = true;
        }
        GetComponent<ManualControlQuay>().KeyboardControl = true;
        InitialCranePosition = transform.position.z;
        InitialTrolleyPosition = Trolley.transform.position.x;
        InitialSpreaderPosition = Spreader.transform.position.y;

        //ControlUI = GameObject.FindGameObjectWithTag("PauseCanvas").transform.GetChild(0).gameObject;
        //CraneControlUI = GameObject.FindGameObjectWithTag("PauseCanvas").transform.GetChild(3).gameObject;

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
            print("Hey, we're touching something!");

            if (other.gameObject.name == "left")
            {
                stop1 = true;
            }
            if (other.gameObject.name == "right")
            {
                stop2 = true;
            }
            //if (other.gameObject.name == "forward")
            //{
            //    stop3 = true;
            //}
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
            //if (other.gameObject.name == "forward")
            //{
            //    stop3 = false;
            //}
        }
    }



    // Update is called once per frame
    void Update()
    {
        
        if (startTask) { stopwatch.Start(); }

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
            {
                RayCastBoundaries(true);

                if (!stop3) Trolley.transform.position += transform.forward * Time.deltaTime * speed;
            }

            if (Input.GetKey(KeyCode.S))
            {
                if (InitialTrolleyPosition < Trolley.transform.position.x)
                {   // S key is being pressed

                    RayCastBoundaries(false);

                    if (!stop4) Trolley.transform.position += -transform.forward * Time.deltaTime * speed;
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
                RayCastBoundaries(true);
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
                //TruckToRemove = hit1.transform.parent.parent.gameObject;
                var obj = hit1.collider.gameObject;
                if (obj.CompareTag("Container") || obj.CompareTag("PickedUpContainer")) 
                {
                    startTask = true;
                    HeldObj = obj; 
                    HeldObj.transform.parent = Spreader.transform;
                    HeldObj.GetComponent<Rigidbody>().isKinematic = true; 
                    HeldObj.tag = "PickedUpContainer"; 
                    HeldObj.transform.rotation = Spreader.transform.rotation * Quaternion.Euler(0, 90, 0); 
                    HeldObj.transform.position = Spreader.transform.position - Spreader.transform.up * SpreaderUpOffset;
                    var HighLightLocation = GameObject.FindGameObjectWithTag("Ship").GetComponentInChildren<ContainerYardScript>().AskPlace();
                    highlightedTempObject = Instantiate(HighLightPreFab, (Vector3)HighLightLocation, Quaternion.identity);
                }
            }
            else 
            {   
                print($"HeldObj: {HeldObj}");
                HeldObj.GetComponent<Rigidbody>().isKinematic = false; 
                HeldObj = null;
            }
        }
        if (Input.GetKeyDown(KeyCode.F) && HeldObj != null) 
        {
            
            HeldObj.transform.SetParent(null);
            HeldObj.transform.parent = null; 
            HeldObj.GetComponent<Rigidbody>().isKinematic = false;
            var Accuracy = Vector3.Distance(HeldObj.transform.position, highlightedTempObject.transform.position);
            Destroy(highlightedTempObject);
            highlightedTempObject = null;

            AccuracyList.Add(Accuracy);
            print($"Added to AccuracyList: {Accuracy}");
            if (AccuracyList.Count == AccuracyFinder.TargetContainers)
            {
                startTask = false;
                stopwatch.Stop();
                AccuracyFinder.WriteDataToFile(AccuracyList, stopwatch.Elapsed);
            }

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

    /// <summary>
    /// RayCasting for trolley movement forward or backward. Set the bool to true to look for forward, and false for backward.
    /// Sets Stop3 for forward movement, and Stop4 for backward moment to true if we are trying to go out of bounds.
    /// </summary>
    /// <param name="forwardorback"></param>
    private void RayCastBoundaries(bool forwardorback)
    {
        if (!forwardorback)
        {
            RaycastHit BackRayHit;
            //Debug.DrawRay(Trolley.transform.position, new Vector3(1,0,0)* 10, Color.blue);
            if (Physics.Raycast(Trolley.transform.position, new Vector3(-1, 0, 0), out BackRayHit, 1.5f))
            {
                if (BackRayHit.collider.CompareTag("LimitCrane"))
                {
                    stop4 = true;
                }
            }
            else { stop4 = false; }
        }
        else
        {
            RaycastHit BackRayHit;
            //Debug.DrawRay(Trolley.transform.position, new Vector3(1,0,0)* 10, Color.blue);
            if (Physics.Raycast(Trolley.transform.position, new Vector3(1, 0, 0), out BackRayHit, 1.5f))
            {
                if (BackRayHit.collider.CompareTag("LimitCrane"))
                {
                    stop3 = true;
                }
            }
            else { stop3 = false; }
        }
        
    }
}
