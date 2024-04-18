using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/*Controls truck navigation and collision behavior. Should be attached to "Terminal Truck" in the prefab.*/

[RequireComponent(typeof(NavMeshAgent))]
public class TruckNavigation : MonoBehaviour
{
    //Assign in prefab editor
    public bool IsInternal = false;
    public float WayPointAccuracy = 5f;
    public float Deceleration = 12f;
    
    NodeGraph wpManager;
    List<WaypointNode> Path;
    int i = 0;

    [SerializeField]
    int CollisionCount = 0; //used to stay stopped until all detected collisions have left the "cone" in front
    NavMeshAgent navMeshAgent;
    float BaseSpeed, BaseAcceleration;

    public AttachDetachCont Trailer;

    private void Start()
    {
        Trailer = transform.parent.GetComponentInChildren<AttachDetachCont>();
        
        //ignore "self-collision" in tight turns
        foreach (Collider col in Trailer.GetComponents<Collider>())
            Physics.IgnoreCollision(transform.GetChild(0).GetComponent<MeshCollider>(), col);

        wpManager = FindObjectOfType<NodeGraph>();
        Path = wpManager.GetRandomPathFromSpawn(GetInstanceID(), IsInternal);
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.destination = Path[i].gameObject.transform.position;
        BaseSpeed = navMeshAgent.speed;
        BaseAcceleration = navMeshAgent.acceleration;
    }

    public void Stop(bool stopped)
    {
        navMeshAgent.isStopped = stopped;
        navMeshAgent.acceleration = stopped ? Deceleration : BaseAcceleration;
    }

    void FixedUpdate()
    {
        if (CalculateDistance(transform.position, navMeshAgent.destination) < WayPointAccuracy)
            GoToNextPoint();
    }

    float CalculateDistance(Vector3 Source, Vector3 Destination) => Mathf.Abs(Vector3.Distance(Source, Destination));

    public void SetSpeed(float speed)
    {
        navMeshAgent.speed = speed;
        navMeshAgent.acceleration = speed <= 0 ? Deceleration : BaseAcceleration;
    }

    void GoToNextPoint()
    {
        switch (Path[i].Type)
        {
            case WaypointNode.WaypointType.StorageCrane:
            case WaypointNode.WaypointType.ShipCrane:
                Stop(true);
                if (Trailer.HasContainer)
                    Path[i].gameObject.transform.GetComponent<TruckDropOff>().EnqueuePosition(true, Trailer.GetPosition());
                else
                    Path[i].gameObject.transform.GetComponent<TruckDropOff>().EnqueuePosition(false, Trailer.GetPosition());

                Trailer.ReleaseContainer(100);
                Path[i].gameObject.transform.GetComponent<TruckDropOff>().RegisterTruck(this);
                break;
            case WaypointNode.WaypointType.Despawn:
                int id = GetInstanceID();
                Path[i].GCost.Remove(id);
                Path[i].HCost.Remove(id);
                Path[i].HeapIndex.Remove(id);
                Destroy(gameObject.transform.parent.gameObject);
                return;
            default:
                break;
        }

        i++;

        if (i >= Path.Count)
        {
            Path = wpManager.GetRandomPath(GetInstanceID(), Path[^1], IsInternal);
            i = 0;
        }

        navMeshAgent.destination = Path[i].gameObject.transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("ExternalTruck") || other.gameObject.CompareTag("CraneSpreader") || other.gameObject.CompareTag("PickedUpContainer"))
        {
            CollisionCount++;
            SetSpeed(0);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("ExternalTruck") || other.gameObject.CompareTag("CraneSpreader") || other.gameObject.CompareTag("PickedUpContainer"))
        {
            CollisionCount = CollisionCount - 1 < 0 ? 0 : CollisionCount - 1;
            if(CollisionCount <= 0)
            {
                SetSpeed(BaseSpeed);
            }
        }
    }
}
