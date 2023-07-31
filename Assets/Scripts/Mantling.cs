using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mantling : MonoBehaviour
{
    [Header("Mantle")]
    public LayerMask whatIsWall;
    private bool checkComplete = false;
    private Vector3 topRayOrigin;
    private Vector3 bottomRayOrigin;


    [Header("References")]
    public Transform orientation;
    //public PlayerCam cam;
    private Controller pm;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<Controller>();
        checkComplete = false;
    }

    // Update is called once per frame
    void Update()
    {
        WallFront();

        if(!(Physics.Raycast(bottomRayOrigin, orientation.forward, 1, whatIsWall)) && !(Physics.Raycast(topRayOrigin, orientation.forward, 1, whatIsWall)))
        {
            checkComplete = false;
        }
    }

    private void WallFront()
    {
        /*if(Physics.Raycast(transform.position, orientation.forward,1,whatIsWall) && !(checkComplete))
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(transform.up * (20), ForceMode.Impulse);
            checkComplete = true;
        }*/
        topRayOrigin = new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z);
        bottomRayOrigin = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);

        if (Physics.Raycast(bottomRayOrigin, orientation.forward, 1, whatIsWall) && !(Physics.Raycast(topRayOrigin, orientation.forward, 1, whatIsWall)) && !(checkComplete))
        {
            rb.velocity = new Vector3(0, 0f, 0);
            rb.AddForce(transform.forward * (-1.5f), ForceMode.Impulse);
            rb.AddForce(transform.up * (8), ForceMode.Impulse);
            checkComplete = true;
            //if (Input.GetKey(KeyCode.W))
            //    transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
        }
    }
}
