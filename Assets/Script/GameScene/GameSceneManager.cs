using UnityEngine;
using Photon.Pun;

public class GameSceneManager : MonoBehaviour
{
    public string monsterPrefabPath = "Monster/Monster"; // Resources/Monster/Monster.prefab
    public Vector2 spawnPosition = new Vector2(0f, 5f); // 위에서 등장


    private void Start()
    {
        // 마스터 클라이언트만 몬스터 생성
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnMonster();
        }
    }

    void SpawnMonster()
    {
        PhotonNetwork.Instantiate(monsterPrefabPath, spawnPosition, Quaternion.identity);
    }

    private void Update()
    {
        // 추후 게임 상태 관리 등 여기서 계속 확장 가능
    }
}
