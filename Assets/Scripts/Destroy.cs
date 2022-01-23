using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour
{
    GameObject player;
    private int threshold = 80;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("CAR");
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(gameObject.transform.position.z);

        if (gameObject.transform.position.z < player.transform.position.z - threshold)
        {
            //Debug.Log($"Destroying {gameObject.name}");
            Destroy(gameObject);
        }

    }
}
