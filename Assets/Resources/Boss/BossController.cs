using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.Collections;

public class BossController : MonoBehaviourPunCallbacks
{
    [Header("보스 체력 설정")]
    public float maxHP = 100f;
    private float currentHP;

    [Header("HP바 연결")]
    public Slider hpSlider;

    [Header("탄막 프리팹 경로 (Resources 폴더 안)")]
    public string bulletPrefabPath = "Boss/BossBulletPrefab";

    [Header("탄막 발사 위치 (없으면 transform.position 사용)")]
    public Transform firePoint;

    private bool hasStarted = false;

    private Material bossMat;
    private Coroutine hitEffectRoutine;

    private void Awake()
    {
        // SpriteRenderer에서 Material 인스턴스를 가져옴
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
            photonView.RPC("PlayHitEffect", RpcTarget.All); // 피격 시 반짝이기
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
            int patternIndex = Random.Range(0, 3); // 0~2번 패턴 중 랜덤 선택

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

            yield return new WaitForSeconds(2.0f); // 다음 패턴 전 딜레이
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

            Vector3 direction = new Vector3(Mathf.Sin(rad), -Mathf.Cos(rad), 0f); // 아래 방향 기준

            GameObject bullet = PhotonNetwork.Instantiate(bulletPrefabPath, spawnPos, Quaternion.identity);
            bullet.GetComponent<BossBullet>().SetDirection(direction);
        }

        yield return null;
    }
}
