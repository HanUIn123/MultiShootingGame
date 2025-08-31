using UnityEngine;
using Photon.Pun;

public class LaserController : MonoBehaviourPun
{
    public float laserDuration = 3f;      // 레이저 유지 시간
    public float damagePerSecond = 100f;  // 초당 데미지

    private BossController targetBoss = null;
    private float damageTickRate = 0.1f;  // 데미지 주기 (0.1초마다)
    private float damageTimer = 0f;

    private void Start()
    {
        Destroy(gameObject, laserDuration);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!photonView.IsMine) return;

        if (collision.CompareTag("Enemy"))
        {
            PhotonView enemyPV = collision.GetComponent<PhotonView>();
            if (enemyPV != null)
            {
                photonView.RPC("RequestDestroyEnemy", RpcTarget.MasterClient, enemyPV.ViewID);
            }
        }
        else if (collision.CompareTag("Boss"))
        {
            BossController boss = collision.GetComponent<BossController>();
            if (boss != null)
            {
                targetBoss = boss;
                boss.photonView.RPC("StartHitFlashLoop", RpcTarget.All); // 반짝이기 시작
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!photonView.IsMine) return;

        if (collision.CompareTag("Boss") && targetBoss != null)
        {
            damageTimer += Time.deltaTime;

            if (damageTimer >= damageTickRate)
            {
                float damage = damagePerSecond * damageTickRate;  // 이 틱에 줄 데미지
                targetBoss.photonView.RPC("TakeDamage", RpcTarget.MasterClient, damage);
                damageTimer = 0f;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!photonView.IsMine) return;

        if (collision.CompareTag("Boss") && targetBoss != null)
        {
            targetBoss.photonView.RPC("StopHitFlashLoop", RpcTarget.All);
            targetBoss = null;
        }
    }

    private void OnDestroy()
    {
        if (photonView.IsMine && targetBoss != null)
        {
            targetBoss.photonView.RPC("StopHitFlashLoop", RpcTarget.All);
        }
    }

    [PunRPC]
    void RequestDestroyEnemy(int viewID)
    {
        PhotonView enemyPV = PhotonView.Find(viewID);
        if (enemyPV != null && enemyPV.IsMine)
        {
            PhotonNetwork.Destroy(enemyPV.gameObject);
        }
    }
}
