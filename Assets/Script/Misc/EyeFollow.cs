using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeFollow : MonoBehaviour
{   
    public GameObject player;
    void Start()
    {
        player = GameObject.Find("Player");
    }

    void Update()
    {
        eyeFollow();
    }

    void eyeFollow(){

        Vector3 playerpos = player.transform.position;
        Vector2 direction = new Vector2(

            (playerpos.x - transform.position.x),

            (playerpos.y - transform.position.y)     
        );
        transform.up = direction;
    }

}