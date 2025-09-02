using UnityEngine;
using Photon.Pun;

public class MonsterBullet : MonoBehaviourPun
{
    public float bulletSpeed = 5f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.down * bulletSpeed;

        if (photonView != null && photonView.IsMine)
            Invoke(nameof(SelfDestruct), 3f);
    }

    void SelfDestruct()
    {
        if (photonView != null && photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);

        else if (gameObject != null)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) 
            return;

        var PlayerHelath= collision.GetComponent<PlayerHealth>();

        if (PlayerHelath != null)
        {
            // PlayerHelath.photonView.Owner : 본인의 클라이언트에서의 기준 주인공.
            PlayerHelath.photonView.RPC("RPC_TakeDamage", PlayerHelath.photonView.Owner, 10);
        }

        if (photonView != null && photonView.IsMine && gameObject != null)
            PhotonNetwork.Destroy(gameObject);
        else
            Destroy(gameObject);
    }
}
