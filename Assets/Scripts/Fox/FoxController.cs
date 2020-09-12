using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// If gate is open - foxes calculate if they are closer to the fox hole or gate and if gate set target to closest chicken

enum FoxState
{
    Idle,
    Teleporting,
    Outside,
    Hunting,
    Fleeing,
};

public class FoxController : MonoBehaviour
{
    private PlayerController player;
    private GameController gameController;

    private Rigidbody rb;

    [Header("Fox Holes")]
    [SerializeField]
    private GameObject[] foxHolesInside;
    [SerializeField]
    private GameObject[] foxHolesOutside;

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
        rb = this.GetComponent<Rigidbody>();
        agent.speed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        if ((inArea) && (foxState == FoxState.Idle))
        {
            foxState = FoxState.Hunting;
        }
        else if ((!inArea) && (foxState != FoxState.Teleporting))
        {
            foxState = FoxState.Outside;
        }

        switch (foxState)
        {
            case FoxState.Idle:
                break;

            case FoxState.Teleporting:
                Teleport();
                break;

            case FoxState.Outside:
                agent.speed = speed;
                FindHole();
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

        if (collision.gameObject.CompareTag("FoxHoleOutside"))
        {   
            for (int holeIndex = 0; holeIndex < foxHolesOutside.Length; holeIndex++)
            {
                if (collision.gameObject == foxHolesOutside[holeIndex]) // Finds which hole in the array
                {
                    currentHoleTarget = foxHolesInside[holeIndex];
                    foxState = FoxState.Teleporting;
                }
            }
        }
    }

    private void Teleport()
    {
        agent.enabled = false;
        transform.position = new Vector3(currentHoleTarget.transform.position.x, currentHoleTarget.transform.position.y + 0.5f, currentHoleTarget.transform.position.z); // Sets fox position to matching hole inside fenced area

        if (inArea) // Checks fox has teleported
        {
            agent.enabled = true;
            foxState = FoxState.Idle;
        }
    }

    private void FindHole()
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

    private void Eat(GameObject chicken)
    {
        hasTarget = false;
        chicken.GetComponent<ChickenController>().Kill(); // Disables chicken obj and removes it from chickenList in gameController
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

            hasTarget = true;
        }
        agent.SetDestination(chickenTarget.transform.position);
    }

    private void Flee()
    {
        Vector3 dirToFlee = transform.position - player.transform.position;
        Vector3 fleePos = transform.position + dirToFlee;

        agent.SetDestination(fleePos);
    }
}
