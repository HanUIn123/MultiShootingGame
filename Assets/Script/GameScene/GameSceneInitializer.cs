using UnityEngine;
using Photon.Pun;

public class GameSceneInitializer : MonoBehaviourPunCallbacks
{
    [Header("플레이어 프리팹 이름 (Resources 폴더 기준)")]
    public string playerPrefabName = "PlayerPrefab";

    [Header("씬 안에 존재하는 UI")]
    public PlayerHealthUI playerHealthUI;
    public UltimateUIManager ultimateUIManager;

    private void Start()
    {
        if (!(PhotonNetwork.IsConnected && PhotonNetwork.InRoom))
        {
            Debug.LogWarning("[GameSceneInitializer] Photon에 연결되어 있지 않거나 방에 입장하지 않음.");
            return;
        }

        // 1. 플레이어 스폰
        float xOffset = PhotonNetwork.IsMasterClient ? -2f : 2f;
        Vector3 spawnPosition = new Vector3(xOffset, -3.5f, 0f);
        GameObject player = PhotonNetwork.Instantiate(playerPrefabName, spawnPosition, Quaternion.identity);

        var health = player.GetComponent<PlayerHealth>();
        if (health && health.photonView.IsMine)
        {
            health.AssignUI(playerHealthUI);
            playerHealthUI.SetHP(health.maxHP);
        }

        var pc = player.GetComponent<PlayerController>();
        if (pc && pc.photonView.IsMine)
        {
            pc.ultimateUI = ultimateUIManager;
            var firePoint = player.transform.Find("FirePoint");
            if (firePoint) pc.InitLaserSpawn(firePoint);
        }

        // 배경음
        SoundManager.Instance.PlayBGM("Stage1BGM");
    }
}
