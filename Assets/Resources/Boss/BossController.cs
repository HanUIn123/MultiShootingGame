using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.Collections;

public class BossController : MonoBehaviourPunCallbacks
{
    [Header("보스 체력 설정")]
    public float fMaxHP = 1000f;
    private float fCurrentHP;
    private float fTargetHP;

    [Header("HP 패널 오브젝트 (SetActive 용)")]
    [HideInInspector] public GameObject goBossHpPanel;

    [Header("HP 이미지 연결 (Fill 방식 Image)")]
    [HideInInspector] public Image imgHpFill;

    [Header("탄막 프리팹 경로 (Resources 폴더 안)")]
    public string strBulletPrefabPath = "Boss/BossBulletPrefab";

    [Header("탄막 발사 위치")]
    public Transform trFirePoint;

    private Material matBoss;
    private Coroutine coFlashLoop;
    private float fLastLaserHitTime = 0f;
    public float fLaserDamageCooldown = 0.2f;

    private void Awake()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        matBoss = Instantiate(sr.material);
        sr.material = matBoss;
    }

    private void Start()
    {
        fCurrentHP = fMaxHP;
        fTargetHP = fMaxHP;

        if (imgHpFill != null)
            imgHpFill.fillAmount = 1f;
    }

    private void Update()
    {
        if (imgHpFill == null) return;

        float fCurrentFill = imgHpFill.fillAmount;
        float fTargetFill = Mathf.Clamp01(fTargetHP / fMaxHP);
        imgHpFill.fillAmount = Mathf.Lerp(fCurrentFill, fTargetFill, Time.deltaTime * 10f);
    }

    public void StartBossBattle()
    {
        if (goBossHpPanel == null) return;

        Transform trBg = goBossHpPanel.transform.Find("BossHp_BG");
        Transform trFill = goBossHpPanel.transform.Find("BossHp_Fill");

        if (trBg) trBg.gameObject.SetActive(true);
        if (trFill) trFill.gameObject.SetActive(true);

        StartCoroutine(BossAttackPattern());
    }

    [PunRPC]
    public void InitBossUI()
    {
        goBossHpPanel = GameObject.Find("BossHp_Panel");
        if (goBossHpPanel == null) return;

        goBossHpPanel.SetActive(true);

        Transform trBg = goBossHpPanel.transform.Find("BossHp_BG");
        Transform trFill = goBossHpPanel.transform.Find("BossHp_Fill");

        if (trBg) trBg.gameObject.SetActive(true);
        if (trFill)
        {
            trFill.gameObject.SetActive(true);
            imgHpFill = trFill.GetComponent<Image>();
        }
    }

    [PunRPC]
    public void TakeDamage(float fDamage)
    {
        if (!PhotonNetwork.IsMasterClient || fCurrentHP <= 0f) return;

        fLastLaserHitTime = Time.time;
        fCurrentHP -= fDamage;
        fCurrentHP = Mathf.Max(fCurrentHP, 0f);

        photonView.RPC("UpdateHP", RpcTarget.All, fCurrentHP);

        if (fCurrentHP <= 0f)
        {
            Die();
            return;
        }

        photonView.RPC("StartHitFlashLoop", RpcTarget.All);
        StartCoroutine(StopHitFlashAfterDelay(0.1f));
    }

    private IEnumerator StopHitFlashAfterDelay(float fDelay)
    {
        yield return new WaitForSeconds(fDelay);
        photonView.RPC("StopHitFlashLoop", RpcTarget.All);
    }

    [PunRPC]
    void UpdateHP(float fHp)
    {
        fCurrentHP = fHp;
        fTargetHP = fHp;
    }

    [PunRPC]
    void StartHitFlashLoop()
    {
        if (coFlashLoop != null)
            StopCoroutine(coFlashLoop);

        coFlashLoop = StartCoroutine(HitFlashLoop());
    }

    [PunRPC]
    void StopHitFlashLoop()
    {
        if (coFlashLoop != null)
            StopCoroutine(coFlashLoop);

        coFlashLoop = null;

        if (matBoss != null)
            matBoss.SetFloat("_WhiteAmount", 0f);
    }

    IEnumerator HitFlashLoop()
    {
        while (true)
        {
            if (matBoss == null) yield break;

            matBoss.SetFloat("_WhiteAmount", 1f);
            yield return new WaitForSeconds(0.05f);

            matBoss.SetFloat("_WhiteAmount", 0f);
            yield return new WaitForSeconds(0.05f);
        }
    }

    void Die()
    {
        photonView.RPC("HideBossUI", RpcTarget.All);

        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(gameObject);
    }

    [PunRPC]
    void HideBossUI()
    {
        if (goBossHpPanel != null)
            goBossHpPanel.SetActive(false);
    }

    public void OnLaserFinished()
    {
        photonView.RPC("StopHitFlashLoop", RpcTarget.All);
    }

    IEnumerator BossAttackPattern()
    {
        while (true)
        {
            int iPatternIndex = Random.Range(0, 3);

            switch (iPatternIndex)
            {
                case 0: yield return StartCoroutine(Pattern_Circle()); break;
                case 1: yield return StartCoroutine(Pattern_Spiral()); break;
                case 2: yield return StartCoroutine(Pattern_Shotgun()); break;
            }

            yield return new WaitForSeconds(2.0f);
        }
    }

    IEnumerator Pattern_Circle()
    {
        Vector3 v3SpawnPos = trFirePoint ? trFirePoint.position : transform.position;
        int iBulletCount = 36;
        float fAngleStep = 360f / iBulletCount;

        for (int i = 0; i < iBulletCount; i++)
        {
            float fAngle = i * fAngleStep * Mathf.Deg2Rad;
            Vector3 v3Dir = new Vector3(Mathf.Cos(fAngle), Mathf.Sin(fAngle), 0f);

            GameObject goBullet = PhotonNetwork.Instantiate(strBulletPrefabPath, v3SpawnPos, Quaternion.identity);
            goBullet.GetComponent<BossBullet>()?.SetDirection(v3Dir);
        }

        yield return null;
    }

    IEnumerator Pattern_Spiral()
    {
        Vector3 v3SpawnPos = trFirePoint ? trFirePoint.position : transform.position;
        float fStartAngle = Random.Range(0f, 360f);

        for (int i = 0; i < 36; i++)
        {
            float fAngle = fStartAngle + i * 10f;
            float fRad = fAngle * Mathf.Deg2Rad;
            Vector3 v3Dir = new Vector3(Mathf.Cos(fRad), Mathf.Sin(fRad), 0f);

            GameObject goBullet = PhotonNetwork.Instantiate(strBulletPrefabPath, v3SpawnPos, Quaternion.identity);
            goBullet.GetComponent<BossBullet>()?.SetDirection(v3Dir);

            yield return new WaitForSeconds(0.05f);
        }
    }

    IEnumerator Pattern_Shotgun()
    {
        Vector3 v3SpawnPos = trFirePoint ? trFirePoint.position : transform.position;
        int iBulletCount = 10;
        float fSpreadAngle = 45f;
        float fBaseAngle = -fSpreadAngle / 2f;

        for (int i = 0; i < iBulletCount; i++)
        {
            float fAngle = fBaseAngle + i * (fSpreadAngle / (iBulletCount - 1));
            float fRad = fAngle * Mathf.Deg2Rad;
            Vector3 v3Dir = new Vector3(Mathf.Sin(fRad), -Mathf.Cos(fRad), 0f);

            GameObject goBullet = PhotonNetwork.Instantiate(strBulletPrefabPath, v3SpawnPos, Quaternion.identity);
            goBullet.GetComponent<BossBullet>()?.SetDirection(v3Dir);
        }

        yield return null;
    }
}
