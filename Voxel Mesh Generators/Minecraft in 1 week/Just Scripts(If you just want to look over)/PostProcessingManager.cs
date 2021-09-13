using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingManager : MonoBehaviour
{
    // this script basically changes the scenes postprocessing depending on the players position.

    // postprocessing variables
    public VolumeProfile[] Profiles;
    public Volume Vol;

    // player
    public GameObject Player;
    private FirstPersonAIO fpaio;
    private Rigidbody rb;

    public float waterHeight;


    private void Start()
    {
        // get player controller and rigid body
        fpaio = Player.GetComponent<FirstPersonAIO>();
        rb = Player.GetComponent<Rigidbody>();
    }


    private void FixedUpdate()
    {

        // depending on the player y position set the post and player controls

        if (Player.transform.position.y < waterHeight)
        {
            Vol.profile = Profiles[1];
            fpaio.advanced.gravityMultiplier = -1;
            rb.mass = 0.6f;
        }
        else
        {
            Vol.profile = Profiles[0];
            fpaio.advanced.gravityMultiplier = 1f;
            rb.mass = 0.8f;
        }
    }

}
