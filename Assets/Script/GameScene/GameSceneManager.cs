using UnityEngine;
using Photon.Pun;

public class GameSceneManager : MonoBehaviour
{
    public string monsterPrefabPath = "Monster/Monster"; // Resources/Monster/Monster.prefab
    public Vector2 spawnPosition = new Vector2(0f, 5f); // ������ ����


    private void Start()
    {
        // ������ Ŭ���̾�Ʈ�� ���� ����
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
        // ���� ���� ���� ���� �� ���⼭ ��� Ȯ�� ����
    }
}
