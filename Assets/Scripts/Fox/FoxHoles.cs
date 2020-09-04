using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoxHoles : MonoBehaviour
{
    [SerializeField]
    private GameObject holePair;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Fox"))
        {
            other.transform.position = holePair.transform.position;
        }
    }
}
