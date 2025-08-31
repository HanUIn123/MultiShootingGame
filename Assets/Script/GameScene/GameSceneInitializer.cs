using UnityEngine;
using Photon.Pun;

public class GameSceneInitializer : MonoBehaviourPunCallbacks
{
    [Header("�÷��̾� ������ �̸� (Resources ���� ����)")]
    public string playerPrefabName = "PlayerPrefab";

    [Header("�� �ȿ� �����ϴ� UI")]
    public PlayerHealthUI playerHealthUI;
    public UltimateUIManager ultimateUIManager;

    private void Start()
    {
        if (!(PhotonNetwork.IsConnected && PhotonNetwork.InRoom))
        {
            Debug.LogWarning("[GameSceneInitializer] Photon�� ����Ǿ� ���� �ʰų� �濡 �������� ����.");
            return;
        }

        // 1. �÷��̾� ����
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

        // �����
        SoundManager.Instance.PlayBGM("Stage1BGM");
    }
}
