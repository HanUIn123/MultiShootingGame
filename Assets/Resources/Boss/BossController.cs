using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.Collections;

public class BossController : MonoBehaviourPunCallbacks
{
    [Header("���� ü�� ����")]
    public float maxHP = 100f;
    private float currentHP;

    [Header("HP�� ����")]
    public Slider hpSlider;

    [Header("ź�� ������ ��� (Resources ���� ��)")]
    public string bulletPrefabPath = "Boss/BossBulletPrefab";

    [Header("ź�� �߻� ��ġ (������ transform.position ���)")]
    public Transform firePoint;

    private bool hasStarted = false;

    private Material bossMat;
    private Coroutine hitEffectRoutine;

    private void Awake()
    {
        // SpriteRenderer���� Material �ν��Ͻ��� ������
        bossMat = GetComponent<SpriteRenderer>().material;
    }

    private void Start()
    {
        currentHP = maxHP;

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }

        if (PhotonNetwork.IsMasterClient && !hasStarted)
        {
            hasStarted = true;
            StartCoroutine(BossAttackPattern());
        }
    }

    [PunRPC]
    public void TakeDamage(float damage)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (currentHP <= 0f) return;

        currentHP -= damage;

        if (currentHP <= 0f)
        {
            Die();
        }
        else
        {
            photonView.RPC("UpdateHP", RpcTarget.All, currentHP);
            photonView.RPC("PlayHitEffect", RpcTarget.All); // �ǰ� �� ��¦�̱�
        }
    }

    [PunRPC]
    void UpdateHP(float hp)
    {
        currentHP = hp;

        if (hpSlider != null)
        {
            hpSlider.value = hp;
        }
    }

    [PunRPC]
    void PlayHitEffect()
    {
        if (hitEffectRoutine != null)
            StopCoroutine(hitEffectRoutine);

        hitEffectRoutine = StartCoroutine(HitFlashRoutine());
    }

    IEnumerator HitFlashRoutine()
    {
        bossMat.SetFloat("_WhiteAmount", 1f);

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - (elapsed / duration);
            bossMat.SetFloat("_WhiteAmount", t);
            yield return null;
        }

        bossMat.SetFloat("_WhiteAmount", 0f);
    }

    void Die()
    {
        photonView.RPC("UpdateHP", RpcTarget.All, 0f);

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    IEnumerator BossAttackPattern()
    {
        while (true)
        {
            int patternIndex = Random.Range(0, 3); // 0~2�� ���� �� ���� ����

            switch (patternIndex)
            {
                case 0:
                    yield return StartCoroutine(Pattern_Circle());
                    break;
                case 1:
                    yield return StartCoroutine(Pattern_Spiral());
                    break;
                case 2:
                    yield return StartCoroutine(Pattern_Shotgun());
                    break;
            }

            yield return new WaitForSeconds(2.0f); // ���� ���� �� ������
        }
    }

    IEnumerator Pattern_Circle()
    {
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        int bulletCount = 36;
        float angleStep = 360f / bulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);

            GameObject bullet = PhotonNetwork.Instantiate(bulletPrefabPath, spawnPos, Quaternion.identity);
            bullet.GetComponent<BossBullet>().SetDirection(direction);
        }

        yield return null;
    }

    IEnumerator Pattern_Spiral()
    {
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        float startAngle = Random.Range(0f, 360f);

        for (int i = 0; i < 36; i++)
        {
            float angle = startAngle + i * 10f;
            float rad = angle * Mathf.Deg2Rad;

            Vector3 direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);

            GameObject bullet = PhotonNetwork.Instantiate(bulletPrefabPath, spawnPos, Quaternion.identity);
            bullet.GetComponent<BossBullet>().SetDirection(direction);

            yield return new WaitForSeconds(0.05f);
        }
    }

    IEnumerator Pattern_Shotgun()
    {
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        int bulletCount = 10;
        float spreadAngle = 45f;
        float baseAngle = -spreadAngle / 2f;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = baseAngle + i * (spreadAngle / (bulletCount - 1));
            float rad = angle * Mathf.Deg2Rad;

            Vector3 direction = new Vector3(Mathf.Sin(rad), -Mathf.Cos(rad), 0f); // �Ʒ� ���� ����

            GameObject bullet = PhotonNetwork.Instantiate(bulletPrefabPath, spawnPos, Quaternion.identity);
            bullet.GetComponent<BossBullet>().SetDirection(direction);
        }

        yield return null;
    }
}
