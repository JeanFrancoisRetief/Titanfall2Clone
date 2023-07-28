using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerScript : MonoBehaviour
{
    private Controller _controller;
    private Vector3 RightVector;
    private Vector3 LeftVector;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RightVector = new Vector3(_controller.rb.transform.eulerAngles.x, _controller.rb.transform.eulerAngles.y + 90, _controller.rb.transform.eulerAngles.z);
        LeftVector = new Vector3(_controller.rb.transform.eulerAngles.x, _controller.rb.transform.eulerAngles.y - 90, _controller.rb.transform.eulerAngles.z);
    }

    public void OnTriggerStay(Collider other)
    {
        if(!(other.tag == "Player") && !_controller.isGrounded)
        {
            if(gameObject.tag == "RightTrigger")
            {
                _controller.rb.velocity += new Vector3(_controller.rb.velocity.x,0, _controller.rb.velocity.y);
            }
            if (gameObject.tag == "LeftTrigger")
            {
                _controller.rb.velocity += new Vector3(_controller.rb.velocity.x, 0, _controller.rb.velocity.y);
            }
        }
    }
}
