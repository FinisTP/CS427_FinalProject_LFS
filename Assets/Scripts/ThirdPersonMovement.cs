using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityStandardAssets.CrossPlatformInput;
using Cinemachine;

public enum Role {
    HIDER,
    SEEKER,
    SPECTATOR
}

public class ThirdPersonMovement : MonoBehaviourPunCallbacks
{
    public static ThirdPersonMovement LocalPlayerInstance = null;
    public bool isMasterClient = false;

    [Header("Controller Specs")]
    public CharacterController controller;
    public float normalSpeed = 6f;
    public float turnSmoothTime = 0.1f;
    public float sprintSpeed = 12f;
    public float crouchSpeed = 3f;
    public float jumpSpeed = 10f;
    public float shootDelay = 0.5f;

    private float _currentSpeed;
    private float _turnSmoothVelocity;
    private Vector3 _moveDirection;
    private float _velocityY;
    private bool _isCrouching = false;
    private float _shootTime;

    // Ground checking

    private bool _isGrounded = false;
    private float _groundCheckDistance = 0.2f;
    private bool _animIsGrounded = false;

    // For animation
    private bool _isJumping = false;
    private float _forwardAmount;
    private bool _isSprinting = false;

    [Header("Associated Components")]
    public Transform feet;
    public Transform waist;
    public LayerMask groundMask;
    public Animator animator;
    public GameObject head;
    public Transform shootBarrel;
    public GameObject flashlight;
    public Player photonPlayer = null;
    public TMP_Text playerNickName;

    public Transform currentLook;
    private Transform _playerCamera;
    private GameObject _TPSCamera;
    private GameObject _FPSCamera;
    private bool _isFPSView = false;

    [Header("Role Specs")]
    public Role currentRole;
    public float SpeedMult = 1f;
    public float JumpMult = 1f;
    private float _defaultHeight;
    private float _defaultCenter;

    public bool isMoving
    {
        get
        {
            return controller.velocity.magnitude > 0.5f;
        }
    }

    [HideInInspector]
    public int id;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    [PunRPC]
    public void Initialize(Player player)
    {
        photonPlayer = player;
        id = player.ActorNumber;
        playerNickName.text = photonPlayer.NickName;
        
        if (photonView.IsMine)
        {
            LocalPlayerInstance = this;
            if (PhotonNetwork.IsMasterClient) isMasterClient = true;
        }

        else isMasterClient = false;
        ModalWindowPanel.Instance.ShowModal("Welcome to the game", null, $"{photonPlayer.NickName} has joined the game!", "Okay");
    }

    [PunRPC]
    public void ToggleLight()
    {
        flashlight.SetActive(!flashlight.activeInHierarchy);
    }

    [PunRPC]
    void Fire()
    {
        GameplayManager.instance.soundPlayer.PlayClip("Laser", 0.1f);
        GameObject bullet = Instantiate(Resources.Load("PlayerBullet") as GameObject, shootBarrel.position, Quaternion.identity);
        bullet.name = photonPlayer.NickName;
        bullet.GetComponent<Rigidbody>().AddForce(this.transform.forward * 100f, ForceMode.Impulse);
        bullet.GetComponent<ProjectileBehavior>().owner = this;
        Destroy(bullet, 5f);
    }

    private void Start()
    {
        flashlight.SetActive(false);

        if (photonPlayer == null || !photonPlayer.IsLocal) return;

        currentLook.transform.position = head.transform.position;
        _playerCamera = Camera.main.transform;
        _TPSCamera = GameObject.Find("TP Camera");
        _FPSCamera = GameObject.Find("FP Camera");
        CinemachineFreeLook cine = _TPSCamera.GetComponent<CinemachineFreeLook>();
        CinemachineVirtualCamera virtualCam = _FPSCamera.GetComponent<CinemachineVirtualCamera>();
        virtualCam.Follow = currentLook;
        virtualCam.LookAt = currentLook;
        cine.Follow = currentLook;
        cine.LookAt = currentLook;
        _TPSCamera.SetActive(true);
        _FPSCamera.SetActive(false);

        _defaultHeight = controller.height;
        _defaultCenter = controller.center.y;

        _shootTime = 0f;
        _currentSpeed = normalSpeed;
        currentRole = Role.SPECTATOR;
    }

    private void Update()
    {
        if (currentRole == Role.SEEKER && GameplayManager.instance.matchPhase == MatchPhase.HIDE) return;

        if (photonPlayer == null || !photonPlayer.IsLocal) return;
       
        if (Input.GetKeyDown(KeyCode.F))
        {
            photonView.RPC("ToggleLight", RpcTarget.All);
        }

        HandleNametag();
        HandleSprint();
        HandleCrouch();
        HandleShoot();
        HandleViewMode();

        Move();
        Jump();

        controller.Move((_moveDirection.normalized * _currentSpeed * SpeedMult + _velocityY * JumpMult * Vector3.up) * Time.deltaTime);
    }

    private void HandleNametag()
    {
        foreach (ThirdPersonMovement tpm in GameplayManager.instance.playerList)
        {
            if (tpm != null)
            {
                tpm.playerNickName.transform.LookAt(Camera.main.transform);
            }
        }
    }

    private void HandleSprint()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            _currentSpeed = sprintSpeed;
            _isSprinting = true;
        }
        else if (!_isCrouching)
        {
            _currentSpeed = normalSpeed;
            _isSprinting = false;
        }
        
    }

    private void HandleViewMode()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            _isFPSView = !_isFPSView;
            if (_isFPSView)
            {
                _TPSCamera.SetActive(false);
                _FPSCamera.SetActive(true);
            }
            else
            {
                _TPSCamera.SetActive(true);
                _FPSCamera.SetActive(false);
            }
        }
    }

    private void HandleShoot()
    {
        if (currentRole == Role.SEEKER && _shootTime >= shootDelay 
            && (Input.GetMouseButton(0) || CrossPlatformInputManager.GetButton("Shoot")))
        {
            _shootTime = 0f;
            photonView.RPC("Fire", RpcTarget.All);
        }
    }

    private void HandleCrouch()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            _isCrouching = true;
            _currentSpeed = crouchSpeed;
            currentLook.transform.position = waist.transform.position;
            controller.center = new Vector3(0, _defaultCenter / 2, 0);
            controller.height = _defaultHeight / 2;

        } 
        else if (!_isSprinting)
        {
            _isCrouching = false;
            _currentSpeed = normalSpeed;
            currentLook.transform.position = head.transform.position;
            controller.center = new Vector3(0, _defaultCenter, 0);
            controller.height = _defaultHeight;
        }
        
    }

    public void AnnounceRole()
    {
        // currentRole = (Role)photonPlayer.CustomProperties["Role"];

        if (currentRole == Role.HIDER)
        {
            ModalWindowPanel.Instance.ShowModal("Match Started!", null, "You are the prey in this match! You have 60 seconds to find " +
                "a place to hide before the hunter wakes up!", "Okay");
        }
        else if (currentRole == Role.SEEKER)
        {
            ModalWindowPanel.Instance.ShowModal("Match Started!", null, "You are the hunter in this match! You can start seeking the prey " +
                "after 60 seconds!", "Okay");
        }
    }


    public void GrantSeekerBuff()
    {
        SpeedMult = 3f;
        JumpMult = 3f;
    }
    private void Move()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _playerCamera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            _moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }
        else _moveDirection = Vector3.zero;
        _forwardAmount = _moveDirection.magnitude / 2;
        if (_isSprinting) _forwardAmount *= 2;
        else if (_isCrouching) _forwardAmount /= 2;
        // UpdateAnimator();
        photonView.RPC("UpdateAnimator", RpcTarget.All, 
            _forwardAmount, _animIsGrounded, _isCrouching, _velocityY, controller.isGrounded);
    }

    void CheckGroundStatus()
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(feet.position, Vector3.down, out hitInfo, _groundCheckDistance))
        {
            _animIsGrounded = true;
            _isGrounded = true;
        }
        else
        {
            _animIsGrounded = false;
            _isGrounded = false;
        }
    }

    private void FixedUpdate()
    {
        if (photonPlayer == null) return;
        _shootTime += Time.fixedDeltaTime;
        CheckGroundStatus();
        _isGrounded = controller.isGrounded;
    }

    private void Jump()
    {
        if (_isGrounded)
        {
            _isJumping = false;
            _velocityY = 0;
            if (Input.GetKey(KeyCode.Space))
            {
                _velocityY = jumpSpeed;
                _isJumping = true;
            }
        } 
        else
        {
            _velocityY += -9.81f * 2 * Time.deltaTime;
        }
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(feet.position, feet.position + Vector3.down * _groundCheckDistance);
    }

    [PunRPC]
    void UpdateAnimator(float forwardAmount, bool animIsGrounded, bool isCrouching, float velocityY, bool controllerIsGrounded)
    {
        animator.SetFloat("Forward", forwardAmount, 0.1f, Time.deltaTime);
        animator.SetBool("OnGround", animIsGrounded);
        animator.SetBool("Crouch", isCrouching);
        if (!controllerIsGrounded && Mathf.Abs(velocityY) >= 1)
        {
            animator.SetFloat("Jump", velocityY);
        }

        // calculate which leg is behind, so as to leave that leg trailing in the jump animation
        // (This code is reliant on the specific run cycle offset in our animations,
        // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
        float runCycle =
            Mathf.Repeat(
                animator.GetCurrentAnimatorStateInfo(0).normalizedTime + 0.2f, 1);
        float jumpLeg = (runCycle < 0.5f ? 1 : -1) * forwardAmount;
        if (controllerIsGrounded)
        {
            animator.SetFloat("JumpLeg", jumpLeg);
        }
    }
}
