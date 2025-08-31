using UnityEngine;
using Photon.Pun;
using System.Collections;

public class MonsterSpawner : MonoBehaviourPun
{
    [Header("프리팹 경로 및 생성 설정")]
    public string monsterPath = "Monster/Monster";

    [Header("몬스터 위치 조정")]
    public float offsetY = 1.5f;            // 화면 위쪽으로 얼마나 더 나올지
    public float horizontalPadding = 0.2f;  // 좌우 여백 비율 (0~0.5)
    public float randomOffset = 0.1f;

    [Header("웨이브 설정")]
    public float spawnInterval = 2f;
    public int monstersPerWave = 3;

    private bool stopSpawning = false;

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SpawnLoop());
        }
    }

    IEnumerator SpawnLoop()
    {
        while (!stopSpawning)
        {
            SpawnWave();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnWave()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[MonsterSpawner] 메인 카메라를 찾을 수 없습니다.");
            return;
        }

        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;

        float left = cam.transform.position.x - (camWidth / 2f) + camWidth * horizontalPadding;
        float right = cam.transform.position.x + (camWidth / 2f) - camWidth * horizontalPadding;

        float topY = cam.transform.position.y + cam.orthographicSize + offsetY;

        for (int i = 0; i < monstersPerWave; i++)
        {
            // X 위치를 좌~우 균등 분배
            float t = monstersPerWave == 1 ? 0.5f : (float)i / (monstersPerWave - 1);
            float xPos = Mathf.Lerp(left, right, t);
            float jitter = Random.Range(-randomOffset, randomOffset);

            Vector3 spawnPos = new Vector3(xPos + jitter, topY, 0f);
            PhotonNetwork.Instantiate(monsterPath, spawnPos, Quaternion.identity);
        }
    }

    public void StopSpawning()
    {
        stopSpawning = true;
    }
}
