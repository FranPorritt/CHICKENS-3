using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

enum ChickenState
{
    Idle,
    Wandering,
    GoingToFood,
    Full,
    InCoop,
};

public class ChickenController : MonoBehaviour
{
    [SerializeField]
    private GameObject[] coops;
    private Gate gate;
    private GameObject feeder;

    private NavMeshAgent agent;
    private ChickenState chickenState = ChickenState.Wandering;

    [SerializeField]
    private GameObject closedMapExtents;
    [SerializeField]
    private GameObject openMapExtents;

    private const int MAX_HUNGER = 4; // How many crops chicken must eat to be full/lay egg
    private int currentHunger = 0;
    private int maxCoops = 0;

    private bool isGateOpen = false;
    private bool lastGateState = false;
    private bool inFencedArea = true;

    private void Awake()
    {
        Random.InitState(System.DateTime.Now.Millisecond); // Sets seed
        agent = this.GetComponent<NavMeshAgent>();
    }

    // Start is called before the first frame update
    void Start()
    {
        gate = GameObject.FindGameObjectWithTag("Gate").GetComponent<Gate>();
        feeder = GameObject.FindGameObjectWithTag("Feeder");
        maxCoops = coops.Length - 1; // Minus 1 so when choosing a random coop in Full() won't choose an element outside the array range.

        // Invoke only calls function once after delay, use InvokeRepeating to continously call function after delay
        Invoke("NeutralBehaviour", 0.5f);   // Switches between wander/idle
        Invoke("Hunger", 0.5f);             // Decreases hunger level
        RandomPosition();

        // Shows pos of coops
        for (int coopTest = 0; coopTest < coops.Length; coopTest++)
        {
            Debug.Log("COOP " + coopTest + ": " + coops[coopTest].transform.position);
        }
    }

    // Update is called once per frame
    void Update()
    {
        lastGateState = isGateOpen;
        isGateOpen = gate.GetGate();

        if ((feeder.GetComponent<Feeder>().GetCrops()) && (currentHunger < MAX_HUNGER) && (chickenState != ChickenState.Full)) // If feeder has crops in it AND chicken is hungry AND not going to a coop(full)
        {
            chickenState = ChickenState.GoingToFood;
        }
        else if (currentHunger == MAX_HUNGER)
        {
            chickenState = ChickenState.Full;
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

            case ChickenState.Full:
                Full();
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
            randomPosition = new Vector3(Random.Range(-openMapExtents.transform.position.x, openMapExtents.transform.position.x), 0f, Random.Range(-openMapExtents.transform.position.z, openMapExtents.transform.position.z));
        }
        else // gate is closed
        {
            if (inFencedArea)
            {
                randomPosition = new Vector3(Random.Range(-closedMapExtents.transform.position.x, closedMapExtents.transform.position.x), 0f, Random.Range(-closedMapExtents.transform.position.z, closedMapExtents.transform.position.z));
            }
            else // chicken can only roam outside fenced area
            {
                float randX = Random.Range(-openMapExtents.transform.position.x, openMapExtents.transform.position.x); // Finds random x pos within whole map
                float randZ = 0;

                if ((randX <= closedMapExtents.transform.position.x) || (randX >= -closedMapExtents.transform.position.x)) // If random x pos is within fenced area
                {
                    // Only use z pos outside of fenced area
                    int rand = Random.Range(0, 1);

                    if (rand == 0)
                    {
                        randZ = Random.Range(-openMapExtents.transform.position.z, -closedMapExtents.transform.position.z); // Back of map
                    }
                    else
                    {
                        randZ = Random.Range(closedMapExtents.transform.position.z, openMapExtents.transform.position.z); // Front of map (crops direction)
                    }

                    randomPosition = new Vector3(randX, 0f, randZ);
                }
                else
                {
                    randZ = Random.Range(-openMapExtents.transform.position.z, openMapExtents.transform.position.z);

                    randomPosition = new Vector3(randX, 0f, randZ);
                }
            }
        }

        agent.SetDestination(randomPosition);
    }

    private const int MAX_RANDOM_IDLE_TIME = 4;
    private void NeutralBehaviour() // Controls rate of wandering/idle switches
    {
        if (chickenState != ChickenState.Full)
        {
            float randomIdleTime = Random.Range(0, MAX_RANDOM_IDLE_TIME);

            switch (chickenState) // Only switches between Idle and Wandering
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
    }

    private const int MIN_RANDOM_HUNGER_TIME = 7;   // Seconds
    private const int MAX_RANDOM_HUNGER_TIME = 15;
    private void Hunger()
    {
        if (chickenState != ChickenState.Full) // Only takes hunger when not full so chickens don't immediately run to food after laying egg
        {
            float randomHungerTime = Random.Range(MIN_RANDOM_HUNGER_TIME, MAX_RANDOM_HUNGER_TIME);

            if (currentHunger > 0)
            {
                currentHunger--;
                Debug.Log("Chicken is hungry!");
            }
            // WHAT HAPPENS WHEN REACHES 0, SHOULD MIN BE HIGHER SO THEY DON'T STARVE TO QUICK / HIGHER HUNGER (10 MAX RATHER THAN 4?)

            Invoke("Hunger", randomHungerTime);
        }
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

    //// Eating crops code ////
    private float startTime = 0f;
    private const float EAT_TIME = 1.0f;

    public void EatStart()
    {
        startTime = Time.time;
    }

    public bool Eat()
    {
        bool didEat = false;

        if ((currentHunger == MAX_HUNGER)) // Chicken is full 
        {
            chickenState = ChickenState.Full;
        }
        else if (!feeder.GetComponent<Feeder>().GetCrops()) // No crops left but still hungry
        {
            chickenState = ChickenState.Wandering;
            didEat = false;
        }
        else if ((currentHunger < MAX_HUNGER) && (Time.time >= startTime + EAT_TIME)) // Chicken is still hungry AND enough time has passed (eat rate)
        {
            currentHunger++;
            didEat = true;
            startTime = Time.time; // Resets timer so chicken can eat again
            Debug.Log("Chicken Hunger: " + currentHunger);
        }

        return didEat;
    }
    ///////////////////////////

    private const float EGG_LAY_TIME = 2.0f;
    private bool hasCoopPos = false;
    public bool inCoop = false; // Passed to coop to trigger egg
    private void Full() // Sets chickens target position to location of a random coop
    {
        inCoop = true; // Lets coop know it's laying an egg and not randomly hitting it

        if (!hasCoopPos) // Prevents selecting a new coop before reaching target
        {
            Debug.Log("Chicken going to coop");
            int randomCoop = Random.Range(0, maxCoops);
            GameObject targetCoop = coops[randomCoop];
            agent.SetDestination(targetCoop.transform.position);
            hasCoopPos = true;
        }
        else if (agent.remainingDistance <= 0.1)
        {
            Debug.Log("EGG LAID!");

            Invoke("InCoop", EGG_LAY_TIME); // Calls in coop after 2 secs, ie. makes chicken wait in the coop for 2 seconds
        }
    }

    private void InCoop()
    {
        chickenState = ChickenState.Wandering;
        RandomPosition();
        inCoop = false;
    }
}
