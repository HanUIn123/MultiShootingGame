using UnityEngine;
using Photon.Pun;

public class GameSceneInitializer : MonoBehaviourPunCallbacks
{
    public string playerPrefabName = "PlayerPrefab";

    private void Awake()
    {
        // 렉 최적화 설정
        Application.targetFrameRate = 120;
        QualitySettings.vSyncCount = 0;

        // 닉네임 설정 (이미 설정 안 되어 있을 경우)
        if (string.IsNullOrEmpty(PhotonNetwork.NickName))
        {
            PhotonNetwork.NickName = "플레이어" + Random.Range(1000, 9999);
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
            // 플레이어 ID에 따라 옆으로 위치 조정
            float xOffset = PhotonNetwork.IsMasterClient ? -2f : 2f;

            Vector3 spawnPos = new Vector3(xOffset, -3.5f, 0f); // 좌우로 배치

            // 회전은 모든 플레이어가 동일하게
            Quaternion spawnRot = Quaternion.identity;

            PhotonNetwork.Instantiate(playerPrefabName, spawnPos, spawnRot);
        }
        else
        {
            Debug.LogWarning("Photon에 연결되어 있지 않거나 방에 들어가 있지 않습니다.");
        }
    }

}