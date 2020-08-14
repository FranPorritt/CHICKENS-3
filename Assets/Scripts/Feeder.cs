using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Feeder : MonoBehaviour
{
    private PlayerController player;

    [SerializeField]
    private GameObject feederCrops;
    private float cropHeight = 0f;

    private const int MAX_CROPS = 10;
    private int cropAmount = 0;

    private bool isPlayerInArea = false;
    private bool isBeingFilled = false;
    private bool hasCrops = false;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlayerInArea)
        {
            if ((Input.GetKeyDown(KeyCode.E)) && (!isBeingFilled))
            {
                Fill(player.GetCropAmount());
            }
        }

        cropHeight = cropAmount * 0.1f;
        feederCrops.transform.position = new Vector3(feederCrops.transform.position.x, cropHeight, feederCrops.transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInArea = true;
        }
        if (other.CompareTag("Chicken"))
        {
            other.GetComponent<ChickenController>().EatStart(); // Starts eat timer in chicken.
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Chicken"))
        {
            ChickenController chicken;
            chicken = other.gameObject.GetComponent<ChickenController>();
            FeedChicken(chicken);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInArea = false;
        }
    }

    private void Fill(int playerCrops)
    {
        playerCrops = player.GetCropAmount();

        if (playerCrops <= 0) // Player has no crops
        {
            // Not enough message
            Debug.Log("Player has 0 crops");
        }
        else
        {
            // Take crops from player
            int takeFromPlayer = 0;
            takeFromPlayer = MAX_CROPS - cropAmount;
            player.AddCropsToFeeder(takeFromPlayer);

            if (playerCrops + cropAmount > MAX_CROPS) // Total crops is more than 10
            {
                // Set feeder to max
                cropAmount = MAX_CROPS;
            }
            else // Total crops is 10 or less
            {
                cropAmount += playerCrops;
            }
            Debug.Log("Feeder: " + cropAmount);
            Debug.Log("Player NOW has: " + (player.GetCropAmount()));
        }
    }

    private void FeedChicken(ChickenController chicken)
    {
        if (chicken.Eat()) // Did the chicken eat anything
        {
            cropAmount--;
        }
    }

    public bool GetCrops()
    {
        if (cropAmount > 0)
        {
            hasCrops = true;
        }
        else if (cropAmount <= 0)
        {
            hasCrops = false;
        }
        return hasCrops;
    }
}
