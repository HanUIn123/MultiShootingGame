using UnityEngine;
using Photon.Pun;

public class GameSceneManager : MonoBehaviour
{
    [Header("���� ����")]
    public string monsterPrefabPath = "Monster/Monster";
    public Vector2 spawnPosition = new Vector2(0f, 5f);

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnMonster();
        }
    }

    void SpawnMonster()
    {
        PhotonNetwork.Instantiate(monsterPrefabPath, spawnPosition, Quaternion.identity);
    }
}
