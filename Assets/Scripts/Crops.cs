using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum CropState
{
    Growing,
    Grown,
    Harvested,
};

public class Crops : MonoBehaviour
{
    private PlayerController player;

    private GameObject crop;
    private CropState currentState = CropState.Growing;

    private Vector3 startPos;

    private int cropAmount = 0;
    private bool isFullyGrown = false;

    private bool isPlayerInArea = false;
    private bool isBeingHarvested = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        crop = gameObject;
        startPos = crop.transform.position;
        InvokeRepeating("Grow", 1f, 3f); // 1sc delay, repeat every 3s
    }

    // Update is called once per frame
    void Update()
    {
        if (cropAmount >= 10)
        {
            currentState = CropState.Grown;
        }

        if (isPlayerInArea)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                currentState = CropState.Harvested;
            }
        }

        switch (currentState)
        {
            case CropState.Growing:
                {
                    isBeingHarvested = false;
                    break;
                }

            case CropState.Grown:
                {
                    isFullyGrown = true;
                    break;
                }

            case CropState.Harvested:
                {
                    Harvested();
                    break;
                }

            default:
                break;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInArea = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInArea = false;
        }
    }

    void Grow()
    {
        if (!isFullyGrown) // Only grows if not at 100% growth
        {
            crop.transform.position = new Vector3(crop.transform.position.x, crop.transform.position.y + 0.5f, crop.transform.position.z); // Moves object up 0.5 every 1s
            cropAmount++;
        }
    }

    void Harvested()
    {
        isFullyGrown = false;

        // Transfer amount to player
        if (!isBeingHarvested)
        {
            player.AddCropsToInventory(cropAmount);
            cropAmount = 0;

            crop.transform.position = startPos;
            currentState = CropState.Growing;
        }
        isBeingHarvested = true; // Stops code from running multiple times and inflating crop amount
    }
}
