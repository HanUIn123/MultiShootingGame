using UnityEngine;
using Photon.Pun;

public class GameSceneInitializer : MonoBehaviourPunCallbacks
{
    public string playerPrefabName = "PlayerPrefab";

    [Header("UI")]
    public PlayerHealthUI playerHealthUI;  // 유니티 인스펙터에 Drag

    private void Awake()
    {
        Application.targetFrameRate = 120;
        QualitySettings.vSyncCount = 0;

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
            float xOffset = PhotonNetwork.IsMasterClient ? -2f : 2f;
            Vector3 spawnPos = new Vector3(xOffset, -3.5f, 0f);
            Quaternion spawnRot = Quaternion.identity;

            GameObject player = PhotonNetwork.Instantiate(playerPrefabName, spawnPos, spawnRot);

            // 직접 연결
            var health = player.GetComponent<PlayerHealth>();
            if (health != null && health.photonView.IsMine)
            {
                health.AssignUI(playerHealthUI); // 여기서 SetHP 내부 실행됨
                playerHealthUI.SetHP(health.maxHP);  // 여기서 안전하게 호출
            }
        }
        else
        {
            Debug.LogWarning("Photon에 연결되어 있지 않거나 방에 들어가 있지 않습니다.");
        }
    }
}
