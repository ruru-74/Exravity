using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class GunController : MonoBehaviour, IPunObservable
{
    public Animator gunAnimator;
    public GameObject gunParticleObj;
    public GameObject baret;
    public GameObject firingObj;
    public Gun gun;

    private int remainingBullets;
    private int numberOfBullets = 300;
    private int maxBarets = 30;
    private float interval;
    private bool outOfAmmo;
    private bool reloadNow;
    public bool endReload;

    private GameObject barets;
    private Text remainingBulletsText;
    private Text numberOfBulletsText;
    private ParticleSystem gunParticle;
    private PhotonView view;
    private void Start()
    {
        interval = gun.maxInterval;
        barets = GameObject.FindWithTag("Barets");
        remainingBulletsText = GameObject.Find("RemainingBarets").GetComponent<Text>();
        numberOfBulletsText = GameObject.Find("NumberOfBarets").GetComponent<Text>();
        view = GetComponent<PhotonView>();
        remainingBullets = maxBarets;
        remainingBulletsText.text = remainingBullets.ToString();
    }
    private void Update()
    {
        if (Input.GetMouseButton(0) && GameController.isMovePossible && !reloadNow)
        {
            if (0 < remainingBullets)
            {
                PlayerController.isShooting = true;
                interval -= Time.deltaTime;
                if (interval < 0)
                {
                    remainingBullets--;
                    remainingBulletsText.text = remainingBullets.ToString();
                    //Rayで当たり判定をするオブジェクトを飛ばす
                    GameObject bareObj = Instantiate(baret, firingObj.transform.position, Camera.main.transform.rotation, barets.transform);
                    var bare = bareObj.GetComponent<Baret>();//Rayのオブジェクトの設定
                    bare.speed = gun.speed;
                    bare.generator = transform.parent.GetComponentInChildren<PlayerHelth>();
                    bare.isMine = true;
                    Destroy(bareObj, 3f);//3秒後削除

                    Vector3 p = firingObj.transform.position;
                    Vector3 r = Camera.main.transform.rotation.eulerAngles;
                    view.RPC("BaretAsync", RpcTarget.Others,
                        p.x, p.y, p.z,
                        r.x, r.y, r.z
                        , gun.speed);

                    gunParticle.Play();
                    gunAnimator.SetBool("Shoot", true);
                    interval = gun.maxInterval;
                }
            }
            if (remainingBullets != 30)//弾薬が最大でなければ
            {
                outOfAmmo = true;
            }
            if (remainingBullets <= 0)
            {
                gunParticle.Stop();
                gunAnimator.SetBool("Shoot", false);
            }
        }
        if (Input.GetKeyDown(KeyCode.R) && outOfAmmo)
        {
            print("あろーり");
            StartCoroutine(ReLoad());
        }
        if (Input.GetMouseButtonUp(0))
        {
            interval = 0;
            PlayerController.isShooting = false;
            gunParticle.Stop();
            gunAnimator.SetBool("Shoot", false);
        }
    }
    private IEnumerator ReLoad()
    {
        gunAnimator.SetTrigger("DoReload");
            gunParticle.Stop();
        reloadNow = true;
        yield return new WaitUntil(() => endReload == true);
        endReload = reloadNow = false;
        numberOfBullets -= 30 - remainingBullets;
        remainingBullets = maxBarets;
        numberOfBulletsText.text = numberOfBullets.ToString();
        remainingBulletsText.text = remainingBullets.ToString();
    }
    public void SetGun()
    {
        gunParticle = gunParticleObj.GetComponent<ParticleSystem>();
    }
    [PunRPC]
    private void BaretAsync(float posx, float posy, float posz, float rotx, float roty, float rotz, float speed)
    {
        Vector3 pos = new Vector3(posx, posy, posz);
        Quaternion rot = Quaternion.Euler(new Vector3(rotx, roty, rotz));
        GameObject bareObj = Instantiate(baret, pos, rot, GameObject.FindWithTag("Barets").transform);
        bareObj.GetComponent<Baret>().speed = speed;
        Destroy(bareObj, 3f);
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}