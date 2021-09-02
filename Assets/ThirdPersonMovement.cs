using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityStandardAssets.CrossPlatformInput;
using Cinemachine;

public enum Role {
    Hider,
    Seeker,
    Spectator
}

public class ThirdPersonMovement : MonoBehaviourPunCallbacks
{
    public CharacterController Controller;
    public float Speed = 6f;
    public float TurnSmoothTime = 0.1f;
    public Transform Cam;
    public float JumpSpeed = 10f;
    public Transform GroundCheckPos;
    public LayerMask GroundMask;
    public float SprintSpeed = 12f;
    public float CrouchSpeed = 3f;
    public Role CurrentRole;
    public float ShootDelay = 0.5f;
    public Transform Barrel;
    public GameObject Flashlight;
    public float SpeedMult = 1f;
    public float JumpMult = 1f;

    private float currentSpeed;
    private float turnSmoothVelocity;
    private Vector3 moveDirection;
    private float velocityY;

    public bool Controlled = false;

    private float shootTime;
    

    public int id;
    [Header("Component")]
    public Player photonPlayer = null;
    public TMP_Text playerNickName;

    private bool inited = false;
    private bool isGrounded = false;

    private void Awake()
    {
        
    }

    [PunRPC]
    public void Initialize(Player player)
    {
        //print("Inited" + player.ActorNumber + " size: " + GameplayManager.instance.players.Length);
        //print(player.NickName);
        photonPlayer = player;
        id = player.ActorNumber;
        GameplayManager.instance.players[id - 1] = this;
        playerNickName.text = photonPlayer.NickName;
        // print("Initialized player");
        if (photonView.IsMine)
        {
            
        }
        if (photonPlayer.IsLocal)
        {
            Cam = Camera.main.transform;
            CinemachineFreeLook cine = GameObject.FindObjectOfType<CinemachineFreeLook>();
            cine.Follow = transform;
            cine.LookAt = transform;
        }

        ModalWindowPanel.Instance.ShowModal("Welcome to the game", null, "Use WASD to move, Mouse to look around, and Space to jump! " +
                "The host can start the game by standing on the portal.", "Okay");
    }

    private void Start()
    {
        // print("Inited");
        // if (!photonView.IsMine) Destroy(gameObject);
        // if (GameplayManager.instance == null) Destroy(gameObject);

        // if (photonPlayer == null) return;
        Flashlight.SetActive(false);
        shootTime = 0f;
        currentSpeed = Speed;
        // photonView.ViewID = 8;
        CurrentRole = Role.Spectator;
        gameObject.transform.parent = GameplayManager.instance.transform;
    }

    public void StartPhase()
    {
        if (CurrentRole == Role.Hider)
        {
            ModalWindowPanel.Instance.ShowModal("Match Started!", null, "You are the prey in this match! You have 60 seconds to find " +
                "a place to hide before the hunter wakes up!", "Okay");
        } 
        else if (CurrentRole == Role.Seeker)
        {
            ModalWindowPanel.Instance.ShowModal("Match Started!", null, "You are the hunter in this match! You can start seeking the prey " +
                "after 60 seconds!", "Okay");
        }
    }
    public void GrantSeekerBuff()
    {
        SpeedMult = 2f;
        JumpMult = 2f;
    }

    private void Update()
    {
        if (photonPlayer == null) return;
        //isGrounded = Physics.OverlapSphere(GroundCheckPos.position, 0.05f, GroundMask).Length > 0;
        if (!photonPlayer.IsLocal) return;
        if (Input.GetKey(KeyCode.LeftShift)) currentSpeed = SprintSpeed;
        else if (Input.GetKey(KeyCode.LeftControl)) currentSpeed = CrouchSpeed;
        else currentSpeed = Speed;

        if (Input.GetKeyDown(KeyCode.F))
        {
            photonView.RPC("ToggleLight", RpcTarget.All, photonPlayer);
        }

        Move();
        Jump();

        Controller.Move((moveDirection.normalized * currentSpeed * SpeedMult + velocityY * JumpMult * Vector3.up) * Time.deltaTime);

        if (CurrentRole == Role.Seeker && shootTime >= ShootDelay && (Input.GetKey(KeyCode.LeftControl) || CrossPlatformInputManager.GetButton("Shoot")))
        {
            shootTime -= ShootDelay;
            photonView.RPC("Fire", RpcTarget.All);
        }

    }

    [PunRPC]
    public void ToggleLight(Player player)
    {
        ThirdPersonMovement togglePlayer = GameplayManager.instance.players[player.ActorNumber - 1];
        if (togglePlayer.Controlled)
        {
            GameObject light = togglePlayer.GetComponent<ThirdPersonMovement>().Flashlight;
            light.SetActive(!light.activeInHierarchy);
        }
        
    }

    private void Move()
    {
        if (CurrentRole == Role.Seeker && GameplayManager.instance.CurrentMatchPhase == MatchPhase.Hide) return;
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + Cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, TurnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }
        else moveDirection = Vector3.zero;
    }

    private void FixedUpdate()
    {
        if (photonPlayer == null) return;
        isGrounded = Controller.isGrounded;
        shootTime += Time.fixedDeltaTime;
        
    }

    private void Jump()
    {
        if (isGrounded)
        {
            velocityY = 0;
            if (Input.GetKey(KeyCode.Space))
            {
                velocityY = JumpSpeed;
            }
         
        } else
        {
            velocityY += -9.81f * 2 * Time.deltaTime;
        }
        
        
        // print(velocityY);
       //  Controller.Move(Vector3.up * velocityY * Time.deltaTime);
    }

    [PunRPC]
    void Fire()
    {
        GameObject bullet = Instantiate(Resources.Load("bullet", typeof(GameObject))) as GameObject;
        bullet.name = photonPlayer.NickName;
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        bullet.transform.localPosition = Barrel.position;
        rb.AddForce(this.transform.forward * 100f, ForceMode.Impulse);
        Destroy(bullet, 5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.name != photonPlayer.NickName)
            {
                Debug.Log("hit");
                // StartCoroutine(PlayerColorChange());
            }
        } else if (other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }

}
