using UnityEngine;
using Photon.Pun;
using System.Collections;

public class MonsterSpawner : MonoBehaviourPun
{
    public string monsterPath = "Monster/Monster";
    public float spawnXMin = -0.5f;
    public float spawnXMax = 0.5f;
    public float spawnY = 7f;

    public float spawnInterval = 2f;  // ���� ���� �ֱ� (��)
    public int monstersPerWave = 3;  // �� ���� ������ ������

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SpawnLoop());
        }
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnWave();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnWave()
    {
        for (int i = 0; i < monstersPerWave; i++)
        {
            float randomX = Random.Range(spawnXMin, spawnXMax);
            Vector3 spawnPos = new Vector3(randomX, spawnY, 0);

            //  ���� ��� (�����ڰ� ������ ������ ����)
            PhotonNetwork.Instantiate(monsterPath, spawnPos, Quaternion.identity);

            //  ���� ��� (MasterClient ������ ������)
            //GameObject monster = PhotonNetwork.InstantiateRoomObject(monsterPath, spawnPos, Quaternion.identity);
        }
    }


}
