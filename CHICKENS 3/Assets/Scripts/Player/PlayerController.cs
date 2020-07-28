using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private int cropAmount = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int GetCropAmount() { return cropAmount; }

    public void AddCropsToInventory(int harvestedCrop)
    {
        cropAmount += harvestedCrop;
        Debug.Log(cropAmount);
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
}
