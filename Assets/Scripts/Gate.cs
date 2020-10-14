using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{

    // Smoothly open a door
    [SerializeField]
    private float doorOpenAngle = 90.0f; //Set either positive or negative number to open the door inwards or outwards
    [SerializeField]
    private float openSpeed = 2.0f; //Increasing this value will make the door open faster

    private bool isOpen = false;
    private bool isPlayerInArea = false;

    private  float defaultRotationAngle;
    private float currentRotationAngle;
    private float openTime = 0;

    void Start()
    {
        defaultRotationAngle = transform.localEulerAngles.y;
        currentRotationAngle = transform.localEulerAngles.y;
    }

    void Update()
    {
        if (openTime < 1)
        {
            openTime += Time.deltaTime * openSpeed;
        }
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, Mathf.LerpAngle(currentRotationAngle, defaultRotationAngle + (isOpen ? doorOpenAngle : 0), openTime), transform.localEulerAngles.z);

        if (Input.GetKeyDown(KeyCode.E) && isPlayerInArea) // Opens/closes gate
        {
            isOpen = !isOpen;
            currentRotationAngle = transform.localEulerAngles.y;
            openTime = 0;
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

    public bool GetGate() 
    {
        return isOpen; 
    }
}
