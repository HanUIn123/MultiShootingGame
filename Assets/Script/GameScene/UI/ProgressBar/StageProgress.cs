using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;

public class StageProgress : MonoBehaviourPun
{
    [Header("진행도 UI")]
    public Image                                            ImgProgressFill;
    public float                                            m_fStageDuration = 60f;

    [Header("Warning UI")]
    public GameObject                                       warningSignObj;
    public float                                            m_fWarningDuration = 3f;
    public float                                            m_fBlinkInterval = 0.3f;

    [Header("보스 소환")]
    public Vector3                                          v3BossSpawnPosition = new Vector3(0f, 4.0f, 0f);
    public string                                           strBossPrefabName = "Boss/TinyShip16";

    [Header("보스 체력바 UI")]
    public GameObject                                       bossHpPanelObj;
    public Image                                            ImgBossHpFill;

    [Header("몬스터 스포너 연결")]
    [SerializeField] private MonsterSpawner                 monsterSpawner;

    private double                                          m_dStartTime = -1;
    private bool                                            warningTriggered = false;

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // 방장은 시간 세팅
            m_dStartTime = PhotonNetwork.Time;
            photonView.RPC(nameof(SetStartTime), RpcTarget.AllBuffered, m_dStartTime);
        }
        else
        {
            // 팀원은 방장한테 시간을 다시 한 번 물어봐서, 
            // 만약 RPC가 씹혔을 경우를 대비해서 방장에게 다시 시간좀 알려달라고 요청
            photonView.RPC(nameof(RequestStartTime), RpcTarget.MasterClient);
        }
    }

    [PunRPC]
    void RequestStartTime(PhotonMessageInfo info)
    {
        // 방장만 실행됨: 요청한 사람한테만 현재 startTime을 쏴줌
        photonView.RPC(nameof(SetStartTime), info.Sender, m_dStartTime);
    }

    [PunRPC]
    void SetStartTime(double time)
    {
        m_dStartTime = time;
        // 팀원이 난입 즉시, UI 게이지를 현재 시간에 맞게 강제로 세팅해줌.

        // Update가 돌기 전, RPC를 받은 시점에 바로 UI를 점프시킴.
        if (m_dStartTime > 0)
        {
            // PhotonNetwork.Time -> 이 double 형 반환함.
            double dElapsedTime = PhotonNetwork.Time - m_dStartTime;

            float fFillAmount = Mathf.Clamp01((float)(dElapsedTime / m_fStageDuration));

            UpdateFill(fFillAmount);
        }
    }

    void Update()
    {
        if (m_dStartTime < 0)
            return;

        double dElapsedTime = PhotonNetwork.Time - m_dStartTime;

        float fFillAmount = Mathf.Clamp01((float)(dElapsedTime / m_fStageDuration));

        UpdateFill(fFillAmount);

        if (PhotonNetwork.IsMasterClient && !warningTriggered && fFillAmount >= 1f)
        {
            warningTriggered = true;
            photonView.RPC(nameof(ShowWarning), RpcTarget.AllBuffered);
        }
    }

    void UpdateFill(float fFillAmount)
    {
        if (ImgProgressFill != null)
            ImgProgressFill.fillAmount = fFillAmount;
    }

    [PunRPC]
    void ShowWarning()
    {
        if (warningSignObj != null)
        {
            warningSignObj.SetActive(true);
            StartCoroutine(BlinkWarning());
        }
    }

    IEnumerator BlinkWarning()
    {
        float fElapsedTime = 0f;
        Image WarningImg = warningSignObj.GetComponent<Image>();

        while (fElapsedTime < m_fWarningDuration)
        {
            WarningImg.enabled = !WarningImg.enabled;

            yield return new WaitForSeconds(m_fBlinkInterval);

            fElapsedTime += m_fBlinkInterval;
        }

        WarningImg.enabled = false;
        warningSignObj.SetActive(false);

        if (PhotonNetwork.IsMasterClient)
        {
            GameObject bossObj = PhotonNetwork.Instantiate(strBossPrefabName, v3BossSpawnPosition, Quaternion.identity);
            BossHealth bossHealthComp = bossObj.GetComponent<BossHealth>();
            BossController bossController = bossObj.GetComponent<BossController>();

            bossHealthComp.m_imgHpFill = ImgBossHpFill;
            bossHealthComp.m_BossHpPanelObj = bossHpPanelObj;

            bossController.photonView.RPC("InitBossUI", RpcTarget.AllBuffered);
            bossController.StartBossBattle();

            if (monsterSpawner == null)
                monsterSpawner = FindFirstObjectByType<MonsterSpawner>();

            if (monsterSpawner != null)
                monsterSpawner.StopSpawning();
        }
    }

    // StageProgress.cs 내부에 추가
    public float GetCurrentProgress()
    {
        if (m_dStartTime < 0) 
            return 0f;

        double dElapsed = PhotonNetwork.Time - m_dStartTime;

        return Mathf.Clamp01((float)(dElapsed / m_fStageDuration));
    }
}
