using UnityEngine;
using Photon.Pun;
using System.Collections;

public class MonsterSpawner : MonoBehaviourPun
{
    [Header("몬스터 리소스 설정")]
    [SerializeField] private string                     strFolderPath = "Monster/";
    [SerializeField] private string[]                   arrMonsterNames = { "TinyShip7", "TinyShip8", "TinyShip9", "TinyShip10" };

    [Header("웨이브 밸런스 설정")]
    //[SerializeField] private float                      m_fSpawnInterval = 3.5f;   // 스폰 간격
    [SerializeField] private float                      m_fRandomOffset = 0.3f;

    [Header("화면 배치 설정")]
    [SerializeField] private float                      m_fOffsetY = 2.0f;
    [SerializeField] private float                      m_fHorizontalPadding = 0.15f;

    [Header("진행도 연동")]
    [SerializeField] private StageProgress              stageProgress;

    [System.Serializable]
    public struct WaveSetting
    {
        [Range(1, 10)] public int                       m_iMonsterCount;     // 스폰 마릿수
        [Range(0.1f, 10f)] public float                 m_fInterval;   // 다음 스폰까지의 대기 시간
    }

    [Header("시작 시간 설정")]
    [SerializeField] private float                      m_fStartDelayTime = 4.0f;

    [Header("진행도별 웨이브 설정 (0%, 20%, 50%, 80% 기준)")]
    public WaveSetting                                  waveStart;    // 0% ~ 20%
    public WaveSetting                                  waveEarly;    // 20% ~ 50%
    public WaveSetting                                  waveMid;      // 50% ~ 80%
    public WaveSetting                                  waveFinal;    // 80% ~ 100%
    private float                                       m_fCurrentInterval; // 현재 적용 중인 간격

    private int                                         m_iCurrentWaveCount;       // 현재 웨이브에서 쏠 마릿수
    private bool                                        m_bStopSpawning = false;

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // 비동기 함수 실행..
            StartCoroutine(CoSpawnLoop());
        }
    }

    // IEnumerator : 코루틴 함수가 반환해야하는 타입. 
    private IEnumerator CoSpawnLoop()
    {
        yield return new WaitForSeconds(m_fStartDelayTime);

        while (!m_bStopSpawning)
        {
            SpawnWave();

            // fSpawnInterval 초 동안 멈추니, 유니티가 다른 프레임 처리를 계속해. 시간 다 되면 다시 여기로 깨워줘라는 뜻
            yield return new WaitForSeconds(m_fCurrentInterval);
        }
    }

    private void SpawnWave()
    {
        Camera pMainCam = Camera.main;
        if (pMainCam == null) return;

        // 현재 진행도 가져오기 (0.0 ~ 1.0)
        float fProgress = stageProgress.GetCurrentProgress();

        WaveSetting tagCurrentSetting;

        // 단계별 마릿수 고정 
        // 진행도 구간에 따라 m_iCurrentWaveCount를 지정.
        if (fProgress < 0.2f)         // 0% ~ 20% 구간
        {
            //m_iCurrentWaveCount = 2;
            //m_fSpawnInterval = 6.5f;
            tagCurrentSetting = waveStart;
        }
        else if (fProgress < 0.5f)    // 20% ~ 50% 구간
        {
            //m_iCurrentWaveCount = 3;
            //m_fSpawnInterval = 6.5f;
            tagCurrentSetting = waveEarly;
        }
        else if (fProgress < 0.8f)    // 50% ~ 80% 구간
        {
            //m_iCurrentWaveCount = 4;
            //m_fSpawnInterval = 6.5f;
            tagCurrentSetting = waveMid;
        }
        else                          // 80% ~ 100% 구간 (최종장)
        {
            //m_iCurrentWaveCount = 5;
            //m_fSpawnInterval = 6.5f;
            tagCurrentSetting = waveFinal;
        }

        m_iCurrentWaveCount = tagCurrentSetting.m_iMonsterCount;
        m_fCurrentInterval = tagCurrentSetting.m_fInterval;

        // 화면 영역 및 위치 계산
        float fCamHeight = pMainCam.orthographicSize * 2f;
        float fCamWidth = fCamHeight * pMainCam.aspect;
        float fLeftBound = pMainCam.transform.position.x - (fCamWidth / 2f) + (fCamWidth * m_fHorizontalPadding);
        float fRightBound = pMainCam.transform.position.x + (fCamWidth / 2f) - (fCamWidth * m_fHorizontalPadding);
        float fSpawnY = pMainCam.transform.position.y + pMainCam.orthographicSize + m_fOffsetY;

        // 몬스터 생성 루프
        for (int i = 0; i < m_iCurrentWaveCount; i++)
        {
            float fXRatio = (m_iCurrentWaveCount == 1 ? 0.5f : (float)i / (m_iCurrentWaveCount - 1));
            float fXPos = Mathf.Lerp(fLeftBound, fRightBound, fXRatio);
            float fRandomX = Random.Range(-m_fRandomOffset, m_fRandomOffset);   
            float fRandomY = Random.Range(-m_fRandomOffset, m_fRandomOffset);

            Vector3 vSpawnPos = new Vector3(fXPos + fRandomX, fSpawnY + fRandomY, 0f);

            CreateRandomMonster(vSpawnPos, fProgress);
        }
    }

    private void CreateRandomMonster(Vector3 vPos, float fProgress)
    {
        int iMonsterIndex = Random.Range(0, arrMonsterNames.Length);
        string strPath = strFolderPath + arrMonsterNames[iMonsterIndex];

        GameObject monsterObj = PhotonNetwork.Instantiate(strPath, vPos, Quaternion.identity);

        MonsterController monsterController = monsterObj.GetComponent<MonsterController>();

        // 몬스터 개별 스탯 강화 
        if (monsterController != null)
        {
            monsterController.SetEnhancedStats(fProgress);
        }
    }

    public void StopSpawning()
    {
        m_bStopSpawning = true;
    }
}