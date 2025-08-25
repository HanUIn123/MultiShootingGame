using UnityEngine;
using Photon.Pun;

public class Bullet : MonoBehaviourPun
{
    public float speed = 10f;
    public float lifeTime = 2f;
    public float damage = 10f; // 보스든 몬스터든 동일하게 줄 데미지

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
        // 일반 몬스터 처리
        if (other.CompareTag("Enemy"))
        {
            PhotonView enemyPV = other.GetComponent<PhotonView>();

            if (enemyPV != null)
            {
                photonView.RPC("RequestDestroyEnemy", RpcTarget.MasterClient, enemyPV.ViewID);
            }

            if (photonView != null && photonView.IsMine)
                PhotonNetwork.Destroy(gameObject);
        }

        // 보스 처리
        else if (other.CompareTag("Boss"))
        {
            BossController boss = other.GetComponent<BossController>();
            if (boss != null)
            {
                // 보스는 체력 시스템이 있으므로, 제거가 아니라 데미지 전달
                boss.photonView.RPC("TakeDamage", RpcTarget.MasterClient, damage);
            }

            if (photonView != null && photonView.IsMine)
                PhotonNetwork.Destroy(gameObject);
        }
    }

    // 몬스터 제거 요청 (마스터만 처리)
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
            Debug.LogWarning($"[무시] 이미 제거됐거나 권한 없음 - ViewID: {viewID}");
        }
    }
}
