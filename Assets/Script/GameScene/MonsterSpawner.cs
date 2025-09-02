using UnityEngine;
using Photon.Pun;
using System.Collections;

public class MonsterSpawner : MonoBehaviourPun
{
    [Header("������ ��� �� ���� ����")]
    public string monsterPath = "Monster/Monster";

    [Header("���� ��ġ ����")]
    public float offsetY = 1.5f;            // ȭ�� �������� �󸶳� �� ������
    public float horizontalPadding = 0.2f;  // �¿� ���� ���� (0~0.5)
    public float randomOffset = 0.1f;

    [Header("���̺� ����")]
    public float spawnInterval = 2f;

    // �ѹ��� ������ ������ ���� .
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
            Debug.LogWarning("���� ī�޶� ã�� �� �����ϴ�.");
            return;
        }

        // ȭ�� �� ������ ������ ����ؼ� ī�޶� �����ͼ�.. 
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
