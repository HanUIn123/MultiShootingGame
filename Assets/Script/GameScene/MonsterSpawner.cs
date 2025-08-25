using UnityEngine;
using Photon.Pun;
using System.Collections;

public class MonsterSpawner : MonoBehaviourPun
{
    public string monsterPath = "Monster/Monster";
    public float spawnXMin = -0.5f;
    public float spawnXMax = 0.5f;
    public float spawnY = 7f;

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
        for (int i = 0; i < monstersPerWave; i++)
        {
            float randomX = Random.Range(spawnXMin, spawnXMax);
            Vector3 spawnPos = new Vector3(randomX, spawnY, 0);
            PhotonNetwork.Instantiate(monsterPath, spawnPos, Quaternion.identity);
        }
    }

    // 외부에서 호출해 멈추게 할 함수
    public void StopSpawning()
    {
        stopSpawning = true;
    }
}
