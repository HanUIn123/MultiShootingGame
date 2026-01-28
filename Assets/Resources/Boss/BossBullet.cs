using UnityEngine;
using Photon.Pun;

public class BossBullet : MonoBehaviourPun
{
    public float                                                            m_fBossBulletSpeed= 5f;
    private Vector3                                                         m_fBossBulletmoveDirection = Vector3.down;

    [Header("보스 총알 세팅")]
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

    void Start()
    {
        GameSceneManager.CheckAndHideObject(this);

        if (photonView != null && photonView.IsMine)
        {
            Invoke(nameof(SelfDestruct), 3f);
        }
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
        else if (gameObject != null)
        {
            Destroy(gameObject);
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
        else
        {
            Destroy(gameObject);
        }
    }
}
