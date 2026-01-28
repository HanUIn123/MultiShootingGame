using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.DebugUI;

public class PlayerController : MonoBehaviourPunCallbacks, Player_InputAction.IGamePlayActions, IPunObservable
{
    // IPunObservable: 내 캐릭터의 위치, 체력, 상태 같은 값들을 네트워크를 통해 다른 사람들에게 실시간으로 보여주거나, 수신.
    private Player_InputAction                                          m_compInput;
    private PhotonView                                                  m_photonView;
    private ChatManager                                                 m_compChatManager;
    private Animator                                                    m_animator;

    [Header("이동 및 공격 설정")]
    [SerializeField] private float                                      m_fMoveSpeed = 6f;
    [SerializeField] private float                                      m_fFireCooldown = 1.0f;
    private float                                                       m_fLastFireTime;
    private Vector2                                                     v2MoveInput;

    [Header("공격 제한 설정")]
    [SerializeField] private float                                      m_fMinFireCooldown = 0.05f; // 최소 공격 속도 쿨타임 제한

    [Header("포인트 설정")]
    [SerializeField] private Transform                                  trFirePoint;
    [SerializeField] private Transform                                  trLaserSpawn;

    [Header("궁극기(Fever) 설정")]
    [SerializeField] private float                                      m_fChargeSpeed = 1.5f;
    [SerializeField] private string                                     m_strLaserPath = "UltimateLaser";
    private float                                                       m_fCurrentGauge = 0f;
    private bool                                                        m_bIsCharging = false;
    private bool                                                        m_bIsFevertime = false;

    public UltimateUIManager                                            ultimateUI;

    private float                                                       m_fCurrentAttackLevel = 0f;
    private float                                                       m_fCurrentSpeedLevel = 0f;

    [Header("캐릭터 설정")]
    [SerializeField] private int                                        m_iCharacterType = 1;

    private List<Transform>                                             m_targetList = new List<Transform>();
    private GameObject                                                  m_currentLaserObj;
    private Rigidbody2D                                                 m_compRigid;
    private SpriteRenderer                                              m_compSprite;
    private Vector3                                                     m_v3CurrentPos;

    // 포톤의 실시간 데이터 동기화 함수 
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // stream.IsWriting (데이터 보내기)
        // 역할: "내가 내 캐릭터의 주인일 때" 실행.
        // 동작: 내 컴퓨터에서 바뀐 게이지 값(m_fCurrentAttackLevel 등)을 서버로 쏨.
        // 비유: 내가 지금 피자를 얼마나 먹었는지 친구들한테 카톡으로 실시간 보고하는 것.
        if (stream.IsWriting)
        {
            stream.SendNext(m_fCurrentGauge);
            stream.SendNext(m_fCurrentAttackLevel); // 추가
            stream.SendNext(m_fCurrentSpeedLevel);  // 추가
        }
        else
        {
            //else 부분 (stream.IsReading) (데이터 받기)
            //역할: "내가 다른 사람의 캐릭터를 보고 있을 때" 실행.
            //동작: 서버에서 날아온 상대방의 게이지 값을 내 변수에 넣음.
            //비유: 친구가 카톡으로 보낸 "피자 3조각 먹음"이라는 메시지를 내가 읽어서 내 머릿속 정보를 업데이트하는 상황.
            this.m_fCurrentGauge = (float)stream.ReceiveNext();
            this.m_fCurrentAttackLevel = (float)stream.ReceiveNext(); // 추가
            this.m_fCurrentSpeedLevel = (float)stream.ReceiveNext();  // 추가
        }
    }

    public void InitLaserSpawn(Transform transform)
    {
        trLaserSpawn = transform;
    }

    private void Awake()
    {
        m_compRigid = GetComponent<Rigidbody2D>();
        m_compSprite = GetComponent<SpriteRenderer>();


        m_photonView = GetComponent<PhotonView>();

        m_animator = GetComponent<Animator>();

        // 소유권 상관없이 일단 이 오브젝트 자체가 씬 전환 시 파괴되지 않게 보호
        // 이게 없으면 팀원이 늦게 들어올 때 이미 생성된 방장 기체를 유니티가 지워버림
        DontDestroyOnLoad(gameObject);

        // 내 플레이어 캐릭터가 아니라면, 모습을 감춘다..
        if (!m_photonView.IsMine)
        {
            SetVisible(false);
        }

        if (m_compInput == null)
        {
            m_compInput = new Player_InputAction();
            m_compInput.GamePlay.SetCallbacks(this);
        }
    }

    private void Start()
    {
        // 처음 생성될 때 씬 상태 체크
        RefreshSceneState();
    }

    #region Input 시스템 활성화 제어
    private new void OnEnable()
    {
        //C++에서 부모 클래스의 가상 함수를 호출 (base 가, __super :: 같은거 ) 
        base.OnEnable();

        // 델리게이트(Delegate) / 이벤트(Event) 문법
        // C#에서는 "이 함수 포인터 목록에 내 함수를 추가해라"라는 뜻
        // 씬 로드가 완료되는 사건(sceneLoaded)이 발생하면, 내가 만든 OnSceneLoaded 함수도 같이 실행
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 소유권이 확실하면, 입력 할 수 있게 끔.
        if (m_photonView != null && m_photonView.IsMine)
        {
            m_compInput.GamePlay.Enable();
        }
    }

    private new void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (m_compInput != null)
            m_compInput.GamePlay.Disable();
    }
    #endregion


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 로드가 끝났으니 이제 쌓여있던 서버 메시지(SetStartTime 등)를 처리해라!
        PhotonNetwork.IsMessageQueueRunning = true;

        if (m_photonView == null)
            return;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        RefreshSceneState();
    }

    private void RefreshSceneState()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        bool isGameScene = currentScene.name == "GameScene";

        m_compChatManager = FindFirstObjectByType<ChatManager>();

        if (isGameScene)
        {
            SetVisible(true);

            if (!m_photonView.IsMine)
            {
                if (m_compRigid == null)
                    m_compRigid = GetComponent<Rigidbody2D>();

                if (m_compRigid != null)
                    m_compRigid.simulated = true;

                if (m_compInput != null)
                {
                    m_compInput.GamePlay.Disable();
                }
            }
            else
            {
                if (ultimateUI == null)
                    ultimateUI = FindFirstObjectByType<UltimateUIManager>();

                if (m_compInput != null)
                {
                    m_compInput.GamePlay.Enable();
                }
            }
        }
        else
        {
            if (!m_photonView.IsMine)
                SetVisible(false);
        }
    }

    private int CompareTargetDistance(Transform a, Transform b)
    {
        // 어떤 몬스터가 더 가까운가, 
        // 내 위치(m_v3CurrentPos)와 몬스터 A, B 사이의 거리를 계산
        // sqrMagnitude는 루트 연산을 안 해서 성능이 훨씬 빠르다고 한다.
        float fDistA = (a.position - m_v3CurrentPos).sqrMagnitude;
        float fDistB = (b.position - m_v3CurrentPos).sqrMagnitude;

        // 두 거리를 비교해서 결과를 반환 (-1, 0, 1 중 하나)
        // A가 더 가까우면 앞으로 보내고, B가 더 가까우면 뒤로 보냅니다.
        return fDistA.CompareTo(fDistB);
    }

    private void SetVisible(bool isVisible)
    {
        if (m_compSprite != null)
            m_compSprite.enabled = isVisible;

        // 자식 오브젝트들도 같이 껏/켜 되게.
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(isVisible);
        }
    }


    private void Update()
    {
        // 나의 플레이어가 아니면 실행하지 않음.
        if (!m_photonView.IsMine)
            return;

        if (IsChatInputFocused())
        {
            v2MoveInput = Vector2.zero;
            return;
        }

        PreventMovement();
        HandleUltimateCharge();
        HandleAutoFire(); // 매 프레임 자동 발사 
    }

    private void HandleAutoFire()
    {
        if (m_bIsFevertime)
            return;

        if (Time.time - m_fLastFireTime >= m_fFireCooldown)
        {
            if (m_iCharacterType == 2) // 레이저 캐릭터일 때, 
            {
                // 스피드 레벨 -> 사거리 확장. 
                float fDetectRange = 4f + (m_fCurrentSpeedLevel * 4f);
                Transform[] targets = GetNearestEnemies(fDetectRange);

                if (targets.Length > 0)
                {
                    m_fLastFireTime = Time.time;
                    FireLaserMulti(targets);
                }
            }
            else // 일반 기체 (Type 1)
            {
                m_fLastFireTime = Time.time;
                FireBullet(null);
            }
        }
    }

    // 사거리 내의 여러 적을 찾는 헬퍼 함수
    private Transform[] GetNearestEnemies(float fRange)
    {
        m_targetList.Clear();
        m_v3CurrentPos = transform.position;
        float fRangePowValue = fRange * fRange;

        // 씬 전체를 뒤지는 FindGameObjectsWithTag를 삭제하고
        // MonsterManager가 관리하는 AllMonsters 명단만 루프 수행.
        var allMonsters = MonsterManager.AllMonsters;

        for (int i = 0; i < allMonsters.Count; i++)
        {
            Transform trTarget = allMonsters[i];

            // 삭제 중 인 Enemy가 있을 수 도 있으니, 
            if (trTarget == null)
                continue;

            // 뒤에 있거나 너무 멀면 패스
            float fDiffer = trTarget.position.y - m_v3CurrentPos.y;

            if (fDiffer < -0.5f || fDiffer > fRange)
                continue;

            // 거리 판정 (루트 연산 없는 sqrMagnitude 사용)
            float fDistancePow = ((Vector2)trTarget.position - (Vector2)m_v3CurrentPos).sqrMagnitude;

            if (fDistancePow <= fRangePowValue)
            {
                // MonsterManager 에 있는 몬스터들을 ( 레이저 공격 범위 내에 들어오는 ) 담아줌.
                m_targetList.Add(trTarget);
            }
        }

        // 거기서 다 담긴 애들 중 가장 가까운 애 부터 정렬 시켜줌.
        m_targetList.Sort(CompareTargetDistance);

        return m_targetList.ToArray();
    }

    private void FireLaserMulti(Transform[] targets)
    {
        if (trFirePoint == null)
            return;

        BendLaser[] allLasers = FindObjectsByType<BendLaser>(FindObjectsSortMode.None);

        int iMyLaserCount = 0;

        for (int i = 0; i < allLasers.Length; i++)
        {
            if (allLasers[i].photonView.IsMine)
                iMyLaserCount++;
        }

        int iMaxAllowed = m_fCurrentAttackLevel >= 0.7f ? 3 : (m_fCurrentAttackLevel >= 0.3f ? 2 : 1);

        if (iMyLaserCount >= iMaxAllowed)
            return;

        int iSpawnLimit = Mathf.Min(iMaxAllowed - iMyLaserCount, targets.Length);

        for (int i = 0; i < iSpawnLimit; i++)
        {
            // 레이저 생성
            GameObject laserObj = PhotonNetwork.Instantiate("BendLaserPrefab", trFirePoint.position, Quaternion.identity);
            var laswerPV = laserObj.GetComponent<PhotonView>();
            var targetPv = targets[i].GetComponent<PhotonView>();

            if (laswerPV != null && targetPv != null)
            {
                float fCurrentDamage = 50f + (m_fCurrentAttackLevel * 100f);

                // RPC 호출: 모든 클라이언트에게 타겟 정보를 보냄. (  내 ViewID, 타겟의 ViewID, 데미지 )
                laswerPV.RPC("RPC_SetupLaser", RpcTarget.All, m_photonView.ViewID, targetPv.ViewID, fCurrentDamage);
            }
        }
    }

    // 실제 발사 로직
    private void FireBullet(Transform trTarget = null)
    {
        if (trFirePoint == null)
            return;

        if (m_animator != null)
            m_animator.SetTrigger("tAttack");

        float fCurrentDamage = 50f + (m_fCurrentAttackLevel * 100f);

        if (m_iCharacterType == 1)
        {
            if (m_fCurrentAttackLevel < 0.3f)
            {
                // 레벨 낮을 때: 1발
                CreateBullet(trFirePoint.position, trFirePoint.rotation, fCurrentDamage);
            }
            else if (m_fCurrentAttackLevel < 0.7f)
            {
                // 레벨 중간: 2발 
                CreateBullet(trFirePoint.position, trFirePoint.rotation * Quaternion.Euler(0, 0, 15f), fCurrentDamage);
                CreateBullet(trFirePoint.position, trFirePoint.rotation * Quaternion.Euler(0, 0, -15f), fCurrentDamage);
            }
            else
            {
                // 레벨 높을 때: 3발 
                CreateBullet(trFirePoint.position, trFirePoint.rotation, fCurrentDamage);
                CreateBullet(trFirePoint.position, trFirePoint.rotation * Quaternion.Euler(0, 0, 25f), fCurrentDamage);
                CreateBullet(trFirePoint.position, trFirePoint.rotation * Quaternion.Euler(0, 0, -25f), fCurrentDamage);
            }

            if (SoundManager.Instance)
                SoundManager.Instance.PlaySFX("BulletSound");
        }
        else if (m_iCharacterType == 2)
        {
            if (m_currentLaserObj != null)
                return;

            if (trTarget != null)
            {
                m_currentLaserObj = PhotonNetwork.Instantiate("BendLaserPrefab", trFirePoint.position, Quaternion.identity);
                var laserPV = m_currentLaserObj.GetComponent<PhotonView>();
                var targetPv = trTarget.GetComponent<PhotonView>();

                if (laserPV != null && targetPv != null)
                {
                    laserPV.RPC("RPC_SetupLaser", RpcTarget.All, m_photonView.ViewID, targetPv.ViewID, fCurrentDamage);
                }
            }
        }
    }

    // 총알 생성 헬퍼 함수 (Type 1 전용)
    private void CreateBullet(Vector3 v3Position, Quaternion rotation, float fBulletDamage)
    {
        GameObject bulletObj = PhotonNetwork.Instantiate("BulletPrefab", v3Position, rotation);

        Bullet bullet = bulletObj.GetComponent<Bullet>();

        if (bullet != null)
        {
            bullet.SetDamage(fBulletDamage);
        }
    }

    // 이동 속도 업그레이드 함수
    public void UpgradeMoveSpeed(float amount)
    {
        if (!m_photonView.IsMine)
            return;

        m_fMoveSpeed += amount;
    }

    private void PreventMovement()
    {
        Vector3 v3NextPos = transform.position + new Vector3(v2MoveInput.x, v2MoveInput.y, 0f) * m_fMoveSpeed * Time.deltaTime;

        // 플레이어 위치 설정 잡아주기.
        // 플레이어가 게임 화면(가로 -3.5 ~ 3.5, 세로 -4.6 ~ 4.6) 밖으로 나가는 것을 막자.
        v3NextPos.x = Mathf.Clamp(v3NextPos.x, -3.5f, 3.5f);
        v3NextPos.y = Mathf.Clamp(v3NextPos.y, -4.6f, 4.6f);

        transform.position = v3NextPos;
    }

    private void HandleUltimateCharge()
    {
        if (m_bIsCharging && !m_bIsFevertime)
        {
            m_fCurrentGauge = Mathf.Min(m_fCurrentGauge + Time.deltaTime * m_fChargeSpeed, 1f);

            if (ultimateUI)
                ultimateUI.UpdateGauge(m_fCurrentGauge);

            if (m_fCurrentGauge >= 1f)
                TriggerFevertime();
        }
    }

    #region 인터페이스 (InputSystem) 
    public void OnMove(InputAction.CallbackContext context)
    {
        if (!m_photonView.IsMine || IsChatInputFocused())
        {
            v2MoveInput = Vector2.zero;
            return;
        }
        v2MoveInput = context.ReadValue<Vector2>();
    }

    public void OnFire(InputAction.CallbackContext context)
    {

    }

    public void AddAttackPower(float fAmount)
    {
        if (!m_photonView.IsMine)
            return;

        m_fCurrentAttackLevel = Mathf.Clamp01(m_fCurrentAttackLevel + fAmount);

        // 매니저한테 UI 갱신 요청
        if (PlayerUIManager.Instance != null)
            PlayerUIManager.Instance.UpdateAttackGauge(m_fCurrentAttackLevel);
    }

    public void AddSpeedPower(float fAmount)
    {
        if (!m_photonView.IsMine)
            return;

        m_fCurrentSpeedLevel = Mathf.Clamp01(m_fCurrentSpeedLevel + fAmount);

        if (PlayerUIManager.Instance != null)
            PlayerUIManager.Instance.UpdateSpeedGauge(m_fCurrentSpeedLevel);

        UpgradeFireRate(fAmount);
    }

    // 연사 속도 업그레이드 함수
    public void UpgradeFireRate(float fAmount)
    {
        // 내 캐릭터의 데이터만 수정
        if (!m_photonView.IsMine)
            return;

        // 쿨타임을 줄여서 더 빠르게 쏘게 함
        m_fFireCooldown -= fAmount;

        // 최소치 제한
        if (m_fFireCooldown < m_fMinFireCooldown)
        {
            m_fFireCooldown = m_fMinFireCooldown;
        }
    }

    public void OnUltimate(InputAction.CallbackContext context)
    {
        if (!m_photonView.IsMine || IsChatInputFocused())
            return;

        if (context.started)
        {
            m_bIsCharging = true;
        }
        else if (context.canceled)
        {
            m_bIsCharging = false;
            ResetUltimateGauge();
        }
    }
    #endregion

    #region 버튼 이벤트 (모바일/UI용)
    public void OnChargeButtonPressed()
    {
        m_bIsCharging = true;

        if (EventSystem.current)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnChargeButtonReleased()
    {
        // 차지 버튼에서 손을 떼면 차징 상태를 해제
        m_bIsCharging = false;
    }

    // 게이지 초기화 공용 함수
    private void ResetUltimateGauge()
    {
        // 피버타임 중에는 리셋하지 않음 (이미 발사 중이므로)
        if (m_bIsFevertime)
            return;

        m_fCurrentGauge = 0f;

        if (ultimateUI)
        {
            ultimateUI.UpdateGauge(0f);
        }
    }

    #endregion

    private void TriggerFevertime()
    {
        m_bIsFevertime = true;

        m_bIsCharging = false; // 충전 완료 시 강제 중단

        if (ultimateUI)
        {
            ultimateUI.onCutInFinished = FinishFevertimeAndFire;
            ultimateUI.PlayCutIn();
        }
        else
            FinishFevertimeAndFire();
    }

    private void FinishFevertimeAndFire()
    {
        if (!m_bIsFevertime)
            return;

        m_bIsFevertime = false;

        m_fCurrentGauge = 0f;

        if (ultimateUI)
            ultimateUI.UpdateGauge(0f);

        FireUltimateLaser();
    }

    private void FireUltimateLaser()
    {
        if (!trLaserSpawn || !m_photonView.IsMine)
            return;

        PhotonNetwork.Instantiate(m_strLaserPath, trLaserSpawn.position, trLaserSpawn.rotation);

        if (SoundManager.Instance)
            SoundManager.Instance.PlaySFX("FireSound");
    }

    private bool IsChatInputFocused()
    {
        if (EventSystem.current == null || m_compChatManager == null)
            return false;

        return m_compChatManager.IsChatInputFocused();
    }
}

// ==========================================================================================

//using UnityEngine;
//using Photon.Pun;
//using UnityEngine.SceneManagement;

//public class PlayerController : MonoBehaviourPunCallbacks, Player_InputAction.IGamePlayActions, IPunObservable
//{
//    private PlayerMovement m_move;
//    private PlayerAttack m_attack;
//    private PlayerStats m_stats;
//    private PlayerHealth m_health; // 형이 준 스크립트 활용

//    private Player_InputAction m_input;
//    private PhotonView m_pv;
//    [SerializeField] private int m_iCharacterType = 1;

//    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
//    {
//        if (stream.IsWriting)
//        {
//            stream.SendNext(m_stats.m_fCurrentGauge);
//            stream.SendNext(m_stats.m_fCurrentAttackLevel);
//            stream.SendNext(m_stats.m_fCurrentSpeedLevel);
//        }
//        else
//        {
//            m_stats.m_fCurrentGauge = (float)stream.ReceiveNext();
//            m_stats.m_fCurrentAttackLevel = (float)stream.ReceiveNext();
//            m_stats.m_fCurrentSpeedLevel = (float)stream.ReceiveNext();
//        }
//    }

//    private void Awake()
//    {
//        m_pv = GetComponent<PhotonView>();
//        m_move = gameObject.AddComponent<PlayerMovement>();
//        m_attack = GetComponent<PlayerAttack>();
//        m_stats = GetComponent<PlayerStats>();
//        m_health = GetComponent<PlayerHealth>();

//        DontDestroyOnLoad(gameObject);
//        //if (!m_pv.IsMine) 
//        //    SetVisible(false);

//        m_input = new Player_InputAction();
//        m_input.GamePlay.SetCallbacks(this);
//    }

//    private void Update()
//    {
//        if (!m_pv.IsMine) return;

//        m_move.ProcessMovement();
//        m_stats.ProcessUltimate();
//        m_attack.HandleAutoFire(m_iCharacterType, m_stats.m_fCurrentAttackLevel, m_stats.m_fCurrentSpeedLevel, m_stats.m_bIsFevertime);
//    }

//    // --- Input Actions ---
//    public void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext context)
//        => m_move.SetMoveInput(context.ReadValue<Vector2>());

//    public void OnUltimate(UnityEngine.InputSystem.InputAction.CallbackContext context)
//    {
//        if (context.started) m_stats.m_bIsCharging = true;
//        else if (context.canceled) { m_stats.m_bIsCharging = false; m_stats.ResetGauge(); }
//    }
//    public void OnFire(UnityEngine.InputSystem.InputAction.CallbackContext context) { }

//    // --- 강화 로직 ---
//    public void AddAttackPower(float fAmount)
//    {
//        if (!m_pv.IsMine) return;
//        m_stats.m_fCurrentAttackLevel = Mathf.Clamp01(m_stats.m_fCurrentAttackLevel + fAmount);
//        if (PlayerUIManager.Instance) PlayerUIManager.Instance.UpdateAttackGauge(m_stats.m_fCurrentAttackLevel);
//    }

//    public void AddSpeedPower(float fAmount)
//    {
//        if (!m_pv.IsMine) return;
//        m_stats.m_fCurrentSpeedLevel = Mathf.Clamp01(m_stats.m_fCurrentSpeedLevel + fAmount);
//        m_attack.SetCooldown(m_attack.GetCooldown() - fAmount);
//        if (PlayerUIManager.Instance) PlayerUIManager.Instance.UpdateSpeedGauge(m_stats.m_fCurrentSpeedLevel);
//    }

//    private void SetVisible(bool isVisible)
//    {
//        var sr = GetComponent<SpriteRenderer>();
//        if (sr) 
//            sr.enabled = isVisible;
//        foreach (Transform child in transform) child.gameObject.SetActive(isVisible);
//    }

//    public override void OnEnable() 
//    { 
//        base.OnEnable(); 
//        SceneManager.sceneLoaded += OnSceneLoaded; 

//        if (m_pv.IsMine) 
//            m_input.GamePlay.Enable(); 
//    }

//    public override void OnDisable() 
//    { 
//        base.OnDisable(); 
//        SceneManager.sceneLoaded -= OnSceneLoaded; 

//        m_input.GamePlay.Disable(); 
//    }

//    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//    {
//        PhotonNetwork.IsMessageQueueRunning = true;

//        if (scene.name == "GameScene")
//        {
//            SetVisible(true);
//            if (m_pv.IsMine)
//            {
//                var ui = FindFirstObjectByType<PlayerHealthUI>();
//                if (ui) m_health.AssignUI(ui);
//                m_stats.ultimateUI = FindFirstObjectByType<UltimateUIManager>();
//            }
//        }
//    }
//}
