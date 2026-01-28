using UnityEngine;
using Photon.Pun;

public class MonsterBullet : MonoBehaviourPun
{
    [Header("몬스터 발사체 설정")]
    [SerializeField] private float              m_fBulletSpeed = 5f;
    [SerializeField] private float              m_fBulletDamage = 10f;
    [SerializeField] private float              m_fBulletLifeTime = 3f;

    private Rigidbody2D                         compRigidBody2D;

    private void Awake()
    {
        compRigidBody2D = GetComponent<Rigidbody2D>();
    }

    // 오브젝트 풀링을 쓸 때는 start 대신 OnEnable 이 핵심이란다..
    private void OnEnable()
    {
        GameSceneManager.CheckAndHideObject(this);

        Vector2 v2ShootDirection = Vector2.down;

        // 모든 플레이어들을 찾아, 가까운 녀석을 타겟팅.
        PlayerController[] allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        float fClosestDistance = float.MaxValue;

        Transform pTarget = null;

        foreach (var player in allPlayers)
        {
            float fDistance = Vector2.Distance(transform.position, player.transform.position);

            if (fDistance < fClosestDistance)
            {
                fClosestDistance = fDistance;

                pTarget = player.transform;
            }
        }

        if (pTarget != null)
        {
            v2ShootDirection = (pTarget.position - transform.position).normalized;

            float fBulletAngle = Mathf.Atan2(v2ShootDirection.y, v2ShootDirection.x) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(0, 0, fBulletAngle + 90f);
        }

        if (compRigidBody2D != null)
        {
            compRigidBody2D.linearVelocity = v2ShootDirection * m_fBulletSpeed;
        }

        // 수명 예약 
        if (photonView.IsMine)
        {
            CancelInvoke(nameof(SelfDestruct));

            Invoke(nameof(SelfDestruct), m_fBulletLifeTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 총알 주인이 아니면 ( 방 생성한 방장이 주인임 ) 
        if (!photonView.IsMine) 
            return;

        if (collision.CompareTag("Player"))
        {
            var playerPV = collision.GetComponent<PhotonView>();

            if (playerPV != null)
            {
                // 방장이 모두에게
                // 방장이 판정을 내리고 결과를 모두에게 공유하는 동기화 방식
                playerPV.RPC("RPC_TakeDamage", RpcTarget.All, m_fBulletDamage);
            }

            // 방장화면에서 지우면, 포톤 넷웍에서 자동으로 지워짐.
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private void SelfDestruct()
    {
        // 이미 파괴된 경우나, 소유권 없으면 return.
        if (photonView == null || !photonView.IsMine) 
            return;

        // 넷웍 상에서 유요한지 체크.
        // 총알이 네트워크를 통해 생성된 정상적인 존재인지 체크
        if (photonView.InstantiationId > 0)
        {
            // 풀링 시스템이 가로채 안전하게 비활성화함.
            // 모든 유저의 화면에서 이 총알을 지운다.
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
