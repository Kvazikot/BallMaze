using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rnd = UnityEngine.Random;


public class PlayerController : MonoBehaviour
{

    public Rigidbody rb;
    public float speed = 10f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

 

    }


    void OnCollisionEnter(Collision collision)
    {

        foreach (ContactPoint contact in collision.contacts)
        {

            Collider collider = contact.otherCollider;
            var renderer = collider.GetComponent<MeshRenderer>();
            Color color = renderer.material.GetColor("_Color");
            //Debug.Log("color " + color);
            if (collider.GetType() == typeof(SphereCollider))
            {
                renderer.enabled = false;
                collider.enabled = false;

            }


        }

    }


    // Update is called once per frame
    void FixedUpdate()
    {
        // add this code to the Heuristic method of the RollerAgent
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(moveHorizontal, 0, moveVertical);
        //Debug.Log("PlayerController: moveHorizontal " + moveHorizontal + " moveVertical" + moveVertical);
        rb.AddForce(movement * speed);

    }

    
  
}

