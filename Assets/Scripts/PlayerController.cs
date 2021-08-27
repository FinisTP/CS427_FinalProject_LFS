using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using UnityStandardAssets.CrossPlatformInput;
public class PlayerController : MonoBehaviourPunCallbacks
{
    [Header("Player Specs")]
    [SerializeField] GameObject cameraHolder;
    [SerializeField] float mouseSensitivity, sprintSpeed, walkSpeed, jumpForce, smoothTime, turnSmoothTime;
    [SerializeField] Transform groundDetectionPos;
    [SerializeField] LayerMask groundLayer;

    float verticalLookRotation;
    float turnSmoothVelocity;
    bool grounded;
    Vector3 smoothMovementVelocity;
    Vector3 moveAmount;

    [HideInInspector]
    public int id;
    [Header("Component")]
    public Rigidbody rig;
    public Player photonPlayer;
    public TMP_Text playerNickName;
    [SerializeField]
    private float speed = 0.2f;
    [PunRPC]
    public void Initialize(Player player)
    {
        photonPlayer = player;
        id = player.ActorNumber;
        speed = 0.2f;
        GameplayManager.instance.players[id - 1] = this;
        if (!photonView.IsMine)
        {
            rig.isKinematic = true;
        }
    }
    private void Start()
    {
        speed = 0.2f;
        // rig.isKinematic = true;
        playerNickName.text = photonPlayer.NickName;

        if (!photonView.IsMine)
        {
            cameraHolder.gameObject.SetActive(false);
            //Destroy(GetComponentInChildren<Camera>());
        }
    }
    private void Update()
    {
        if (photonPlayer.IsLocal)
        {
            Movements();
            if (Input.GetKey(KeyCode.LeftControl) || CrossPlatformInputManager.GetButton("Shoot"))
            {
                photonView.RPC("Fire", RpcTarget.All);
            }
        }
    }

    void Look()
    {
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);
        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
        /*
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;
        if (direction.magnitude > 0)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
        }
        */
    }

    void Move()
    {
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed)
            , ref smoothMovementVelocity, smoothTime);
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            rig.AddForce(transform.up * jumpForce);
        }
    }

    void DetectGround()
    {
        grounded = Physics.OverlapSphere(groundDetectionPos.position, 0.2f, groundLayer).Length > 0;
    }

    private void FixedUpdate()
    {
        rig.MovePosition(rig.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }

    void Movements()
    {
        DetectGround();
        Look();
        Move();
        Jump();

        /*
        float horizontal = CrossPlatformInputManager.GetAxisRaw("Horizontal");
        float vertical = CrossPlatformInputManager.GetAxisRaw("Vertical");
        float hori = Input.GetAxis("Horizontal");
        float verti = Input.GetAxis("Vertical");
        if (horizontal != 0 || vertical != 0 || hori != 0 || verti != 0)
        {
            speed = 0.2f;
        }
        else
        {
            speed = 0;
        }
        if ((horizontal > 0 && vertical > 0) || (hori > 0 && verti > 0))
        {
            transform.localEulerAngles = new Vector3(0, 45, 0);
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
        else if ((horizontal > 0 && vertical < 0) || (hori > 0 && verti < 0))
        {
            transform.localEulerAngles = new Vector3(0, 135, 0);
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
        else if ((horizontal < 0 && vertical < 0) || (hori < 0 && verti < 0))
        {
            transform.localEulerAngles = new Vector3(0, -135, 0);
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
        else if ((horizontal < 0 && vertical > 0) || (hori < 0 && verti > 0))
        {
            transform.localEulerAngles = new Vector3(0, -45, 0);
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
        else if ((horizontal > 0 && vertical == 0) || (hori > 0 && verti == 0))
        {
            transform.localEulerAngles = new Vector3(0, 90, 0);
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
        else if ((horizontal < 0 && vertical == 0) || (hori < 0 && verti == 0))
        {
            transform.localEulerAngles = new Vector3(0, -90, 0);
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
        else if ((vertical > 0 && horizontal == 0) || (verti > 0 && hori == 0))
        {
            transform.localEulerAngles = new Vector3(0, 0, 0);
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
        else if ((vertical < 0 && horizontal == 0) || (verti < 0 && hori == 0))
        {
            transform.localEulerAngles = new Vector3(0, 180, 0);
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
        */
    }

    [PunRPC]
    void Fire()
    {
        GameObject bullet = Instantiate(Resources.Load("bullet", typeof(GameObject))) as GameObject;
        bullet.name = photonPlayer.NickName;
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        bullet.transform.localPosition = transform.position;
        bullet.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        rb.AddForce(this.transform.forward * 300f);
        Destroy(bullet, 1);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "bullet")
{
            if (other.name != photonPlayer.NickName)
            {
                Debug.Log("hit");
                StartCoroutine(PlayerColorChange());
            }
        }
    }
    IEnumerator PlayerColorChange()
    {
        this.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
        yield return new WaitForSeconds(2);
        this.gameObject.GetComponent<MeshRenderer>().material.color = Color.green;
    }
}