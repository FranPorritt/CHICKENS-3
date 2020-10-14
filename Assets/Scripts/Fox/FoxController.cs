using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// If gate is open - foxes calculate if they are closer to the fox hole or gate and if gate set target to closest chicken

enum FoxState
{
    Idle,
    Outside,        // Trying to get in fenced area
    Hunting,        // Chasing a chicken
    Fleeing,        // Running from player
    Leaving,        // Leaving fenced area
};

public class FoxController : MonoBehaviour
{
    private PlayerController player;
    private GameController gameController;
    private GameObject fox;

    private Rigidbody rb;

    [Header("Fox Holes")]
    [SerializeField]
    private GameObject[] foxHolesInside;
    [SerializeField]
    private GameObject[] foxHolesOutside;

    private GameObject[] entryPoints;
    private bool hasLeavingTarget = false;

    private NavMeshAgent agent;
    private FoxState foxState = FoxState.Outside;
    
    [Header("Speed")]
    [SerializeField]
    private float speed = 5.0f;
    [SerializeField]
    private float fleeSpeed = 7.0f;

    private List<GameObject> chickenList;
    private GameObject chickenTarget;
    private bool hasTarget = false;

    private GameObject currentHoleTarget;
    private bool inArea = false;

    // Start is called before the first frame update
    void Awake()
    {
        agent = this.GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
    }

    private void Start()
    {
        fox = this.gameObject;
        rb = this.GetComponent<Rigidbody>();
        entryPoints = gameController.GetEntryPoints();
        agent.speed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        if ((inArea) && (foxState == FoxState.Idle))
        {
            foxState = FoxState.Hunting;
        }

        switch (foxState)
        {
            case FoxState.Idle:
                break;

            case FoxState.Outside:
                agent.speed = speed;
                FindEntryHole();
                break;

            case FoxState.Hunting:
                agent.speed = speed;
                Hunt();
                break;

            case FoxState.Fleeing:
                agent.speed = fleeSpeed;
                hasTarget = false;
                Flee();
                break;

            case FoxState.Leaving:
                agent.speed = speed;
                Leave();
                break;

            default:
                break;
        }
    }

    private void OnTriggerEnter(Collider other) // should be collision changed to trigger for debugging
    {
        if (other.CompareTag("Player"))
        {
            foxState = FoxState.Fleeing;
        }
        else if (other.CompareTag("fencedArea"))
        {
            inArea = true;
        }
        else if (other.CompareTag("Boundary"))
        { // If hitting a boundary and trying to leave play area
            if (hasLeavingTarget)
            {
                Debug.Log("BOUNDARY");
                Destroy(fox);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            foxState = FoxState.Idle;
        }
        if (other.CompareTag("fencedArea"))
        {
            inArea = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Chicken"))
        {
            Eat(collision.gameObject);
        }       

        if ((collision.gameObject.CompareTag("FoxHoleOutside")) && (foxState != FoxState.Leaving))
        {   
            for (int holeIndex = 0; holeIndex < foxHolesOutside.Length; holeIndex++)
            {
                if (collision.gameObject == foxHolesOutside[holeIndex]) // Finds which hole in the array
                {
                    currentHoleTarget = foxHolesInside[holeIndex];
                    Teleport();
                    foxState = FoxState.Idle;
                }
            }
        }
        else if ((collision.gameObject.CompareTag("FoxHoleInside")) && (foxState == FoxState.Leaving))
        {
            for (int holeIndex = 0; holeIndex < foxHolesInside.Length; holeIndex++)
            {
                if (collision.gameObject == foxHolesInside[holeIndex]) // Finds which hole in the array
                {
                    currentHoleTarget = foxHolesOutside[holeIndex];
                    Teleport();
                    foxState = FoxState.Leaving;
                }
            }
        }
    }

    private void Teleport()
    {
        agent.enabled = false;
        transform.position = new Vector3(currentHoleTarget.transform.position.x, currentHoleTarget.transform.position.y + 1f, currentHoleTarget.transform.position.z); // Sets fox position to matching hole inside fenced area
        
        agent.enabled = true;
    }

    private void FindEntryHole()
    {
        float smallestDistance = Mathf.Infinity;
        
        foreach(GameObject hole in foxHolesOutside)
        {
            float distance = Vector3.Distance(transform.position, hole.transform.position);

            if (distance < smallestDistance)
            {
                smallestDistance = distance;
                currentHoleTarget = hole;
            }            
        }

        agent.SetDestination(currentHoleTarget.transform.position);
    }

    private void FindExitHole()
    {
        float smallestDistance = Mathf.Infinity;

        foreach (GameObject hole in foxHolesInside)
        {
            float distance = Vector3.Distance(transform.position, hole.transform.position);

            if (distance < smallestDistance)
            {
                smallestDistance = distance;
                currentHoleTarget = hole;
            }
        }

        agent.SetDestination(currentHoleTarget.transform.position);
    }

    private void Eat(GameObject chicken)
    {
        hasTarget = false;
        chicken.GetComponent<ChickenController>().Kill(); // Disables chicken obj and removes it from chickenList in gameController
        foxState = FoxState.Leaving;
    }

    private void Hunt()
    {
        if (!hasTarget) // Doesn't have a chicken yet
        {
            float closestDistance = Mathf.Infinity;
            chickenList = gameController.GetChickenList(); // Gets each time state switches to hunting in case a chicken has been ate

            foreach(GameObject chick in chickenList)
            {
                float distance = Vector3.Distance(transform.position, chick.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    chickenTarget = chick;
                }
            }
        }
        hasTarget = true;
        agent.SetDestination(chickenTarget.transform.position);
    }

    private void Flee()
    {
        Vector3 dirToFlee = transform.position - player.transform.position;
        Vector3 fleePos = transform.position + dirToFlee;

        agent.SetDestination(fleePos);
    }

    private void Leave()
    {
        if (inArea)
        {
            FindExitHole();
        }
        else if (!hasLeavingTarget) // Is heading to an entry/exit point outside boundary
        {
            float closestDistance = Mathf.Infinity;
            Vector3 targetPos = new Vector3(0,0,0);
            foreach (GameObject point in entryPoints)
            {
                float distance = Vector3.Distance(transform.position, point.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    targetPos = point.transform.position;
                }
            }
            hasLeavingTarget = true; // NEEDS RESETTING WHEN AT TARGET
            agent.SetDestination(targetPos);
        }
    }
}
