using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [HideInInspector] public float walkSpeed;
    [HideInInspector] public float sprintSpeed;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode shootKey = KeyCode.Mouse0;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    [SerializeField] bool grounded;

    [Header("Shooting")]
    [SerializeField] MoveArrow arrow;
    [SerializeField] Transform shootingPosition;
    [SerializeField] float shootCooldown;
    bool isCountingDown;

    [Header("Animations")]
    Animator anim;


    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    Vector3 moveDirectionHolder = Vector3.zero;

    Rigidbody rb;

    private void Start()
    {
        
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        anim = GetComponentInChildren<Animator>();

        readyToJump = true;
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();

        // handle drag
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;

        if (isCountingDown) shootCooldown -= Time.deltaTime;

        if (shootCooldown <= 0)
        {
            isCountingDown = false;
            shootCooldown = 0;

        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (horizontalInput != 0 || verticalInput != 0) ToggleWalk(true);
        else if (horizontalInput == 0 && verticalInput == 0) ToggleWalk(false);

        // when to jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            TriggerJump();
            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKeyDown(shootKey) && shootCooldown <= 0)
        {
            TriggerShoot();
            Invoke("ShootArrow", .5f);
            shootCooldown = 1;
            isCountingDown = true;
        }
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        moveDirection.y = 0;

        // on ground
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;
    }

    void ShootArrow()
    {
        MoveArrow shot = Instantiate(arrow, shootingPosition.position, Quaternion.identity);
        shot.transform.Rotate(orientation.transform.eulerAngles);
        

    }

    #region Animations
    void TriggerShoot()
    {
        anim.SetTrigger("Shoot");
    }

    void TriggerJump()
    {
        anim.SetTrigger("Jump");
    }

    void ToggleWalk(bool boolean)
    {
        if (boolean) anim.SetBool("isWalking", true);
        else anim.SetBool("isWalking", false);
    }
    #endregion
}