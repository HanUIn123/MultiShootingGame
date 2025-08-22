using UnityEngine;
using Photon.Pun;

public class GameSceneInitializer : MonoBehaviourPunCallbacks
{
    public string playerPrefabName = "PlayerPrefab";

    private void Awake()
    {
        // �� ����ȭ ����
        Application.targetFrameRate = 120;
        QualitySettings.vSyncCount = 0;

        // �г��� ���� (�̹� ���� �� �Ǿ� ���� ���)
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
            // �÷��̾� ID�� ���� ������ ��ġ ����
            float xOffset = PhotonNetwork.IsMasterClient ? -2f : 2f;

            Vector3 spawnPos = new Vector3(xOffset, -3.5f, 0f); // �¿�� ��ġ

            // ȸ���� ��� �÷��̾ �����ϰ�
            Quaternion spawnRot = Quaternion.identity;

            PhotonNetwork.Instantiate(playerPrefabName, spawnPos, spawnRot);
        }
        else
        {
            Debug.LogWarning("Photon�� ����Ǿ� ���� �ʰų� �濡 �� ���� �ʽ��ϴ�.");
        }
    }

}