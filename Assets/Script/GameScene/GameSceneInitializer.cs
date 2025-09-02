using UnityEngine;
using Photon.Pun;

public class GameSceneInitializer : MonoBehaviourPunCallbacks
{
    [Header("�÷��̾� ������ �̸�")]
    public string playerPrefabName = "PlayerPrefab";

    [Header("�÷��̾� ������ ü�� & �ñر� UI")]
    public PlayerHealthUI playerHealthUI;
    public UltimateUIManager ultimateUIManager;

    private void Start()
    {
        if (!(PhotonNetwork.IsConnected && PhotonNetwork.InRoom))
        {
            Debug.LogWarning("Photon ������ ���� ����! or �濡 �������� �ʾ���!");
            return;
        }

        float xOffset = PhotonNetwork.IsMasterClient ? -2f : 2f;

        Vector3 spawnPosition = new Vector3(xOffset, -3.5f, 0f);
        GameObject player = PhotonNetwork.Instantiate(playerPrefabName, spawnPosition, Quaternion.identity);

        var playerHealth = player.GetComponent<PlayerHealth>();

        if (playerHealth && playerHealth.photonView.IsMine)
        {
            playerHealth.AssignUI(playerHealthUI);

            playerHealthUI.SetHP(playerHealth.maxHP);
        }

        var playerController = player.GetComponent<PlayerController>();

        if (playerController && playerController.photonView.IsMine)
        {
            playerController.ultimateUI = ultimateUIManager;

            var firePoint = player.transform.Find("FirePoint");

            if (firePoint) playerController.InitLaserSpawn(firePoint);
        }

        SoundManager.Instance.PlayBGM("Stage1BGM");
    }
}
