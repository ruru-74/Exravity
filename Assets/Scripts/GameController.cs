using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using Photon.Pun;
using Photon.Realtime;
public class GameController : MonoBehaviourPunCallbacks
{
    public GameObject playerAnim;
    public GameObject endCamera;
    public GameObject subCamera;
    public GameObject reticl;
    public GameObject pauseObj;
    public GameObject networkError;
    public GameObject countDownObj;
    public GameObject gameUI;
    public Text countDown;
    public Text log;
    public Text timerText;
    public Image timerImage;
    public Image fadeImage;
    public Settings settings;

    public float fadeSpeed;
    public static bool gameNow;
    public static bool isHitPossible;
    public static bool isMovePossible;
    public static int dethCount;
    public static int killCount;

    private float time;
    private bool pause;
    private GameObject player;
    private GameObject aniObj;
    private void Start()
    {
        if (!PhotonNetwork.InRoom)
        {
            PhotonNetwork.OfflineMode = true;
            PhotonNetwork.JoinOrCreateRoom("11", new RoomOptions(), TypedLobby.Default);
        }
        time = 180;
        gameUI.SetActive(false);
        gameNow =isMovePossible = isHitPossible = false;
        dethCount = killCount = 0;
        player = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);//キャラクター生成
        player.transform.Find("PlayerController").gameObject.SetActive(true);//生成したキャラクターのコントローラーアクティブ化
        player.SetActive(false);


        aniObj = Instantiate(playerAnim);
        aniObj.transform.position = GameObject.FindWithTag("Generate").transform.GetChild(PhotonNetwork.LocalPlayer.ActorNumber - 1).position;
        var p = aniObj.transform.position;
        p.y += 0.7f;
        aniObj.transform.position = p;
        int MyState = 0;
        if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
            MyState = 1;
        else if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
            MyState = -1;
        aniObj.transform.rotation = Quaternion.Euler(new Vector3(0, 90 * MyState, 0));
        subCamera.transform.rotation = Quaternion.Euler(new Vector3(10, -90 * MyState, 0));
        subCamera.transform.position = new Vector3(-14 * MyState, 1.7f, 0);


        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        StartCoroutine(AnimationEnd());
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && gameNow)
        {
            pause = !pause;
            pauseObj.SetActive(pause);

            if (pause)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                isMovePossible = false;
                settings.Set();
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                isMovePossible = true;
            }
        }
        if (gameNow)
        {
            time -= Time.deltaTime;
            int minutes = (int)time / 60;
            int seconds = (int)time % 60;
            timerText.text = minutes + ":" + seconds.ToString("D2");
            timerImage.fillAmount = time / 180f;
            if (time <= 0)
            {
                GameEnd();
            }
        }
    }
    private IEnumerator AnimationEnd()
    {
        yield return new WaitForSeconds(3.5f);//アニメーション終了までの時間
        fadeImage.gameObject.SetActive(true);
        float alpha = 0;
        while (alpha <= 1)
        {
            yield return null;
            alpha += Time.deltaTime / fadeSpeed;
            fadeImage.color = new Color(0, 0, 0, alpha);
        }
        WaitCountDown();
        yield return new WaitForSeconds(1f);
        alpha = 1;
        while (0 <= alpha)
        {
            yield return null;
            alpha -= Time.deltaTime / fadeSpeed;
            fadeImage.color = new Color(0, 0, 0, alpha);
        }
        fadeImage.gameObject.SetActive(false);
        //カウントダウン
        countDownObj.SetActive(true);
        countDown.text = "3";
        yield return new WaitForSeconds(1f);
        countDown.text = "2";
        yield return new WaitForSeconds(1f);
        countDown.text = "1";
        yield return new WaitForSeconds(1f);
        countDown.text = "Start";
        isMovePossible = isHitPossible = true;
        gameNow = true;
        yield return new WaitForSeconds(1f);
        countDown.text = "";
        countDownObj.SetActive(false);
    }
    public void WaitCountDown()//開幕アニメーション後
    {
        subCamera.gameObject.SetActive(false);
        player.SetActive(true);
        aniObj.SetActive(false);
        gameUI.SetActive(true);
    }
    public void GameEnd()//ゲーム終了後
    {
        print("GameEnd");
        gameNow = false;
        isHitPossible = false;//攻撃を受けない
        endCamera.SetActive(true);
        gameUI.SetActive(false);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        GetComponent<Result>().ResultDisplay(dethCount);
    }
    public static int StateCheck(int playerID)
    {
        if (playerID == 1)
            return -1;
        else if (playerID == 2)
            return 1;
        else
            return 0;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError("通信が切断されました。\nプレイを続行出来ません");
        subCamera.gameObject.SetActive(true);
        networkError.SetActive(true);
        isMovePossible = false;//移動不可
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        isMovePossible = false;
    }
    public void Reconnect()
    {
        bool reconnect = PhotonNetwork.ReconnectAndRejoin();
        if (!reconnect)
        {
            log.text = "Reconnection failed";
            StartCoroutine(cor());
        }

        IEnumerator cor()
        {
            yield return new WaitForSeconds(3f);
            log.text = "";
        }
    }
    public void Respone()
    {
        StartCoroutine(player.GetComponentInChildren<PlayerHelth>().ReSpone());
        pauseObj.SetActive(false);
        pause = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void SceneLoad(string scene)
    {
        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }
}