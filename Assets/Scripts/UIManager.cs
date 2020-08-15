using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private PlayerController player;

    [SerializeField]
    private Text cropsAmountText; // ALWAYS NULL - CAUSING ERROR IN CONSOLE

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        cropsAmountText.text = player.GetCropAmount().ToString();
    }
}
