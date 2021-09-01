using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityStandardAssets.CrossPlatformInput;
using Cinemachine;

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

    private float currentSpeed;
    private float turnSmoothVelocity;
    private Vector3 moveDirection;
    private float velocityY;
    private bool isGrounded = false;

    [HideInInspector]
    public int id;
    [Header("Component")]
    public Player photonPlayer;
    public TMP_Text playerNickName;

    [PunRPC]
    public void Initialize(Player player)
    {
        photonPlayer = player;
        id = player.ActorNumber;
        GameplayManager.instance.players[id - 1] = this;
        playerNickName.text = photonPlayer.NickName;

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
    }

    private void Start()
    {
        currentSpeed = Speed;
    }

    private void Update()
    {
        //isGrounded = Physics.OverlapSphere(GroundCheckPos.position, 0.05f, GroundMask).Length > 0;
        if (!photonPlayer.IsLocal) return;
        if (Input.GetKey(KeyCode.LeftShift)) currentSpeed = SprintSpeed;
        else if (Input.GetKey(KeyCode.LeftControl)) currentSpeed = CrouchSpeed;
        else currentSpeed = Speed;

        Move();
        Jump();

        Controller.Move((moveDirection.normalized * currentSpeed + velocityY * Vector3.up) * Time.deltaTime);
    }

    private void Move()
    {
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

    private void Jump()
    {
        if (Controller.isGrounded)
        {
            velocityY = 0;
            if (Input.GetKeyDown(KeyCode.Space))
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

}
