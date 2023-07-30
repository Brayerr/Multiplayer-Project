using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

[Serializable]
public class PlayerController : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    public static event Action PlayerDied;

    [Header("Attributes")]
    [SerializeField] int maxHP = 3;
    [SerializeField] public int currentHP;
    public PlayerNameLookAt lookAt;
    public float lastActorHit;

    [Header("Movement")]
    public float moveSpeed;
    public float groundDrag;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;
    System.Random random;

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

    PhotonView currentPhotonViewOnObject;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public int spawnPoint { get; private set; }

    private void Start()
    {
        OnlineGameManager.Instance.PlayerInitialized += SendPlayerController;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        anim = GetComponentInChildren<Animator>();
        readyToJump = true;
        currentHP = maxHP;
        random = new System.Random();

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
        if (photonView.AmOwner)
        {
            if (OnlineGameManager.Instance.GetLocalPlayerController() == null)
            {
                OnlineGameManager.Instance.SetPlayerControllerLocally(this);
            }

            MovePlayer();
        }
    }

    public Transform GetOrientation() => orientation;

    private void MyInput()
    {
        if (photonView.AmOwner)
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
        var shot = PhotonNetwork.Instantiate("Arrow", shootingPosition.position, Quaternion.identity);
        shot.transform.Rotate(new Vector3(orientation.transform.eulerAngles.x, orientation.transform.eulerAngles.y, PhotonNetwork.LocalPlayer.ActorNumber));
    }

    public void TakeDamage()
    {
        currentHP--;
    }

    public void Respawn()
    {
        rb.velocity = Vector3.zero;
        transform.position = new Vector3(random.Next(0, 5), random.Next(0, 5), 1);
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


    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        if (info.photonView.AmOwner)
        {
            OnlineGameManager.Instance.SetPlayerControllerLocally(this);
        }
    }

    public void SetSpawn(int spawnID)
    {
        spawnPoint = spawnID;
    }

    public void SendPlayerController(int oldActorNumber)
    {
        if (oldActorNumber == photonView.CreatorActorNr)
        {
            print("assigning playercontroller");
            OnlineGameManager.Instance.SetPlayerControllerLocally(this);
        }
    }
}
