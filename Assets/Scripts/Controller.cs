using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;



public class Controller : NetworkBehaviour //interface MonoBehavior provides methods Start() Update() OnCollisionEnter() OnCollisionExit()
{
    [Header("Movement")]
    private float moveSpeed;
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;
    public float wallrunSpeed;

    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;


    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        wallrunning,
        crouching,
        sliding,
        air,
        freeze
    }

    public bool sliding;
    public bool crouching;
    public bool wallrunning;
    public bool freeze;

    public bool activeGrapple;
    public bool swinging;

    [Header("Camera")]
    public float sensX;
    public float sensY;

    //public Transform orientation;
    //public Transform camHolder;

    float xRotation;
    float yRotation;

    public bool recoil = false;

    [Header("Old")]
    /*
    //Fields marked as public are to be changed in the inspector
    public float speed = 5;
    //cms is current move speed and is here so you can control the move speed via script
    private float cms;
    //cs is current sensitivity and is here so you can control the sensitivity via script
    private float cs;
    public bool isGrounded;
    public float sensitivity = 10;
    */
    private Vector2 movementInput;
    private Vector2 localEulerAnglesInput;
    /*
    public LayerMask ground;
    public float JumpForce = 10f;*/
    public Transform playerCam;
    //public Rigidbody rb;
    /*
    private bool isWallRunning = false;
    private float camRot;*/

    //public Transform rayRightOrigin;
    //public Transform rayLeftOrigin;
    //public float wallrunSpeed;

    //Camera height
    [SerializeField] private float cameraYoffset;

    private int doubleJumpCount;

    PlayerShoot shooter;

    private void Awake() {
        playerCam = GameObject.FindWithTag("Camera").transform;
    }


    public override void OnStartClient(){
        base.OnStartClient();
        if(base.IsOwner){
            playerCam.transform.position = new Vector3(transform.position.x, transform.position.y + cameraYoffset, transform.position.z);
            playerCam.transform.SetParent(transform);
            // GameObject.Find("Gun?").SetActive(false);
            shooter = gameObject.GetComponent<PlayerShoot>();
        }else{
            gameObject.GetComponent<Controller>().enabled = false;
            gameObject.GetComponent<PowerSliding>().enabled = false;
            // gameObject.GetComponent<Rigidbody>(.ena).enabled = false;
        }
    }


    //called on start
    private void Start()
    {
        /*
        //assign rigidbody
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        //lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        */
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;

        doubleJumpCount = 1;

    }
    //called every frame
    private void Update()
    {
        if(!base.IsOwner){
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        
        
        /*
        float mousey = Input.GetAxisRaw("Mouse Y");
        movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        localEulerAnglesInput = new Vector2(Input.GetAxisRaw("Mouse X"), mousey);
        */

        // get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        if(recoil){
            recoilFunction(shooter.horizontalRecoil, shooter.verticalRecoil, mouseX, mouseY);
        }else{
            yRotation += mouseX;
            xRotation -= mouseY;
        }

       
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // rotate cam and orientation
        playerCam.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);

        /*
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }
        */
        /*
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
        */
        /*if (isWallRunning && rb.velocity.magnitude <= 30)
        {
            rb.velocity += transform.forward * 100 * Time.deltaTime + transform.up * -0.1f;
        }*/
        /*
        cs = sensitivity;
        cms = speed;
        */
        /*if (isWallRunning && isGrounded)
        {
            isWallRunning = false;
        }*/
        /*
        camRot -= localEulerAnglesInput.y * Time.deltaTime * cs;
        camRot = Mathf.Clamp(camRot, -70, 70);
        isGrounded = Physics.Raycast(transform.position, -transform.up, 2, ground);
        rb.velocity += transform.forward * movementInput.y * cms * Time.deltaTime + transform.right * movementInput.x * cms * Time.deltaTime;
        transform.localEulerAngles += new Vector3(0, localEulerAnglesInput.x, 0) * Time.deltaTime * cs;
        playerCam.localEulerAngles = new Vector3(camRot, playerCam.localEulerAngles.y, playerCam.localEulerAngles.z);
        */

        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();
        //TextStuff();

        // handle drag
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;


        /*if(grounded && Input.GetKey(KeyCode.W) && Input.GetKeyDown(crouchKey))
        {
            PowerSlide();
        }
        //test
        if(Input.GetKeyDown(KeyCode.Q))
        {
            PowerSlide();
        }*/
        if (grounded)
            doubleJumpCount = 1;

        if (wallrunning)
            doubleJumpCount = 0;

    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void DoubleJump()
    {
        // reset y velocity
        if(doubleJumpCount != 0)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            rb.AddForce(transform.up * (jumpForce), ForceMode.Impulse);

            doubleJumpCount--;
        }
        
    }
    /*
    private void PowerSlide()
    {
        //exitingSlope = true;
        //rb.velocity = new Vector3(0f, 0f,0f);
       // rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        //rb.AddForce(transform.forward * jumpForce/4, ForceMode.Impulse);
        //rb.AddForce(playerCam.forward * 2000, ForceMode.Acceleration);
        
    }
    */
    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if (Input.GetKeyDown(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        //doubleJump
        if (Input.GetKeyDown(jumpKey) && !grounded)
        {
            DoubleJump();
        }

        // start crouch
        if (Input.GetKeyDown(crouchKey) && horizontalInput == 0 && verticalInput == 0)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

            crouching = true;
        }

        // stop crouch
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

            crouching = false;
        }
    }

    private void StateHandler()
    {
        // Mode - Wallrunning
        if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallrunSpeed;
        }

        // Mode - Sliding
        else if (sliding)
        {
            state = MovementState.sliding;

            // increase speed by one every second
            if (OnSlope() && rb.velocity.y < 0.1f)
                desiredMoveSpeed = slideSpeed;

            else
                desiredMoveSpeed = sprintSpeed;
        }

        // Mode - Crouching
        else if (crouching)
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }

        // Mode - Sprinting
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }

        // Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        // Mode - Freeze
        else if(freeze)
        {
            state = MovementState.freeze;
            moveSpeed = 0;
            rb.velocity = Vector3.zero;
        } 

        // Mode - Air
        else
        {
            state = MovementState.air;
        }

        // check if desired move speed has changed drastically
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());

            print("Lerp Started!");
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // on ground
        else if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        // turn gravity off while on slope
        if (!wallrunning) rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }

        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    /*private void TextStuff()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (OnSlope())
            text_speed.SetText("Speed: " + Round(rb.velocity.magnitude, 1));

        else
            text_speed.SetText("Speed: " + Round(flatVel.magnitude, 1));

        text_mode.SetText(state.ToString());
    }*/

    public static float Round(float value, int digits)
    {
        float mult = Mathf.Pow(10.0f, (float)digits);
        return Mathf.Round(value * mult) / mult;
    }

    private bool enableMovementOnNextTouch;
    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        activeGrapple = true;

        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);

        Invoke(nameof(ResetRestrictions), 3f);
    }

    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
            + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }

    private Vector3 velocityToSet;
    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        rb.velocity = velocityToSet;

        //cam.DoFov(grappleFov);
    }

    public void ResetRestrictions()
    {
        activeGrapple = false;
        //cam.DoFov(85f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();

            GetComponent<Grappling>().StopGrapple();
        }
    }

    //I'm handling recoil here because it simplifies a conflict with the camera contoller

    public void recoilFunction(float horRecoil, float verRecoil, float mouX, float mouY){
        float xRecoil = Random.Range(-horRecoil, horRecoil) / 2;
        float yRecoil = Random.Range(-verRecoil, verRecoil) / 2;
        
        xRotation += mouX + yRecoil;
        yRotation += mouY + xRecoil;

        recoil = false;
    }


    /*
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
    */
    /*
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
    */



    /*
     
    public class PlayerMovementAdvanced : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;
    public float wallrunSpeed;

    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;


    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        wallrunning,
        crouching,
        sliding,
        air
    }

    public bool sliding;
    public bool crouching;
    public bool wallrunning;

    public TextMeshProUGUI text_speed;
    public TextMeshProUGUI text_mode;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();
        TextStuff();

        // handle drag
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // start crouch
        if (Input.GetKeyDown(crouchKey) && horizontalInput == 0 && verticalInput == 0)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

            crouching = true;
        }

        // stop crouch
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

            crouching = false;
        }
    }

    private void StateHandler()
    {
        // Mode - Wallrunning
        if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallrunSpeed;
        }

        // Mode - Sliding
        else if (sliding)
        {
            state = MovementState.sliding;

            // increase speed by one every second
            if (OnSlope() && rb.velocity.y < 0.1f)
                desiredMoveSpeed = slideSpeed;

            else
                desiredMoveSpeed = sprintSpeed;
        }

        // Mode - Crouching
        else if (crouching)
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }

        // Mode - Sprinting
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }

        // Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        // Mode - Air
        else
        {
            state = MovementState.air;
        }

        // check if desired move speed has changed drastically
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());

            print("Lerp Started!");
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // on ground
        else if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        // turn gravity off while on slope
        if(!wallrunning) rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }

        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    private void TextStuff()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (OnSlope())
            text_speed.SetText("Speed: " + Round(rb.velocity.magnitude, 1));

        else
            text_speed.SetText("Speed: " + Round(flatVel.magnitude, 1));

        text_mode.SetText(state.ToString());
    }

    public static float Round(float value, int digits)
    {
        float mult = Mathf.Pow(10.0f, (float)digits);
        return Mathf.Round(value * mult) / mult;
    }
     
     */


}
