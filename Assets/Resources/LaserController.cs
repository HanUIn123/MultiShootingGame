using UnityEngine;
using Photon.Pun;

public class LaserController : MonoBehaviourPun
{
    public float                                            m_fLaserDuration= 3f;                       // 레이저 유지 시간
    public float                                            m_fDamagePerSecond = 100f;                  // 초당 데미지

    private BossController                                  targetBossController = null;
    private float                                           m_fDamageTickRate = 0.1f;                   // 데미지 주기 (0.1초마다)
    private float                                           m_fDamageTimer = 0f;

    private void Start()
    {
        GameSceneManager.CheckAndHideObject(this);

        Destroy(gameObject, m_fLaserDuration);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!photonView.IsMine) 
            return;

        if (collision.CompareTag("Enemy"))
        {
            PhotonView enemyPV = collision.GetComponent<PhotonView>();

            if (enemyPV != null)
            {
                enemyPV.RPC("TakeDamage", RpcTarget.MasterClient, 9999f);
            }
        }
        else if (collision.CompareTag("Boss"))
        {
            BossController bossController = collision.GetComponent<BossController>();

            if (bossController != null)
            {
                targetBossController = bossController;
                bossController.photonView.RPC("StartHitFlashLoop", RpcTarget.All); // 반짝이기 시작
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!photonView.IsMine) 
            return;

        if (collision.CompareTag("Boss") && targetBossController != null)
        {
            m_fDamageTimer += Time.deltaTime;

            if (m_fDamageTimer >= m_fDamageTickRate)
            {
                float fLaserDamage = m_fDamagePerSecond * m_fDamageTickRate;  // 이 틱에 줄 데미지

                targetBossController.photonView.RPC("TakeDamage", RpcTarget.MasterClient, fLaserDamage);
                m_fDamageTimer = 0f;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!photonView.IsMine) 
            return;

        if (collision.CompareTag("Boss") && targetBossController != null)
        {
            targetBossController.photonView.RPC("StopHitFlashLoop", RpcTarget.All);
            targetBossController = null;
        }
    }

    private void OnDestroy()
    {
        if (photonView.IsMine && targetBossController != null)
        {
            targetBossController.photonView.RPC("StopHitFlashLoop", RpcTarget.All);
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
