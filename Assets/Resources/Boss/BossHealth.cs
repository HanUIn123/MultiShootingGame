using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.Collections;

public class BossHealth : MonoBehaviourPunCallbacks
{
    [Header("보스 체력 설정")]
    public float                    m_fMaxHP = 1000f;
    private float                   m_fCurrentHP;
    private float                   m_fTargetHP;

    [Header("HP UI 연결")]
    public GameObject               m_BossHpPanelObj;
    public Image                    m_imgHpFill;

    [Header("데미지 텍스트")]
    public GameObject               m_prefDamageText;
    private float                   m_fAccumulatedDamage = 0f;
    private float                   m_fLastDamageTextTime = 0f;
    public float                    m_fDamageTextCooldown = 0.15f;

    [Header("사망 연출")]
    public string                   m_strExplosionPrefabPath = "Boss/Explosion";

    private Material                m_matBoss;
    private Coroutine               m_coFlashLoop;
    private bool                    m_bIsDead = false;

    // public bool IsDead {get { return m_bIsDead; } }
    public bool IsDead =>           m_bIsDead;

    void Awake()
    {
        SpriteRenderer _sr = GetComponent<SpriteRenderer>();

        if (_sr != null)
        {
            m_matBoss = Instantiate(_sr.material);
            _sr.material = m_matBoss;
        }
    }

    void Start()
    {
        m_fCurrentHP = m_fMaxHP;
        m_fTargetHP = m_fMaxHP;
    }

    void Update()
    {
        if (m_imgHpFill != null)
        {
            float _fCurrentFill = m_imgHpFill.fillAmount;
            float _fTargetFill = Mathf.Clamp01(m_fTargetHP / m_fMaxHP);

            m_imgHpFill.fillAmount = Mathf.Lerp(_fCurrentFill, _fTargetFill, Time.deltaTime * 10f);
        }
    }

    [PunRPC]
    public void TakeDamage(float fDamage)
    {
        if (m_bIsDead || !gameObject.activeInHierarchy) 
            return;

        m_fAccumulatedDamage += fDamage;

        if (PhotonNetwork.IsMasterClient)
        {
            m_fCurrentHP = Mathf.Max(m_fCurrentHP - fDamage, 0f);

            photonView.RPC("UpdateHP", RpcTarget.All, m_fCurrentHP);

            if (m_fCurrentHP <= 0f)
            {
                m_bIsDead = true;
                photonView.RPC("Die", RpcTarget.All);
            }
            else
            {
                photonView.RPC("StartHitFlashLoop", RpcTarget.All);

                StartCoroutine(StopHitFlashAfterDelay(0.1f));
            }
        }

        if (Time.time - m_fLastDamageTextTime >= m_fDamageTextCooldown && m_fAccumulatedDamage >= 1f)
        {
            photonView.RPC("RPC_ShowDamageText", RpcTarget.All, m_fAccumulatedDamage);
            m_fAccumulatedDamage = 0f;
            m_fLastDamageTextTime = Time.time;
        }
    }

    [PunRPC]
    void UpdateHP(float fHp)
    {
        m_fCurrentHP = fHp;
        m_fTargetHP = fHp;
    }

    [PunRPC]
    void StartHitFlashLoop()
    {
        if (m_coFlashLoop != null) 
            StopCoroutine(m_coFlashLoop);

        m_coFlashLoop = StartCoroutine(HitFlashLoop());
    }

    private IEnumerator StopHitFlashAfterDelay(float fDelay)
    {
        yield return new WaitForSeconds(fDelay);

        photonView.RPC("StopHitFlashLoop", RpcTarget.All);
    }

    [PunRPC]
    public void StopHitFlashLoop()
    {
        if (m_coFlashLoop != null) 
            StopCoroutine(m_coFlashLoop);

        m_coFlashLoop = null;

        if (m_matBoss != null) 
            m_matBoss.SetFloat("_WhiteAmount", 0f);
    }

    IEnumerator HitFlashLoop()
    {
        while (true)
        {
            if (m_matBoss == null) break;
            m_matBoss.SetFloat("_WhiteAmount", 1f);
            yield return new WaitForSeconds(0.05f);
            m_matBoss.SetFloat("_WhiteAmount", 0f);
            yield return new WaitForSeconds(0.05f);
        }
    }

    [PunRPC]
    void RPC_ShowDamageText(float fAmount)
    {
        GameObject canvasObj = GameObject.Find("DamageTextCanvas");

        if (m_prefDamageText == null || canvasObj == null) 
            return;

        GameObject textObj = Instantiate(m_prefDamageText, canvasObj.transform);

        textObj.transform.position = transform.position + new Vector3(0, 1.2f, 0);
        textObj.transform.localScale = new Vector3(0.25f, 0.25f, 1f);

        /*
          
            DamageText damageScript = textObj.GetComponent<DamageText>();
            
            if(damageScript != null) 
            {
               damageScript.Setup(Mathf.RoundToInt(fMount)); 
            }

        */

        textObj.GetComponent<DamageText>()?.Setup(Mathf.RoundToInt(fAmount));
    }

    [PunRPC]
    void Die()
    {
        m_bIsDead = true;

        GetComponent<BossController>().StopAllCoroutines();

        if (m_BossHpPanelObj != null) 
            m_BossHpPanelObj.SetActive(false);

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        float fExplosionRad = 1.5f;
        int iExplosionCount = 28;

        for (int i = 0; i < iExplosionCount; i++)
        {
            Vector3 v3ExplodePos = 
                transform.position + new Vector3(Random.Range(-fExplosionRad, fExplosionRad), Random.Range(-fExplosionRad, fExplosionRad), 0);

            GameObject explodeObj = Resources.Load<GameObject>(m_strExplosionPrefabPath);

            if (explodeObj != null)
            {
                GameObject instance = Instantiate(explodeObj, v3ExplodePos, Quaternion.identity);
                instance.transform.localScale = Vector3.one * Random.Range(0.8f, 1.5f);
            }
            yield return new WaitForSeconds(0.08f);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            if (GameSceneManager.Instance != null) 
                GameSceneManager.Instance.StartClearSequence();

            PhotonNetwork.Destroy(gameObject);
        }
    }
}