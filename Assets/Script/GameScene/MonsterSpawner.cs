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
            Debug.LogWarning("[MonsterSpawner] ���� ī�޶� ã�� �� �����ϴ�.");
            return;
        }

        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;

        float left = cam.transform.position.x - (camWidth / 2f) + camWidth * horizontalPadding;
        float right = cam.transform.position.x + (camWidth / 2f) - camWidth * horizontalPadding;

        float topY = cam.transform.position.y + cam.orthographicSize + offsetY;

        for (int i = 0; i < monstersPerWave; i++)
        {
            // X ��ġ�� ��~�� �յ� �й�
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
