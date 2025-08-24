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

            // ������ Ŭ���̾�Ʈ���� ���� ���� ��û
            if (enemyPV != null)
            {
                photonView.RPC("RequestDestroyEnemy", RpcTarget.MasterClient, enemyPV.ViewID);
            }

            // �� �Ѿ� ����
            if (photonView != null && photonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }

    // �����Ͱ� ȣ����ϴ� �Լ�
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
            // �̹� ���ŵưų� ���� ���� ? ����
            Debug.LogWarning($"[����] �̹� ���ŵưų� �� ���� �ƴ� - ViewID: {viewID}");
        }
    }
}
