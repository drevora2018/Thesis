using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class ManualControl : MonoBehaviour
{
    public GantryCraneMovement anotherScript;

    public GameObject Trolley;
    public GameObject Spreader;
    public GameObject Ropes;
    public GameObject Limit;
    private GameObject ControlUI;
    private GameObject CraneControlUI;

    public bool ControlEnabled = false;

    public Camera targetCamera;
    public Camera main;

    float SpreaderUpOffset = 3.7f;
    private GameObject HeldObj;
    private float InitialCranePosition;
    private float InitialTrolleyPosition;
    private float InitialSpreaderPosition;
    private bool stop1,stop2,stop3,stop4 = false;
    public bool stop1Ghost, stop2Ghost, stop3Ghost, stop4Ghost = false;

    public float speed = 3f;
    private float RopeSpeed = 0.052f;

    public GameObject StartingContainersPrefab;
    public GameObject CurrentContainers;
    
    KeyCode curKey = KeyCode.None;
    private KeyCode CurKey {  
        get { return curKey; }
        set
        {
            if (value == KeyCode.None && curKey != value) 
            {
                Invoke(nameof(ResyncGhost), latency / 1000f + 0.1f);
            }
            curKey = value;
        }
    }

    KeyCode curGhostKey = KeyCode.None;

    private KeyCode[] keyInputs = { 
        KeyCode.UpArrow, 
        KeyCode.DownArrow, 
        KeyCode.RightArrow, 
        KeyCode.LeftArrow, 
        KeyCode.W, 
        KeyCode.S
    };
    public int latency = 500; //latency in milliseconds
    public RectTransform MapOverlay;
    public float MapOverlaySpeed = 29f;
    public Targets targets;

    public bool GhostOverlayActive = false;
    public GameObject GhostSpreader;
    public MeshRenderer GhostContainer;

    public Image[] arrows; //up, down, left, right, forward, back
    public Color arrowBaseColor = Color.white;
    public Color arrowActiveColor = Color.green;
    public bool ArrowOverlayActive = false;

    public TextMeshProUGUI latencyLabel;
    public TruckDropOff LoadingSpot;


    void Awake()
    {
        main.enabled = false;
        targetCamera.enabled = true;
        InitialCranePosition = transform.position.z;
        InitialTrolleyPosition = Trolley.transform.position.x;
        InitialSpreaderPosition = Spreader.transform.position.y;

        InitializeScenario();

        ControlUI = GameObject.FindGameObjectWithTag("PauseCanvas").transform.GetChild(0).gameObject;
        CraneControlUI = GameObject.FindGameObjectWithTag("PauseCanvas").transform.GetChild(3).gameObject;
        EnableControl();

        GhostSpreader.SetActive(GhostOverlayActive);
        foreach (var arrow in arrows)
        {
            arrow.color = arrowBaseColor;
            arrow.enabled = ArrowOverlayActive;
        }
        latencyLabel.enabled = GhostOverlayActive || ArrowOverlayActive;
        latencyLabel.text = latency.ToString() + " ms";
        latencyLabel.transform.parent.gameObject.SetActive(GhostOverlayActive || ArrowOverlayActive);

        ResyncGhost();
    }
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit3;
            UnityEngine.Debug.Log("click\n");

            if (Physics.Raycast(ray, out hit3))
            {
                if (hit3.collider.tag == "Crane")
                {
                    hit3.collider.gameObject.GetComponent<ManualControl>().EnableControl();
                }
            }
        }

        if (ControlEnabled)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PickUpDrop();

                return;
            }
            else if (Input.GetKeyDown(KeyCode.O))
            {
                //send current truck away
                LoadingSpot.LetTruckLeave();
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                //reset containers if collisions have messed up the grid
                ResetContainers();
            }
            else if (Input.anyKey)
            {
                foreach (var key in keyInputs)
                {
                    if (Input.GetKeyDown(key))
                    {
                        UpdateKey(key);
                        return;
                    }
                }

                if (Input.GetKey(CurKey))
                {
                    return;
                }

                foreach (var key in keyInputs)
                {
                    if (Input.GetKey(key))
                    {
                        UpdateKey(key);
                        return;
                    }
                }
            }
            else
            {
                UpdateKey(KeyCode.None);
            }
        }
    }
    
    void FixedUpdate()
    {
        Move();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "LimitCrane")
        {
            if (other.gameObject.name=="left")
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
            if (other.gameObject.name == "back")
            {
                stop4 = true;
            }
        }

    }

    void OnTriggerExit(Collider other)
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
            if (other.gameObject.name == "back")
            {
                stop4 = false;
            }
        }

    }
    
    public void DisableScript()
    {
        enabled = false;
    }

    void EnableControl()
    {
        this.ControlEnabled = true;
        CraneControlUI.SetActive(true);
        ControlUI.SetActive(false);
        main.GetComponent<CameraController>().enabled = false;
        this.main.enabled = false;
        this.targetCamera.enabled = true;
        this.anotherScript.AutoControlActive = false;
    }

    public void InitializeScenario()
    {
        var scenario = PlayerPrefs.GetInt("Scenario");
        GhostOverlayActive = false;
        ArrowOverlayActive = false;

        switch (scenario)
        {
            case 1: latency = 0;
                break;
            case 2: latency = 500;
                break;
            case 3: latency = 800;
                break;
            case 4:
                GhostOverlayActive = true;
                latency = 0;
                break;
            case 5:
                GhostOverlayActive = true;
                latency = 500;
                break;
            case 6:
                GhostOverlayActive = true;
                latency = 800;
                break;
            case 7:
                ArrowOverlayActive = true;
                latency = 0;
                break;
            case 8:
                ArrowOverlayActive = true;
                latency = 500;
                break;
            case 9:
                ArrowOverlayActive = true;
                latency = 800;
                break;
            default:
                latency = 0;
                break;
        }
    }
    
    void Move() 
    {
        var dist = Time.fixedDeltaTime * speed;
        var mapSpeed = Time.fixedDeltaTime * MapOverlaySpeed;

        if (GhostOverlayActive)
        {
            MoveGhost(dist);
        }

        bool moveAllowed = false;

        RaycastHit hit;
        switch (CurKey)
        {
            case KeyCode.UpArrow:
                if (!stop3)
                {
                    Trolley.transform.position += transform.forward * dist;
                    moveAllowed = true;
                }
                break;
            case KeyCode.DownArrow:
                if (!stop4)
                {
                    Trolley.transform.position += -transform.forward * dist;
                    moveAllowed = true;
                }
                break;
            case KeyCode.LeftArrow:
                if (!stop1)
                {
                    transform.position += -transform.right * dist;
                    MapOverlay.position += -MapOverlay.right * mapSpeed;
                    moveAllowed = true;
                }
                break;
            case KeyCode.RightArrow:
                if (!stop2)
                {
                    transform.position += transform.right * dist;
                    MapOverlay.position += MapOverlay.right * mapSpeed;
                    moveAllowed = true;
                }
                break;
            case KeyCode.W:
                if (InitialSpreaderPosition > Spreader.transform.position.y)
                {
                    Spreader.transform.position += transform.up * dist;
                    Ropes.transform.localScale += RopeSpeed * Spreader.transform.up * dist;
                    moveAllowed = true;
                }
                break;
            case KeyCode.S:
                if (!Physics.Raycast(Spreader.transform.position - Spreader.transform.up*0.2f, -Spreader.transform.up, out hit, 0.8f) ||
                (HeldObj != null && !Physics.Raycast(new(Spreader.transform.position.x, Spreader.transform.position.y - HeldObj.GetComponent<BoxCollider>().size.y, Spreader.transform.position.z), -Spreader.transform.up, out hit, 0.8f)) || hit.collider.isTrigger || hit.collider.gameObject.CompareTag("CraneSpreader"))
                {
                    Spreader.transform.position += -transform.up * dist;
                    Ropes.transform.localScale += -RopeSpeed * Spreader.transform.up * dist;
                    moveAllowed = true;
                }
                break;
            default:
                break;
        }

        if (moveAllowed || CurKey == KeyCode.None)
        {
            UpdateArrows(CurKey);
        }
    }

    void MoveGhost(float dist)
    {
        if (!GhostOverlayActive)
        {
            return; 
        }

        RaycastHit hit;
        switch (curGhostKey)
        {
            case KeyCode.UpArrow:
                if (!stop3Ghost)
                {
                    GhostSpreader.transform.position += GhostSpreader.transform.forward * dist;
                }
                break;
            case KeyCode.DownArrow:
                if (!stop4Ghost)
                {
                    GhostSpreader.transform.position += -GhostSpreader.transform.forward * dist;
                }
                break;
            case KeyCode.LeftArrow:
                if (!stop1Ghost)
                {
                    GhostSpreader.transform.position += -GhostSpreader.transform.right * dist;
                    //MapOverlay.position += -MapOverlay.right * Time.deltaTime * MapOverlaySpeed;
                }
                break;
            case KeyCode.RightArrow:
                if (!stop2Ghost)
                {
                    GhostSpreader.transform.position += GhostSpreader.transform.right * dist;
                    //MapOverlay.position += MapOverlay.right * Time.deltaTime * MapOverlaySpeed;
                }
                break;
            case KeyCode.W:
                if (InitialSpreaderPosition > GhostSpreader.transform.position.y)
                {
                    GhostSpreader.transform.position += GhostSpreader.transform.up * dist;
                    //Ropes.transform.localScale += RopeSpeed * speed * Time.deltaTime * Spreader.transform.up;
                }
                break;
            case KeyCode.S:
                if (!Physics.Raycast(GhostSpreader.transform.position, -GhostSpreader.transform.up, out hit, 0.8f) ||
                (HeldObj != null && !Physics.Raycast(new(GhostSpreader.transform.position.x, GhostSpreader.transform.position.y - HeldObj.GetComponent<BoxCollider>().size.y, GhostSpreader.transform.position.z), -GhostSpreader.transform.up, out hit, 0.8f)) || hit.collider.isTrigger || hit.collider.gameObject.CompareTag("CraneSpreader"))
                {
                    GhostSpreader.transform.position += -GhostSpreader.transform.up * dist;
                    //Ropes.transform.localScale += -RopeSpeed * speed * Time.deltaTime * Spreader.transform.up;
                }
                break;
            default:
                break;
        }
    }
    
    async void PickUpDrop()
    {
        await Task.Delay(latency); 

        RaycastHit hit1;
        if (HeldObj == null && Physics.Raycast(Spreader.transform.position - Spreader.transform.up*0.2f, -Spreader.transform.up, out hit1, 1.5f))
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
                GhostContainer.enabled = true;
            }
        }
        else
        {
            HeldObj.transform.parent = CurrentContainers.transform;
            HeldObj.GetComponent<Rigidbody>().isKinematic = false;
            HeldObj = null;
            GhostContainer.enabled = false;
        }

    }
   
    void ResetContainers()
    {
        var temp = CurrentContainers;
        CurrentContainers = Instantiate(StartingContainersPrefab, temp.transform.position, temp.transform.rotation);
        Destroy(temp);
    }

    void ResyncGhost()
    {
        if (GhostOverlayActive && GhostSpreader != null && CurKey == KeyCode.None && curGhostKey == KeyCode.None) 
        {
            GhostSpreader.transform.position = Spreader.transform.position;
        }
    }

    void UpdateArrows(KeyCode key)
    {
        if (!ArrowOverlayActive)
        {
            return;
        }

        foreach (var arrow in arrows)
        {
            arrow.color = arrowBaseColor;
        }

        switch (key)
        {
            case KeyCode.UpArrow:
                arrows[4].color = arrowActiveColor;
                break;
            case KeyCode.DownArrow:
                arrows[5].color = arrowActiveColor;
                break;
            case KeyCode.LeftArrow:
                arrows[2].color = arrowActiveColor;
                break;
            case KeyCode.RightArrow:
                arrows[3].color = arrowActiveColor;                
                break;
            case KeyCode.W:
                arrows[0].color = arrowActiveColor;                
                break;
            case KeyCode.S:
                arrows[1].color = arrowActiveColor;
                break;
            default:
                break;
        }
    }
    
    async void UpdateKey(KeyCode key)
    {
        curGhostKey = key;
        if (key != KeyCode.None)
        {
            CancelInvoke(nameof(ResyncGhost));
        }
        await Task.Delay(latency);
        CurKey = key;
    }

}
