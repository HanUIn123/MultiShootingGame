using UnityEngine;
using Photon.Pun;

public class GameSceneInitializer : MonoBehaviourPunCallbacks
{
    public string playerPrefabName = "PlayerPrefab";

    [Header("UI")]
    public PlayerHealthUI playerHealthUI;  // ����Ƽ �ν����Ϳ� Drag

    private void Awake()
    {
        Application.targetFrameRate = 120;
        QualitySettings.vSyncCount = 0;

        if (string.IsNullOrEmpty(PhotonNetwork.NickName))
        {
            PhotonNetwork.NickName = "�÷��̾�" + Random.Range(1000, 9999);
        }

        if (FindFirstObjectByType<FPSDisplay>() == null)
        {
            gameObject.AddComponent<FPSDisplay>();
        }
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            float xOffset = PhotonNetwork.IsMasterClient ? -2f : 2f;
            Vector3 spawnPos = new Vector3(xOffset, -3.5f, 0f);
            Quaternion spawnRot = Quaternion.identity;

            GameObject player = PhotonNetwork.Instantiate(playerPrefabName, spawnPos, spawnRot);

            // ���� ����
            var health = player.GetComponent<PlayerHealth>();
            if (health != null && health.photonView.IsMine)
            {
                health.AssignUI(playerHealthUI); // ���⼭ SetHP ���� �����
                playerHealthUI.SetHP(health.maxHP);  // ���⼭ �����ϰ� ȣ��
            }
        }
        else
        {
            Debug.LogWarning("Photon�� ����Ǿ� ���� �ʰų� �濡 �� ���� �ʽ��ϴ�.");
        }
    }
}
