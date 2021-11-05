using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Result : MonoBehaviour
{
    public GameObject result;
    public Text winOrLose;
    public Text player1Name;
    public Text player2Name;
    public Text player1KillCount;
    public Text player2KillCount;
    private bool answer;
    private int myKillCount;
    private int eneKillCount;
    private PhotonView view;

    public void ResultDisplay(int killCount)//自分のデス数
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)//相手が回線落ちしたとき(いないとき)
        {
            winOrLose.text = "Win";
            player1Name.text = PhotonNetwork.LocalPlayer.NickName;
            result.SetActive(true);
            return;
        }
        view = GetComponent<PhotonView>();
        view.RPC("KillCount", RpcTarget.Others, killCount);
        player1Name.text = PhotonNetwork.PlayerList[0].NickName;
        player2Name.text = PhotonNetwork.PlayerList[1].NickName;
        eneKillCount = killCount;
        if (PhotonNetwork.LocalPlayer.ActorNumber == 1)//相手のキル数の設定(自分のデス数)
            player2KillCount.text = eneKillCount.ToString();
        else
            player1KillCount.text = eneKillCount.ToString();
        StartCoroutine(AnswerWait());
    }
    private IEnumerator AnswerWait()
    {
        yield return new WaitUntil(() => answer == true);//相手のキル数が来るまで待機

        if (PhotonNetwork.LocalPlayer.ActorNumber == 1)//自分のキル数の設定
            player1KillCount.text = myKillCount.ToString();
        else
            player2KillCount.text = myKillCount.ToString();
        if (myKillCount < eneKillCount)
        {
            winOrLose.text = "Lose";
        }
        else if (eneKillCount < myKillCount)
        {
            winOrLose.text = "Win";
        }
        else if (eneKillCount == myKillCount)
        {
            winOrLose.text = "Drow";
        }
        result.SetActive(true);
        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
    }
    [PunRPC]
    public void KillCount(int killCount)
    {
        answer = true;
        myKillCount = killCount;
    }
}