using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("CHICKENS")]
    [SerializeField]
    private GameObject chickenPrefab;
    [SerializeField]
    private int NumberOfChickens;
    private GameObject chickenObject;
    private List<GameObject> chickenList;

    [SerializeField]
    private GameObject mapExtents;
    private Vector3 mapExtentsPos;

    [SerializeField]
    private GameObject[] coops;
    [SerializeField]
    private GameObject[] entryPoints;

    // Start is called before the first frame update
    void Awake()
    {
        mapExtentsPos = mapExtents.transform.position;

        chickenList = new List<GameObject>();

        for (int chickenIndex = 0; chickenIndex < NumberOfChickens; chickenIndex++)
        {
            GameObject chickenObject = GameObject.Instantiate(chickenPrefab, new Vector3(Random.Range(-mapExtentsPos.x, mapExtentsPos.x), 0f, Random.Range(-mapExtentsPos.z, mapExtentsPos.z)), Quaternion.identity);
            chickenList.Add(chickenObject);
        }
    }

    public List<GameObject> GetChickenList()
    {
        return chickenList;
    }

    public GameObject[] GetCoops()
    {
        return coops;
    }

    public GameObject[] GetEntryPoints()
    {
        return entryPoints;
    }

    public void ChickenDeath(GameObject deadChick)
    {
        foreach (GameObject chick in chickenList)
        {
            if (chick == deadChick)
            {
                chickenList.Remove(chick);
                NumberOfChickens--;
                break;
            }
        }
    }
}
