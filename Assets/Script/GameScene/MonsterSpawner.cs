using UnityEngine;
using Photon.Pun;
using System.Collections;

public class MonsterSpawner : MonoBehaviourPun
{
    public string monsterPath = "Monster/Monster";
    public float spawnXMin = -0.5f;
    public float spawnXMax = 0.5f;
    public float spawnY = 7f;

    public float spawnInterval = 2f;  // 몬스터 생성 주기 (초)
    public int monstersPerWave = 3;  // 한 번에 생성할 마릿수

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

            //  기존 방식 (소유자가 나가면 문제가 생김)
            PhotonNetwork.Instantiate(monsterPath, spawnPos, Quaternion.identity);

            //  변경 방식 (MasterClient 소유로 고정됨)
            //GameObject monster = PhotonNetwork.InstantiateRoomObject(monsterPath, spawnPos, Quaternion.identity);
        }
    }


}
