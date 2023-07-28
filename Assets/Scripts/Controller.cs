using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;



public class Controller : NetworkBehaviour //interface MonoBehavior provides methods Start() Update() OnCollisionEnter() OnCollisionExit()
{
    //Fields marked as public are to be changed in the inspector
    public float speed = 5;
    //cms is current move speed and is here so you can control the move speed via script
    private float cms;
    //cs is current sensitivity and is here so you can control the sensitivity via script
    private float cs;
    public bool isGrounded;
    public float sensitivity = 10;
    private Vector2 movementInput;
    private Vector2 localEulerAnglesInput;
    public LayerMask ground;
    public float JumpForce = 10f;
    public Transform playerCam;
    public Rigidbody rb;
    private bool isWallRunning = false;
    private float camRot;

    //public Transform rayRightOrigin;
    //public Transform rayLeftOrigin;

    //Camera height
    [SerializeField] private float cameraYoffset;

    private void Awake() {
        playerCam = GameObject.FindWithTag("Camera").transform;
    }


    public override void OnStartClient(){
        base.OnStartClient();
        if(base.IsOwner){
            playerCam.transform.position = new Vector3(transform.position.x, transform.position.y + cameraYoffset, transform.position.z);
            playerCam.transform.SetParent(transform);
        }else{
            gameObject.GetComponent<Controller>().enabled = false;
        }
    }


    //called on start
    private void Start()
    {
        //assign rigidbody
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        //lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    //called every frame
    private void Update()
    {
        float mousey = Input.GetAxisRaw("Mouse Y");
        movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        localEulerAnglesInput = new Vector2(Input.GetAxisRaw("Mouse X"), mousey);


        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }

        if (isWallRunning)
        {

            playerCam.GetComponent<Camera>().fieldOfView = 96;

            //rayRightOrigin = new Vector3(transform.position.x + 0.5f, transform.position.y, transform.position.z);
            //rayLeftOrigin = new Vector3(transform.position.x - 0.5f, transform.position.y, transform.position.z);

            if (Physics.Raycast(transform.position, transform.right, 1f, ground))
            //if (Physics.Raycast(rayRightOrigin, transform.right, 1f, ground))
            {
                rb.velocity += transform.right * 0.1f;

                if (playerCam.localEulerAngles.z < 15f || playerCam.localEulerAngles.z > 345f)
                {
                    //we multiply by Time.deltaTime to make frame rate not affect speed
                    playerCam.localEulerAngles += new Vector3(0, 0, 100f * Time.deltaTime);
                }

            }
            if (Physics.Raycast(transform.position, -transform.right, 1f, ground))
            //if (Physics.Raycast(rayLeftOrigin, transform.right, 1f, ground))
            {
                rb.velocity += transform.right * -0.1f;

                if (playerCam.localEulerAngles.z > 345f)
                {
                    playerCam.localEulerAngles += new Vector3(0, 0, -100f * Time.deltaTime);
                }
                //playerCam.localEulerAngles += new Vector3(0, 0, -10f * Time.deltaTime);
                
            }
        }
        else
        {
            //90 fov is the best fov don't argue with me
            playerCam.GetComponent<Camera>().fieldOfView = 90;
            playerCam.localEulerAngles = new Vector3(0, 0, 0);
        }
        if (isWallRunning && rb.velocity.magnitude <= 30)
        {
            rb.velocity += transform.forward * 100 * Time.deltaTime + transform.up * -0.1f;
        }
        cs = sensitivity;
        cms = speed;
        if (isWallRunning && isGrounded)
        {
            isWallRunning = false;
        }
        camRot -= localEulerAnglesInput.y * Time.deltaTime * cs;
        camRot = Mathf.Clamp(camRot, -70, 70);
        isGrounded = Physics.Raycast(transform.position, -transform.up, 2, ground);
        rb.velocity += transform.forward * movementInput.y * cms * Time.deltaTime + transform.right * movementInput.x * cms * Time.deltaTime;
        transform.localEulerAngles += new Vector3(0, localEulerAnglesInput.x, 0) * Time.deltaTime * cs;
        playerCam.localEulerAngles = new Vector3(camRot, playerCam.localEulerAngles.y, playerCam.localEulerAngles.z);
    }



    private void Jump()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, JumpForce, rb.velocity.z);
        }
        if (isWallRunning)
        {
            if (Physics.Raycast(transform.position, transform.right, 1f, ground))
            {
                rb.velocity = new Vector3(0, JumpForce / 2, 0) + transform.right * -40;
            }
            if (Physics.Raycast(transform.position, -transform.right, 1f, ground))
            {
                rb.velocity = new Vector3(0, JumpForce / 2, 0) + transform.right * 40;
            }
            if (Physics.Raycast(transform.position, transform.forward, 1f, ground))
            {
                rb.velocity = new Vector3(0, JumpForce / 2, 0) + transform.forward * -40;
            }
        }
    }

    private bool canWallRun = true;
    //called when collision is entered
    
    private void OnCollisionEnter(Collision other)
    {
        //using dot product to make sure we are infront of or behind the collision normal and not on top of or below
        if (Mathf.Abs(Vector3.Dot(other.GetContact(0).normal, Vector3.up)) < 0.1f && canWallRun)
        {
            rb.velocity = new Vector3(0, 20, 0) + transform.forward * 50;
            isWallRunning = true;
        }
    }
    //called on collision exit
    void OnCollisionExit(Collision other)
    {
        isWallRunning = false;
        StartCoroutine(WallRunCooldown());
    }

    private IEnumerator WallRunCooldown()
    {
        canWallRun = false;
        yield return new WaitForSeconds(0.3f);
        canWallRun = true;
    }

    /*
    
    //Other script I found
    //https://www.youtube.com/watch?v=gNt9wBOrQO4
     
    [Header("Wallrunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float wallClimbSpeed;
    public float maxWallRunTime;
    private float wallRunTimer;

    [Header("Input")]
    public KeyCode upwardsRunKey = KeyCode.LeftShift;
    public KeyCode downwardsRunKey = KeyCode.LeftControl;
    private bool upwardsRunning;
    private bool downwardsRunning;
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallhit;
    private RaycastHit rightWallhit;
    private bool wallLeft;
    private bool wallRight;

    [Header("References")]
    public Transform orientation;
    private PlayerMovementAdvanced pm;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovementAdvanced>();
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
    }

    private void FixedUpdate()
    {
        if (pm.wallrunning)
            WallRunningMovement();
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallhit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallhit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine()
    {
        // Getting Inputs
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        upwardsRunning = Input.GetKey(upwardsRunKey);
        downwardsRunning = Input.GetKey(downwardsRunKey);

        // State 1 - Wallrunning
        if((wallLeft || wallRight) && verticalInput > 0 && AboveGround())
        {
            if (!pm.wallrunning)
                StartWallRun();
        }

        // State 3 - None
        else
        {
            if (pm.wallrunning)
                StopWallRun();
        }
    }

    private void StartWallRun()
    {
        pm.wallrunning = true;
    }

    private void WallRunningMovement()
    {
        rb.useGravity = false;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        // forward force
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        // upwards/downwards force
        if (upwardsRunning)
            rb.velocity = new Vector3(rb.velocity.x, wallClimbSpeed, rb.velocity.z);
        if (downwardsRunning)
            rb.velocity = new Vector3(rb.velocity.x, -wallClimbSpeed, rb.velocity.z);

        // push to wall force
        if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
            rb.AddForce(-wallNormal * 100, ForceMode.Force);
    }

    private void StopWallRun()
    {
        pm.wallrunning = false;
    }
     
     */



}
