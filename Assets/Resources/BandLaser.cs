using UnityEngine;
using Photon.Pun;

public class BendLaser : MonoBehaviourPun
{
    private LineRenderer                                m_compLineRenderer;
    private Transform                                   m_target;
    private MonsterController                           m_targetMonster; 
    private float                                       m_fDamage;
    private float                                       m_fLifeTime = 0.8f; 
    private float                                       m_fTimer;

    [SerializeField] private int                        m_iSegmentCount = 8; 
    [SerializeField] private float                      m_fBendAmount = 0.3f;           // 구불거림 정도 

    private Transform                                   m_trPlayer;
    private PhotonView                                  m_targetPV;                     // 타겟의 포톤뷰 미리 저장


    private float                                       m_fAccumulatedDeltaDamage = 0f; // RPC 쏠 때까지 모아둘 데미지
    private float                                       m_fNetworkSendInterval = 0.2f;  // 0.2초 간격 
    private float                                       m_fNetworkTimer = 0f;           // 간격 계산용 타이머


    private void Awake()
    {
        m_compLineRenderer = GetComponent<LineRenderer>();
    }

    // RPC를 통해 모든 클라이언트에서 타겟 설정. 
    [PunRPC]
    public void RPC_SetupLaser(int iPlayerViewId, int iTargetViewId, float fDamage)
    {
        PhotonView playerPV= PhotonView.Find(iPlayerViewId);

        if (playerPV != null) 
            m_trPlayer = playerPV.transform;

        PhotonView targetPV = PhotonView.Find(iTargetViewId);

        if (targetPV != null)
        {
            m_target = targetPV.transform;
            m_targetMonster = m_target.GetComponent<MonsterController>();
            m_targetPV = targetPV;
        }

        m_fDamage = fDamage;

        m_fTimer = 0f;

        m_compLineRenderer.positionCount = m_iSegmentCount;

    }

    private void Update()
    {
        if (m_trPlayer == null || m_target == null)
            return;

        if (photonView.IsMine)
        {
            m_fTimer += Time.deltaTime;

            // 레이저 수명 다하거나 타겟 사라지면 파괴
            if (m_fTimer >= m_fLifeTime || !m_target.gameObject.activeInHierarchy)
            {
                // 파괴 직전에 남은 데미지가 있다면 쏘고 가는 게 깔끔함 (선택사항)
                if (m_fAccumulatedDeltaDamage > 0)
                    m_targetPV.RPC("TakeDamage", RpcTarget.MasterClient, m_fAccumulatedDeltaDamage);

                PhotonNetwork.Destroy(this.gameObject);

                return;
            }

            if (m_targetPV != null)
            {
                // 매 프레임 데미지를 변수에 계속 더함 (아직 RPC 안 쏨)
                float fFrameDamage = m_fDamage * Time.deltaTime * 5f;

                m_fAccumulatedDeltaDamage += fFrameDamage;

                m_fNetworkTimer += Time.deltaTime;

                // 0.2초가 됐을 때만 RPC 한 번 딱!
                if (m_fNetworkTimer >= m_fNetworkSendInterval)
                {
                    if (m_targetPV != null && m_targetPV.gameObject.activeInHierarchy)
                    {
                        m_targetPV.RPC("TakeDamage", RpcTarget.MasterClient, m_fAccumulatedDeltaDamage);
                    }

                    // 쐈으니까 누적값들 초기화
                    m_fAccumulatedDeltaDamage = 0f;
                    m_fNetworkTimer = 0f;
                }
            }
        }

        RenderLaser();
    }

    private void RenderLaser()
    {
        if (m_trPlayer == null || m_target == null) 
            return;

        // 레이저 시작지점 , 끝 지점 세팅
        Vector3 v3StartPos = m_trPlayer.position;
        Vector3 v3EndPos = m_target.position;

        float fTimeValue= Time.time * 20f;

        // 레이저를 몇 개의 점으로?
        for (int i = 0; i < m_iSegmentCount; i++)
        {
            // 0 ~ 1
            float t = (float)i / (m_iSegmentCount - 1);

            Vector3 v3Pos = Vector3.Lerp(v3StartPos, v3EndPos, t);

            // 양 끝 점 사이의 점들을 sin 으로 흔든다.
            if (i > 0 && i < m_iSegmentCount - 1)
            {
                v3Pos.x += Mathf.Sin(fTimeValue + i) * m_fBendAmount * t;
            }

            m_compLineRenderer.SetPosition(i, v3Pos);
        }
    }
}
