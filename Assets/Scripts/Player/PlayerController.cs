using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private int cropAmount = 50; // 50 for testing
    private int eggAmount = 0;

    public int GetCropAmount() { return cropAmount; }

    public void AddCropsToInventory(int harvestedCrop)
    {
        cropAmount += harvestedCrop;
    }

    public void AddCropsToFeeder(int takeFromPlayer)
    {
        if(cropAmount - takeFromPlayer < 0)
        {
            cropAmount = 0;
        }
        else
        {
            cropAmount -= takeFromPlayer;
        }
    }

    public void AddEggsToInventory(int eggsCollected)
    {
        eggAmount += eggsCollected;
        Debug.Log("PLAYERS EGG COUNT: " + eggAmount);
    }
}
