using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.Collections;

public class BossController : MonoBehaviourPunCallbacks
{
    [Header("���� ü�� ����")]
    public float maxHP = 1000f;
    private float currentHP;
    private float targetHP;

    [Header("HP �г� ������Ʈ (SetActive ��)")]
    [HideInInspector] public GameObject bossHpPanel;

    [Header("HP �̹��� ���� (Fill ��� Image)")]
    [HideInInspector] public Image hpFillImage;

    [Header("ź�� ������ ��� (Resources ���� ��)")]
    public string bulletPrefabPath = "Boss/BossBulletPrefab";

    [Header("ź�� �߻� ��ġ")]
    public Transform firePoint;

    private Material bossMat;
    private Coroutine flashLoopRoutine;
    private float lastLaserHitTime = 0f;
    public float laserDamageCooldown = 0.2f;

    private void Awake()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            bossMat = Instantiate(sr.material); // �ν��Ͻ� �����Ͽ� ���� ����
            sr.material = bossMat;
        }
        else
        {
            Debug.LogWarning("SpriteRenderer�� �����ϴ�.");
        }
    }

    private void Start()
    {
        currentHP = maxHP;
        targetHP = maxHP;

        if (hpFillImage != null)
            hpFillImage.fillAmount = 1f;
    }

    private void Update()
    {
        if (hpFillImage != null)
        {
            float currentFill = hpFillImage.fillAmount;
            float targetFill = Mathf.Clamp01(targetHP / maxHP);
            hpFillImage.fillAmount = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * 10f);
        }
    }

    public void StartBossBattle()
    {
        if (bossHpPanel != null)
        {
            Transform bg = bossHpPanel.transform.Find("BossHp_BG");
            Transform fill = bossHpPanel.transform.Find("BossHp_Fill");

            if (bg) bg.gameObject.SetActive(true);
            if (fill) fill.gameObject.SetActive(true);
        }

        StartCoroutine(BossAttackPattern());
    }

    [PunRPC]
    public void InitBossUI()
    {
        bossHpPanel = GameObject.Find("BossHp_Panel");

        if (bossHpPanel)
        {
            bossHpPanel.SetActive(true);

            Transform bg = bossHpPanel.transform.Find("BossHp_BG");
            Transform fill = bossHpPanel.transform.Find("BossHp_Fill");

            if (bg) bg.gameObject.SetActive(true);
            if (fill)
            {
                fill.gameObject.SetActive(true);
                hpFillImage = fill.GetComponent<Image>();
            }
        }
    }

    [PunRPC]
    public void TakeDamage(float damage)
    {
        if (!PhotonNetwork.IsMasterClient || currentHP <= 0f) return;

        lastLaserHitTime = Time.time;
        currentHP -= damage;

        if (currentHP <= 0f)
        {
            currentHP = 0f;
            photonView.RPC("UpdateHP", RpcTarget.All, 0f);
            Die();
        }
        else
        {
            photonView.RPC("UpdateHP", RpcTarget.All, currentHP);
            photonView.RPC("StartHitFlashLoop", RpcTarget.All);
            StartCoroutine(StopHitFlashAfterDelay(0.1f));
        }
    }

    private IEnumerator StopHitFlashAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        photonView.RPC("StopHitFlashLoop", RpcTarget.All);
    }

    [PunRPC]
    void UpdateHP(float hp)
    {
        currentHP = hp;
        targetHP = hp;
    }

    [PunRPC]
    void StartHitFlashLoop()
    {
        if (flashLoopRoutine != null)
            StopCoroutine(flashLoopRoutine);

        flashLoopRoutine = StartCoroutine(HitFlashLoop());
    }

    [PunRPC]
    void StopHitFlashLoop()
    {
        if (flashLoopRoutine != null)
            StopCoroutine(flashLoopRoutine);

        flashLoopRoutine = null;

        if (bossMat != null)
            bossMat.SetFloat("_WhiteAmount", 0f);
    }

    IEnumerator HitFlashLoop()
    {
        while (true)
        {
            if (bossMat != null)
            {
                bossMat.SetFloat("_WhiteAmount", 1f);
                yield return new WaitForSeconds(0.05f);
                bossMat.SetFloat("_WhiteAmount", 0f);
                yield return new WaitForSeconds(0.05f);
            }
            else
            {
                yield break;
            }
        }
    }

    void Die()
    {
        //if (bossHpPanel)
        //    bossHpPanel.SetActive(false);

        //if (PhotonNetwork.IsMasterClient)
        //    PhotonNetwork.Destroy(gameObject);

        // ��� Ŭ���̾�Ʈ���� UI ��Ȱ��ȭ
        photonView.RPC("HideBossUI", RpcTarget.All);

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    [PunRPC]
    void HideBossUI()
    {
        if (bossHpPanel != null)
            bossHpPanel.SetActive(false);
    }

    public void OnLaserFinished()
    {
        photonView.RPC("StopHitFlashLoop", RpcTarget.All);
    }

    IEnumerator BossAttackPattern()
    {
        while (true)
        {
            int patternIndex = Random.Range(0, 3);

            switch (patternIndex)
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
        Vector3 spawnPos = firePoint ? firePoint.position : transform.position;
        int bulletCount = 36;
        float angleStep = 360f / bulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);

            GameObject bullet = PhotonNetwork.Instantiate(bulletPrefabPath, spawnPos, Quaternion.identity);
            bullet.GetComponent<BossBullet>()?.SetDirection(dir);
        }

        yield return null;
    }

    IEnumerator Pattern_Spiral()
    {
        Vector3 spawnPos = firePoint ? firePoint.position : transform.position;
        float startAngle = Random.Range(0f, 360f);

        for (int i = 0; i < 36; i++)
        {
            float angle = startAngle + i * 10f;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);

            GameObject bullet = PhotonNetwork.Instantiate(bulletPrefabPath, spawnPos, Quaternion.identity);
            bullet.GetComponent<BossBullet>()?.SetDirection(dir);

            yield return new WaitForSeconds(0.05f);
        }
    }

    IEnumerator Pattern_Shotgun()
    {
        Vector3 spawnPos = firePoint ? firePoint.position : transform.position;
        int bulletCount = 10;
        float spreadAngle = 45f;
        float baseAngle = -spreadAngle / 2f;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = baseAngle + i * (spreadAngle / (bulletCount - 1));
            float rad = angle * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Sin(rad), -Mathf.Cos(rad), 0f);

            GameObject bullet = PhotonNetwork.Instantiate(bulletPrefabPath, spawnPos, Quaternion.identity);
            bullet.GetComponent<BossBullet>()?.SetDirection(dir);
        }

        yield return null;
    }
}
