using UnityEngine;
using Photon.Pun;

public class MonsterController : MonoBehaviourPun, IPunObservable
{
    [Header("이동 및 체력 설정")]
    [SerializeField] private float                                      m_fMoveSpeed = 2f;
    [SerializeField] private float                                      m_fMaxHP = 100f;
    private float                                                       m_fCurrentHP;

    [Header("발사 설정")]
    [SerializeField] private string                                     strBulletPrefabPath = "Monster/EnemyBullet";
    [SerializeField] private Transform                                  pFirePoint;
    [SerializeField] private float                                      m_fFireRate = 2f;
    private float                                                       m_fFireTimer;

    [Header("패턴 설정")]
    [SerializeField] private bool                                       bIsSinMove = false;     // 좌우 흔들림 여부
    [SerializeField] private float                                      m_fSinWidth = 2.0f;     // 흔들리는 폭
    [SerializeField] private float                                      m_fSinSpeed = 3.0f;     // 흔들리는 속도
    private float                                                       fOriginalX;

    [Header("발사 거리 제한 설정")]
    [SerializeField] private float                                      m_fFireStopY = -2.0f;   // 이 밑으로는 안 쏨

    private Rigidbody2D                                                 compRigidBody2D;
    private VisualEffect                                                compVisualEffect;       // 연출용 스크립트 참조 추가

    private Vector3                                                     v3NetworkPosition;
    private Vector2                                                     v2MoveDirection = Vector2.down;

    private bool                                                        m_bIsDead = false;

    [Header("타겟팅 설정")]
    [SerializeField] private float                                      m_fDetectionRange = 10f;  // 플레이어 탐지 거리
    private Transform                                                   m_pTarget;


    [Header("아이템 드롭 설정")]
    [SerializeField]
    private string[]                                                    m_strItemPaths = { "Item/PowerItem", "Item/SpeedItem" };
    [Header("아이템 드롭 확률")]
    [Range(0, 100)]
    [SerializeField] private float                                      m_fPowerItemChance = 10f;
    [Range(0, 100)]
    [SerializeField] private float                                      m_fSpeedItemChance = 10f;

    [SerializeField] private GameObject                                 m_prefDamageText; // 데미지 텍스트 프리팹 등록

    private float m_fAccumulatedDamage = 0f;    // 쌓이고 있는 데미지
    private float m_fDamageTextCooldown = 0.1f; // 0.2초마다 숫자를 띄움 (조절 가능)
    private float m_fLastDamageTextTime;

    private void Awake()
    {
        compRigidBody2D = GetComponent<Rigidbody2D>();
        compVisualEffect = GetComponent<VisualEffect>();
    }

    // 풀에서 꺼내질 때마다 실행되는 부분이다. start 대신 OnEnable로.
    private void OnEnable()
    {
        // MonsterManager에 몬스터들 등록.
        if (!MonsterManager.AllMonsters.Contains(this.transform))
        {
            MonsterManager.AllMonsters.Add(this.transform);
        }

        GameSceneManager.CheckAndHideObject(this);

        m_bIsDead = false;

        // 체력 풀피로 초기화 (안 하면 0인 상태로 부활함)
        m_fCurrentHP = m_fMaxHP;

        compVisualEffect.ResetVisuals();

        // 물리 엔진 잠시 끄기
        if (compRigidBody2D != null)
        {
            compRigidBody2D.simulated = false;
        }

        // 0.05초 뒤에 물리 엔진 켜기 (스폰 시 팅김 방지)
        Invoke(nameof(EnablePhysics), 0.05f);

        // 스폰 위치 저장 (Sin 이동 기준점)
        v3NetworkPosition = transform.position;
        fOriginalX = transform.position.x;

        // 발사 타이머 랜덤 초기화 (애들이 동시에 쏘는 거 방지)
        if (PhotonNetwork.IsMasterClient)
        {
            m_fFireTimer = Random.Range(0f, m_fFireRate);
        }
    }

    private void OnDisable()
    {
        // MonsterManager에서 제거 .
        MonsterManager.AllMonsters.Remove(this.transform);
    }

    private void EnablePhysics()
    {
        if (compRigidBody2D != null)
        {
            compRigidBody2D.simulated = true;
        }
    }

    private void Update()
    {
        // 모든 로직은 방장만 계산해서 알려준다.
        if (PhotonNetwork.IsMasterClient)
        {
            // 가장 가까운 플레이어 찾기 
            FindNearestPlayer();

            // 하강 및 Sin 이동 로직
            float fNextY = transform.position.y + (v2MoveDirection.y * m_fMoveSpeed * Time.deltaTime);
            float fNextX = bIsSinMove ? fOriginalX + Mathf.Sin(Time.time * m_fSinSpeed) * m_fSinWidth : transform.position.x;

            transform.position = new Vector3(fNextX, fNextY, 0f);

            // 발사 로직
            m_fFireTimer += Time.deltaTime;

            if (m_fFireTimer >= m_fFireRate)
            {
                m_fFireTimer = 0f;

                if (transform.position.y > m_fFireStopY)
                {
                    FireBullet();
                }
            }

            // 화면 밖 반납
            if (transform.position.y < -6.0f)
            {
                DestroySelf();
            }
        }
        else
        {
            // 방장이 아닌 클라이언트들은 부드럽게 위치 동기화
            transform.position = Vector3.Lerp(transform.position, v3NetworkPosition, Time.deltaTime * 10f);
        }
    }

    // 플레이어 들중 가까운 걸 타겟으로 설정해서.
    private void FindNearestPlayer()
    {
        GameObject[] playersObj = GameObject.FindGameObjectsWithTag("Player");

        Transform trNearest = null;

        float fMinDistance = Mathf.Infinity;

        foreach (GameObject playerTarget in playersObj)
        {
            // 난입한 팀원도, player 태그 찾아서 거리 찾기 가능.
            float fDistance = Vector3.Distance(transform.position, playerTarget.transform.position);

            if (fDistance < fMinDistance && fDistance <= m_fDetectionRange)
            {
                fMinDistance = fDistance;

                trNearest = playerTarget.transform;
            }
        }

        m_pTarget = trNearest;
    }

    private void FireBullet()
    {
        if (pFirePoint == null)
            return;

        Quaternion qRotation = Quaternion.identity;

        // 타겟이 있다면 타겟 방향으로 회전 후 발사함.
        if (m_pTarget != null)
        {
            Vector2 v2Direction = (m_pTarget.position - pFirePoint.position).normalized;

            float fBulletAngle = Mathf.Atan2(v2Direction.y, v2Direction.x) * Mathf.Rad2Deg;

            qRotation = Quaternion.Euler(0, 0, fBulletAngle + 90f);
        }

        PhotonNetwork.Instantiate(strBulletPrefabPath, pFirePoint.position, qRotation);
    }

    [PunRPC]
    public void TakeDamage(float fAmount)
    {
        //photonView.RPC("RPC_PlayHitEffect", RpcTarget.All);

        //// 2. 데미지 텍스트 생성 (모든 클라이언트가 자기 화면에서 숫자를 보게 함)
        //// 레이저처럼 자잘한 데미지가 너무 많이 들어오면 'fAmount > 1f' 같은 조건을 넣으세요.
        //ShowDamageText(fAmount);

        //// 2. 방장만 체력 계산
        //if (!photonView.IsMine) 
        //    return;

        //if (m_bIsDead) 
        //    return;

        //m_fCurrentHP -= fAmount;

        //if (m_fCurrentHP <= 0f)
        //{
        //    m_bIsDead = true;
        //    m_fCurrentHP = 0;
        //    // 죽는 연출도 모든 사람에게
        //    photonView.RPC("RPC_StartDissolve", RpcTarget.All);
        //}



        // --- [여기서부터 추가] ---
        // 1. 이미 죽었거나 포톤뷰가 제정신이 아니면 아예 밑으로 내려가지도 마라!
        if (m_bIsDead || photonView == null) 
            return;

        // 2. 만약 오브젝트가 파괴 중인데 RPC가 들어왔다면 무시 (가장 확실한 입막음)
        if (!gameObject.activeInHierarchy) return;
        // --- [여기까지 추가] ---

        // 1. 데미지는 들어오는 대로 무조건 누적
        m_fAccumulatedDamage += fAmount;

        // 2. 방장만 체력 계산 (기존 로직 유지)
        if (photonView.IsMine)
        {
            if (m_bIsDead) return;
            m_fCurrentHP -= fAmount;

            if (m_fCurrentHP <= 0f)
            {
                m_bIsDead = true;
                m_fCurrentHP = 0;
                photonView.RPC("RPC_StartDissolve", RpcTarget.All);
            }
        }

        // 3. [핵심] 쿨타임이 지났을 때만 누적된 데미지를 한 번에 띄움
        if (Time.time - m_fLastDamageTextTime >= m_fDamageTextCooldown)
        {
            // 쌓인 데미지가 거의 없을 때는 안 띄우게 세이프티 가드 (선택 사항)
            if (m_fAccumulatedDamage >= 1f)
            {
                // 이펙트도 숫자 뜰 때 같이 터져야 덜 정신없음
                photonView.RPC("RPC_PlayHitEffect", RpcTarget.All);

                // 쌓인 데미지 전달
                //ShowDamageText(m_fAccumulatedDamage);

                // 중요: 그냥 ShowDamageText가 아니라 RPC로 쏩니다!
                photonView.RPC("RPC_ShowDamageText", RpcTarget.All, m_fAccumulatedDamage);

                // 띄웠으니까 누적 데미지 초기화 및 시간 갱신
                m_fAccumulatedDamage = 0f;
                m_fLastDamageTextTime = Time.time;
            }
        }
    }

    // 3. [추가/수정] 텍스트 생성 RPC 함수
    [PunRPC]
    private void RPC_ShowDamageText(float fAmount)
    {
        if (m_prefDamageText == null) return;

        GameObject canvasObj = GameObject.Find("DamageTextCanvas");
        if (canvasObj == null) return;

        // 각자의 화면에서 텍스트 프리팹 생성
        GameObject textObj = Instantiate(m_prefDamageText);
        textObj.transform.SetParent(canvasObj.transform, false);

        // 폰트 크기 문제 해결: 빌드 시 너무 크면 여기서 수치를 더 줄이세요 (예: 0.1f)
        textObj.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
        textObj.transform.position = transform.position;

        textObj.GetComponent<DamageText>().Setup(fAmount);
    }

    private void ShowDamageText(float fAmount)
    {
        if (m_prefDamageText == null) 
            return;

        Vector3 spawnPos = transform.position;

        GameObject canvasObj = GameObject.Find("DamageTextCanvas");
        if (canvasObj == null) return;

        GameObject textObj = Instantiate(m_prefDamageText);

        textObj.transform.SetParent(canvasObj.transform, false);

        textObj.transform.localScale = new Vector3(0.25f, 0.25f, 1f);

        textObj.transform.position = spawnPos;

        textObj.GetComponent<DamageText>().Setup(fAmount);
    }

    [PunRPC]
    private void RPC_PlayHitEffect()
    {
        // VisualEffect에 만든 반짝임 실행
        if (compVisualEffect != null)
        {
            compVisualEffect.StartHitFlash();
        }
    }

    [PunRPC]
    private void RPC_StartDissolve()
    {
        if (compVisualEffect != null)
        {
            compVisualEffect.PlayDissolve(0.6f, () =>
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    // 아이템 드롭 호출 
                    ItemManager.Instance.ManageDropItem(transform.position, m_fPowerItemChance, m_fSpeedItemChance);
                    DestroySelf();
                }
            });
        }
    }

    // MonsterController.cs 내부에 추가
    public void SetEnhancedStats(float fProgressValue) // fProgressValue : 0 ~ 1 
    {
        float fHpMultiplier = 0.5f;

        m_fMaxHP = m_fMaxHP + (m_fMaxHP * fHpMultiplier * fProgressValue);

        m_fCurrentHP = m_fMaxHP;
    }

    private void DestroySelf()
    {
        if (photonView == null || !photonView.IsMine)
            return;

        // 네트워크 상에 아이디가 살아있을 때만 파괴 시도
        if (photonView.InstantiationId > 0)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            //stream.SendNext(m_fCurrentHP);
            //stream.SendNext(m_fMoveSpeed);
        }
        else
        {
            v3NetworkPosition = (Vector3)stream.ReceiveNext();
            //m_fCurrentHP = (float)stream.ReceiveNext();
            //m_fMoveSpeed = (float)stream.ReceiveNext();
        }
    }
}
