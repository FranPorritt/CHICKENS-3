using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coop : MonoBehaviour
{
    private PlayerController player;

    [SerializeField]
    private GameObject eggText;

    private int eggAmount = 0;
    private bool isPlayerInArea = false;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        eggText.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlayerInArea)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                eggText.SetActive(false);
                player.AddEggsToInventory(eggAmount); // Gives player num of eggs
                eggAmount = 0;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Chicken"))
        {
            ChickenController chicken;
            chicken = other.gameObject.GetComponent<ChickenController>();

            if (chicken.inCoop) // Checks chicken is in coop laying egg and not just randomly running into it
            {
                int randomEggNum = Random.Range(1, 3);
                eggAmount += randomEggNum;
                eggText.SetActive(true);
            }
        }
        if (other.CompareTag("Player"))
        {
            isPlayerInArea = true;
        }
    }

    // When chicken lays egg in coop, eggText.SetActive(true)
    // Can't do on trigger with chicken in case chicken just randomly runs in, onTrigger + isFull?
}
