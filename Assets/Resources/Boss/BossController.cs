//using UnityEngine;
//using Photon.Pun;
//using UnityEngine.UI;
//using System.Collections;

//public class BossController : MonoBehaviourPunCallbacks
//{
//    [Header("보스 체력 설정")]
//    public float                                                    m_fMaxHP = 1000f;
//    private float                                                   m_fCurrentHP;
//    private float                                                   m_fTargetHP;

//    [Header("HP 패널 오브젝트")]
//    [HideInInspector]
//    public GameObject                                               m_BossHpPanelObj;

//    [Header("HP 이미지 연결")]
//    [HideInInspector]
//    public Image                                                    m_imgHpFill;

//    [Header("탄막 프리팹 경로")]
//    public string                                                   m_strBulletPrefabPath = "Boss/BossBulletPrefab";

//    [Header("탄막 발사 위치")]
//    public Transform                                                m_trFirePoint;

//    private Material                                                m_matBoss;
//    private Coroutine                                               m_coFlashLoop;

//    [Header("데미지 텍스트 설정")]
//    public GameObject                                               m_prefDamageText;
//    private float                                                   m_fAccumulatedDamage = 0f;
//    private float                                                   m_fLastDamageTextTime = 0f;
//    public float                                                    m_fDamageTextCooldown = 0.15f;  // 뎀지 폰트 뜨는 쿨타임.
//    private bool m_bIsDead = false;

//    [Header("보스 무빙 설정")]
//    public float                                                    m_fMoveSpeed = 2.0f;            // 이동 속도
//    public float                                                    m_fMoveRangeX = 2.5f;           // 좌우 이동 범위
//    private Vector3                                                 m_v3TargetPos;                  // 이동 목표 지점
//    private float                                                   m_fLastMoveTime = 0f;
//    public float                                                    m_fMoveChangeInterval = 2f;     // 몇 초마다 방향을 바꿀지

//    [Header("폭발 이펙트 설정")]
//    public string                                                   m_strExplosionPrefabPath = "Boss/Explosion";


//    void Awake()
//    {
//        SpriteRenderer _sr = GetComponent<SpriteRenderer>();

//        if (_sr != null)
//        {
//            m_matBoss = Instantiate(_sr.material);
//            _sr.material = m_matBoss;
//        }
//    }

//    public override void OnEnable()
//    {
//        base.OnEnable(); 

//        // 보스가 활성화될 때 매니저 추가 
//        if (MonsterManager.AllMonsters != null && !MonsterManager.AllMonsters.Contains(transform))
//        {
//            MonsterManager.AllMonsters.Add(transform);
//        }
//    }

//    public override void OnDisable()
//    {
//        base.OnDisable(); 

//        // 보스가 죽거나 비활성화될 때 명단에서 제거
//        if (MonsterManager.AllMonsters != null)
//        {
//            MonsterManager.AllMonsters.Remove(transform);
//        }
//    }


//    void Start()
//    {
//        GameSceneManager.CheckAndHideObject(this);

//        m_fCurrentHP = m_fMaxHP;
//        m_fTargetHP = m_fMaxHP;

//        if (m_imgHpFill != null)
//        {
//            m_imgHpFill.fillAmount = 1f;
//        }

//        if (PhotonNetwork.IsMasterClient)
//        {
//            // AllBuffered를 써서, 난입한 팀원들도 볼 수 있게 한다.
//            photonView.RPC("InitBossUI", RpcTarget.AllBuffered);
//        }

//        m_v3TargetPos = transform.position; // 시작 위치를 첫 목표로
//    }

//    void Update()
//    {
//        if (m_imgHpFill != null)
//        {
//            float _fCurrentFill = m_imgHpFill.fillAmount;
//            float _fTargetFill = Mathf.Clamp01(m_fTargetHP / m_fMaxHP);
//            m_imgHpFill.fillAmount = Mathf.Lerp(_fCurrentFill, _fTargetFill, Time.deltaTime * 10f);
//        }

//        // 방장만 이동 체크
//        HandleMovement();
//    }

//    private void HandleMovement()
//    {
//        if (m_bIsDead) 
//            return;

//        // 방장만 새로운 목표 위치를 정함 -> 팀원들은 보간된거 따라가서 보이게,
//        if (PhotonNetwork.IsMasterClient)
//        {
//            if (Time.time - m_fLastMoveTime >= m_fMoveChangeInterval)
//            {
//                // m_fMoveRangeX : 2.5f
//                float _fRandomX = Random.Range(-m_fMoveRangeX, m_fMoveRangeX);

//                m_v3TargetPos = new Vector3(_fRandomX, transform.position.y, 0f);

//                m_fLastMoveTime = Time.time;
//            }
//        }

//        // 부드럽게 이동 (모든 클라이언트 공통 실행)
//        transform.position = Vector3.Lerp(transform.position, m_v3TargetPos, Time.deltaTime * m_fMoveSpeed);
//    }

//    public void StartBossBattle()
//    {
//        // 여기서 직접 UI 켜지 말고, 이미 실행 중인 InitBossUI RPC에 의존함
//        if (PhotonNetwork.IsMasterClient)
//        {
//            StartCoroutine(BossAttackPattern());
//        }
//    }

//    [PunRPC]
//    void InitBossUI()
//    {
//        GameObject GameSceneCanvas = GameObject.Find("Canvas");

//        if (GameSceneCanvas != null)
//        {
//            // transform.Find로, 꺼져 있는 자식 BossHp_Panel 을 찾자.
//            Transform transformPanel = GameSceneCanvas.transform.Find("BossHp_Panel");

//            if (transformPanel != null)
//            {
//                m_BossHpPanelObj = transformPanel.gameObject;
//                m_BossHpPanelObj.SetActive(true);

//                // 자식인 Fill 이미지 연결
//                Transform transfromFillImg = transformPanel.Find("BossHp_Fill");
//                if (transfromFillImg != null)
//                {
//                    m_imgHpFill = transfromFillImg.GetComponent<Image>();

//                    // 현재 체력 즉시 반영
//                    float _fTargetFill = Mathf.Clamp01(m_fCurrentHP / m_fMaxHP);
//                    m_imgHpFill.fillAmount = _fTargetFill;
//                }
//            }
//        }
//    }

//    [PunRPC]
//    void TakeDamage(float _fDamage)
//    {
//        if (m_bIsDead || !gameObject.activeInHierarchy) 
//            return;

//        m_fAccumulatedDamage += _fDamage;

//        // 방장만 체력 깎고 -> 동기화 해줌 
//        if (PhotonNetwork.IsMasterClient)
//        {
//            m_fCurrentHP -= _fDamage;
//            m_fCurrentHP = Mathf.Max(m_fCurrentHP, 0f);

//            // 체력 업데이트 RPC
//            photonView.RPC("UpdateHP", RpcTarget.All, m_fCurrentHP);

//            if (m_fCurrentHP <= 0f)
//            {
//                m_bIsDead = true;
//                photonView.RPC("Die", RpcTarget.All);
//                return;
//            }
//            else
//            {
//                // 피격 깜빡이 효과
//                photonView.RPC("StartHitFlashLoop", RpcTarget.All);

//                StartCoroutine(StopHitFlashAfterDelay(0.1f));
//            }
//        }

//        // 쿨타임마다 모든 클라이언트에게 데미지 텍스트를 띄우라고 명령
//        if (Time.time - m_fLastDamageTextTime >= m_fDamageTextCooldown)
//        {
//            if (m_fAccumulatedDamage >= 1f)
//            {
//                // 내 화면 포함 모든 사람에게 텍스트 띄우기
//                photonView.RPC("RPC_ShowDamageText", RpcTarget.All, m_fAccumulatedDamage);

//                m_fAccumulatedDamage = 0f;

//                m_fLastDamageTextTime = Time.time;
//            }
//        }
//    }

//    [PunRPC]
//    private void RPC_ShowDamageText(float fAmount)
//    {
//        if (m_prefDamageText == null) 
//            return;

//        GameObject canvasObj = GameObject.Find("DamageTextCanvas");

//        if (canvasObj == null) 
//            return;

//        GameObject textObj = Instantiate(m_prefDamageText, canvasObj.transform);

//        // 위치 세팅
//        textObj.transform.position = transform.position + new Vector3(0, 1.2f, 0);
//        textObj.transform.localScale = new Vector3(0.25f, 0.25f, 1f);

//        DamageText DamageText = textObj.GetComponent<DamageText>();

//        if (DamageText != null)
//        {
//            DamageText.Setup(Mathf.RoundToInt(fAmount));
//        }
//    }   

//    private IEnumerator StopHitFlashAfterDelay(float fDelay)
//    {
//        yield return new WaitForSeconds(fDelay);

//        photonView.RPC("StopHitFlashLoop", RpcTarget.All);
//    }

//    [PunRPC]
//    void UpdateHP(float fHp)
//    {
//        m_fCurrentHP = fHp;

//        m_fTargetHP = fHp;
//    }

//    [PunRPC]
//    void StartHitFlashLoop()
//    {
//        if (m_coFlashLoop != null)
//        {
//            StopCoroutine(m_coFlashLoop);
//        }

//        m_coFlashLoop = StartCoroutine(HitFlashLoop());
//    }

//    [PunRPC]
//    void StopHitFlashLoop()
//    {
//        if (m_coFlashLoop != null)
//        {
//            StopCoroutine(m_coFlashLoop);
//        }

//        m_coFlashLoop = null;

//        if (m_matBoss != null)
//        {
//            m_matBoss.SetFloat("_WhiteAmount", 0f);
//        }
//    }

//    IEnumerator HitFlashLoop()
//    {
//        while (true)
//        {
//            if (m_matBoss == null)
//            {
//                break;
//            }

//            m_matBoss.SetFloat("_WhiteAmount", 1f);
//            yield return new WaitForSeconds(0.05f);

//            m_matBoss.SetFloat("_WhiteAmount", 0f);
//            yield return new WaitForSeconds(0.05f);
//        }
//    }

//    [PunRPC]
//    void Die()
//    {
//        if (m_bIsDead == false) 
//            m_bIsDead = true;

//        StopAllCoroutines();

//        // UI 끄기
//        if (m_BossHpPanelObj != null)
//            m_BossHpPanelObj.SetActive(false);

//        // 연쇄 폭발 연출 시작 (모든 클라이언트가 각자 자기 화면에서 연출을 보게 함)
//        StartCoroutine(DeathSequence());
//    }

//    IEnumerator DeathSequence()
//    {
//        // 폭발 관련 변수 
//        float fExplosionRad = 1.5f;
//        int iExplosionCount = 28; // 폭발 횟수

//        for (int i = 0; i < iExplosionCount; i++)
//        {
//            // 보스 중심점 기준으로 랜덤한 위치 계산
//            Vector3 randomOffset = new Vector3(
//                Random.Range(-fExplosionRad, fExplosionRad),
//                Random.Range(-fExplosionRad, fExplosionRad),
//                0
//            );

//            Vector3 v3ExplodePos = transform.position + randomOffset;

//            GameObject explodeObj = Resources.Load<GameObject>(m_strExplosionPrefabPath);

//            if (explodeObj != null)
//            {
//                GameObject instance = Instantiate(explodeObj, v3ExplodePos, Quaternion.identity);

//                float fRandomScale = Random.Range(0.8f, 1.5f);
//                instance.transform.localScale = Vector3.one * fRandomScale;
//            }

//            // 잠깐 대기 (파바바박 하는 느낌)
//            yield return new WaitForSeconds(0.08f);
//        }

//        // 모든 폭발 연출이 끝나면 실제로 보스를 파괴 (방장만 실행)
//        if (PhotonNetwork.IsMasterClient)
//        {
//            if (photonView.InstantiationId > 0)
//            {
//                if (GameSceneManager.Instance != null) 
//                    GameSceneManager.Instance.StartClearSequence();

//                PhotonNetwork.Destroy(gameObject);
//            }
//        }
//    }

//    // 코드상에서 HideBossUI()라고 직접 호출하는 게 아니라,
//    // photonView.RPC("HideBossUI", ...) 처럼 문자열 방식으로 호출
//    [PunRPC]
//    void HideBossUI()
//    {
//        if (m_BossHpPanelObj != null)
//        {
//            m_BossHpPanelObj.SetActive(false);
//        }
//    }

//    public void OnLaserFinished()
//    {
//        photonView.RPC("StopHitFlashLoop", RpcTarget.All);
//    }

//    IEnumerator BossAttackPattern()
//    {
//        while (true)
//        {
//            int iPatternIndex = Random.Range(0, 3);

//            switch (iPatternIndex)
//            {
//                case 0:
//                    yield return StartCoroutine(Pattern_Circle());
//                    break;
//                case 1:
//                    yield return StartCoroutine(Pattern_Spiral());
//                    break;
//                case 2:
//                    yield return StartCoroutine(Pattern_Shotgun());
//                    break;
//            }

//            yield return new WaitForSeconds(2.0f);
//        }
//    }

//    IEnumerator Pattern_Circle()
//    {
//        Vector3 v3SpawnPos = m_trFirePoint ? m_trFirePoint.position : transform.position;

//        int iBulletCount = 20;

//        float fPatternAngle = 360f / iBulletCount;

//        for (int i = 0; i < iBulletCount; i++)
//        {
//            float fAngle = i * fPatternAngle * Mathf.Deg2Rad;

//            Vector3 v3Dir = new Vector3(Mathf.Cos(fAngle), Mathf.Sin(fAngle), 0f);

//            FireBossBullet(v3SpawnPos, v3Dir);
//        }

//        yield return null;
//    }

//    IEnumerator Pattern_Spiral()
//    {
//        Vector3 v3SpawnPos = m_trFirePoint ? m_trFirePoint.position : transform.position;

//        //float fStartAngle = Random.Range(0f, 360f);
//        float fStartAngle = -90f;

//        for (int i = 0; i < 36; i++)
//        {
//            float fAngle = fStartAngle + i * 10f;
//            float fRadian = fAngle * Mathf.Deg2Rad;

//            Vector3 v3Dir = new Vector3(Mathf.Cos(fRadian), Mathf.Sin(fRadian), 0f);

//            FireBossBullet(v3SpawnPos, v3Dir);

//            yield return new WaitForSeconds(0.05f);
//        }
//    }

//    IEnumerator Pattern_Shotgun()
//    {
//        Vector3 v3SpawnPos = m_trFirePoint ? m_trFirePoint.position : transform.position;

//        int iBulletCount = 10;
//        float fSpreadAngle = 45f;
//        float fBaseAngle = -fSpreadAngle / 2f;

//        for (int i = 0; i < iBulletCount; i++)
//        {
//            float fAngle = fBaseAngle + i * (fSpreadAngle / (iBulletCount - 1));
//            float fRadian = fAngle * Mathf.Deg2Rad;

//            Vector3 v3Dir = new Vector3(Mathf.Sin(fRadian), -Mathf.Cos(fRadian), 0f);

//            FireBossBullet(v3SpawnPos, v3Dir);
//        }

//        yield return null;
//    }

//    private void FireBossBullet(Vector3 v3SpawnPos, Vector3 v3Dir)
//    {
//        GameObject bulletObj = PhotonNetwork.Instantiate(m_strBulletPrefabPath, v3SpawnPos, Quaternion.identity);

//        BossBullet bullet = bulletObj.GetComponent<BossBullet>();

//        if (bullet != null)
//        {
//            bullet.SetDirection(v3Dir);
//        }
//    }
//}



using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.Collections;

public class BossController : MonoBehaviourPunCallbacks
{
    private BossHealth          m_compHealth;
    private BossMovement        m_compMovement;
    private BossAttack          m_compAttack;

    public float                m_fMoveChangeInterval = 2f;

    void Awake()
    {
        m_compHealth = GetComponent<BossHealth>();
        m_compMovement = GetComponent<BossMovement>();
        m_compAttack = GetComponent<BossAttack>();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        if (MonsterManager.AllMonsters != null && !MonsterManager.AllMonsters.Contains(transform))
            MonsterManager.AllMonsters.Add(transform);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (MonsterManager.AllMonsters != null)
            MonsterManager.AllMonsters.Remove(transform);
    }

    void Start()
    {
        GameSceneManager.CheckAndHideObject(this);

        if (PhotonNetwork.IsMasterClient)
            photonView.RPC("InitBossUI", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void InitBossUI()
    {
        GameObject canvasObj = GameObject.Find("Canvas");

        /*
            Transform panel = null; // 결과물을 담을 변수 미리 선언

            // canvasObj 체크,
            if (canvasObj != null) 
            {
                panel = canvasObj.transform.Find("BossHp_Panel");
            }

         */
        Transform panel = canvasObj?.transform.Find("BossHp_Panel");

        if (panel != null)
        {
            m_compHealth.m_BossHpPanelObj = panel.gameObject;
            m_compHealth.m_BossHpPanelObj.SetActive(true);
            m_compHealth.m_imgHpFill = panel.Find("BossHp_Fill")?.GetComponent<Image>();
        }
    }

    public void StartBossBattle()
    {
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(BossAI());
    }

    IEnumerator BossAI()
    {
        while (!m_compHealth.IsDead)
        {
            // 보스 이동
            m_compMovement.SetNewRandomTarget();

            // 보스 공격 패턴 
            int iPatternIndex = Random.Range(0, 3);

            switch (iPatternIndex)
            {
                case 0: yield return StartCoroutine(m_compAttack.Pattern_Circle()); 
                    break;
                case 1: yield return StartCoroutine(m_compAttack.Pattern_Spiral()); 
                    break;
                case 2: yield return StartCoroutine(m_compAttack.Pattern_Shotgun()); 
                    break;
            }

            yield return new WaitForSeconds(m_fMoveChangeInterval);
        }
    }

    //public void OnLaserFinished() => m_compHealth.StopHitFlashLoop();
}