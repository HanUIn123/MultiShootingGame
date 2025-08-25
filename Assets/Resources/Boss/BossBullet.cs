using UnityEngine;
using Photon.Pun;

public class BossBullet : MonoBehaviourPun
{
    public float speed = 5f;
    private Vector3 moveDirection = Vector3.down; // 기본 방향

    public void SetDirection(Vector3 dir)
    {
        moveDirection = dir.normalized;

        // 다른 클라이언트에게도 방향 전달
        photonView.RPC("RPC_SetDirection", RpcTarget.OthersBuffered, dir.x, dir.y, dir.z);
    }

    [PunRPC]
    void RPC_SetDirection(float x, float y, float z)
    {
        moveDirection = new Vector3(x, y, z).normalized;
    }

    void Start()
    {
        if (photonView != null && photonView.IsMine)
        {
            Invoke(nameof(SelfDestruct), 3f);
        }
    }

    void Update()
    {
        transform.Translate(moveDirection * speed * Time.deltaTime);
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
        if (!collision.CompareTag("Player")) return;

        var hp = collision.GetComponent<PlayerHealth>();
        if (hp != null)
        {
            hp.photonView.RPC("RPC_TakeDamage", hp.photonView.Owner, 10);
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
