using UnityEngine;
using Photon.Pun;

public class Bullet : MonoBehaviourPun
{
    public float speed = 10f;
    public float lifeTime = 2f;
    public float damage = 10f; // ������ ���͵� �����ϰ� �� ������

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
        // �Ϲ� ���� ó��
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

        // ���� ó��
        else if (other.CompareTag("Boss"))
        {
            BossController boss = other.GetComponent<BossController>();
            if (boss != null)
            {
                // ������ ü�� �ý����� �����Ƿ�, ���Ű� �ƴ϶� ������ ����
                boss.photonView.RPC("TakeDamage", RpcTarget.MasterClient, damage);
            }

            if (photonView != null && photonView.IsMine)
                PhotonNetwork.Destroy(gameObject);
        }
    }

    // ���� ���� ��û (�����͸� ó��)
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
            Debug.LogWarning($"[����] �̹� ���ŵưų� ���� ���� - ViewID: {viewID}");
        }
    }
}
