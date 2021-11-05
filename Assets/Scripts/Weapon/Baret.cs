using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Baret : MonoBehaviour
{
    public float speed;
    public PlayerHelth generator;
    public bool isMine=false;

    private Vector3 prepos;

    private void Start()
    {
        prepos = transform.position;
    }
    private void Update()
    {
        transform.position += transform.rotation * (new Vector3(0, 0, speed)*Time.deltaTime);
        if (!isMine) return;//生成者でなければこれ以降の処理を実行しない
        Vector3 pos = transform.position;
        Ray ray = new Ray(prepos, (pos - prepos).normalized);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, (pos - prepos).magnitude, 1 << 9))//レイヤー9に当たったら
        {
            generator.Attack(5);//敵のHPを減らして
            Destroy(this.gameObject);//自分自身(弾)を消す
        }
        else if (Physics.Raycast(ray, out hit, (pos - prepos).magnitude))
        {
            Destroy(this.gameObject);
        }
        prepos = pos;
    }
}