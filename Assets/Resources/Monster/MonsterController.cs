using UnityEngine;
using Photon.Pun;

public class MonsterController : MonoBehaviourPun, IPunObservable
{
    public float moveSpeed = 2f;
    private Vector2 direction = Vector2.down;
    private Vector3 networkPosition;

    [Header("총알 관련")]
    public string bulletPrefabPath = "Monster/MonsterBullet";
    public Transform firePoint;
    public float fireRate = 2f;  // 발사 간격
    private float fireTimer;

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            moveSpeed = Random.Range(1.5f, 4f);
            fireTimer = Random.Range(0f, fireRate); // 발사 간격 랜덤하게 오프셋 주기
        }
    }

    private void Update()
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

            if (transform.position.y < -7f)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10f);
        }
    }

    private void FireBullet()
    {
        if (firePoint == null)
        {
            Debug.LogWarning("[MonsterController] firePoint가 비어있습니다.");
            return;
        }

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
