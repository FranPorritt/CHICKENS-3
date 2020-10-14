using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AI;

enum ChickenState
{
    Idle,
    Wandering,
    GoingToFood,
    Full,
    InCoop,
    Fleeing,
};

public class ChickenController : MonoBehaviour
{
    private PlayerController player;
    private GameController gameController;

    private GameObject[] coops;
    private Gate gate;
    private GameObject feeder;

    private NavMeshAgent agent;
    private ChickenState chickenState = ChickenState.Wandering;

    private GameObject fleeObject; // Stores object chicken is fleeing from
    [SerializeField]
    private int fleeSpeed = 7;
    [SerializeField]
    private int speed = 5;

    [Header("Map Extents")]
    [SerializeField]
    private Vector3 closedMapExtents;
    [SerializeField]
    private Vector3 openMapExtents;

    [SerializeField]
    private const int MAX_HUNGER = 6; // How many crops chicken must eat to be full/lay egg
    private int currentHunger = 0;
    private int maxCoops = 0;

    private bool isGateOpen = false;
    private bool lastGateState = false;
    private bool inFencedArea = true;

    private void Awake()
    {
        Random.InitState(System.DateTime.Now.Millisecond); // Sets seed
        agent = this.GetComponent<NavMeshAgent>();
        agent.speed = speed;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        gate = GameObject.FindGameObjectWithTag("Gate").GetComponent<Gate>();
        feeder = GameObject.FindGameObjectWithTag("Feeder");
        coops = gameController.GetCoops(); // Coop pos' stored in game controller due to instantiating chickens 
        maxCoops = coops.Length - 1; // Minus 1 so when choosing a random coop in Full() won't choose an element outside the array range.

        // Invoke only calls function once after delay, use InvokeRepeating to continously call function after delay
        Invoke("NeutralBehaviour", 0.5f);   // Switches between wander/idle
        Invoke("Hunger", 0.5f);             // Decreases hunger level
        RandomPosition();
    }

    // Update is called once per frame
    void Update()
    {
        CheckGateState(); // Checks if gate is open or closed

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

            case ChickenState.Fleeing:
                agent.isStopped = false;
                Flee();
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
        
        if ((other.CompareTag("Player")) || (other.CompareTag("Fox")))
        {
            chickenState = ChickenState.Fleeing;
            fleeObject = other.gameObject; // Tells chicken what it's running from
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("fencedArea"))
        {
            inFencedArea = false;
        }
        else if ((other.CompareTag("Player")) || (other.CompareTag("Fox")))
        {
            chickenState = ChickenState.Wandering;
            agent.speed = speed; // Resets speed from flee speed
        }
    }

    private void CheckGateState()
    {
        lastGateState = isGateOpen;
        isGateOpen = gate.GetGate();
    }

    private void RandomPosition()
    {
        Vector3 randomPosition = new Vector3(0, 0, 0);

        if (isGateOpen) // chicken can roam whole map
        {
            randomPosition = new Vector3(Random.Range(-openMapExtents.x, openMapExtents.x), 0f, Random.Range(-openMapExtents.z, openMapExtents.z));
        }
        else // gate is closed
        {
            if (inFencedArea)
            {
                randomPosition = new Vector3(Random.Range(-closedMapExtents.x, closedMapExtents.x), 0f, Random.Range(-closedMapExtents.z, closedMapExtents.z));
            }
            else // chicken can only roam outside fenced area
            {
                float randX = Random.Range(-openMapExtents.x, openMapExtents.x); // Finds random x pos within whole map
                float randZ = 0;

                if ((randX <= closedMapExtents.x) || (randX >= -closedMapExtents.x)) // If random x pos is within fenced area
                {
                    // Only use z pos outside of fenced area
                    int rand = Random.Range(0, 1);

                    if (rand == 0)
                    {
                        randZ = Random.Range(-openMapExtents.z, -closedMapExtents.z); // Back of map
                    }
                    else
                    {
                        randZ = Random.Range(closedMapExtents.z, openMapExtents.z); // Front of map (crops direction)
                    }

                    randomPosition = new Vector3(randX, 0f, randZ);
                }
                else
                {
                    randZ = Random.Range(-openMapExtents.z, openMapExtents.z);

                    randomPosition = new Vector3(randX, 0f, randZ);
                }
            }
        }
        agent.SetDestination(randomPosition);
    }

    [SerializeField]
    private const int MAX_RANDOM_IDLE_TIME = 4; // How long between switching idle/wandering
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

    // Range of time chicken loses hunger
    [Header("Hunger Time Range")]
    [SerializeField]
    private int MIN_RANDOM_HUNGER_TIME = 7;
    [SerializeField]
    private int MAX_RANDOM_HUNGER_TIME = 15;
    private void Hunger()
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

    // ~ Eating crops ~ //
    private float startTime = 0f;
    [SerializeField]
    private float EAT_TIME = 1.0f; // Time it takes the chicken to eat 1 crop

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
    // ~ Eating crops END ~ //

    // ~ Chicken Full and laying egg ~ //
    [SerializeField]
    private const float EGG_LAY_TIME = 2.0f; // Time it takes for chicken to lay egg (how long they're in the coop for)
    private bool hasCoopPos = false;
    [HideInInspector]
    public bool inCoop = false; // Passed to coop to trigger egg

    private void Full() // Sets chickens target position to location of a random coop
    {
        inCoop = true; // Lets coop know it's laying an egg and not randomly hitting it

        if (!hasCoopPos) // Prevents selecting a new coop before reaching target
        {
            int randomCoop = Random.Range(0, maxCoops);
            GameObject targetCoop = coops[randomCoop];
            agent.SetDestination(targetCoop.transform.position);
            hasCoopPos = true;
        }
        else if (agent.remainingDistance <= 0.1)
        {
            Invoke("InCoop", EGG_LAY_TIME); // Calls in coop after 2 secs, ie. makes chicken wait in the coop for 2 seconds
        }
    }

    private void InCoop()
    {
        chickenState = ChickenState.Wandering;
        RandomPosition();
        inCoop = false;
        hasCoopPos = false;
    }
    // ~ Chicken Full and laying egg END ~ //

    private void Flee()
    {
        Vector3 dirToFlee = transform.position - fleeObject.transform.position;
        Vector3 fleePos = transform.position + dirToFlee;

        agent.speed = fleeSpeed;
        agent.SetDestination(fleePos);
    }

    public void Kill()
    {
        gameController.ChickenDeath(this.gameObject);
        //Destroy(this);
        this.gameObject.SetActive(false);
    }
}
