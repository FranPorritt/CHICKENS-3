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
    private Rigidbody rb;

    [Header("Fox Holes")]
    [SerializeField]
    private GameObject[] foxHolesInside;
    [SerializeField]
    private GameObject[] foxHolesOutside;

    private NavMeshAgent agent;
    private FoxState foxState = FoxState.Outside;

    // Start is called before the first frame update
    void Awake()
    {
        agent = this.GetComponent<NavMeshAgent>();
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
                agent.isStopped = true;
                break;

            case FoxState.Fleeing:
                break;

            default:
                break;
        }
    }

    private void OnTriggerEnter(Collider other) // should be collision changed to trigger for debugging
    {
        //if (other.CompareTag("FoxHoleOutside"))
        //{
        //    for (int holeIndex = 0; holeIndex < 1; holeIndex++)
        //    {
        //        if (other == foxHolesOutside[holeIndex]) // Finds which hole in the array
        //        {
        //            transform.position = foxHolesInside[holeIndex].transform.position;
        //            //agent.SetDestination(foxHolesInside[holeIndex].transform.position);
        //            foxState = FoxState.Hunting;
        //        }
        //    }
        //}

        if (other.CompareTag("fencedArea"))
        {
            foxState = FoxState.Hunting;
            Debug.Log("INSIDE AREA");
        }
    }

    void FindHole()
    {
        float foxPosX = transform.position.x;
        float foxPosY = transform.position.y;
        float foxPosZ = transform.position.z;

        float smallestDistance = 0;
        int closestHole = 0;

        for (int distanceIndex = 0; distanceIndex < 1; distanceIndex++) // Magic num
        {
            float holePosX = foxHolesOutside[distanceIndex].transform.position.x;
            float holePosY = foxHolesOutside[distanceIndex].transform.position.y;
            float holePosZ = foxHolesOutside[distanceIndex].transform.position.z;

            float xSq = Mathf.Pow(holePosX - foxPosX, 2);
            float ySq = Mathf.Pow(holePosY - foxPosY, 2);
            float zSq = Mathf.Pow(holePosZ - foxPosZ, 2);

            float distance = xSq + ySq + zSq; // Haven't bothered finding the root, don't need exact distance just which is closest

            if (distanceIndex == 0) // If it's the first loop meaning smallest distance is 0. Will always do the first loop
            {
                smallestDistance = distance;
                closestHole = distanceIndex;
            }
            else if (distance < smallestDistance) // If it's not the first loop so smallest distance has a value. Only does if the new distance is smaller
            {
                smallestDistance = distance;
                closestHole = distanceIndex;
            }
        }

        agent.SetDestination(foxHolesOutside[closestHole].transform.position);
    }

    private void FindClosestChicken()
    {

    }
}
