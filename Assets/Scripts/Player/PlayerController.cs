using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using Photon.Pun;
using Photon.Realtime;
public class PlayerController : MonoBehaviour
{
    public GameObject axisObj;
    public float verticalSpeed;
    public float horizontalSpeed;
    public float shootingSpeed;
    public float jumpPower;
    public float mouseXSpeed;
    public float mouseYSpeed;
    public int maxRotateValue = 180;

    public static bool isShooting;//射撃中か

    public int MyState { get; set; }
    public bool IsGround { get; set; }

    private Animator animator;
    private Rigidbody rb;
    private Transform hed;
    private Transform body;


    private int gravity;
    private float nextRotate;
    private float nowRotate;
    private float time;

    private bool isRun, isIdle, isRunLeft, isRunRight;
    private bool isRot;
    private bool jumpKey;

    private Vector3 rotAxis;
    private Vector3 NewAngle;
    private Vector3 resPos;
    private Vector3 resRot;

    private void Start()
    {
        //プレイヤーの初期化
        this.tag = "Player";//タグ設定
        this.transform.parent.tag = "Allies";
        SetLayer(transform.parent.gameObject, 8);//レイヤー設定

        var gun = GetComponentInChildren<GunController>();
        gun.SetGun();
        transform.parent.GetComponent<ColliderCheck>().player = this;//ColliderCheckにプレイヤーを設定
        animator = transform.parent.GetComponent<Animator>();
        rb = transform.parent.GetComponent<Rigidbody>();

        GameObject camera = transform.parent.Find("Body/Hed/Main Camera").gameObject;//カメラ取得
        camera.GetComponent<Camera>().enabled = true;//カメラアクティブ化
        camera.tag = "MainCamera";//タグ変更

        body = transform.parent.Find("Body").transform;
        hed = body.transform.Find("Hed").transform;
        if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
            MyState = 1;
        else if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
            MyState = -1;
        transform.parent.position = GameObject.FindWithTag("Generate").transform.GetChild(PhotonNetwork.LocalPlayer.ActorNumber - 1).position;//初期位置設定
        body.transform.localRotation = Quaternion.Euler(new Vector3(0, 90 * MyState, 0));

        resPos = GameObject.FindWithTag("Generate").transform.GetChild(PhotonNetwork.LocalPlayer.ActorNumber - 1).position;
        resRot = new Vector3(0, 90 * MyState, 0);
        PlayerPosReset();

        //重力の初期化
        gravity = -1;
        nextRotate = maxRotateValue;

        //感度設定
        mouseXSpeed = PlayerPrefs.GetFloat("MouseX");
        mouseYSpeed = PlayerPrefs.GetFloat("MouseY");

        GameObject.FindWithTag("GameController").GetComponent<Settings>().pc = this;
        void SetLayer(GameObject self, int layer)
        {
            self.layer = layer;

            foreach (Transform n in self.transform)
            {
                if (n.CompareTag("Gun") || n.name == "Particle")
                    n.gameObject.layer = 0;
                else
                    SetLayer(n.gameObject, layer);
            }
        }
    }
    private void Update()
    {
        jumpKey = Input.GetKeyDown(KeyCode.Space);
    }
    private void FixedUpdate()
    {
        if (GameController.isMovePossible)
        {
            Mouse();
            Player();
            Physic();
        }
    }
    private void Player()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        {
            if (0 < z)
            {
                if (!isRun)
                {
                    animator.SetTrigger("Run");
                    isRun = true;
                    isIdle = false;
                }
            }

            else if (0 < x)
            {
                if (!isRunLeft)
                {
                    animator.SetTrigger("RunRight");
                    isRunLeft = true;
                    isRunRight = isIdle = false;
                }
            }
            else if (x < 0)
            {
                if (!isRunRight)
                {
                    animator.SetTrigger("RunLeft");
                    isRunLeft = isIdle = false;
                    isRunRight = true;
                }
            }
            else if (x == 0 && z == 0 && !isIdle)
            {
                animator.SetTrigger("Idle");
                isIdle = true;
                isRun = isRunLeft = isRunRight = false;
            }
        }
        if (isShooting && IsGround)//地面にいて射撃中ならば
        {
            x *= shootingSpeed;
            z *= shootingSpeed;
            print("isGround&&IsShoting");
        }
        else
        {
            x *= horizontalSpeed;
            z *= verticalSpeed;
        }
        float y = 0;
        if (jumpKey)//ジャンプキーを入力されていたら
        {
            if (gravity == 1)
            {
                y = jumpPower;
            }
            else
            {
                y = jumpPower;
            }
        }
        x *= Time.deltaTime;
        z *= Time.deltaTime;
        y *= Time.deltaTime;
        Vector3 plaDir = new Vector3(0, body.rotation.eulerAngles.y, transform.parent.rotation.eulerAngles.z);
        Vector3 velocity = Quaternion.Euler(plaDir) * (new Vector3(x, 0, z));
        rb.AddForce(new Vector3(0, y * -gravity, 0));
        this.transform.parent.position += velocity;
    }
    private void Mouse()
    {

        NewAngle.y = Input.GetAxis("Mouse X") * mouseXSpeed;
        NewAngle.x = Input.GetAxis("Mouse Y") * mouseYSpeed;
        NewAngle.z = 0;
        NewAngle *= 10;
        NewAngle *= Time.deltaTime;

        NewAngle = Camera.main.transform.localRotation.normalized * NewAngle;
        body.Rotate(0, NewAngle.y, 0);
        hed.Rotate(-NewAngle.x, 0, 0);
    }
    private void Physic()
    {
        if (Input.GetKeyDown(KeyCode.F) && !isRot)
        {
            isRot = true;
            nowRotate = nextRotate;
            rotAxis = axisObj.transform.forward;
            rotAxis.y = 0;
            if (gravity == -1)
            {
                nextRotate = 0;
                gravity = 1;
            }
            else if (gravity == 1)
            {
                nextRotate = maxRotateValue;
                gravity = -1;
            }
        }
        if (isRot)
        {
            if (maxRotateValue < time)
            {
                isRot = false;
                time = 0;
                Vector3 newRot = transform.parent.rotation.eulerAngles;
                newRot.z = nowRotate;
                newRot.x = 0;
                transform.parent.localRotation = Quaternion.Euler(newRot);
                return;
            }
            transform.parent.RotateAround(Camera.main.transform.position, rotAxis, 270 * Time.deltaTime);
            time += 270 * Time.deltaTime;

        }
        rb.AddForce(new Vector3(0, gravity * 9.8f));//重力
    }
    public void PlayerPosReset()
    {
        transform.parent.position = resPos;
        body.transform.localRotation = Quaternion.Euler(resRot);
        transform.parent.rotation = Quaternion.Euler(Vector3.zero);
        gravity = -1;
    }
}