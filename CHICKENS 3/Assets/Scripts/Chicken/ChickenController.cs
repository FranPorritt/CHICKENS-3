using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

enum ChickenState
{
    Idle,
    Wandering,
    GoingToFood,
};

public class ChickenController : MonoBehaviour
{
    private Gate gate;
    private GameObject feeder;

    private NavMeshAgent agent;
    private ChickenState chickenState = ChickenState.Wandering;

    private float closedMapExtentX = 0f;
    private float closedMapExtentZ = 0f;
    private float openMapExtentX = 0f;
    private float openMapExtentZ = 0f;

    private const int MAX_HUNGER = 4; // How many crops chicken must eat to be full/lay egg
    private int currentHunger = 0;

    private bool isFull = false;

    private bool isGateOpen = false;
    private bool lastGateState = false;
    private bool inFencedArea = true;

    private void Awake()
    {
        agent = this.GetComponent<NavMeshAgent>();
    }

    // Start is called before the first frame update
    void Start()
    {
        gate = GameObject.FindGameObjectWithTag("Gate").GetComponent<Gate>();
        feeder = GameObject.FindGameObjectWithTag("Feeder");

        closedMapExtentX = GameObject.FindGameObjectWithTag("gateClosed").transform.position.x;
        closedMapExtentZ = GameObject.FindGameObjectWithTag("gateClosed").transform.position.z;
        openMapExtentX = GameObject.FindGameObjectWithTag("gateOpen").transform.position.x;
        openMapExtentZ = GameObject.FindGameObjectWithTag("gateOpen").transform.position.z;

        Invoke("NeutralBehaviour", 0.5f);   // Switches between wander/idle
        Invoke("Hunger", 0.5f);             // Decreases hunger level
        RandomPosition();
    }

    // Update is called once per frame
    void Update()
    {
        lastGateState = isGateOpen;
        isGateOpen = gate.GetGate();

        if ((feeder.GetComponent<Feeder>().GetCrops()) && (currentHunger < MAX_HUNGER)) // If feeder has crops in it AND chicken is hungry
        {
            chickenState = ChickenState.GoingToFood;
        }

        switch (chickenState)
        {
            case ChickenState.Idle:
                agent.isStopped = true;
                break;

            case ChickenState.Wandering:
                agent.isStopped = false;
                Wandering();
                break;

            case ChickenState.GoingToFood:
                agent.isStopped = false;
                GoingToFood();
                break;

            default:
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("fencedArea"))
        {
            inFencedArea = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("fencedArea"))
        {
            inFencedArea = false;
        }
    }

    private void RandomPosition()
    {
        Vector3 randomPosition = new Vector3(0, 0, 0);

        if (isGateOpen) // chicken can roam whole map
        {
            randomPosition = new Vector3(Random.Range(-openMapExtentX, openMapExtentX), 0f, Random.Range(-openMapExtentZ, openMapExtentZ));
        }
        else // gate is closed
        {
            if (inFencedArea)
            {
                randomPosition = new Vector3(Random.Range(-closedMapExtentX, closedMapExtentX), 0f, Random.Range(-closedMapExtentZ, closedMapExtentZ));
            }
            else // chicken can only roam outside fenced area
            {
                float randX = Random.Range(-openMapExtentX, openMapExtentX); // Finds random x pos within whole map
                float randZ = 0;

                if ((randX <= closedMapExtentX) || (randX >= -closedMapExtentX)) // If random x pos is within fenced area
                {
                    // Only use z pos outside of fenced area
                    int rand = Random.Range(0, 1);

                    if (rand == 0)
                    {
                        randZ = Random.Range(-openMapExtentZ, -closedMapExtentZ); // Back of map
                    }
                    else
                    {
                        randZ = Random.Range(closedMapExtentZ, openMapExtentZ); // Front of map (crops direction)
                    }

                    randomPosition = new Vector3(randX, 0f, randZ);
                }
                else
                {
                    randZ = Random.Range(-openMapExtentZ, openMapExtentZ);

                    randomPosition = new Vector3(randX, 0f, randZ);
                }
            }
        }

        agent.SetDestination(randomPosition);
    }

    private const int MAX_RANDOM_IDLE_TIME = 4;
    private void NeutralBehaviour() // Controls rate of wandering/idle switches
    {
        float randomIdleTime = Random.Range(0, MAX_RANDOM_IDLE_TIME);

        switch (chickenState)
        {
            case ChickenState.Idle:
                chickenState = ChickenState.Wandering;
                break;

            case ChickenState.Wandering:
                chickenState = ChickenState.Idle;
                break;

            default:
                break;
        }

        Invoke("NeutralBehaviour", randomIdleTime);
    }

    private const int MIN_RANDOM_HUNGER_TIME = 7;
    private const int MAX_RANDOM_HUNGER_TIME = 15;
    private void Hunger()
    {
        float randomHungerTime = Random.Range(MIN_RANDOM_HUNGER_TIME, MAX_RANDOM_HUNGER_TIME);

        if (currentHunger > 0)
        {
            currentHunger--;
            Debug.Log("Chicken is hungry!");
        }

        Invoke("Hunger", randomHungerTime);
    }

    private void Wandering()
    {
        if ((agent.remainingDistance < 0.5) || (lastGateState != isGateOpen))// Gate has been closed/opened since pos was last set -- may affect ability to reach pos
        {
            RandomPosition();
        }
    }

    private void GoingToFood()
    {
        Vector3 newPos = new Vector3(feeder.transform.position.x, 0f, feeder.transform.position.z + 2f);
        agent.SetDestination(newPos);
    }

    private float startTime = 0f;
    private const float EAT_TIME = 1.0f;

    private bool beenDone = false;
    public void EatStart()
    {
        beenDone = false; // Chicken just entered trigger -- reset checks
        startTime = Time.time;
    }

    public bool Eat()
    {
        bool didEat = false;

        if ((currentHunger == MAX_HUNGER)) // Chicken is full 
        {
            if (!beenDone)
            {
                chickenState = ChickenState.Wandering; // TEMPORARY -- NEEDS TO BE LAY EGG
                didEat = false;
                RandomPosition();
                beenDone = true;
            }
        }
        else if (!feeder.GetComponent<Feeder>().GetCrops()) // No crops left but still hungry
        {
            if (!beenDone)
            {
                chickenState = ChickenState.Wandering; 
                didEat = false;
                RandomPosition();
                beenDone = true;
            }
        }
        else if ((currentHunger < MAX_HUNGER) && (Time.time >= startTime + EAT_TIME)) // Chicken is hungry AND enough time has passed
        {
            currentHunger++;
            didEat = true;
            startTime = Time.time; // Resets timer so chicken can eat again
            Debug.Log("Chicken Hunger: " + currentHunger);
        }

        return didEat;
    }
}
