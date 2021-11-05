using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
public class MatchingPun : MonoBehaviourPunCallbacks
{
    public string gameSceneName;//ゲームシーンの名前
    public GameObject input;//部屋情報入力画面
    public GameObject load;//マッチング待機画面

    public InputField playerName;//プレイヤー名入力
    public InputField roomId;//ルームID入力

    public GaussianBlur blueQuad;

    private int playerNumber;
    private bool sceneLoad;
    private AsyncOperation async;
    private Coroutine waitMatching;
    private PhotonView view;

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();//Photonサーバーに接続
        view = GetComponent<PhotonView>();
        blueQuad._blur = 400f;
        blueQuad.UpdateWeights();
        blueQuad.Blur();
    }
    public void Decision(int maxPlayerNumber)//確定ボタン押された時
    {
        print("Decision");
        blueQuad._blur = 50f;
        blueQuad.UpdateWeights();
        blueQuad.Blur();
        PhotonNetwork.LocalPlayer.NickName = playerName.text;//ニックネーム設定
        if (playerName.text == "" || playerName.text == null)
        {
            PhotonNetwork.LocalPlayer.NickName = "NoName";
        }
        ChangeScreen(load);
        if (!sceneLoad)
        {
            async = SceneManager.LoadSceneAsync(gameSceneName);//非同期シーンロード
            async.allowSceneActivation = false;//シーンロード後自動遷移しない
            sceneLoad = true;
        }
        waitMatching = StartCoroutine(WaitMatching(maxPlayerNumber));
    }
    public void Cancel()
    {
        if (waitMatching != null)
            StopCoroutine(waitMatching);
        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
        ChangeScreen(input);
    }
    private IEnumerator WaitMatching(int maxPlayerNumber)
    {
        print("waitMatching");
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);//マスターサーバー接続まで待機
        if (roomId.text == "" || roomId.text == null)
            roomId.text = "123456";

        PhotonNetwork.JoinOrCreateRoom(roomId.text, new RoomOptions(), TypedLobby.Default);

        yield return new WaitUntil(() => PhotonNetwork.InRoom);//ルーム接続まで待機
        print("IsMaster"+PhotonNetwork.IsMasterClient);
        while (true)
        {
            yield return null;
            if (0.9f <= async.progress)//ロード完了していて
            {
                if (PhotonNetwork.IsMasterClient)//マスターだったら
                {
                    yield return new WaitUntil(() => playerNumber == maxPlayerNumber - 1);//人数が揃ったら
                    view.RPC("LoadDone", RpcTarget.AllViaServer);
                }
                else
                {
                    view.RPC("ClientComplete", RpcTarget.MasterClient);
                }
                break;
            }
        }
    }
    [PunRPC]
    private void ClientComplete()//クライアント準備完了
    {
        playerNumber++;
    }
    [PunRPC]
    private void LoadDone()//マッチング,ロード完了
    {
        print("LoadDone");
        async.allowSceneActivation = true;
    }
    private void ChangeScreen(GameObject remainObj)
    {
        input.SetActive(false);
        load.SetActive(false);
        remainObj.SetActive(true);
    }
}