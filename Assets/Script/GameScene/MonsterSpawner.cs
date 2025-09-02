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

    // 한번에 생성할 몬스터의 갯수 .
    public int monsterCountPerWave = 3;

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
            Debug.LogWarning("메인 카메라를 찾을 수 없습니다.");
            return;
        }

        // 화면 상에 보여질 비율을 고려해서 카메라를 가져와서.. 
        float fCamHeight = cam.orthographicSize * 2f;
        float fCamWidth = fCamHeight * cam.aspect;

        float fLeft = cam.transform.position.x - (fCamWidth / 2f) + fCamWidth * horizontalPadding;
        float fRight = cam.transform.position.x + (fCamWidth / 2f) - fCamWidth * horizontalPadding;

        float fTopY = cam.transform.position.y + cam.orthographicSize + offsetY;

        for (int i = 0; i < monsterCountPerWave; i++)
        {
            float xRatio = ( monsterCountPerWave == 1 ? 0.5f : (float)i / (monsterCountPerWave - 1) );
            
            float xPos = Mathf.Lerp(fLeft, fRight, xRatio);

            float xOffsetNoise = Random.Range(-randomOffset, randomOffset);

            Vector3 spawnPos = new Vector3(xPos + xOffsetNoise, fTopY, 0f);

            PhotonNetwork.Instantiate(monsterPath, spawnPos, Quaternion.identity);
        }
    }

    public void StopSpawning()
    {
        stopSpawning = true;
    }
}
