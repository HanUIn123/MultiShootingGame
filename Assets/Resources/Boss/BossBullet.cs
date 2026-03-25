using UnityEngine;
using Photon.Pun;

public class BossBullet : MonoBehaviourPun
{
    public float                                                            m_fBossBulletSpeed= 5f;
    private Vector3                                                         m_fBossBulletmoveDirection = Vector3.down;

    [Header("º¸½º ÃÑ¾Ë ¼¼ÆÃ")]
    [SerializeField]
    private float                                                           m_fBossBulletDamage = 20.0f;

    public void SetDirection(Vector3 v3Direction)
    {
        m_fBossBulletmoveDirection = v3Direction.normalized;

        photonView.RPC("RPC_SetDirection", RpcTarget.OthersBuffered, v3Direction.x, v3Direction.y, v3Direction.z);
    }

    [PunRPC]
    void RPC_SetDirection(float fX, float fY, float fZ)
    {
        m_fBossBulletmoveDirection = new Vector3(fX, fY, fZ).normalized;
    }

    void OnEnable()
    {
        // 풀에서 꺼내질 때마다 실행됨
        if (photonView != null && photonView.IsMine)
        {
            // 이전 예약이 남아있을 수 있으니 취소하고 새로 예약
            CancelInvoke(nameof(SelfDestruct));
            
            Invoke(nameof(SelfDestruct), 3f);
        }
    }

    void Start()
    {
        GameSceneManager.CheckAndHideObject(this);
    }

    void Update()
    {
        transform.Translate(m_fBossBulletmoveDirection * m_fBossBulletSpeed * Time.deltaTime);
    }

    private void SelfDestruct()
    {
        if (photonView != null && photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) 
            return;

        var PlayerHelath = collision.GetComponent<PlayerHealth>();

        if (PlayerHelath != null)
        {
            PlayerHelath.photonView.RPC("RPC_TakeDamage", PlayerHelath.photonView.Owner, m_fBossBulletDamage);
        }

        if (photonView != null && photonView.IsMine && gameObject != null)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
