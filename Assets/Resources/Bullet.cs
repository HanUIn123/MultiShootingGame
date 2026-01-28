using UnityEngine;
using Photon.Pun;

public class Bullet : MonoBehaviourPun
{
    [Header("총알 발사체 설정")]
    [SerializeField] private float                  m_fBulletSpeed = 10.0f;
    [SerializeField] private float                  m_fBulletLifeTime = 2.0f;
    [SerializeField] private float                  m_fBulletDamage = 50.0f;

    private Rigidbody2D                             compRigidBody2D;

    private bool                                    m_bIsDestroying = false;

    private void Awake()
    {
        // 컴포넌트 캐싱 (최적화)
        compRigidBody2D = GetComponent<Rigidbody2D>();
    }

    // 풀에서 꺼내질 때마다 매번 실행되는 함수.
    private void OnEnable()
    {
        GameSceneManager.CheckAndHideObject(this);

        m_bIsDestroying = false;

        // 물리 속도 초기화 및 재설정
        if (compRigidBody2D != null)
        {
            // 이전의 물리력을 깨끗이 지우고 다시 세팅
            compRigidBody2D.linearVelocity = Vector2.zero;
            compRigidBody2D.linearVelocity = transform.up * m_fBulletSpeed;
        }

        // 수명 타이머 리셋 (재활용할 때마다 다시 카운트)
        if (photonView.IsMine)
        {
            // 기존에 혹시 돌아가고 있을지 모를 예약 취소 후 새로 등록
            CancelInvoke(nameof(DestroySelf));

            Invoke(nameof(DestroySelf), m_fBulletLifeTime);
        }
    }

    // Bullet.cs 내부에 추가
    public void SetDamage(float damage)
    {
        m_fBulletDamage = damage;
    }   

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 소유권 확인 및 이미 파괴 중이면 리턴
        if (!photonView.IsMine || m_bIsDestroying) 
            return;

        if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
        {
            var targetPV = other.GetComponent<PhotonView>();

            if (targetPV != null)
            {
                targetPV.RPC("TakeDamage", RpcTarget.MasterClient, m_fBulletDamage);
            }

            // 충돌 시 즉시 파괴
            DestroySelf();
        }
    }

    private void DestroySelf()
    {
        // 소유권 확인 및 중복 파괴 시도 차단 -> 이거 안하니, 에러 뜨더라;
        if (photonView == null || !photonView.IsMine || m_bIsDestroying) 
            return;

        // 네트워크 ID가 살아있을 때만 파괴 실행
        if (photonView.InstantiationId > 0)
        {
            m_bIsDestroying = true; // 파괴 시작했음을 알리고.

            PhotonNetwork.Destroy(gameObject);
        }
    }
}
