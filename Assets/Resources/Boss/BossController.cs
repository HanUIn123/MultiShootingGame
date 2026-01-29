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
