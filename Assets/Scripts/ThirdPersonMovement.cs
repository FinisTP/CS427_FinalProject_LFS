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
    SPECTATOR,
    UNASSIGNED
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
    public bool isFound = false;

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

    public GameObject GFX;
    public GameObject morphObject = null;
    public Transform currentLook;
    private Transform _playerCamera;
    private GameObject _TPSCamera;
    private GameObject _FPSCamera;
    private bool _isFPSView = false;
    private bool _flashlightOn = false;

    [Header("Role Specs")]
    public Role currentRole;
    public float SpeedMult = 1f;
    public float JumpMult = 1f;
    private float _defaultHeight;
    private float _defaultCenter;

    [Header("Player Stats")]
    public bool isDefeated = false;
    public float maxHealth;
    
    public float health
    {
        get
        {
            return _health;
        }
    }
    private float _health;
    

    public bool isMoving
    {
        get
        {
            return !_isCrouching || controller.velocity.magnitude > 0.2f;
        }
    }
    private bool _isFPS = false;

    public int toolCount = 0;
    public int bulletCount = 0;
    private bool _isInvisible = false;

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
        _flashlightOn = !_flashlightOn;
        GameplayManager.instance.soundPlayer.Play3DSound("Flashlight", transform.position, 0.5f, 3f);
        flashlight.SetActive(!flashlight.activeInHierarchy);
    }

    [PunRPC]
    void Fire(float x = 0, float y = 0, float z = 0)
    {
        GameplayManager.instance.soundPlayer.Play3DSound("Laser", transform.position, 0.1f, 3f);
        GameObject bullet = Instantiate(Resources.Load("PlayerBullet") as GameObject, shootBarrel.position, Quaternion.identity);
        bullet.transform.LookAt(new Vector3(x, y, z));
        bulletCount--;
        bullet.name = photonPlayer.NickName;
        bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * 200f, ForceMode.Impulse);
        bullet.GetComponent<ProjectileBehavior>().owner = this;
        Destroy(bullet, 3f);
    }

    [PunRPC]
    void AddHealth(float amount)
    {
        if (_isInvisible) return;
        if (_health + amount <= 0)
        {
            FindObjectOfType<BattleRoyaleController>().KillPlayer();
            SetDefeated(500f);
        }
        _health = Mathf.Clamp(_health + amount, -1f, maxHealth);
        StartCoroutine(Invisible());
    }

    [PunRPC]
    void AddBullet(int amount)
    {
        bulletCount += amount;
    }

    private void Start()
    {
        flashlight.SetActive(false);
        GetComponent<Photon.Voice.Unity.UtilityScripts.MicAmplifier>().AmplificationFactor = 5;
        GetComponent<Photon.Voice.Unity.UtilityScripts.MicAmplifier>().BoostValue = 3;
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
        _health = maxHealth;
        _shootTime = 0f;
        _currentSpeed = normalSpeed;
        currentRole = Role.SPECTATOR;
    }

    private void Update()
    {
        // if (currentRole == Role.SEEKER && GameplayManager.instance.matchPhase == MatchPhase.HIDE) return;
        if (GameplayManager.instance.isGameOver) return;
        if (GameplayManager.instance._isChatting) return;

        if (photonPlayer == null || !photonPlayer.IsLocal) return;
       
        if (Input.GetKeyDown(KeyCode.F))
        {
            photonView.RPC("ToggleLight", RpcTarget.All);
        }
        // if (morphObject != null) morphObject.transform.position = feet.position;

        HandleNametag();
        HandleSprint();
        HandleCrouch();
        HandleShoot();
        HandleViewMode();

        Move();
        Jump();

        controller.Move((_moveDirection.normalized * _currentSpeed * SpeedMult + _velocityY * JumpMult * Vector3.up) * Time.deltaTime);
    }

    public void Morph(string name, GameObject morphObj)
    {
        photonView.RPC("MorphRPC", RpcTarget.All, name, morphObj.transform.position.y);
    }

    [PunRPC]
    public void MorphRPC(string objName, float y)
    {
        if (morphObject != null)
        {
            PhotonNetwork.Destroy(morphObject);
            morphObject = null;
        }
        morphObject = PhotonNetwork.Instantiate("Props\\" + "Prop_" + objName, transform.position, transform.rotation);
        morphObject.transform.position = new Vector3(feet.position.x, y, feet.position.z);
        morphObject.transform.SetParent(transform);
        
        morphObject.transform.localRotation = Quaternion.identity;
        morphObject.transform.localScale = new Vector3(1, 1, 1); ;
        morphObject.tag = "Player";
        morphObject.layer = LayerMask.NameToLayer("Default");
        ToggleFX(false);
        morphObject.GetComponent<Interactable>().enabled = false;
        GFX.GetComponent<Collider>().enabled = false;
        morphObject.GetComponent<Collider>().isTrigger = true;
        morphObject.GetComponent<Rigidbody>().isKinematic = true;
    }

    [PunRPC]
    void SetInvisibility()
    {
        StartCoroutine(Invisible());
    }

    IEnumerator Invisible()
    {
        _isInvisible = true;
        yield return new WaitForSeconds(1f);
        _isInvisible = false;
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
                ToggleFX(false);
                _TPSCamera.SetActive(false);
                _FPSCamera.SetActive(true);
            }
            else
            {
                ToggleFX(true);
                _TPSCamera.SetActive(true);
                _FPSCamera.SetActive(false);
            }
        }
    }

    private void HandleShoot()
    {
        if (bulletCount <= 0) return;
        if (currentRole == Role.SPECTATOR && _shootTime >= shootDelay 
            && (Input.GetMouseButton(0) || CrossPlatformInputManager.GetButton("Shoot")))
        {
            Vector3 position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, int.MaxValue);
            position = Camera.main.ScreenToWorldPoint(position);

            // Vector3 target = (position - shootBarrel.position).normalized;
            _shootTime = 0f;
            photonView.RPC("Fire", RpcTarget.All, position.x, position.y, position.z);
            
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

    public void SetDefeated(float time)
    {
        transform.position = GameplayManager.instance.defeatedRoom.position;
        isDefeated = true;
        StartCoroutine(DefeatState(time));
    }

    private void ToggleFX(bool state)
    {
        if (state)
        {
            foreach (Transform child in GFX.transform)
            {
                child.gameObject.SetActive(true);
            }
        } else
        {
            foreach (Transform child in GFX.transform)
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator DefeatState(float time)
    {
        yield return new WaitForSeconds(time);
        isDefeated = true;
        transform.position = GameplayManager.instance.startPosition.position;
    }

    public void AnnounceRole()
    {
        // currentRole = (Role)photonPlayer.CustomProperties["Role"];

        if (currentRole == Role.HIDER)
        {
            ModalWindowPanel.Instance.ShowModal("Match Started!", null, "You are the prey in this match! You have 60 seconds to find " +
                "a place to hide before the hunter wakes up! You can morph into object to blend in the scenery, but it is gameover if the " +
                "hunter touches you.", "Okay");
        }
        else if (currentRole == Role.SEEKER)
        {
            ModalWindowPanel.Instance.ShowModal("Match Started!", null, "You are the hunter in this match! You can start seeking the prey " +
                "after 60 seconds! They may be morphed into various objects and you can only find by touching them!", "Okay");
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
        if (GameplayManager.instance._isChatting) return;
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
        // if (!GFX.activeInHierarchy) return;
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


    private void OnTriggerEnter(Collider collision)
    {
        if (currentRole == Role.SEEKER)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                ThirdPersonMovement tpm = collision.gameObject.GetComponentInParent<ThirdPersonMovement>();
                if (tpm.currentRole == Role.HIDER && !tpm.isFound)
                {
                    //HideNSeekController controller = 
                    FindObjectOfType<HideNSeekController>().photonView.RPC("FoundPlayer", RpcTarget.All);
                    tpm.isFound = true;
                    tpm.SetDefeated(500f);
                }
            }
        }
    }
    
}
