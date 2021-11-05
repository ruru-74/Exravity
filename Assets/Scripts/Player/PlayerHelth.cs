using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerHelth : MonoBehaviour, IPunObservable
{
    public int Hp;
    private PhotonView view;
    private void Start()
    {
        view = GetComponent<PhotonView>();
        Hp = 100;
    }
    public void Attack(int damage)
    {
            view.RPC("Hit", RpcTarget.Others, damage);
    }
    [PunRPC]
    private void Hit(int damage)
    {
        if (!GameController.isHitPossible)
            return;
        print("hit");
        Hp -= damage;
        GameObject.FindWithTag("HPText").GetComponent<Text>().text = Hp + "/100";

        GameObject hpSlider = GameObject.FindWithTag("HPSlider");
        hpSlider.GetComponent<Slider>().value = Hp;
        hpSlider.transform.Find("Fill Area/HPFrontGround").GetComponent<HPGaugeColorChange>().Damage(Hp);

        if (Hp <= 0)
        {
            print("死んだ");
            Hp = 100;
            StartCoroutine(ReSpone());
        }
    }
    public IEnumerator ReSpone()
    {
        Text countDown = GameObject.FindWithTag("CountDown").GetComponent<Text>();
        GameObject countDownObj = GameObject.Find("Canvas").transform.Find("Countdown").gameObject;
        Image fadeImage = GameObject.Find("Canvas").transform.Find("FadeImage").GetComponent<Image>();
        countDown.text = "Deth";
        GameController.isMovePossible = GameController.isHitPossible = false;
        GameController.dethCount++;

        fadeImage.gameObject.SetActive(true);
        float alpha = 0;
        while (alpha <= 1)//暗転
        {
            yield return null;
            alpha += Time.deltaTime / 1;
            fadeImage.color = new Color(0, 0, 0, alpha);
        }//暗転
        Hp = 100;
        yield return new WaitForSeconds(1f);

        var p_c = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        p_c.PlayerPosReset();//プレイヤーの座標リセット
        var particle = GameObject.FindWithTag("Allies").transform.Find("Particle").GetComponent<ParticleSystem>();
        particle.Play();
        view.RPC("Particle", RpcTarget.Others, true);
        GameController.isHitPossible = true;
        Hit(0);
        GameController.isHitPossible = false;
        alpha = 1;
        while (0 <= alpha)
        {
            yield return null;
            alpha -= Time.deltaTime / 1;
            fadeImage.color = new Color(0, 0, 0, alpha);
        }//明転
        fadeImage.gameObject.SetActive(false);


        countDownObj.SetActive(true);
        countDown.text = "3";
        yield return new WaitForSeconds(1f);
        countDown.text = "2";
        yield return new WaitForSeconds(1f);
        countDown.text = "1";
        yield return new WaitForSeconds(1f);
        countDown.text = "Start";
        GameController.isMovePossible = true;

        yield return new WaitForSeconds(1f);
        countDown.text = "";
        countDownObj.SetActive(false);
        yield return new WaitForSeconds(5f);
        //無敵時間のカウントダウン表示
        GameController.isHitPossible = true;
        particle.Stop();
        view.RPC("Particle", RpcTarget.Others, false);
    }
    [PunRPC]
    private void Particle(bool b)
    {
        if (b)
        {
            GameObject.FindWithTag("Enemy").transform.Find("Particle").GetComponent<ParticleSystem>().Play();
        }
        else
        {
            GameObject.FindWithTag("Enemy").transform.Find("Particle").GetComponent<ParticleSystem>().Stop();
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}