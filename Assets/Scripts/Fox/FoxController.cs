using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// If gate is open - foxes calculate if they are closer to the fox hole or gate and if gate set target to closest chicken

enum FoxState
{
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

    private List<GameObject> chickenList;
    private GameObject chickenTarget;
    private bool hasTarget = false;

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
    }

    // Update is called once per frame
    void Update()
    {
        switch (foxState)
        {
            case FoxState.Outside:
                FindHole();
                break;

            case FoxState.Hunting:
                Hunt();
                break;

            case FoxState.Fleeing:
                Flee();
                hasTarget = false;
                break;

            default:
                break;
        }
    }

    private void OnTriggerEnter(Collider other) // should be collision changed to trigger for debugging
    {
        if (other.CompareTag("FoxHoleOutside"))
        {
            for (int holeIndex = 0; holeIndex < 1; holeIndex++)
            {
                if (other == foxHolesOutside[holeIndex]) // Finds which hole in the array
                {
                    transform.position = foxHolesInside[holeIndex].transform.position; // Sets fox position to matching hole inside fenced area
                }
            }
        }

        if (other.CompareTag("Player"))
        {
            foxState = FoxState.Fleeing;
        }
        else if (other.CompareTag("fencedArea"))
        {
            foxState = FoxState.Hunting;
        }
    }

    void FindHole()
    {
        float smallestDistance = 0;
        int closestHole = 0;

        for (int distanceIndex = 0; distanceIndex < 1; distanceIndex++) // Magic num
        {
            float distance = Vector3.Distance(transform.position, foxHolesOutside[distanceIndex].transform.position);

            if (distanceIndex == 0) // If it's the first loop meaning smallest distance DOES NOT have a value yet. Will always run the first loop
            {
                smallestDistance = distance;
                closestHole = distanceIndex; // Stores pos within array
            }
            else if (distance < smallestDistance) // If it's not the first loop so smallest distance DOES has a value. Only runs if the new distance is smaller
            {
                smallestDistance = distance;
                closestHole = distanceIndex;
            }            
        }

        agent.SetDestination(foxHolesOutside[closestHole].transform.position);
    }

    private void Hunt()
    {
        if (!hasTarget) // Doesn't have a chicken yet
        {
            // Find closest chicken //

            float closestDistance = 0;
            chickenList = gameController.GetChickenList(); // Gets each time state switches to hunting in case a chicken has been ate

            foreach(GameObject chick in chickenList)
            {
                float distance = Vector3.Distance(transform.position, chick.transform.position);

                if (closestDistance == 0) // If it's the first loop meaning closest distance DOES NOT have a value yet. Will always run the first loop
                {
                    closestDistance = distance;
                    chickenTarget = chick;
                }
                else if (distance < closestDistance) // If it's not the first loop so closest distance DOES has a value. Only runs if the new distance is smaller
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
