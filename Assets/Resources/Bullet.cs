using UnityEngine;
using Photon.Pun;

public class Bullet : MonoBehaviourPun
{
    public float speed = 10f;
    public float lifeTime = 2f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            PhotonView enemyPV = other.GetComponent<PhotonView>();

            // 마스터 클라이언트에게 몬스터 제거 요청
            if (enemyPV != null)
            {
                photonView.RPC("RequestDestroyEnemy", RpcTarget.MasterClient, enemyPV.ViewID);
            }

            // 내 총알 제거
            if (photonView != null && photonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }

    // 마스터가 호출당하는 함수
    [PunRPC]
    void RequestDestroyEnemy(int viewID)
    {
        PhotonView enemyPV = PhotonView.Find(viewID);
        if (enemyPV != null && enemyPV.IsMine)
        {
            PhotonNetwork.Destroy(enemyPV.gameObject);
        }
        else
        {
            // 이미 제거됐거나 권한 없음 ? 무시
            Debug.LogWarning($"[무시] 이미 제거됐거나 내 소유 아님 - ViewID: {viewID}");
        }
    }
}
