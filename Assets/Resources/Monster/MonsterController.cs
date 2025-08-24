using UnityEngine;
using Photon.Pun;

public class MonsterController : MonoBehaviourPun, IPunObservable
{
    public float moveSpeed = 2f;
    private Vector2 direction = Vector2.down;
    private Vector3 networkPosition;

    [Header("�Ѿ� ����")]
    public string bulletPrefabPath = "Monster/MonsterBullet";
    public Transform firePoint;
    public float fireRate = 2f;
    private float fireTimer;

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            moveSpeed = Random.Range(1.5f, 4f);
            fireTimer = Random.Range(0f, fireRate);
        }
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            transform.Translate(direction * moveSpeed * Time.deltaTime);

            fireTimer += Time.deltaTime;
            if (fireTimer >= fireRate)
            {
                fireTimer = 0f;
                FireBullet();
            }

            // ������ Ŭ���̾�Ʈ�� �ı� ���
            if (transform.position.y < -7f)
            {
                if (photonView != null && photonView.IsMine)
                    PhotonNetwork.Destroy(gameObject);
            }
        }
        else
        {
            // ��ġ ����
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10f);
        }
    }

    void FireBullet()
    {
        if (firePoint == null) return;
        PhotonNetwork.Instantiate(bulletPrefabPath, firePoint.position, Quaternion.identity);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
        }
    }
}
